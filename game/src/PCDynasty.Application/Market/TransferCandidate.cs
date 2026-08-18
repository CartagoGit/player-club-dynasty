namespace PCDynasty.Application.Market;

public sealed record TransferCandidate
{
    public TransferCandidate(
        string playerExternalId,
        string playerName,
        string sellerClubExternalId,
        string sellerClubName,
        int birthYear,
        int technique,
        int form,
        int physical,
        int mental,
        int overall,
        long askingPrice,
        string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(birthYear);
        ValidateRating(technique, nameof(technique));
        ValidateRating(form, nameof(form));
        ValidateRating(physical, nameof(physical));
        ValidateRating(mental, nameof(mental));
        ValidateRating(overall, nameof(overall));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(askingPrice);

        PlayerExternalId = Required(playerExternalId, nameof(playerExternalId));
        PlayerName = Required(playerName, nameof(playerName));
        SellerClubExternalId = Required(sellerClubExternalId, nameof(sellerClubExternalId));
        SellerClubName = Required(sellerClubName, nameof(sellerClubName));
        Currency = Required(currency, nameof(currency)).ToUpperInvariant();
        if (Currency.Length != 3 || Currency.Any(character => !char.IsAsciiLetter(character)))
        {
            throw new ArgumentException("Currency must be a three-letter code.", nameof(currency));
        }

        BirthYear = birthYear;
        Technique = technique;
        Form = form;
        Physical = physical;
        Mental = mental;
        Overall = overall;
        AskingPrice = askingPrice;
    }

    public string PlayerExternalId { get; }
    public string PlayerName { get; }
    public string SellerClubExternalId { get; }
    public string SellerClubName { get; }
    public int BirthYear { get; }
    public int Technique { get; }
    public int Form { get; }
    public int Physical { get; }
    public int Mental { get; }
    public int Overall { get; }
    public long AskingPrice { get; }
    public string Currency { get; }

    private static string Required(string value, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(value, parameterName);
        string normalized = value.Trim();
        return normalized.Length > 0
            ? normalized
            : throw new ArgumentException("A non-empty value is required.", parameterName);
    }

    private static void ValidateRating(int value, string parameterName)
    {
        if (value is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
