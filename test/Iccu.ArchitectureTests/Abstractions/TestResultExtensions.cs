namespace Iccu.ArchitectureTests.Abstractions;

using NetArchTest.Rules;

internal static class TestResultExtensions
{
    public static void ShouldBeSuccessful(this TestResult result)
    {
        IEnumerable<string> failingTypes = result.FailingTypeNames ?? [];

        Assert.True(result.IsSuccessful, $"Failing types: {string.Join(", ", failingTypes)}");
    }
}
