using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a generic type parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeData">The attribute data type.</typeparam>
public abstract class AttributeTypeParameter<TAttributeData>
{
    private readonly Action<TAttributeData, ITypeSymbol> valueAction;

    /// <summary>
    ///     Attribute type parameter, representing a generic attribute type parameter.
    /// </summary>
    /// <param name="valueAction">The type symbol value action.</param>
    protected AttributeTypeParameter(Action<TAttributeData, ITypeSymbol> valueAction)
    {
        this.valueAction = valueAction;
    }

    /// <summary>
    ///     Apply the type value to the <paramref name="attributeData"/>.
    /// </summary>
    /// <param name="attributeData">The attribute data model.</param>
    /// <param name="symbol">The constant type symbol value.</param>
    /// <remarks>
    ///     Internal so that a generated subclass can declare a member named <c>Accept</c> after an attribute type
    ///     parameter of that name without hiding this one.
    /// </remarks>
    internal void Accept(TAttributeData attributeData, ITypeSymbol symbol)
    {
        this.valueAction(attributeData, symbol);
    }
}
