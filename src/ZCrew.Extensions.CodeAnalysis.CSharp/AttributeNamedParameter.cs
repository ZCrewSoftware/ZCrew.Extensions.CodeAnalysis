using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a named constructor parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeDataBuilder">The attribute data builder type.</typeparam>
public abstract class AttributeNamedParameter<TAttributeDataBuilder>
{
    private readonly Action<TAttributeDataBuilder, string, TypedConstant> valueAction;

    /// <summary>
    ///     Named parameter, referencing a property on the attribute.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="valueAction">The value action.</param>
    protected AttributeNamedParameter(string name, Action<TAttributeDataBuilder, string, TypedConstant> valueAction)
    {
        Name = name;
        this.valueAction = valueAction;
    }

    /// <summary>
    ///     The name of the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Apply the value to the <paramref name="attributeDataBuilder"/>.
    /// </summary>
    /// <param name="attributeDataBuilder">The domain attribute data builder.</param>
    /// <param name="namedConstant">The constant parameter value.</param>
    public void Accept(TAttributeDataBuilder attributeDataBuilder, KeyValuePair<string, TypedConstant> namedConstant)
    {
        this.valueAction(attributeDataBuilder, namedConstant.Key, namedConstant.Value);
    }
}
