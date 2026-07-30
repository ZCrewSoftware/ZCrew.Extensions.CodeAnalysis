using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a named constructor parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeData">The attribute data type.</typeparam>
public abstract class AttributeNamedParameter<TAttributeData>
{
    private readonly string name;
    private readonly Action<TAttributeData, string, TypedConstant> valueAction;

    /// <summary>
    ///     Named parameter, referencing a property on the attribute.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="valueAction">The value action.</param>
    protected AttributeNamedParameter(string name, Action<TAttributeData, string, TypedConstant> valueAction)
    {
        this.name = name;
        this.valueAction = valueAction;
    }

    /// <summary>
    ///     Apply the value to the <paramref name="attributeData"/>.
    /// </summary>
    /// <param name="attributeData">The domain attribute data model.</param>
    /// <param name="namedConstant">The constant parameter value.</param>
    /// <returns><see langword="true"/> if the name of the parameter matched the <paramref name="namedConstant"/>.</returns>
    /// <remarks>
    ///     Internal so that a generated subclass can declare a member named <c>Accept</c> after an attribute property
    ///     of that name without hiding this one.
    /// </remarks>
    internal bool Accept(TAttributeData attributeData, KeyValuePair<string, TypedConstant> namedConstant)
    {
        if (this.name == namedConstant.Key)
        {
            this.valueAction(attributeData, namedConstant.Key, namedConstant.Value);
            return true;
        }
        return false;
    }
}
