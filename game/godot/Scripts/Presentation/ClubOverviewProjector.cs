using System.Collections.Immutable;
using PCDynasty.Application.Workspace;
using PCDynasty.Presentation.Clubs;
using PCDynasty.Presentation.Components;

namespace PCDynasty.Presentation.Presentation;

/// <summary>
/// Translates the engine-neutral <see cref="ClubRow"/> projection into a
/// <see cref="ClubOverviewView"/> the existing <see cref="ClubOverviewController"/>
/// already knows how to render. Kept separate from the Godot controller so
/// the projection is unit-testable.
/// </summary>
public static class ClubOverviewProjector
{
    public static ClubOverviewView FromClubRow(ClubRow club)
    {
        ArgumentNullException.ThrowIfNull(club);

        var metrics = new[]
        {
            new MetricCardView(
                "Año de fundación",
                club.FoundedYear,
                "Dato histórico del club",
                $"Fundado en {club.FoundedYear}."),
            new MetricCardView(
                "Prestigio",
                club.Prestige.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "Reputación competitiva",
                $"Prestigio actual: {club.Prestige} sobre 100."),
            new MetricCardView(
                "Afición",
                club.Fanbase.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "Base de seguidores",
                $"Afición estimada: {club.Fanbase} sobre 100."),
            new MetricCardView(
                "Plantilla",
                club.SquadSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "Jugadores registrados",
                $"Plantilla actual: {club.SquadSize} jugadores."),
        };

        return new ClubOverviewView(
            club.ExternalId,
            club.DisplayName,
            $"{club.City} · {club.CountryShortName}",
            metrics,
            alerts: ["Sin alertas activas — temporada en pretemporada."],
            nextActions: ["Avanzar a pretemporada", "Revisar plantilla", "Configurar alineación"]);
    }
}
