using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZCrew.Extensions.CodeAnalysis.CSharp.Text;

/// <summary>
///     Wrapper for a <see cref="StringBuilder" /> type that adds indentation to new lines.
/// </summary>
public sealed class FormattedStringBuilder
{
    private readonly StringBuilder builder;

    private int leadingSpaces;

    #region Delegating constructors
    /// <inheritdoc cref="StringBuilder()" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder()
    {
        this.builder = new StringBuilder();
    }

    /// <inheritdoc cref="StringBuilder(int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder(int capacity)
    {
        this.builder = new StringBuilder(capacity);
    }

    /// <inheritdoc cref="StringBuilder(int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder(int capacity, int maxCapacity)
    {
        this.builder = new StringBuilder(capacity, maxCapacity);
    }

    /// <inheritdoc cref="StringBuilder(string)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder(string value)
    {
        this.builder = new StringBuilder(value);
    }

    /// <inheritdoc cref="StringBuilder(string, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder(string value, int capacity)
    {
        this.builder = new StringBuilder(value, capacity);
    }

    /// <inheritdoc cref="StringBuilder(string, int, int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder(string value, int startIndex, int length, int capacity)
    {
        this.builder = new StringBuilder(value, startIndex, length, capacity);
    }
    #endregion

    #region Delegating properties
    /// <inheritdoc cref="StringBuilder.Capacity" />
    [ExcludeFromCodeCoverage]
    public int Capacity
    {
        get => this.builder.Capacity;
        set => this.builder.Capacity = value;
    }

    /// <inheritdoc cref="StringBuilder.this" />
    [ExcludeFromCodeCoverage]
    [IndexerName("Chars")]
    public char this[int index]
    {
        get => this.builder[index];
        set => this.builder[index] = value;
    }

    /// <inheritdoc cref="StringBuilder.Length" />
    [ExcludeFromCodeCoverage]
    public int Length
    {
        get => this.builder.Length;
        set => this.builder.Length = value;
    }

    /// <inheritdoc cref="StringBuilder.MaxCapacity" />
    [ExcludeFromCodeCoverage]
    public int MaxCapacity => this.builder.MaxCapacity;
    #endregion

    /// <summary>
    ///     Increments the indent level that will be used the next time either <see cref="AppendLine()" /> or
    ///     <see cref="AppendLine(string)" /> is called.
    /// </summary>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    public FormattedStringBuilder Indent()
    {
        this.leadingSpaces += 4;
        return this;
    }

    /// <summary>
    ///     Decrements the indent level that will be used the next time either <see cref="AppendLine()" /> or
    ///     <see cref="AppendLine(string)" /> is called.
    /// </summary>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="InvalidOperationException">When the string builder has no leading whitespace.</exception>
    public FormattedStringBuilder Unindent()
    {
        if (this.leadingSpaces == 0)
        {
            throw new InvalidOperationException("Unable to unindent any further");
        }
        this.leadingSpaces -= 4;
        return this;
    }

    /// <summary>
    ///     Appends the default line terminator to the end of the current <see cref="FormattedStringBuilder"></see>
    ///     object followed by the current indentation whitespace.
    /// </summary>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Enlarging the value of this instance would exceed <see cref="FormattedStringBuilder.MaxCapacity"></see>.
    /// </exception>
    public FormattedStringBuilder AppendLine()
    {
        this.builder.AppendLine().Append(' ', this.leadingSpaces);
        return this;
    }

    /// <summary>
    ///     Appends a copy of the specified string followed by the default line terminator to the end of the current
    ///     <see cref="FormattedStringBuilder"></see> object followed by the current indentation whitespace.
    /// </summary>
    /// <param name="value">The string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Enlarging the value of this instance would exceed <see cref="FormattedStringBuilder.MaxCapacity"></see>.
    /// </exception>
    public FormattedStringBuilder AppendLine(string value)
    {
        this.builder.AppendLine(value).Append(' ', this.leadingSpaces);
        return this;
    }

    /// <summary>
    ///     Appends the string representation of a specified <see cref="char"/> object to this instance followed by the
    ///     default line terminator to the end of the current <see cref="FormattedStringBuilder"></see> object followed
    ///     by the current indentation whitespace.
    /// </summary>
    /// <param name="value">The UTF-16-encoded code unit to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Enlarging the value of this instance would exceed <see cref="FormattedStringBuilder.MaxCapacity"></see>.
    /// </exception>
    public FormattedStringBuilder AppendLine(char value)
    {
        this.builder.Append(value).AppendLine().Append(' ', this.leadingSpaces);
        return this;
    }

