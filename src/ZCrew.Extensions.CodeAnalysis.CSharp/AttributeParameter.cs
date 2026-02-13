using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a constructor parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeDataBuilder">The attribute data builder type.</typeparam>
public abstract class AttributeParameter<TAttributeDataBuilder>
{
    private readonly string type;

    private readonly Action<TAttributeDataBuilder, TypedConstant> valueAction;

    protected AttributeParameter(string type, Action<TAttributeDataBuilder, TypedConstant> valueAction)
    {
        this.type = type;
        this.valueAction = valueAction;
    }

    public bool Matches(TypedConstant constant)
    {
        var argumentType = constant.Type?.ToGenericTypeName();
        return argumentType == this.type;
    }

    public void Accept(TAttributeDataBuilder attributeDataBuilder, TypedConstant constant)
    {
        this.valueAction(attributeDataBuilder, constant);
    }
}
