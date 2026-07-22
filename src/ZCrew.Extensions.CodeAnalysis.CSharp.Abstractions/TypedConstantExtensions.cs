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
