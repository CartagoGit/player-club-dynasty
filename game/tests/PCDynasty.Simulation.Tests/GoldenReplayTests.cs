using System.Diagnostics;
using System.Text.Json;
using PCDynasty.Simulation.Determinism;
using Xunit;

namespace PCDynasty.Simulation.Tests;

public sealed class GoldenReplayTests
{
    [Fact]
    public void CanonicalReplayMatchesVersionedGoldenFacts()
    {
        GoldenReplayFixture fixture = GoldenReplayFixture.Load();

        Assert.Equal(fixture.ExpectedFacts, CanonicalReplay.Execute(fixture));
    }

    [Fact]
    public async Task IndependentProcessMatchesTheSameGoldenFixture()
    {
        if (Environment.GetEnvironmentVariable("PCDYNASTY_REPLAY_CHILD") == "1")
        {
            return;
        }

        string assemblyPath = typeof(GoldenReplayTests).Assembly.Location;
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(assemblyPath);
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add(
            $"FullyQualifiedName={typeof(GoldenReplayTests).FullName}.{nameof(ChildReplayMatchesFixture)}");
        startInfo.Environment["PCDYNASTY_REPLAY_CHILD"] = "1";

        using Process process = Process.Start(startInfo) ??
            throw new InvalidOperationException("Could not start the independent replay process.");
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(timeout.Token);
        string output = await outputTask;
        string error = await errorTask;

        Assert.True(
            process.ExitCode == 0,
            $"Independent replay failed.{Environment.NewLine}{output}{Environment.NewLine}{error}");
    }

    [Fact]
    public void ChildReplayMatchesFixture()
    {
        GoldenReplayFixture fixture = GoldenReplayFixture.Load();
        Assert.Equal(fixture.ExpectedFacts, CanonicalReplay.Execute(fixture));
    }
}

internal sealed class GoldenReplayFixture
{
    public int ProtocolVersion { get; init; }
    public ulong RootSeed { get; init; }
    public string[] Scopes { get; init; } = [];
    public string[] ExpectedFacts { get; init; } = [];

    public static GoldenReplayFixture Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "GoldenReplay.json");
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GoldenReplayFixture>(json, JsonOptions) ??
            throw new InvalidDataException("Golden replay fixture is empty.");
    }

    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

internal static class CanonicalReplay
{
    public static string[] Execute(GoldenReplayFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        if (fixture.ProtocolVersion != 1)
        {
            throw new InvalidDataException("Unsupported golden replay protocol version.");
        }

        ulong seed = SeedDeriver.Derive(fixture.RootSeed, fixture.Scopes);
        DeterministicNumber oneThirdTimesThree =
            DeterministicNumber.FromRatio(1, 3) * DeterministicNumber.FromInt32(3);
        DeterministicNumber oneAndHalfDividedByTwo =
            DeterministicNumber.FromRatio(3, 2) / DeterministicNumber.FromInt32(2);
        return
        [
            $"seed:{seed}",
            $"fixed:one-third-times-three:{oneThirdTimesThree.RawValue}",
            $"fixed:one-and-half-divided-by-two:{oneAndHalfDividedByTwo.RawValue}",
        ];
    }
}
