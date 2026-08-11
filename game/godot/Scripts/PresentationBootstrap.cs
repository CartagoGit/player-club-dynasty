using Godot;
using PCDynasty.Content.Importers;
using PCDynasty.Presentation.Core;
using PCDynasty.Presentation.Presentation;

namespace PCDynasty.Presentation;

public partial class PresentationBootstrap : Control
{
    private Label StatusLabel => GetNode<Label>("%StatusLabel");

    public override void _Ready()
    {
        _ = InitializeAndLoadAsync();
    }

    private async Task InitializeAndLoadAsync()
    {
        try
        {
            // Content root: parent of godot/ is game/, content/ is at game/content/.
            // Resolved through Godot's ProjectSettings so a packaged build keeps working.
            string gameRoot = ProjectSettings.GlobalizePath("res://../");
            string contentRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(gameRoot, "content"));

            var reader = new JsonContentReader();
            var service = WorldService.Initialize(reader, contentRoot);
            await service.ReloadAsync();

            int clubCount = service.ListClubs().Count;
            StatusLabel.Text = $"Mundo cargado: {clubCount} clubs listos para la temporada 1";

            // Boot the main controller now that the domain is available.
            AttachMainController();
        }
        catch (System.Exception exception)
        {
            StatusLabel.Text = $"Error cargando contenido: {exception.Message}";
            GD.PushError($"[WorldService] {exception}");
        }
    }

    private void AttachMainController()
    {
        var controller = new MainController
        {
            Name = "MainController",
        };
        AddChild(controller);
    }
}
