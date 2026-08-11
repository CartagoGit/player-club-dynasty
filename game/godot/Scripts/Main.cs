using Godot;
using PCDynasty.Core.Domain;

namespace PCDynasty.Presentation;

public partial class Main : Control
{
    private static readonly int[] PreviewAttributeValues = [72, 84, 91];

    private Label CoreValueLabel => GetNode<Label>("%CoreValueLabel");

    public override void _Ready()
    {
        CoreValueLabel.Text = CreateCoreSummary();
    }

    private static string CreateCoreSummary()
    {
        var previewAttributes = new AttributeVector(PreviewAttributeValues);
        return $"{previewAttributes.Count} atributos cargados desde el núcleo";
    }
}
