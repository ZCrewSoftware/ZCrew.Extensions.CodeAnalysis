using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ZCrew.Extensions.CodeAnalysis.CSharp;

/// <summary>
///     Extensions for the <see cref="TypedConstant"/> type.
/// </summary>
public static class TypedConstantExtensions
{
    extension(TypedConstant constant)
    {
        /// <summary>
        ///     Whether the <see cref="TypedConstant"/> represents a <see cref="string"/> value. If this returns
        ///     <see langword="true"/> then the <see cref="TypedConstant.Value"/> will not throw an exception.
        /// </summary>
        public bool IsString
        {
            get => IsPrimitiveType(constant, "string");
        }

        /// <summary>
        ///     Whether the <see cref="TypedConstant"/> represents an <see cref="int"/> value. If this returns
        ///     <see langword="true"/> then the <see cref="TypedConstant.Value"/> will not throw an exception.
        /// </summary>
        public bool IsInt
        {
            get => IsPrimitiveType(constant, "int");
        }

        /// <summary>
        ///     Whether the <see cref="TypedConstant"/> represents an <see cref="long"/> value. If this returns
        ///     <see langword="true"/> then the <see cref="TypedConstant.Value"/> will not throw an exception.
        /// </summary>
        public bool IsLong
        {
            get => IsPrimitiveType(constant, "long");
        }

        /// <summary>
        ///     Whether the <see cref="TypedConstant"/> represents an <see cref="float"/> value. If this returns
        ///     <see langword="true"/> then the <see cref="TypedConstant.Value"/> will not throw an exception.
        /// </summary>
        public bool IsFloat
        {
            get => IsPrimitiveType(constant, "float");
        }

        /// <summary>
        ///     Whether the <see cref="TypedConstant"/> represents an <see cref="double"/> value. If this returns
        ///     <see langword="true"/> then the <see cref="TypedConstant.Value"/> will not throw an exception.
        /// </summary>
        public bool IsDouble
        {
            get => IsPrimitiveType(constant, "double");
        }

        /// <summary>
        ///     Whether the <see cref="TypedConstant"/> represents a <see cref="bool"/> value. If this returns
        ///     <see langword="true"/> then the <see cref="TypedConstant.Value"/> will not throw an exception.
        /// </summary>
        public bool IsBool
        {
            get => IsPrimitiveType(constant, "bool");
        }

        /// <summary>
        ///     Gets the value of the <see cref="TypedConstant"/> as a <typeparamref name="T"/>. The value is written
        ///     to <paramref name="value"/> rather than returned so that <typeparamref name="T"/> is inferred from the
        ///     assignment target, which lets an array constant bind to the overload taking an
        ///     <see cref="ImmutableArray{T}"/> without the caller choosing between them.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <exception cref="InvalidCastException">The constant does not hold a <typeparamref name="T"/>.</exception>
        public void GetValue<T>(out T value)
        {
            value = Cast<T>(constant);
        }

        /// <summary>
        ///     Gets the elements of an array <see cref="TypedConstant"/>. An array constant has no
        ///     <see cref="TypedConstant.Value"/>, so its elements are read from <see cref="TypedConstant.Values"/>
        ///     individually.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="values">The elements, empty when the constant is <see langword="null"/>.</param>
        /// <exception cref="InvalidCastException">An element does not hold a <typeparamref name="T"/>.</exception>
        public void GetValue<T>(out ImmutableArray<T> values)
        {
            if (constant.IsNull)
            {
                values = ImmutableArray<T>.Empty;
                return;
            }

            var builder = ImmutableArray.CreateBuilder<T>(constant.Values.Length);
            foreach (var element in constant.Values)
            {
                builder.Add(Cast<T>(element));
            }

            values = builder.MoveToImmutable();
        }
    }

    private static T Cast<T>(TypedConstant constant)
    {
        var value = constant.Value!;

        // A boxed enum constant holds its underlying integral type, so it cannot be unboxed to the enum directly.
        return typeof(T).IsEnum ? (T)Enum.ToObject(typeof(T), value) : (T)value;
    }

    private static bool IsPrimitiveType(TypedConstant constant, string typeName)
    {
        if (constant.Kind != TypedConstantKind.Primitive)
        {
            return false;
        }

        if (constant.Type == null)
        {
            return false;
        }

        if (constant.Type.ToDisplayString() != typeName)
        {
            return false;
        }

        return true;
    }
}