    #region Delegating methods
    /// <inheritdoc cref="StringBuilder.Append(bool)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(bool value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(byte)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(byte value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(char)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(char value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(char, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(char value, int repeatCount)
    {
        this.builder.Append(value, repeatCount);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(char[])" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(char[] value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(char[], int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(char[] value, int startIndex, int charCount)
    {
        this.builder.Append(value, startIndex, charCount);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(decimal)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(decimal value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(double)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(double value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(short)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(short value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(int value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(long)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(long value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(object value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(sbyte)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(sbyte value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(float)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(float value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(string)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(string value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(string, int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(string value, int startIndex, int count)
    {
        this.builder.Append(value, startIndex, count);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(ushort)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(ushort value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(uint)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(uint value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Append(ulong)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Append(ulong value)
    {
        this.builder.Append(value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(IFormatProvider, string, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(IFormatProvider provider, string format, object arg0)
    {
        this.builder.AppendFormat(provider, format, arg0);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(IFormatProvider, string, object, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(IFormatProvider provider, string format, object arg0, object arg1)
    {
        this.builder.AppendFormat(provider, format, arg0, arg1);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(IFormatProvider, string, object, object, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(
        IFormatProvider provider,
        string format,
        object arg0,
        object arg1,
        object arg2
    )
    {
        this.builder.AppendFormat(provider, format, arg0, arg1, arg2);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(IFormatProvider, string, object[])" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
    {
        this.builder.AppendFormat(provider, format, args);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(string, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(string format, object arg0)
    {
        this.builder.AppendFormat(format, arg0);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(string, object, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(string format, object arg0, object arg1)
    {
        this.builder.AppendFormat(format, arg0, arg1);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(string, object, object, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
    {
        this.builder.AppendFormat(format, arg0, arg1, arg2);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.AppendFormat(string, object[])" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder AppendFormat(string format, params object[] args)
    {
        this.builder.AppendFormat(format, args);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Clear()" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Clear()
    {
        this.builder.Clear();
        return this;
    }

    /// <inheritdoc cref="StringBuilder.CopyTo(int, char[], int, int)" />
    [ExcludeFromCodeCoverage]
    public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        this.builder.CopyTo(sourceIndex, destination, destinationIndex, count);
    }

    /// <inheritdoc cref="StringBuilder.EnsureCapacity(int)" />
    [ExcludeFromCodeCoverage]
    public int EnsureCapacity(int capacity)
    {
        return this.builder.EnsureCapacity(capacity);
    }

    /// <inheritdoc cref="StringBuilder.Equals(StringBuilder)" />
    [ExcludeFromCodeCoverage]
    public bool Equals(StringBuilder sb)
    {
        return this.builder.Equals(sb);
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, bool)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, bool value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, byte)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, byte value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, char)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, char value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, char[])" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, char[] value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, char[], int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, char[] value, int startIndex, int charCount)
    {
        this.builder.Insert(index, value, startIndex, charCount);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, decimal)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, decimal value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, double)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, double value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, short)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, short value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, int value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, long)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, long value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, object)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, object value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, sbyte)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, sbyte value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, float)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, float value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, string)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, string value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, string, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, string value, int count)
    {
        this.builder.Insert(index, value, count);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, ushort)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, ushort value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, uint)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, uint value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Insert(int, ulong)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Insert(int index, ulong value)
    {
        this.builder.Insert(index, value);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Remove(int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Remove(int startIndex, int length)
    {
        this.builder.Remove(startIndex, length);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Replace(char, char)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Replace(char oldChar, char newChar)
    {
        this.builder.Replace(oldChar, newChar);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Replace(char, char, int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
    {
        this.builder.Replace(oldChar, newChar, startIndex, count);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Replace(string, string)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Replace(string oldValue, string newValue)
    {
        this.builder.Replace(oldValue, newValue);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.Replace(string, string, int, int)" />
    [ExcludeFromCodeCoverage]
    public FormattedStringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
    {
        this.builder.Replace(oldValue, newValue, startIndex, count);
        return this;
    }

    /// <inheritdoc cref="StringBuilder.ToString(int, int)" />
    [ExcludeFromCodeCoverage]
    public string ToString(int startIndex, int length)
    {
        return this.builder.ToString(startIndex, length);
    }

    /// <inheritdoc cref="StringBuilder.ToString()" />
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return this.builder.ToString();
    }
    #endregion
}
