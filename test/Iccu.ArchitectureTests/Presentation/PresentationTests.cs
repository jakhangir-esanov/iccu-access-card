namespace Iccu.ArchitectureTests.Presentation;

using NetArchTest.Rules;
using Iccu.ArchitectureTests.Abstractions;
using Iccu.Presentation.Common.Endpoints;

public class PresentationTests : BaseTest
{
    [Fact]
    public void Endpoints_Should_BeInternalAndSealed()
    {
        PredicateList endpoints = Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint));

        Assert.NotEmpty(endpoints.GetTypes());

        endpoints.Should().NotBePublic().GetResult().ShouldBeSuccessful();
        endpoints.Should().BeSealed().GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void Presentation_ShouldNotHaveDependencyOn_DataAccess()
    {
        Types.InAssembly(PresentationAssembly)
            .Should()
            .NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper", "Npgsql")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Presentation_ShouldNotHaveDependencyOn_Repositories()
    {
        Types.InAssembly(PresentationAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Iccu.Domain.Readers.IReaderRepository",
                "Iccu.Domain.RegistrationRequests.IRegistrationRequestRepository",
                "Iccu.Domain.Users.IUserRepository",
                "Iccu.Domain.RefreshTokens.IRefreshTokenRepository",
                "Iccu.Application.Common.Data.IUnitOfWork",
                "Iccu.Application.Common.Data.IDbConnectionFactory")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
