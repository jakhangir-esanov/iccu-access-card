namespace Iccu.ArchitectureTests.Infrastructure;

using NetArchTest.Rules;
using Iccu.ArchitectureTests.Abstractions;

public class InfrastructureTests : BaseTest
{
    [Fact]
    public void Repositories_Should_BeInternalAndSealed()
    {
        PredicateList repositories = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreClasses();

        Assert.NotEmpty(repositories.GetTypes());

        repositories.Should().NotBePublic().GetResult().ShouldBeSuccessful();
        repositories.Should().BeSealed().GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void EntityConfigurations_Should_BeInternalAndSealed()
    {
        PredicateList configurations = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>));

        Assert.NotEmpty(configurations.GetTypes());

        configurations.Should().NotBePublic().GetResult().ShouldBeSuccessful();
        configurations.Should().BeSealed().GetResult().ShouldBeSuccessful();
    }
}
