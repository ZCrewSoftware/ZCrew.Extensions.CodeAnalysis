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

    /// <summary>
    ///     Constructor parameter.
    /// </summary>
    /// <param name="type">The type of the parameter.</param>
    /// <param name="valueAction">The value action.</param>
    protected AttributeParameter(string type, Action<TAttributeDataBuilder, TypedConstant> valueAction)
    {
        this.type = type;
        this.valueAction = valueAction;
    }

    /// <summary>
    ///     Whether the parameter matches the <paramref name="constant"/>. All attribute constructor parameters are
    ///     constants - this is checking if it is the matching one.
    /// </summary>
    /// <param name="constant">The constant parameter value.</param>
    /// <returns><see langword="true"/> if this is the matching constant.</returns>
    public bool Matches(TypedConstant constant)
    {
        var argumentType = constant.Type?.ToGenericTypeName();
        return argumentType == this.type;
    }

    /// <summary>
    ///     Apply the value to the <paramref name="attributeDataBuilder"/>.
    /// </summary>
    /// <param name="attributeDataBuilder">The domain attribute data builder.</param>
    /// <param name="constant">The constant parameter value.</param>
    public void Accept(TAttributeDataBuilder attributeDataBuilder, TypedConstant constant)
    {
        this.valueAction(attributeDataBuilder, constant);
    }
}
