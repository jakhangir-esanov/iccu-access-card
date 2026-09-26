namespace Iccu.ArchitectureTests.Application;

using FluentValidation;
using NetArchTest.Rules;
using Iccu.ArchitectureTests.Abstractions;
using Iccu.Application.Common.Messaging;

public class ApplicationTests : BaseTest
{
    [Fact]
    public void Commands_Should_BeSealedAndNamedCommand()
    {
        PredicateList commands = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>));

        Assert.NotEmpty(commands.GetTypes());

        commands.Should().BeSealed().GetResult().ShouldBeSuccessful();
        commands.Should().HaveNameEndingWith("Command").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void CommandHandlers_Should_BeInternalSealedAndNamedCommandHandler()
    {
        PredicateList handlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>));

        Assert.NotEmpty(handlers.GetTypes());

        handlers.Should().NotBePublic().GetResult().ShouldBeSuccessful();
        handlers.Should().BeSealed().GetResult().ShouldBeSuccessful();
        handlers.Should().HaveNameEndingWith("CommandHandler").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void Queries_Should_BeSealedAndNamedQuery()
    {
        PredicateList queries = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Or()
            .ImplementInterface(typeof(IPagedListQuery<>));

        Assert.NotEmpty(queries.GetTypes());

        queries.Should().BeSealed().GetResult().ShouldBeSuccessful();
        queries.Should().HaveNameEndingWith("Query").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void QueryHandlers_Should_BeInternalSealedAndNamedQueryHandler()
    {
        PredicateList handlers = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Or()
            .ImplementInterface(typeof(IPagedListQueryHandler<,>));

        Assert.NotEmpty(handlers.GetTypes());

        handlers.Should().NotBePublic().GetResult().ShouldBeSuccessful();
        handlers.Should().BeSealed().GetResult().ShouldBeSuccessful();
        handlers.Should().HaveNameEndingWith("QueryHandler").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void Validators_Should_BeInternalSealedAndNamedValidator()
    {
        PredicateList validators = Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>));

        Assert.NotEmpty(validators.GetTypes());

        validators.Should().NotBePublic().GetResult().ShouldBeSuccessful();
        validators.Should().BeSealed().GetResult().ShouldBeSuccessful();
        validators.Should().HaveNameEndingWith("Validator").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void ErrorCatalogs_ShouldResideIn_Domain()
    {
        Assert.Empty(Types.InAssembly(ApplicationAssembly).That().HaveNameEndingWith("Errors").GetTypes());
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOn_WriteSidePersistence()
    {
        Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
