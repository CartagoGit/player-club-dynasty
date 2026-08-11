using PCDynasty.Core.Domain;
using PCDynasty.Core.People;
using Xunit;

namespace PCDynasty.Core.Tests.People;

public sealed class PersonTests
{
    [Fact]
    public void KeepsIdentityAndProfileImmutable()
    {
        var sourceLanguages = new[] { "es", "en" };
        var profile = new PersonProfile(
            "Iria",
            "Valdés",
            new DateOnly(2001, 3, 17),
            "fictional:country:monteluz",
            "fictional:region:norte",
            sourceLanguages);
        var person = new Person(new PersonId(37), profile);

        sourceLanguages[0] = "changed";

        Assert.Equal(37, person.Id.Value);
        Assert.Same(profile, person.Profile);
        Assert.Equal("Iria", person.Profile.GivenName);
        Assert.Equal("Valdés", person.Profile.FamilyName);
        Assert.Equal(new DateOnly(2001, 3, 17), person.Profile.BirthDate);
        Assert.Equal("fictional:country:monteluz", person.Profile.NationalityId);
        Assert.Equal("fictional:region:norte", person.Profile.RegionId);
        Assert.Equal(["es", "en"], person.Profile.LanguageIds.AsSpan().ToArray());
        Assert.All(
            typeof(Person).GetProperties().Concat(typeof(PersonProfile).GetProperties()),
            property => Assert.Null(property.SetMethod));
    }

    [Theory]
    [InlineData("", "Valdés", "fictional:country:monteluz")]
    [InlineData("Iria", " ", "fictional:country:monteluz")]
    [InlineData("Iria", "Valdés", "")]
    public void RejectsMissingRequiredProfileData(
        string givenName,
        string familyName,
        string nationalityId)
    {
        Assert.Throws<ArgumentException>(() => new PersonProfile(
            givenName,
            familyName,
            new DateOnly(2001, 3, 17),
            nationalityId));
    }

    [Fact]
    public void RejectsDuplicateOrBlankLanguageIdentifiers()
    {
        Assert.Throws<ArgumentException>(() => new PersonProfile(
            "Iria",
            "Valdés",
            new DateOnly(2001, 3, 17),
            "fictional:country:monteluz",
            languageIds: ["es", "ES"]));

        Assert.Throws<ArgumentException>(() => new PersonProfile(
            "Iria",
            "Valdés",
            new DateOnly(2001, 3, 17),
            "fictional:country:monteluz",
            languageIds: ["es", " "]));
    }
}
