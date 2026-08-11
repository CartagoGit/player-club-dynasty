using Godot;

namespace PCDynasty.Presentation;

public partial class PresentationBootstrap : Control
{
    private Label StatusLabel => GetNode<Label>("%StatusLabel");

    public override void _Ready()
    {
        StatusLabel.Text = "Presentación preparada para modelos de lectura";
    }
}
