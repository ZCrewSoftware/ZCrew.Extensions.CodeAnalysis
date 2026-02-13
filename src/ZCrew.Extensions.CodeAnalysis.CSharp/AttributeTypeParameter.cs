using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a generic type parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeDataBuilder">The attribute data builder type.</typeparam>
public abstract class AttributeTypeParameter<TAttributeDataBuilder>
{
    private readonly Action<TAttributeDataBuilder, ITypeSymbol> valueAction;

    protected AttributeTypeParameter(Action<TAttributeDataBuilder, ITypeSymbol> valueAction)
    {
        this.valueAction = valueAction;
    }

    public void Accept(TAttributeDataBuilder attributeDataBuilder, ITypeSymbol symbol)
    {
        this.valueAction(attributeDataBuilder, symbol);
    }
}
