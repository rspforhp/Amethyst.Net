using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace LexRules;

public interface IParsableValue<T>
{
    public Optional<T> Value { get; set; }
    public static abstract Optional<T> Parse(string from);
}

public partial class BooleanRule : IParsableValue<bool>
{
    public override bool Validate()
    {
        Value = Parse(ReadValue);
        return Value.HasValue;
    }

    public Optional<bool> Value { get; set; }

    public static Optional<bool> Parse(string from) => from switch
    {
        "true" => true,
        "false" => false,
        _ => default(Optional<bool>)
    };
}

public partial class BinaryIntegerLiteral : IParsableValue<Int64>
{
    public bool Unsigned { get; private set; }
    public bool Long { get; private set; }
    public Optional<long> Value { get; set; }
    public override bool Validate()
    {
        if (ReadValue.Contains("l", StringComparison.OrdinalIgnoreCase)) Long = true;
        if (ReadValue.Contains("u", StringComparison.OrdinalIgnoreCase)) Unsigned = true;
        ReadValue = ReadValue.Replace("l", "", StringComparison.OrdinalIgnoreCase)
            .Replace("u", "", StringComparison.OrdinalIgnoreCase);
        
        Value = Parse(ReadValue);
        return Value.HasValue;
    }
    public static Optional<long> Parse(string from)
    {
        string remove0b = from.Substring(2).Replace("_","");
        long binary = long.Parse(remove0b, NumberStyles.BinaryNumber);
        return binary;
    }
}
public partial class HexadecimalIntegerLiteral : IParsableValue<Int64>
{
    public bool Unsigned { get; private set; }
    public bool Long { get; private set; }
    public Optional<long> Value { get; set; }
    public override bool Validate()
    {
        if (ReadValue.Contains("l", StringComparison.OrdinalIgnoreCase)) Long = true;
        if (ReadValue.Contains("u", StringComparison.OrdinalIgnoreCase)) Unsigned = true;
        ReadValue = ReadValue.Replace("l", "", StringComparison.OrdinalIgnoreCase)
            .Replace("u", "", StringComparison.OrdinalIgnoreCase);
        Value = Parse(ReadValue);
        return Value.HasValue;
    }
    public static Optional<long> Parse(string from)
    {
        string remove0x = from.Substring(2).Replace("_","");
        long binary = long.Parse(remove0x, NumberStyles.HexNumber);
        return binary;
    }
}
public partial class DecimalIntegerLiteral : IParsableValue<Int64>
{
    public bool Unsigned { get; private set; }
    public bool Long { get; private set; }
    public Optional<long> Value { get; set; }
    public override bool Validate()
    {
        if (ReadValue.Contains("l", StringComparison.OrdinalIgnoreCase)) Long = true;
        if (ReadValue.Contains("u", StringComparison.OrdinalIgnoreCase)) Unsigned = true;
        ReadValue = ReadValue.Replace("l", "", StringComparison.OrdinalIgnoreCase)
            .Replace("u", "", StringComparison.OrdinalIgnoreCase);
        Value = Parse(ReadValue);
        return Value.HasValue;
    }
    public static Optional<long> Parse(string from)
    {
        string remove = from.Replace("_","");
        long binary = long.Parse(remove, NumberStyles.Number);
        return binary;
    }
}
public partial class IntegerLiteral : IParsableValue<Int64>
{
    public bool Unsigned { get; private set; }
    public bool Long { get; private set; }
    public Optional<long> Value { get; set; }
    public override bool Validate()
    {
        if (ReadValue.Contains("l", StringComparison.OrdinalIgnoreCase)) Long = true;
        if (ReadValue.Contains("u", StringComparison.OrdinalIgnoreCase)) Unsigned = true;
        ReadValue = ReadValue.Replace("l", "", StringComparison.OrdinalIgnoreCase)
            .Replace("u", "", StringComparison.OrdinalIgnoreCase);
        Value = Parse(ReadValue);
        return Value.HasValue;
    }
    public static Optional<long> Parse(string from)
    {
        if (from.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return HexadecimalIntegerLiteral.Parse(from);
        }
        else  if (from.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            return BinaryIntegerLiteral.Parse(from);
        }
        else
        {
            return DecimalIntegerLiteral.Parse(from);
        }
    }
}
public partial class RealLiteral : IParsableValue<double>
{
    public bool Float { get; private set; }
    public bool Double { get; private set; }
    public bool Decimal { get; private set; }
    public Optional<double> Value { get; set; }
    public override bool Validate()
    {
        if (ReadValue.EndsWith("d", StringComparison.OrdinalIgnoreCase)) Double = true;
        else if (ReadValue.EndsWith("f", StringComparison.OrdinalIgnoreCase)) Float = true;
        else if (ReadValue.EndsWith("m", StringComparison.OrdinalIgnoreCase)) Decimal = true;
        if(Double||Float||Decimal)ReadValue = ReadValue.Substring(0, ReadValue.Length - 1);
        Value = Parse(ReadValue);
        return Value.HasValue;
    }

    public static Optional<double> Parse(string from)
    {
        return double.Parse(from.Replace("_",""), NumberStyles.Float);
    }
}