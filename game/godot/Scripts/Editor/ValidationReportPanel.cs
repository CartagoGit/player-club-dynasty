using System.Collections.Immutable;
using Godot;

namespace PCDynasty.Presentation.Editor;

public sealed record ValidationMessageView(
    string Code,
    string Message,
    string? Context = null);

public partial class ValidationReportPanel : VBoxContainer
{
    public void Bind(IEnumerable<ValidationMessageView> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ImmutableArray<ValidationMessageView> snapshot = messages
            .Select(message => message ?? throw new ArgumentException(
                "Validation messages cannot contain null entries.",
                nameof(messages)))
            .ToImmutableArray();

        Clear();
        if (snapshot.IsEmpty)
        {
            AddChild(new Label
            {
                Text = "Sin errores. El contenido está listo para exportar.",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            });
            return;
        }

        foreach (ValidationMessageView message in snapshot)
        {
            string context = string.IsNullOrWhiteSpace(message.Context)
                ? string.Empty
                : $" · {message.Context}";
            AddChild(new Label
            {
                Text = $"{message.Code}{context}\n{message.Message}",
                TooltipText = message.Message,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            });
        }
    }

    private void Clear()
    {
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }
}
