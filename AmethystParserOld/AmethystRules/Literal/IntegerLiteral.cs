using System.Globalization;
using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Literal;

public abstract class AbstractIntLiteral : AbstractLiteral<Int64>
{
   
    public LexRule Prefix { get; protected init; }
    public LexRule Suffix { get; protected init; } =
        ("U".Rule() | "u" | "L" | "l" | "UL" | "Ul" | "uL" | "ul" | "LU" | "Lu" | "lU" | "lu").Optional();
    
    public LexRule Digit { get; protected init; }
    public LexRule DecoratedDigit { get; protected set; }
    

    public AbstractIntLiteral()
    {
    }

    public bool Long { get; protected set; }
    public bool Unsigned { get; protected set; }

    public override void AfterParse()
    {
        Long = Suffix.MyString.Contains("l", StringComparison.OrdinalIgnoreCase);
        Unsigned = Suffix.MyString.Contains("u", StringComparison.OrdinalIgnoreCase);
        base.AfterParse();
    }

    public LexRule DigitsList { get; protected set; } 
    public void SetDecoratedDigit()
    {
        DecoratedDigit= "_".Rule().List() & Digit;
        DigitsList=DecoratedDigit.List(true);
        ComplexRule = Prefix & DigitsList & Suffix;
    }
}
public sealed class DecimalLiteral :AbstractIntLiteral
{
    public DecimalLiteral()
    {
        Prefix = True;
        Digit =('0'..'9').Rule();
        SetDecoratedDigit();
    }
    public override void AfterParse()
    {
        base.AfterParse();
        string num = DigitsList.MyString.Replace("_", "");
        Value=long.Parse(num,NumberStyles.Integer);
    }
}
public sealed class HexLiteral :AbstractIntLiteral
{
    public HexLiteral()
    {
        Prefix="0x".Rule()|"0X";
        Digit =('0'..'9').Rule() | 'A'..'F' | 'a'..'f';
        SetDecoratedDigit();
    }
    public override void AfterParse()
    {
        base.AfterParse();
        string num = DigitsList.MyString.Replace("_", "");
        Value=long.Parse(num,NumberStyles.HexNumber);
    }
}
public sealed class BinaryLiteral : AbstractIntLiteral
{
    public BinaryLiteral()
    {
        Prefix="0b".Rule()|"0b";
        Digit ="1".Rule()|"0";
        SetDecoratedDigit();
    }

    public override void AfterParse()
    {
        base.AfterParse();
        string num = DigitsList.MyString.Replace("_", "");
        Value=long.Parse(num,NumberStyles.BinaryNumber);
    }
}
public sealed class IntegerLiteral : AbstractLiteral<Int64>
{
    public DecimalLiteral Decimal;
    public HexLiteral Hex;
    public BinaryLiteral Bin;
    
    public bool Long { get; private set; }
    public bool Unsigned { get; private set; }
    
    public object TrueValue
    {
        get
        {
            if (Unsigned)
            {
                if (Long) return (UInt64)Value;
                return (UInt32)Value;
            }
            if (Long) return (Int64)Value;
            return (Int32)Value;
        }
    }
    public IntegerLiteral()
    {
        Decimal = new DecimalLiteral();
        Decimal.Action(delegate
        {
            Long = Decimal.Long;
            Unsigned = Decimal.Unsigned;
            Value = Decimal.Value;
        });
        Hex = new HexLiteral();
        Hex.Action(delegate
        {
            Long = Hex.Long;
            Unsigned = Hex.Unsigned;
            Value = Hex.Value;
        });
        Bin = new BinaryLiteral();
        Bin.Action(delegate
        {
            Long = Bin.Long;
            Unsigned = Bin.Unsigned;
            Value = Bin.Value;
        });
        ComplexRule = Hex | Bin | Decimal;
    }
    //maybe todo: change it to be more static and not use actions
 
}