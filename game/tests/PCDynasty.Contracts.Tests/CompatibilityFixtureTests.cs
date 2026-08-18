using System.Reflection;
using System.Text.Json;
using PCDynasty.Contracts.Commands;
using Xunit;

namespace PCDynasty.Contracts.Tests;

public sealed class CompatibilityFixtureTests
{
    [Fact]
    public void CurrentCommandEnvelopeMatchesThePublishedV1Fixture()
    {
        ContractSchemaFixture baseline = ContractSchemaFixture.Load();
        ContractProperty[] current = ContractCompatibility.Describe(typeof(CommandEnvelope));

        ContractChangeKind change = ContractCompatibility.Compare(
            baseline.Properties,
            current);

        Assert.Equal(1, baseline.MajorVersion);
        Assert.Equal(ContractChangeKind.Unchanged, change);
    }

    [Fact]
    public void ClassifiesBreakingCompatibleAndDeprecatedChanges()
    {
        ContractProperty[] baseline =
        [
            new("id", "String", Required: true, Deprecated: false),
        ];

        Assert.Equal(
            ContractChangeKind.Breaking,
            ContractCompatibility.Compare(baseline, []));
        Assert.Equal(
            ContractChangeKind.Compatible,
            ContractCompatibility.Compare(
                baseline,
                [.. baseline, new ContractProperty("note", "String", false, false)]));
        Assert.Equal(
            ContractChangeKind.Deprecated,
            ContractCompatibility.Compare(
                baseline,
                [new ContractProperty("id", "String", true, true)]));
    }
}

[Flags]
internal enum ContractChangeKind
{
    Unchanged = 0,
    Compatible = 1,
    Breaking = 2,
    Deprecated = 4,
}

internal sealed record ContractProperty(
    string Name,
    string Type,
    bool Required,
    bool Deprecated);

internal sealed class ContractSchemaFixture
{
    public int MajorVersion { get; init; }
    public ContractProperty[] Properties { get; init; } = [];

    public static ContractSchemaFixture Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "v1-command.json");
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ContractSchemaFixture>(json, Options) ??
            throw new InvalidDataException("Contract fixture is empty.");
    }

    private static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

internal static class ContractCompatibility
{
    public static ContractProperty[] Describe(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var nullability = new NullabilityInfoContext();
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => new ContractProperty(
                JsonNamingPolicy.CamelCase.ConvertName(property.Name),
                DescribeType(property.PropertyType),
                property.PropertyType.IsValueType ||
                nullability.Create(property).ReadState == NullabilityState.NotNull,
                property.GetCustomAttribute<ObsoleteAttribute>() is not null))
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
    }

    public static ContractChangeKind Compare(
        IEnumerable<ContractProperty> baseline,
        IEnumerable<ContractProperty> current)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(current);
        Dictionary<string, ContractProperty> baselineByName = baseline.ToDictionary(
            property => property.Name,
            StringComparer.Ordinal);
        Dictionary<string, ContractProperty> currentByName = current.ToDictionary(
            property => property.Name,
            StringComparer.Ordinal);
        ContractChangeKind result = ContractChangeKind.Unchanged;
        foreach ((string name, ContractProperty oldProperty) in baselineByName)
        {
            if (!currentByName.TryGetValue(name, out ContractProperty? newProperty) ||
                !string.Equals(oldProperty.Type, newProperty.Type, StringComparison.Ordinal) ||
                oldProperty.Required != newProperty.Required)
            {
                result |= ContractChangeKind.Breaking;
                continue;
            }

            if (!oldProperty.Deprecated && newProperty.Deprecated)
            {
                result |= ContractChangeKind.Deprecated;
            }
        }

        if (currentByName.Keys.Except(baselineByName.Keys, StringComparer.Ordinal).Any())
        {
            result |= ContractChangeKind.Compatible;
        }

        return result;
    }

    private static string DescribeType(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string name = type.Name[..type.Name.IndexOf('`')];
        return $"{name}<{string.Join(',', type.GetGenericArguments().Select(DescribeType))}>";
    }
}
