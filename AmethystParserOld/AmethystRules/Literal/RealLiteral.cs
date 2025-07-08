using System.Diagnostics;
using System.Globalization;
using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Literal;

public sealed class RealLiteral : AbstractLiteral<Double>
{
   
    
    public LexRule Digit { get; protected init; }
    public LexRule DecoratedDigit { get; protected set; }
    

    public RealLiteral()
    {
        Digit =('0'..'9').Rule();
        SetDecoratedDigit();
    }


    public override void AfterParse()
    {
        //TODO: exponent part if id need that
        string num1 = DigitsBeforeDotList.MyString.Replace("_", "");
        string num2 = DigitsAfterDotList.MyString.Replace("_", "");
        if (string.IsNullOrEmpty(num1)) num1 = "0";
        Value = double.Parse(num1 + "." + num2, NumberStyles.Float);

    }
    public LexRule Suffix { get; init; } =
        ("F".Rule() | "f" | "D" | "d" | "M" | "m").Optional();
    public bool Float { get; private set; }
    public bool Double { get; private set; }
    public bool Decimal { get; private set; }

    public LexRule DigitsBeforeDotList { get; set; } 
    public LexRule DigitsAfterDotList { get; set; } 
    
    public object TrueValue
    {
        get
        {
            if (Float) return (float)Value;
            if (Decimal) return (decimal)Value;
            if (Double) return (double)Value;
            return Value;
        }
    }
    public void SetDecoratedDigit()
    {
        DecoratedDigit= "_".Rule().List() & Digit;
        DigitsBeforeDotList=DecoratedDigit.List();
        DigitsAfterDotList=DecoratedDigit.List(true);
        ComplexRule = DigitsBeforeDotList&"." & DigitsAfterDotList & Suffix;
    }
}