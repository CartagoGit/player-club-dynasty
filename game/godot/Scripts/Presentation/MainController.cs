using PCDynasty.Presentation.Workspace;

namespace PCDynasty.Presentation.Presentation;

/// <summary>
/// Compatibility alias for scenes created before the workspace was decomposed.
/// New composition code uses <see cref="ManagerWorkspaceController"/> directly.
/// </summary>
public partial class MainController : ManagerWorkspaceController
{
}
