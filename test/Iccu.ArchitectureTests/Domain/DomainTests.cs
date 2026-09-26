namespace Iccu.ArchitectureTests.Domain;

using NetArchTest.Rules;
using Iccu.ArchitectureTests.Abstractions;

public class DomainTests : BaseTest
{
    private const string CommonNamespace = "Iccu.Domain.Common";
    private const string ErrorsSuffix = "Errors";
    private const string RepositorySuffix = "Repository";

    [Fact]
    public void EntityFolders_Should_ContainOnlyEntitiesErrorsAndRepositories()
    {
        IEnumerable<string> misplacedTypes = EntityFolderTypes()
            .Where(type => !IsEntity(type) && !IsErrorCatalog(type) && !IsRepository(type))
            .Select(type => type.FullName!);

        Assert.Empty(misplacedTypes);
    }

    [Fact]
    public void Entities_Should_BeSealedWithoutPublicConstructors()
    {
        Type[] entities = [.. EntityFolderTypes().Where(IsEntity)];

        Assert.NotEmpty(entities);
        Assert.All(entities, entity => Assert.True(entity.IsSealed, $"{entity.Name} must be sealed"));
        Assert.All(entities, entity => Assert.Empty(entity.GetConstructors()));
    }

    [Fact]
    public void Entities_ShouldNotReturn_Results()
    {
        Types.InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotStatic()
            .And()
            .DoNotResideInNamespaceStartingWith(CommonNamespace)
            .Should()
            .NotHaveDependencyOnAny(
                "Iccu.Domain.Common.Result",
                "Iccu.Domain.Common.ValidationError")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void ErrorCatalogs_Should_BeStatic()
    {
        PredicateList errorCatalogs = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith(ErrorsSuffix);

        Assert.NotEmpty(errorCatalogs.GetTypes());

        errorCatalogs.Should().BeStatic().GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void Repositories_Should_BeInterfaces()
    {
        PredicateList repositories = Types.InAssembly(DomainAssembly)
            .That()
            .HaveNameEndingWith(RepositorySuffix);

        Assert.NotEmpty(repositories.GetTypes());

        repositories.Should().BeInterfaces().GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void Domain_ShouldNotHaveDependencyOn_Frameworks()
    {
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "MediatR",
                "FluentValidation",
                "Microsoft.AspNetCore",
                "Npgsql")
            .GetResult()
            .ShouldBeSuccessful();
    }

    private static IEnumerable<Type> EntityFolderTypes() => Types.InAssembly(DomainAssembly)
        .GetTypes()
        .Where(type => type.Namespace is not null &&
                       !type.Namespace.StartsWith(CommonNamespace, StringComparison.Ordinal) &&
                       !type.IsNested);

    private static bool IsEntity(Type type) =>
        type is { IsClass: true, IsAbstract: false } && !type.Name.EndsWith(ErrorsSuffix, StringComparison.Ordinal);

    private static bool IsErrorCatalog(Type type) =>
        type is { IsClass: true, IsAbstract: true, IsSealed: true } && type.Name.EndsWith(ErrorsSuffix, StringComparison.Ordinal);

    private static bool IsRepository(Type type) =>
        type.IsInterface && type.Name.EndsWith(RepositorySuffix, StringComparison.Ordinal);
}
