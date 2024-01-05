using SpaceWarp.API.Versions;

namespace SpaceWarp.Tests.Versions;

/// <summary>
/// NUnit tests for SemanticVersion.
/// </summary>
[TestFixture]
public class SemanticVersionTests
{
    [TestCase(1, 2, 3, "alpha", "build1")]
    [TestCase(1, 2, 3, null, null)]
    public void SemanticVersion_ConstructWithDetails_ConstructsObject(
        int major,
        int minor,
        int patch,
        string? prerelease,
        string? build
    )
    {
        var semanticVersion = new SemanticVersion(major, minor, patch, prerelease, build);
        Assert.Multiple(() =>
        {
            Assert.That(semanticVersion.Major, Is.EqualTo(major));
            Assert.That(semanticVersion.Minor, Is.EqualTo(minor));
            Assert.That(semanticVersion.Patch, Is.EqualTo(patch));
            Assert.That(semanticVersion.Prerelease, Is.EqualTo(prerelease ?? string.Empty));
            Assert.That(semanticVersion.Build, Is.EqualTo(build ?? string.Empty));
        });
    }

    [TestCase("1.2.3-alpha+build1", ExpectedResult = true)]
    [TestCase("1.2", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(null, ExpectedResult = false)]
    [TestCase("a.b.c", ExpectedResult = false)]
    [TestCase("1.2.3.4.5.6.7-alpha+build1", ExpectedResult = true)]
    public bool SemanticVersion_ConstructWithVersionString_ConstructsObject(string version)
    {
        try
        {
            // ReSharper disable once UnusedVariable
            var semanticVersion = new SemanticVersion(version);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    [TestCase("1.2.3-alpha+build1", "1.2.3-beta+build2", ExpectedResult = false)]
    [TestCase("1.2.3-alpha+build1", "1.2.3-alpha+build1", ExpectedResult = true)]
    [TestCase("1.2.3-alpha+build1", "1.2.3-alpha+build2", ExpectedResult = true)]
    public bool SemanticVersion_OperatorEquality_ComparesVersions(string version1, string version2)
    {
        var semanticVersion1 = new SemanticVersion(version1);
        var semanticVersion2 = new SemanticVersion(version2);

        return semanticVersion1 == semanticVersion2;
    }

    [TestCase("1.2.3-alpha+build1", "1.2.3-beta+build2", ExpectedResult = true)]
    [TestCase("1.2.3-alpha+build1", "1.2.3-alpha+build1", ExpectedResult = false)]
    [TestCase("1.2.3-alpha+build1", "1.2.3-alpha+build2", ExpectedResult = false)]
    public bool SemanticVersion_OperatorInequality_ComparesVersions(string version1, string version2)
    {
        var semanticVersion1 = new SemanticVersion(version1);
        var semanticVersion2 = new SemanticVersion(version2);

        return semanticVersion1 != semanticVersion2;
    }

    // Major difference
    [TestCase("1.0.0", "1.0.0", ExpectedResult = false)]
    [TestCase("1.0.0", "2.0.0", ExpectedResult = true)]
    [TestCase("2.0.0", "1.0.0", ExpectedResult = false)]
    // Minor difference
    [TestCase("1.1.0", "1.2.0", ExpectedResult = true)]
    [TestCase("1.2.0", "1.1.0", ExpectedResult = false)]
    // Patch difference
    [TestCase("1.1.1", "1.1.2", ExpectedResult = true)]
    [TestCase("1.1.2", "1.1.1", ExpectedResult = false)]
    // Prerelease difference
    [TestCase("1.0.0-alpha", "1.0.0-alpha", ExpectedResult = false)]
    [TestCase("1.1.1-alpha", "1.1.1-beta", ExpectedResult = true)]
    [TestCase("1.1.1-beta", "1.1.1-alpha", ExpectedResult = false)]
    // Build metadata ignored
    [TestCase("1.0.0-alpha", "1.0.0-alpha+build1", ExpectedResult = false)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build1", ExpectedResult = false)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build2", ExpectedResult = false)]
    public bool SemanticVersion_OperatorLessThan_ComparesVersions(string version1, string version2)
    {
        var semanticVersion1 = new SemanticVersion(version1);
        var semanticVersion2 = new SemanticVersion(version2);

        return semanticVersion1 < semanticVersion2;
    }

    // Major difference
    [TestCase("1.0.0", "1.0.0", ExpectedResult = false)]
    [TestCase("1.0.0", "2.0.0", ExpectedResult = false)]
    [TestCase("2.0.0", "1.0.0", ExpectedResult = true)]
    // Minor difference
    [TestCase("1.1.0", "1.2.0", ExpectedResult = false)]
    [TestCase("1.2.0", "1.1.0", ExpectedResult = true)]
    // Patch difference
    [TestCase("1.1.1", "1.1.2", ExpectedResult = false)]
    [TestCase("1.1.2", "1.1.1", ExpectedResult = true)]
    // Prerelease difference
    [TestCase("1.0.0-alpha", "1.0.0-alpha", ExpectedResult = false)]
    [TestCase("1.1.1-alpha", "1.1.1-beta", ExpectedResult = false)]
    [TestCase("1.1.1-beta", "1.1.1-alpha", ExpectedResult = true)]
    // Build metadata ignored
    [TestCase("1.0.0-alpha", "1.0.0-alpha+build1", ExpectedResult = false)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build1", ExpectedResult = false)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build2", ExpectedResult = false)]
    public bool SemanticVersion_OperatorGreaterThan_ComparesVersions(string version1, string version2)
    {
        var semanticVersion1 = new SemanticVersion(version1);
        var semanticVersion2 = new SemanticVersion(version2);

        return semanticVersion1 > semanticVersion2;
    }

    // Major difference
    [TestCase("1.0.0", "1.0.0", ExpectedResult = true)]
    [TestCase("1.0.0", "2.0.0", ExpectedResult = true)]
    [TestCase("2.0.0", "1.0.0", ExpectedResult = false)]
    // Minor difference
    [TestCase("1.1.0", "1.2.0", ExpectedResult = true)]
    [TestCase("1.2.0", "1.1.0", ExpectedResult = false)]
    // Patch difference
    [TestCase("1.1.1", "1.1.2", ExpectedResult = true)]
    [TestCase("1.1.2", "1.1.1", ExpectedResult = false)]
    // Prerelease difference
    [TestCase("1.0.0-alpha", "1.0.0-alpha", ExpectedResult = true)]
    [TestCase("1.1.1-alpha", "1.1.1-beta", ExpectedResult = true)]
    [TestCase("1.1.1-beta", "1.1.1-alpha", ExpectedResult = false)]
    // Build metadata ignored
    [TestCase("1.0.0-alpha", "1.0.0-alpha+build1", ExpectedResult = true)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build1", ExpectedResult = true)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build2", ExpectedResult = true)]
    public bool SemanticVersion_OperatorLessThanOrEqual_ComparesVersions(string version1, string version2)
    {
        var semanticVersion1 = new SemanticVersion(version1);
        var semanticVersion2 = new SemanticVersion(version2);

        return semanticVersion1 <= semanticVersion2;
    }

