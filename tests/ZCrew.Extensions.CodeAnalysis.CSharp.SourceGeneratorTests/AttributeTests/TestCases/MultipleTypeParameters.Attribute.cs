using System;

namespace AttributeTests;

[Microsoft.CodeAnalysis.Embedded]
public class TestAttribute<TService, TImplementation> : Attribute { }
