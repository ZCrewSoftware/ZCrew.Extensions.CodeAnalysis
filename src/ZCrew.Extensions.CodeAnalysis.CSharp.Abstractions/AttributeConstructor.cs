using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Used to match an attribute constructor so the type arguments, constructor arguments, and named arguments can be
///     interpreted during compilation. The <typeparamref name="TAttributeData" /> will be supplied to each
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
/// <typeparam name="TAttributeData">The attribute data type.</typeparam>
public abstract class AttributeConstructor<TAttributeData>
    where TAttributeData : new()
{
    private readonly AttributeParameter<TAttributeData>[] parameters;
    private readonly AttributeNamedParameter<TAttributeData>[] namedParameters;
    private readonly AttributeTypeParameter<TAttributeData>[] typeParameters;

    /// <summary>
    ///     Creates a new constructor with only constructor parameters.
    /// </summary>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    protected AttributeConstructor(AttributeParameter<TAttributeData>[] parameters)
        : this([], parameters, []) { }

    /// <summary>
    ///     Creates a new constructor with constructor parameters and named properties.
    /// </summary>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    /// <param name="namedParameters">The named attribute properties.</param>
    protected AttributeConstructor(
        AttributeParameter<TAttributeData>[] parameters,
        AttributeNamedParameter<TAttributeData>[] namedParameters
    )
        : this([], parameters, namedParameters) { }

    /// <summary>
    ///     Creates a new constructor with type parameters and constructor parameters.
    /// </summary>
    /// <param name="typeParameters">The attribute type parameters.</param>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    protected AttributeConstructor(
        AttributeTypeParameter<TAttributeData>[] typeParameters,
        AttributeParameter<TAttributeData>[] parameters
    )
        : this(typeParameters, parameters, []) { }

    /// <summary>
    ///     Creates a new constructor with all options.
    /// </summary>
    /// <param name="typeParameters">The attribute type parameters.</param>
    /// <param name="parameters">The attribute parameters from the constructor.</param>
    /// <param name="namedParameters">The named attribute properties.</param>
    protected AttributeConstructor(
        AttributeTypeParameter<TAttributeData>[] typeParameters,
        AttributeParameter<TAttributeData>[] parameters,
        AttributeNamedParameter<TAttributeData>[] namedParameters
    )
    {
        this.typeParameters = typeParameters;
        this.parameters = parameters;
        this.namedParameters = namedParameters;
    }

    /// <summary>
    ///     Optional metadata name for the attribute. When specified <see cref="TryCreateAttributeFor"/> will check the
    ///     name of the <see cref="AttributeData"/> to make sure it matches <see cref="AttributeMetadataName"/>.
    /// </summary>
    /// <example>
    ///     <see cref="List{T}"/> would be <c>"System.Collections.Generic.List`1"</c>.
    /// </example>
    protected virtual string? AttributeMetadataName => null;

    /// <summary>
    ///     Create an instance of the <typeparamref name="TAttributeData"/> based on the constructor called by the
    ///     <paramref name="attributeData"/>. When <see cref="AttributeMetadataName"/> is set the
    ///     <paramref name="attributeData"/> symbol must match the <see cref="AttributeMetadataName"/>.
    /// </summary>
    /// <param name="attributeData">The semantic attribute data.</param>
    /// <param name="data">The attribute data model created from the <paramref name="attributeData"/>.</param>
    /// <returns><see langword="true"/> if the attribute definition called this constructor.</returns>
    public bool TryCreateAttributeFor(AttributeData attributeData, [NotNullWhen(true)] out TAttributeData? data)
    {
        if (attributeData.AttributeClass == null)
        {
            data = default;
            return false;
        }

        if (AttributeMetadataName != null && !attributeData.AttributeClass.HasFullMetadataName(AttributeMetadataName))
        {
            data = default;
            return false;
        }

        // Only checking the arity is sufficient
        var typeArguments = attributeData.AttributeClass.TypeArguments;
        if (typeArguments.Length != this.typeParameters.Length)
        {
            data = default;
            return false;
        }

        var arguments = attributeData.ConstructorArguments;
        if (arguments.Length != this.parameters.Length)
        {
            data = default;
            return false;
        }

        // Overloads can share an argument count, so the argument types decide which one was called. Checked before
        // anything is written so a rejected constructor leaves no partially populated model behind.
        for (var i = 0; i < arguments.Length; i++)
        {
            if (!this.parameters[i].Matches(arguments[i]))
            {
                data = default;
                return false;
            }
        }

        var model = new TAttributeData();

        for (var i = 0; i < typeArguments.Length; i++)
        {
            this.typeParameters[i].Accept(model, typeArguments[i]);
        }

        for (var i = 0; i < arguments.Length; i++)
        {
            this.parameters[i].Accept(model, arguments[i]);
        }

        // Lastly, apply each named parameter if there was a named argument present with the same name
        foreach (var namedArgument in attributeData.NamedArguments)
        {
            foreach (var namedParameter in this.namedParameters)
            {
                if (namedParameter.Accept(model, namedArgument))
                {
                    break;
                }
            }
        }

        data = model;
        return true;
    }
}
