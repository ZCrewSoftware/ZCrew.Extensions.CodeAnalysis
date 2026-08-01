using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.IsType.IntegrationTests;

/// <summary>
///     Exercises the generator-produced <see cref="IsTypeFixture"/> methods against real Roslyn symbols, verifying the
///     emitted <c>is</c> patterns behave correctly at runtime -- in particular that generic checks constrain their type
///     arguments (a check authored for <c>Task&lt;string&gt;</c> must reject <c>Task&lt;int&gt;</c>).
/// </summary>
public class IsTypeIntegrationTests
{
    // A metadata-only compilation over the runtime reference assemblies, used to resolve real symbols (e.g.
    // System.Threading.Tasks.Task<T>) and construct closed generics to assert the generated checks against.
    private static readonly CSharpCompilation compilation = CreateCompilation();

    /// <summary>
    ///     A symbol to feed a check, named so each <see cref="InlineDataAttribute"/> row pinpoints its exact input.
    /// </summary>
    public enum Sample
    {
        Null,
        String,
        Int32,
        TaskOfString,
        TaskOfInt,
        TaskOfListOfString,
        TaskOfListOfInt,
        ListOfString,
        OpenTaskDefinition,
        NonGenericTask,
    }

    [Theory]
    [InlineData(Sample.TaskOfString, true)]
    [InlineData(Sample.TaskOfInt, false)]
    [InlineData(Sample.TaskOfListOfString, false)]
    [InlineData(Sample.OpenTaskDefinition, false)]
    [InlineData(Sample.String, false)]
    [InlineData(Sample.Null, false)]
    public void IsTaskOfString_MatchesOnlyTaskOfString(Sample input, bool expected)
    {
        Assert.Equal(expected, IsTypeFixture.IsTaskOfString(Resolve(input)));
    }

    [Theory]
    [InlineData(Sample.TaskOfListOfString, true)]
    [InlineData(Sample.TaskOfListOfInt, false)]
    [InlineData(Sample.TaskOfString, false)]
    [InlineData(Sample.ListOfString, false)]
    [InlineData(Sample.Null, false)]
    public void IsTaskOfListOfString_MatchesOnlyTheNestedClosedGeneric(Sample input, bool expected)
    {
        Assert.Equal(expected, IsTypeFixture.IsTaskOfListOfString(Resolve(input)));
    }

    [Theory]
    [InlineData(Sample.TaskOfString, true)]
    [InlineData(Sample.TaskOfInt, true)]
    [InlineData(Sample.TaskOfListOfString, true)]
    [InlineData(Sample.OpenTaskDefinition, true)]
    [InlineData(Sample.NonGenericTask, false)] // keyed on arity: non-generic Task (arity 0) does not match
    [InlineData(Sample.String, false)]
    [InlineData(Sample.Null, false)]
    public void IsAnyTask_MatchesAnyTaskRegardlessOfTypeArgument(Sample input, bool expected)
    {
        Assert.Equal(expected, IsTypeFixture.IsAnyTask(Resolve(input)));
    }

    [Theory]
    [InlineData(Sample.String, true)]
    [InlineData(Sample.Int32, false)]
    [InlineData(Sample.TaskOfString, false)]
    [InlineData(Sample.Null, false)]
    public void IsString_MatchesOnlyTheStringSpecialType(Sample input, bool expected)
    {
        Assert.Equal(expected, IsTypeFixture.IsString(Resolve(input)));
    }

    private static ISymbol? Resolve(Sample sample)
    {
        return sample switch
        {
            Sample.Null => null,
            Sample.String => String,
            Sample.Int32 => Int32,
            Sample.TaskOfString => Task(String),
            Sample.TaskOfInt => Task(Int32),
            Sample.TaskOfListOfString => Task(List(String)),
            Sample.TaskOfListOfInt => Task(List(Int32)),
            Sample.ListOfString => List(String),
            Sample.OpenTaskDefinition => OpenTask,
            Sample.NonGenericTask => NamedType("System.Threading.Tasks.Task"),
            _ => throw new ArgumentOutOfRangeException(nameof(sample), sample, "Unhandled sample."),
        };
    }

    private static ITypeSymbol String => compilation.GetSpecialType(SpecialType.System_String);

    private static ITypeSymbol Int32 => compilation.GetSpecialType(SpecialType.System_Int32);

    // The unbound Task<> definition (its single type argument is an open type parameter).
    private static INamedTypeSymbol OpenTask => NamedType("System.Threading.Tasks.Task`1");

    private static INamedTypeSymbol Task(ITypeSymbol argument)
    {
        return NamedType("System.Threading.Tasks.Task`1").Construct(argument);
    }

    private static INamedTypeSymbol List(ITypeSymbol argument)
    {
        return NamedType("System.Collections.Generic.List`1").Construct(argument);
    }

    private static INamedTypeSymbol NamedType(string fullyQualifiedMetadataName)
    {
        return compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
            ?? throw new InvalidOperationException($"Could not resolve the type '{fullyQualifiedMetadataName}'.");
    }

    private static CSharpCompilation CreateCompilation()
    {
        var trustedAssemblies = (string)(
            AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("The trusted platform assemblies list is unavailable.")
        );

        var references = trustedAssemblies
            .Split(Path.PathSeparator)
            .Where(path => path.Length > 0)
            .Select(MetadataReference (path) => MetadataReference.CreateFromFile(path));

        return CSharpCompilation.Create("IsTypeIntegrationTests", references: references);
    }
}
