using System.Collections.Immutable;

namespace PCDynasty.Application.Workspace;

/// <summary>One competition a club can take part in.</summary>
public sealed record CompetitionRow
{
    public CompetitionRow(
        string ExternalId,
        string DisplayName,
        string Format,
        string Frequency,
        IReadOnlyList<string> Rivals)
    {
        ArgumentNullException.ThrowIfNull(Rivals);
        this.ExternalId = ExternalId;
        this.DisplayName = DisplayName;
        this.Format = Format;
        this.Frequency = Frequency;
        this.Rivals = Rivals.ToImmutableArray();
    }

    public string ExternalId { get; }

    public string DisplayName { get; }

    public string Format { get; }

    public string Frequency { get; }

    public IReadOnlyList<string> Rivals { get; }
}
