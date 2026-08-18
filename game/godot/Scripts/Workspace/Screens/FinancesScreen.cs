using Godot;
using PCDynasty.Application.Finances;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Theme;
using PCDynasty.Presentation.Workspace.Components;

namespace PCDynasty.Presentation.Workspace.Screens;

internal sealed class FinancesScreen
{
    private readonly ICareerFinanceService _finances;
    private readonly ManagedClubContext _managed;

    internal FinancesScreen(ICareerFinanceService finances, ManagedClubContext managed)
    {
        _finances = finances ?? throw new ArgumentNullException(nameof(finances));
        _managed = managed ?? throw new ArgumentNullException(nameof(managed));
    }

    internal void Render(Control root, ClubRow club)
    {
        ClubFinanceOverview overview = _finances.GetOverview(_managed);
        var office = new VBoxContainer { Name = "FinancesOffice" };
        office.AddThemeConstantOverride("separation", ManagerDesignTokens.Space16);
        office.AddChild(Header(club, overview));

        var metrics = new GridContainer { Name = "FinanceMetrics", Columns = 3 };
        metrics.AddThemeConstantOverride("h_separation", ManagerDesignTokens.Space12);
        metrics.AddThemeConstantOverride("v_separation", ManagerDesignTokens.Space12);
        metrics.AddChild(Metric("Cash", "TESORERÍA", overview.Cash, overview.Currency, ManagerDesignTokens.Positive));
        metrics.AddChild(Metric("TransferBudget", "PRESUPUESTO FICHAJES", overview.TransferBudget, overview.Currency, ManagerDesignTokens.Accent));
        metrics.AddChild(Metric("WageBudget", "MASA SALARIAL OBJETIVO", overview.WageBudget, overview.Currency, ManagerDesignTokens.Focus));
        office.AddChild(metrics);

        office.AddChild(WorkspaceUiFactory.CreatePanel(
            "CUENTA DE RESULTADOS",
            [
                $"Ingresos previstos · {WorkspaceUiFactory.FormatMoney(overview.Revenue, overview.Currency)}",
                $"Costes operativos · {WorkspaceUiFactory.FormatMoney(overview.Costs, overview.Currency)}",
                $"Resultado operativo · {WorkspaceUiFactory.FormatMoney(overview.OperatingResult, overview.Currency)}",
                $"Deuda financiera · {WorkspaceUiFactory.FormatMoney(overview.Debt, overview.Currency)}",
            ],
            overview.OperatingResult >= 0 ? ManagerDesignTokens.Positive : ManagerDesignTokens.Negative));
        office.AddChild(WorkspaceUiFactory.CreatePanel(
            "CRITERIO DEL CONSEJO",
            [
                "Mantener liquidez positiva durante la temporada.",
                "El presupuesto de fichajes está reservado sobre la tesorería disponible.",
                "Las cifras se guardan en la base externa de esta carrera.",
            ],
            ManagerDesignTokens.Accent));
        root.AddChild(office);
    }

    private static VBoxContainer Header(ClubRow club, ClubFinanceOverview overview)
    {
        var header = new VBoxContainer();
        header.AddThemeConstantOverride("separation", ManagerDesignTokens.Space4);
        var title = new Label { Text = "FINANZAS · CONTROL PRESUPUESTARIO" };
        title.AddThemeColorOverride("font_color", ManagerDesignTokens.TextPrimary);
        title.AddThemeFontSizeOverride("font_size", 22);
        header.AddChild(title);
        var subtitle = new Label
        {
            Text = $"{club.DisplayName} · ejercicio {overview.SeasonYear} · importes en {overview.Currency}",
        };
        subtitle.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        subtitle.AddThemeFontSizeOverride("font_size", 13);
        header.AddChild(subtitle);
        return header;
    }

    private static PanelContainer Metric(
        string name,
        string label,
        long amount,
        string currency,
        Color color)
    {
        PanelContainer panel = WorkspaceUiFactory.CreateSurfacePanel();
        panel.Name = name;
        var content = new VBoxContainer { Name = "Content" };
        var caption = new Label { Text = label };
        caption.AddThemeColorOverride("font_color", ManagerDesignTokens.TextSecondary);
        caption.AddThemeFontSizeOverride("font_size", 11);
        content.AddChild(caption);
        var value = new Label { Name = "Value", Text = WorkspaceUiFactory.FormatMoney(amount, currency) };
        value.AddThemeColorOverride("font_color", color);
        value.AddThemeFontSizeOverride("font_size", 23);
        content.AddChild(value);
        panel.AddChild(content);
        return panel;
    }

}
