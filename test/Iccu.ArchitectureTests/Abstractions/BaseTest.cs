namespace Iccu.ArchitectureTests.Abstractions;

using System.Reflection;
using Iccu.Infrastructure;
using Iccu.Domain.Readers;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Reader).Assembly;

    protected static readonly Assembly ApplicationAssembly = Iccu.Application.AssemblyReference.Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(InfrastructureConfiguration).Assembly;

    protected static readonly Assembly PresentationAssembly = Iccu.Presentation.AssemblyReference.Assembly;

    protected static string NameOf(Assembly assembly) => assembly.GetName().Name!;
}
