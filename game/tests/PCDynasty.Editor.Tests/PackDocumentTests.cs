using PCDynasty.Content.Abstractions;
using PCDynasty.Editor.Documents;
using Xunit;

namespace PCDynasty.Editor.Tests;

public sealed class PackDocumentTests
{
    [Fact]
    public void ValidatesDraftsBeforeProducingAnExternalContentModel()
    {
        var invalid = new PackDocument(
            "community:test",
            schemaVersion: 1,
            [
                Entity("competition:cup", "competition"),
                Entity("competition:cup", "competition"),
                new PackEntityDocument(
                    "facility:academy",
                    "facility",
                    [],
                    [new PackReferenceDocument("dependency:competition", "missing:competition", true)]),
            ]);

        PackDocumentResult result = new PackDocumentService().ValidateAndNormalize(invalid);

        Assert.False(result.IsValid);
        Assert.Null(result.Model);
        Assert.Contains(result.Diagnostics, item => item.Code == "editor.entity.duplicate-id");
        Assert.Contains(result.Diagnostics, item => item.Code == "editor.reference.missing");
    }

    [Fact]
    public void ProducesOnlyTheExternalDeclarativeContractWhenValid()
    {
        var document = new PackDocument(
            "community:test",
            2,
            [
                Entity("competition:cup", "competition"),
                new PackEntityDocument(
                    "facility:academy",
                    "facility",
                    [new PackValueDocument("quality", "72")],
                    [new PackReferenceDocument("dependency:competition", "competition:cup", true)]),
            ]);

        PackDocumentResult result = new PackDocumentService().ValidateAndNormalize(document);

        Assert.True(result.IsValid);
        NormalizedContentModel model = Assert.IsType<NormalizedContentModel>(result.Model);
        Assert.Equal("community:test", model.ProviderId);
        Assert.Equal(2, model.SchemaVersion);
        Assert.Equal(2, model.Entities.Length);
        Assert.NotEqual(typeof(PackEntityDocument), model.Entities[0].GetType());
    }

    [Fact]
    public void EditingCreatesANewDocumentAndLeavesThePreviousDraftUntouched()
    {
        var original = new PackDocument(
            "community:test",
            1,
            [Entity("competition:cup", "competition")]);
        PackEntityDocument kit = Entity("kit:home", "kit");

        PackDocument edited = original.Upsert(kit);
        PackDocument replaced = edited.Upsert(new PackEntityDocument(
            "kit:home",
            "kit",
            [new PackValueDocument("primary-color", "#224466")],
            []));

        Assert.Single(original.Entities);
        Assert.Equal(2, edited.Entities.Length);
        Assert.Equal(2, replaced.Entities.Length);
        Assert.Equal("#224466", replaced.Entities.Single(item => item.ExternalId == "kit:home").Values[0].Value);
    }

    private static PackEntityDocument Entity(string externalId, string kindId) =>
        new(externalId, kindId, [], []);
}
