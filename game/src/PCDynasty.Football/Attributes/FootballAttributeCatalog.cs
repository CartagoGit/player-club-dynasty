using System.Collections.Immutable;
using PCDynasty.Core.Domain;

namespace PCDynasty.Football.Attributes;

public enum FootballAttributeGroup
{
    Technique,
    Defense,
    Physical,
    Mental,
    Goalkeeping,
}

public readonly record struct FootballAttributeDefinition(
    AttributeId Id,
    string Key,
    FootballAttributeGroup Group);

public static class FootballAttributeCatalog
{
    public static readonly AttributeId BallControl = new(0);
    public static readonly AttributeId Dribbling = new(1);
    public static readonly AttributeId ShortPassing = new(2);
    public static readonly AttributeId LongPassing = new(3);
    public static readonly AttributeId Crossing = new(4);
    public static readonly AttributeId Shooting = new(5);
    public static readonly AttributeId Finishing = new(6);
    public static readonly AttributeId Heading = new(7);
    public static readonly AttributeId Tackling = new(8);
    public static readonly AttributeId Marking = new(9);
    public static readonly AttributeId DefensivePositioning = new(10);
    public static readonly AttributeId Anticipation = new(11);
    public static readonly AttributeId Acceleration = new(12);
    public static readonly AttributeId Pace = new(13);
    public static readonly AttributeId Strength = new(14);
    public static readonly AttributeId Stamina = new(15);
    public static readonly AttributeId Jumping = new(16);
    public static readonly AttributeId Agility = new(17);
    public static readonly AttributeId Decisions = new(18);
    public static readonly AttributeId Concentration = new(19);
    public static readonly AttributeId Vision = new(20);
    public static readonly AttributeId Composure = new(21);
    public static readonly AttributeId Bravery = new(22);
    public static readonly AttributeId Reflexes = new(23);
    public static readonly AttributeId Handling = new(24);
    public static readonly AttributeId GoalkeeperPositioning = new(25);
    public static readonly AttributeId OneOnOne = new(26);
    public static readonly AttributeId AerialAbility = new(27);
    public static readonly AttributeId RushingOut = new(28);
    public static readonly AttributeId Distribution = new(29);

    public static ImmutableArray<FootballAttributeDefinition> All { get; } =
    [
        Define(BallControl, "ball-control", FootballAttributeGroup.Technique),
        Define(Dribbling, "dribbling", FootballAttributeGroup.Technique),
        Define(ShortPassing, "short-passing", FootballAttributeGroup.Technique),
        Define(LongPassing, "long-passing", FootballAttributeGroup.Technique),
        Define(Crossing, "crossing", FootballAttributeGroup.Technique),
        Define(Shooting, "shooting", FootballAttributeGroup.Technique),
        Define(Finishing, "finishing", FootballAttributeGroup.Technique),
        Define(Heading, "heading", FootballAttributeGroup.Technique),
        Define(Tackling, "tackling", FootballAttributeGroup.Defense),
        Define(Marking, "marking", FootballAttributeGroup.Defense),
        Define(DefensivePositioning, "defensive-positioning", FootballAttributeGroup.Defense),
        Define(Anticipation, "anticipation", FootballAttributeGroup.Defense),
        Define(Acceleration, "acceleration", FootballAttributeGroup.Physical),
        Define(Pace, "pace", FootballAttributeGroup.Physical),
        Define(Strength, "strength", FootballAttributeGroup.Physical),
        Define(Stamina, "stamina", FootballAttributeGroup.Physical),
        Define(Jumping, "jumping", FootballAttributeGroup.Physical),
        Define(Agility, "agility", FootballAttributeGroup.Physical),
        Define(Decisions, "decisions", FootballAttributeGroup.Mental),
        Define(Concentration, "concentration", FootballAttributeGroup.Mental),
        Define(Vision, "vision", FootballAttributeGroup.Mental),
        Define(Composure, "composure", FootballAttributeGroup.Mental),
        Define(Bravery, "bravery", FootballAttributeGroup.Mental),
        Define(Reflexes, "reflexes", FootballAttributeGroup.Goalkeeping),
        Define(Handling, "handling", FootballAttributeGroup.Goalkeeping),
        Define(GoalkeeperPositioning, "goalkeeper-positioning", FootballAttributeGroup.Goalkeeping),
        Define(OneOnOne, "one-on-one", FootballAttributeGroup.Goalkeeping),
        Define(AerialAbility, "aerial-ability", FootballAttributeGroup.Goalkeeping),
        Define(RushingOut, "rushing-out", FootballAttributeGroup.Goalkeeping),
        Define(Distribution, "distribution", FootballAttributeGroup.Goalkeeping),
    ];

    public static int Count => All.Length;

    public static AttributeVector Compile(ReadOnlySpan<int> values)
    {
        if (values.Length != Count)
        {
            throw new ArgumentException(
                $"A football attribute vector must contain exactly {Count} values.",
                nameof(values));
        }

        for (int index = 0; index < values.Length; index++)
        {
            if (values[index] is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(values),
                    values[index],
                    $"The value at compiled attribute index {index} must be between zero and one hundred.");
            }
        }

        return new AttributeVector(values);
    }

    private static FootballAttributeDefinition Define(
        AttributeId id,
        string key,
        FootballAttributeGroup group)
    {
        return new FootballAttributeDefinition(id, key, group);
    }
}
