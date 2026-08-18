using Godot;
using PCDynasty.Application.Matches;
using PCDynasty.Contracts.Careers;

namespace PCDynasty.Presentation.Matches;

public partial class LineupSelectionController : Control
{
    private readonly List<CandidateControls> _candidateControls = [];
    private Action<SubmitLineupCommand>? _sendCommand;
    private LineupSelectionView? _view;

    private Container CandidateRows => GetNode<Container>("%CandidateRows");
    private LineEdit OptionValue => GetNode<LineEdit>("%OptionValue");
    private Label Status => GetNode<Label>("%Status");
    private Button Submit => GetNode<Button>("%Submit");
    private Label Title => GetNode<Label>("%Title");

    public override void _Ready()
    {
        Submit.Pressed += SubmitSelection;
    }

    public void Bind(
        LineupSelectionView view,
        Action<SubmitLineupCommand> sendCommand,
        LineupPlan? savedPlan = null)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _sendCommand = sendCommand ?? throw new ArgumentNullException(nameof(sendCommand));
        Title.Text = view.Title;
        Status.Text = savedPlan is null
            ? "Propuesta inicial: los once primeros jugadores están seleccionados."
            : $"Plan guardado · {savedPlan.SubmittedAtUtc:dd/MM/yyyy HH:mm} UTC";
        OptionValue.Text = savedPlan?.Options.GetValueOrDefault("approach") ?? "balanced";
        ClearCandidates();
        for (int index = 0; index < view.Candidates.Length; index++)
        {
            LineupCandidateView candidate = view.Candidates[index];
            LineupAssignment? savedAssignment = savedPlan?.Assignments.FirstOrDefault(item =>
                string.Equals(item.ParticipantId, candidate.ParticipantId, StringComparison.Ordinal));
            CandidateControls controls = CreateCandidateControls(
                candidate,
                savedAssignment,
                preselected: savedPlan is null && index < 11);
            _candidateControls.Add(controls);
            CandidateRows.AddChild(controls.Row);
        }
    }

    private void SubmitSelection()
    {
        if (_view is null || _sendCommand is null)
        {
            Status.Text = "La vista aún no está preparada.";
            return;
        }

        CandidateControls[] selected = _candidateControls
            .Where(candidate => candidate.Selected.ButtonPressed)
            .ToArray();
        if (selected.Length == 0)
        {
            Status.Text = "Selecciona al menos un participante.";
            return;
        }

        if (selected.Any(candidate => string.IsNullOrWhiteSpace(candidate.Assignment.Text)))
        {
            Status.Text = "Cada selección necesita una asignación.";
            return;
        }

        LineupDraftSelection[] draft = selected.Select((candidate, order) =>
            new LineupDraftSelection(
                candidate.ParticipantId,
                candidate.Assignment.Text,
                order)).ToArray();
        KeyValuePair<string, string>[] options = string.IsNullOrWhiteSpace(OptionValue.Text)
            ? []
            : [KeyValuePair.Create("approach", OptionValue.Text.Trim())];

        SubmitLineupCommand command = LineupCommandMapper.Map(_view, draft, options);
        _sendCommand(command);
        Status.Text = "Selección enviada.";
    }

    private static CandidateControls CreateCandidateControls(
        LineupCandidateView candidate,
        LineupAssignment? savedAssignment,
        bool preselected)
    {
        var row = new HBoxContainer();
        var selected = new CheckBox
        {
            Text = candidate.DisplayName,
            ButtonPressed = savedAssignment is not null || preselected,
            TooltipText = $"Incluir a {candidate.DisplayName}",
            FocusMode = FocusModeEnum.All,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        var assignment = new LineEdit
        {
            Text = savedAssignment?.AssignmentId ?? candidate.SuggestedAssignmentId,
            PlaceholderText = "Asignación",
            TooltipText = $"Asignación para {candidate.DisplayName}",
            FocusMode = FocusModeEnum.All,
            CustomMinimumSize = new Vector2(220, 40),
        };
        row.AddChild(selected);
        row.AddChild(assignment);
        return new CandidateControls(candidate.ParticipantId, row, selected, assignment);
    }

    private void ClearCandidates()
    {
        _candidateControls.Clear();
        foreach (Node child in CandidateRows.GetChildren())
        {
            CandidateRows.RemoveChild(child);
            child.QueueFree();
        }
    }

    private sealed record CandidateControls(
        string ParticipantId,
        HBoxContainer Row,
        CheckBox Selected,
        LineEdit Assignment);
}
