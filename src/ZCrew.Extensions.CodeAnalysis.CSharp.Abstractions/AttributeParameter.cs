using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Matches a constructor parameter on an attribute.
/// </summary>
/// <typeparam name="TAttributeData">The attribute data type.</typeparam>
public abstract class AttributeParameter<TAttributeData>
{
    private readonly string type;

    private readonly Action<TAttributeData, TypedConstant> valueAction;

    /// <summary>
    ///     Constructor parameter.
    /// </summary>
    /// <param name="type">The type of the parameter.</param>
    /// <param name="valueAction">The value action.</param>
    protected AttributeParameter(string type, Action<TAttributeData, TypedConstant> valueAction)
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
    /// <remarks>
    ///     Internal so that a generated subclass can declare a member named <c>Matches</c> after an attribute
    ///     parameter of that name without hiding this one.
    /// </remarks>
    internal bool Matches(TypedConstant constant)
    {
        // Every attribute constant is assignable to object, so an object parameter takes the argument whatever it is
        if (this.type == "object")
        {
            return true;
        }

        // The constant's type is never annotated, so the declared type is matched without annotations too
        var argumentType = constant.Type?.ToFullyQualifiedName(nullableAnnotations: false);
        return argumentType == this.type;
    }

    /// <summary>
    ///     Apply the value to the <paramref name="attributeData"/>.
    /// </summary>
    /// <param name="attributeData">The attribute data model.</param>
    /// <param name="constant">The constant parameter value.</param>
    /// <remarks>
    ///     Internal so that a generated subclass can declare a member named <c>Accept</c> after an attribute
    ///     parameter of that name without hiding this one.
    /// </remarks>
    internal void Accept(TAttributeData attributeData, TypedConstant constant)
    {
        this.valueAction(attributeData, constant);
    }
}
