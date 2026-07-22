using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Used to match an attribute constructor so the type arguments, constructor arguments, and named arguments can be
///     interpreted during compilation. The <typeparamref name="TAttributeDataBuilder" /> will be supplied to each
///     argument so that the interpreted value can contribute to building the attribute data.
/// </summary>
/// <example>
///     An attribute defined as:
///     <code>
///     public class TestAttribute&lt;T&gt; : Attribute
///     {
///         public string Argument { get; private set; }
///         public int NamedArgument { get; set; }
///         public TestAttribute(string argument)
///         {
///             Argument = argument;
///         }
///     }
///     </code>
///     Defining this attribute as:
///     <code>
///     [Test&lt;bool&gt;(argument: "xyz", NamedArgument = 123)]
///     public class ArbitraryClass;
///     </code>
///     The <c>bool</c> is a generic type argument. The <c>argument: "xyz"</c> is a constructor argument, as it sets an
///     argument used in the <c>TestAttribute</c> constructor. The <c>NamedArgument = 123</c> is a named argument, as it
///     sets a property on the <c>TestAttribute</c> type.
/// </example>
/// <typeparam name="TAttributeDataBuilder">The attribute data builder type.</typeparam>
public abstract class AttributeConstructor<TAttributeDataBuilder>
{
    private readonly AttributeParameter<TAttributeDataBuilder>[] parameters;

    private readonly Dictionary<string, AttributeNamedParameter<TAttributeDataBuilder>> namedParameters = [];

    private readonly AttributeTypeParameter<TAttributeDataBuilder>[] typeParameters;

    /// <summary>
    ///     Creates a new constructor with only constructor parameters.
    /// </summary>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    protected AttributeConstructor(AttributeParameter<TAttributeDataBuilder>[] parameters)
        : this([], parameters, []) { }

    /// <summary>
    ///     Creates a new constructor with constructor parameters and named properties.
    /// </summary>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    /// <param name="namedParameters">The named attribute properties.</param>
    protected AttributeConstructor(
        AttributeParameter<TAttributeDataBuilder>[] parameters,
        AttributeNamedParameter<TAttributeDataBuilder>[] namedParameters
    )
        : this([], parameters, namedParameters) { }

    /// <summary>
    ///     Creates a new constructor with type parameters and constructor parameters.
    /// </summary>
    /// <param name="typeParameters">The attribute type parameters.</param>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    protected AttributeConstructor(
        AttributeTypeParameter<TAttributeDataBuilder>[] typeParameters,
        AttributeParameter<TAttributeDataBuilder>[] parameters
    )
        : this(typeParameters, parameters, []) { }

    /// <summary>
    ///     Creates a new constructor with all options.
    /// </summary>
    /// <param name="typeParameters">The attribute type parameters.</param>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    /// <param name="namedParameters">The named attribute properties.</param>
    protected AttributeConstructor(
        AttributeTypeParameter<TAttributeDataBuilder>[] typeParameters,
        AttributeParameter<TAttributeDataBuilder>[] parameters,
        AttributeNamedParameter<TAttributeDataBuilder>[] namedParameters
    )
    {
        this.typeParameters = typeParameters;
        this.parameters = parameters;

        foreach (var namedParameter in namedParameters)
        {
            this.namedParameters[namedParameter.Name] = namedParameter;
        }
    }

    /// <summary>
    ///     Whether this constructor was called by the attribute definition.
    /// </summary>
    /// <param name="attributeData">The semantic attribute data.</param>
    /// <returns><see langword="true"/> if the attribute definition called this constructor.</returns>
    public bool IsCalledBy(AttributeData attributeData)
    {
        var isTypeParameterMatch = MatchesTypeParameters(attributeData);
        if (!isTypeParameterMatch)
        {
            return false;
        }

        var isParameterMatch = MatchesParameters(attributeData);
        if (!isParameterMatch)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Accept the <paramref name="attributeDataBuilder"/> with the <paramref name="attributeData"/> for this
    ///     constructor. This will apply the constructor's setup to the <paramref name="attributeDataBuilder"/>.
    /// </summary>
    /// <param name="attributeDataBuilder">The domain attribute data builder.</param>
    /// <param name="attributeData">The semantic attribute data.</param>
    public void Accept(TAttributeDataBuilder attributeDataBuilder, AttributeData attributeData)
    {
        // Attribute may have not been loaded fully, this will generate a diagnostic from the compiler anyway
        if (attributeData.AttributeClass != null)
        {
            // If the attribute symbol was loaded then parse the generic type arguments
            var typeArguments = attributeData.AttributeClass.TypeArguments;
            for (var i = 0; i < typeArguments.Length; i++)
            {
                this.typeParameters[i].Accept(attributeDataBuilder, typeArguments[i]);
            }
        }

        var arguments = attributeData.ConstructorArguments;
        for (var i = 0; i < arguments.Length; i++)
        {
            this.parameters[i].Accept(attributeDataBuilder, arguments[i]);
        }

        // Lastly, apply each named parameter if there was a named argument present with the same name
        foreach (var namedArgument in attributeData.NamedArguments)
        {
            if (!this.namedParameters.TryGetValue(namedArgument.Key, out var namedParameter))
            {
                continue;
            }

            namedParameter.Accept(attributeDataBuilder, namedArgument);
        }
    }

    private bool MatchesParameters(AttributeData attributeData)
    {
        var arguments = attributeData.ConstructorArguments;
        if (arguments.Length != this.parameters.Length)
        {
            return false;
        }

        for (var i = 0; i < arguments.Length; i++)
        {
            if (!this.parameters[i].Matches(arguments[i]))
            {
                return false;
            }
        }
        return true;
    }

    private bool MatchesTypeParameters(AttributeData attributeData)
    {
        // Possibly missing a using statement and so the type info is missing for the attribute. Other diagnostics will
        // report an error here
        if (attributeData.AttributeClass == null)
        {
            return false;
        }

        // Only checking the arity is sufficient
        var typeArguments = attributeData.AttributeClass.TypeArguments;
        if (typeArguments.Length != this.typeParameters.Length)
        {
            return false;
        }

        return true;
    }
}
