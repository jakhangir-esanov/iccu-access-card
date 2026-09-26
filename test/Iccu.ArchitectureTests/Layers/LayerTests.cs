namespace Iccu.ArchitectureTests.Layers;

using NetArchTest.Rules;
using Iccu.ArchitectureTests.Abstractions;

public class LayerTests : BaseTest
{
    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_OtherLayers()
    {
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                NameOf(ApplicationAssembly),
                NameOf(InfrastructureAssembly),
                NameOf(PresentationAssembly))
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(NameOf(InfrastructureAssembly))
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(NameOf(PresentationAssembly))
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void PresentationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        Types.InAssembly(PresentationAssembly)
            .Should()
            .NotHaveDependencyOn(NameOf(InfrastructureAssembly))
            .GetResult()
            .ShouldBeSuccessful();
    }
}
