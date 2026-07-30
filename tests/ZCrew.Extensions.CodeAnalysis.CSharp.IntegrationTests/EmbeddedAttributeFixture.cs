using System;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IntegrationTests;

// Both attributes live in this one file because the generator captures the whole file into the SourceText.
// EmbeddedAttributeIntegrationTests feeds that captured text back through Roslyn as a stand-in consumer project.

[Microsoft.CodeAnalysis.Embedded]
internal class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type serviceType) { }

    public ServiceAttribute(Type serviceType, Type implementationType, string[] tags) { }

    public string? Name { get; init; }
}

[Microsoft.CodeAnalysis.Embedded]
internal class ServiceAttribute<TService, TImplementation> : Attribute
{
    public ServiceAttribute(string name) { }
}

// Three overloads that share an argument count, so only the argument type tells them apart.
[Microsoft.CodeAnalysis.Embedded]
internal class OverloadAttribute : Attribute
{
    public OverloadAttribute(string name) { }

    public OverloadAttribute(Type type) { }

    public OverloadAttribute(int value) { }
}

// A null constant and an object-typed parameter, both of which the argument type check has to tolerate.
[Microsoft.CodeAnalysis.Embedded]
internal class NoteAttribute : Attribute
{
    public NoteAttribute(string? text, object? payload) { }
}