    // Major difference
    [TestCase("1.0.0", "1.0.0", ExpectedResult = true)]
    [TestCase("1.0.0", "2.0.0", ExpectedResult = false)]
    [TestCase("2.0.0", "1.0.0", ExpectedResult = true)]
    // Minor difference
    [TestCase("1.1.0", "1.2.0", ExpectedResult = false)]
    [TestCase("1.2.0", "1.1.0", ExpectedResult = true)]
    // Patch difference
    [TestCase("1.1.1", "1.1.2", ExpectedResult = false)]
    [TestCase("1.1.2", "1.1.1", ExpectedResult = true)]
    // Prerelease difference
    [TestCase("1.0.0-alpha", "1.0.0-alpha", ExpectedResult = true)]
    [TestCase("1.1.1-alpha", "1.1.1-beta", ExpectedResult = false)]
    [TestCase("1.1.1-beta", "1.1.1-alpha", ExpectedResult = true)]
    // Build metadata ignored
    [TestCase("1.0.0-alpha", "1.0.0-alpha+build1", ExpectedResult = true)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build1", ExpectedResult = true)]
    [TestCase("1.0.0-alpha+build1", "1.0.0-alpha+build2", ExpectedResult = true)]
    public bool SemanticVersion_OperatorGreaterThanOrEqual_ComparesVersions(string version1, string version2)
    {
        var semanticVersion1 = new SemanticVersion(version1);
        var semanticVersion2 = new SemanticVersion(version2);

        return semanticVersion1 >= semanticVersion2;
    }
    
    [TestCase("1", ExpectedResult = "1")]
    [TestCase("1.2.3", ExpectedResult = "1.2.3")]
    [TestCase("1.2.3-alpha", ExpectedResult = "1.2.3-alpha")]
    [TestCase("1.2.3-alpha+build1", ExpectedResult = "1.2.3-alpha+build1")]
    public string SemanticVersion_ToString_ReturnsVersionString(string version)
    {
        var semanticVersion = new SemanticVersion(version);
        return semanticVersion.ToString();
    }
    
}