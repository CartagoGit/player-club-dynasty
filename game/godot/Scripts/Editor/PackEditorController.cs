using System.Collections.Immutable;
using Godot;
using PCDynasty.Editor.Documents;

namespace PCDynasty.Presentation.Editor;

public partial class PackEditorController : Control
{
    private IPackDocumentService documentService = new PackDocumentService();
    private PackDocument document = new("community:new", 1, []);

    [Signal]
    public delegate void ValidPackPreparedEventHandler(string providerId, int entityCount);

    private Button ApplyEntityButton => GetNode<Button>("%ApplyEntityButton");
    private LineEdit EntityId => GetNode<LineEdit>("%EntityId");
    private LineEdit EntityKind => GetNode<LineEdit>("%EntityKind");
    private ItemList Entities => GetNode<ItemList>("%Entities");
    private LineEdit ProviderId => GetNode<LineEdit>("%ProviderId");
    private SpinBox SchemaVersion => GetNode<SpinBox>("%SchemaVersion");
    private Label Status => GetNode<Label>("%Status");
    private Button ValidateButton => GetNode<Button>("%ValidateButton");
    private ValidationReportPanel ValidationReport =>
        GetNode<ValidationReportPanel>("%ValidationReport");
    private LineEdit ValueKey => GetNode<LineEdit>("%ValueKey");
    private LineEdit ValueValue => GetNode<LineEdit>("%ValueValue");

    public override void _Ready()
    {
        ApplyEntityButton.Pressed += ApplyEntity;
        ValidateButton.Pressed += ValidateDraft;
        Bind(document);
    }

    public void Configure(IPackDocumentService service)
    {
        documentService = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Bind(PackDocument packDocument)
    {
        document = packDocument ?? throw new ArgumentNullException(nameof(packDocument));
        ProviderId.Text = document.ProviderId;
        SchemaVersion.Value = document.SchemaVersion;
        RefreshEntities();
        Status.Text = "Borrador sin validar";
        ValidationReport.Bind([]);
    }

    private void ApplyEntity()
    {
        ImmutableArray<PackValueDocument> values = string.IsNullOrWhiteSpace(ValueKey.Text)
            ? []
            : [new PackValueDocument(ValueKey.Text, ValueValue.Text)];
        document = ReadPackMetadata().Upsert(new PackEntityDocument(
            EntityId.Text,
            EntityKind.Text,
            values,
            []));
        RefreshEntities();
        Status.Text = "Cambios pendientes de validar";
    }

    private void ValidateDraft()
    {
        document = ReadPackMetadata();
        PackDocumentResult result = documentService.ValidateAndNormalize(document);
        ValidationReport.Bind(result.Diagnostics.Select(diagnostic =>
            new ValidationMessageView(
                diagnostic.Code,
                diagnostic.Message,
                diagnostic.ExternalId)));

        if (!result.IsValid)
        {
            Status.Text = $"Revisión necesaria · {result.Diagnostics.Length} incidencias";
            return;
        }

        Status.Text = "Validado · listo para entregar al servicio de exportación";
        EmitSignal(
            SignalName.ValidPackPrepared,
            document.ProviderId,
            document.Entities.Length);
    }

    private PackDocument ReadPackMetadata() => new(
        ProviderId.Text,
        (int)SchemaVersion.Value,
        document.Entities);

    private void RefreshEntities()
    {
        Entities.Clear();
        foreach (PackEntityDocument entity in document.Entities.OrderBy(
                     item => item.ExternalId,
                     StringComparer.Ordinal))
        {
            Entities.AddItem($"{entity.ExternalId}  ·  {entity.KindId}");
        }
    }
}
