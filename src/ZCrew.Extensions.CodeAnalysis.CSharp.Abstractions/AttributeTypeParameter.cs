using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a generic type parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeDataBuilder">The attribute data builder type.</typeparam>
public abstract class AttributeTypeParameter<TAttributeDataBuilder>
{
    private readonly Action<TAttributeDataBuilder, ITypeSymbol> valueAction;

    /// <summary>
    ///     Attribute type parameter, representing a generic attribute type parameter.
    /// </summary>
    /// <param name="valueAction">The type symbol value action.</param>
    protected AttributeTypeParameter(Action<TAttributeDataBuilder, ITypeSymbol> valueAction)
    {
        this.valueAction = valueAction;
    }

    /// <summary>
    ///     Apply the type value to the <paramref name="attributeDataBuilder"/>.
    /// </summary>
    /// <param name="attributeDataBuilder">The domain attribute data builder.</param>
    /// <param name="symbol">The constant type symbol value.</param>
    public void Accept(TAttributeDataBuilder attributeDataBuilder, ITypeSymbol symbol)
    {
        this.valueAction(attributeDataBuilder, symbol);
    }
}
