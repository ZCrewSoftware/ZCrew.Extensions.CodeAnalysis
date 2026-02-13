using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a named constructor parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeDataBuilder">The attribute data builder type.</typeparam>
public abstract class AttributeNamedParameter<TAttributeDataBuilder>
{
    private readonly Action<TAttributeDataBuilder, string, TypedConstant> valueAction;

    protected AttributeNamedParameter(string name, Action<TAttributeDataBuilder, string, TypedConstant> valueAction)
    {
        Name = name;
        this.valueAction = valueAction;
    }

    public string Name { get; }

    public void Accept(TAttributeDataBuilder attributeDataBuilder, KeyValuePair<string, TypedConstant> namedConstant)
    {
        this.valueAction(attributeDataBuilder, namedConstant.Key, namedConstant.Value);
    }
}
