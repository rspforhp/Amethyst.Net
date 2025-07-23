using System.Diagnostics;
using System.Globalization;
using System.Text;
using SequentialParser;

namespace Tests;

public static class AmethystShit
{
    public static ParsableSequence BoolLiteral = StringSequence.Make("true", "false")
        .AddValidation(BoolValidation)
        .Verify();

    public static bool BoolValidation(string str, Dictionary<string, object> d)
    {
        bool? Value = str switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };
        if (d != null) d[ParsableSequence.ExtraParsedValue] = Value;
        return true;
    }

    public static ParsableSequence Underscore = StringSequence.Make("_").Verify();

    [Flags]
    public enum IntegerSuffix : byte
    {
        l = 0b1,
        u = 0b10,
        L = l,
        U = u,

        ul = u | l,
        uL = u | L,
        UL = U | L,
        Ul = U | l,

        lu = u | l,
        Lu = u | L,
        LU = U | L,
        lU = U | l,
    }

    public static ParsableSequence IntSuffix = StringSequence.Make<IntegerSuffix>().Verify();

    public static ParsableSequence DecoratedBinDigit = ParsableSequence.Make()
        .Add("_[]d")
        .Define('_', Underscore)
        .Define('d', FunctionParsableSequence.BinDigit)
        .Verify();

    public static ParsableSequence BinSpecificator = StringSequence.Make("0b", "0B").Verify();

    public static ParsableSequence BinIntLiteral = ParsableSequence.Make()
        .Add("sd[1.]f[0.1]")
        .Define('s', BinSpecificator)
        .Define('d', DecoratedBinDigit)
        .Define('f', IntSuffix)
        .AddValidation(IntValidation)
        .Verify();


    public static ParsableSequence DecoratedHexDigit = ParsableSequence.Make()
        .Add("_[]d")
        .Define('_', Underscore)
        .Define('d', FunctionParsableSequence.HexDigit)
        .Verify();

    public static ParsableSequence HexSpecificator = StringSequence.Make("0x", "0X").Verify();

    public static ParsableSequence HexIntLiteral = ParsableSequence.Make()
        .Add("sd[1.]f[0.1]")
        .Define('s', HexSpecificator)
        .Define('d', DecoratedHexDigit)
        .Define('f', IntSuffix)
        .AddValidation(IntValidation)
        .Verify();


    public static ParsableSequence DecoratedDecimalDigit = ParsableSequence.Make()
        .Add("_[]d")
        .Define('_', Underscore)
        .Define('d', FunctionParsableSequence.Digit)
        .Verify();


    public static ParsableSequence DecimalIntLiteral = ParsableSequence.Make()
        .Add("sd[1.]f[0.1]")
        .Define('s', FunctionParsableSequence.Digit)
        .Define('d', DecoratedDecimalDigit)
        .Define('f', IntSuffix)
        .AddValidation(IntValidation)
        .Verify();

    public static ParsableSequence IntLiteral =
        SwitchSequence.Make(HexIntLiteral, BinIntLiteral, DecimalIntLiteral).Verify();

    private static bool IntValidation(string arg1, Dictionary<string, object> arg2)
    {
        //TODO: suffix handling
        switch (arg1)
        {
            case { Length: > 0 } when arg1.StartsWith("0b", true, CultureInfo.InvariantCulture):
            {
                var trimmed = arg1[2..];
                arg2[ParsableSequence.ExtraParsedValue] = ulong.Parse(trimmed, NumberStyles.BinaryNumber);
            }
                //bin
                break;
            case { Length: > 0 } when arg1.StartsWith("0x", true, CultureInfo.InvariantCulture):
            {
                var trimmed = arg1[2..];
                arg2[ParsableSequence.ExtraParsedValue] = ulong.Parse(trimmed, NumberStyles.HexNumber);
            }
                //hex
                break;
            case { Length: > 0 }:
                arg2[ParsableSequence.ExtraParsedValue] = ulong.Parse(arg1, NumberStyles.Integer);
                //dec
                break;
            default:
                return false;
        }

        return true;
    }


    public enum FloatSuffix : byte
    {
        f = 0b1,
        d = 0b10,
        m = 0b100,
        F = f,
        D = d,
        M = m
    }

    public static ParsableSequence RealSuffix = StringSequence.Make<FloatSuffix>().Verify();

    public static ParsableSequence ExponentSign = StringSequence.Make("+", "-").Verify();

    public static ParsableSequence Exponent = ParsableSequence.Make()
        .Add("es[0.1]_d[]")
        .Define('e', StringSequence.Make("e", "E"))
        .Define('s', ExponentSign)
        .Define('_', FunctionParsableSequence.Digit)
        .Define('d', DecoratedDecimalDigit)
        .Verify();

    public static ParsableSequence RealLiteral = SwitchSequence.Make(
        (
            ParsableSequence.Make()
                .Add("_d[]._d[]e[0.1]s[0.1]")
                .Define('_', FunctionParsableSequence.Digit)
                .Define('d', DecoratedDecimalDigit)
                .Define('.', StringSequence.Make("."))
                .Define('e', Exponent)
                .Define('s', RealSuffix)
                .Verify()
        ),
        (
            ParsableSequence.Make()
                .Add("._d[]e[0.1]s[0.1]")
                .Define('_', FunctionParsableSequence.Digit)
                .Define('d', DecoratedDecimalDigit)
                .Define('.', StringSequence.Make("."))
                .Define('e', Exponent)
                .Define('s', RealSuffix)
                .Verify()
        ),
        (
            ParsableSequence.Make()
                .Add("_d[]es[0.1]")
                .Define('_', FunctionParsableSequence.Digit)
                .Define('d', DecoratedDecimalDigit)
                .Define('.', StringSequence.Make("."))
                .Define('e', Exponent)
                .Define('s', RealSuffix)
                .Verify()
        ),
        (
            ParsableSequence.Make()
                .Add("_d[]s")
                .Define('_', FunctionParsableSequence.Digit)
                .Define('d', DecoratedDecimalDigit)
                .Define('.', StringSequence.Make("."))
                .Define('e', Exponent)
                .Define('s', RealSuffix)
                .Verify()
        )
    ).AddValidation(ValidateReal).Verify();

    public static bool ValidateReal(string arg1, Dictionary<string, object> arg2)
    {
        var removeSuffix = arg1.Replace("d", "", StringComparison.InvariantCultureIgnoreCase)
            .Replace("f", "", StringComparison.InvariantCultureIgnoreCase)
            .Replace("m", "", StringComparison.InvariantCultureIgnoreCase);
        arg2[ParsableSequence.ExtraParsedValue] = double.Parse(removeSuffix, NumberStyles.Float);
        return true;
    }

    public static char[] NewLineCharacters =
    [
        '\u000D', // carriage return
        '\u000A', // line feed
        '\u0085', // next line
        '\u2028', // line separator
        '\u2029', // paragraph separator
    ];

    public static ParsableSequence SingleCharacter = FunctionParsableSequence
        .Make(c => (c != '\'' && c != '\\' && !NewLineCharacters.Contains(c))).AddValidation(
            delegate(string arg1, Dictionary<string, object> arg2)
            {
                arg2[ParsableSequence.ExtraParsedValue] = arg1[0];
                return true;
            }).Verify();


    public static ParsableSequence SimpleEscapeSequence = StringSequence
        .Make(@"\'", "\\\"", @"\\", @"\0", @"\a", @"\b", @"\f", @"\n", @"\r", @"\t", @"\v").AddValidation(SimpleEscape)
        .Verify();

    public static bool SimpleEscape(string arg1, Dictionary<string, object> arg2)
    {
        char? value = null;
        switch (arg1)
        {
            case @"\'":
                value = '\'';
                break;
            case "\\\"":
                value = '\"';
                break;
            case @"\\":
                value = '\\';
                break;
            case @"\0":
                value = '\0';
                break;
            case @"\a":
                value = '\a';
                break;
            case @"\b":
                value = '\b';
                break;
            case @"\f":
                value = '\f';
                break;
            case @"\n":
                value = '\n';
                break;
            case @"\r":
                value = '\r';
                break;
            case @"\t":
                value = '\t';
                break;
            case @"\v":
                value = '\v';
                break;
            default:
                return false;
        }

        arg2[ParsableSequence.ExtraParsedValue] = value;
        return true;
    }

    public static ParsableSequence HexEscapeSequence = ParsableSequence.Make()
        .Add("xdd[0.1]d[0.1]d[0.1]")
        .Define('x', StringSequence.Make("\\x"))
        .Define('d', FunctionParsableSequence.HexDigit)
        .AddValidation(HexEscape)
        .Verify();

    public static bool HexEscape(string arg1, Dictionary<string, object> arg2)
    {
        var trim = arg1[2..];
        var ch = (char)uint.Parse(trim, NumberStyles.HexNumber);
        arg2[ParsableSequence.ExtraParsedValue] = ch;
        return true;
    }


    public static ParsableSequence UnicodeEscapeSequence = SwitchSequence.Make(
            (ParsableSequence.Make()
                .Add("udddd")
                .Define('u', StringSequence.Make("\\u"))
                .Define('d', FunctionParsableSequence.HexDigit)
                .Verify()),
            (ParsableSequence.Make()
                .Add("Udddddddd")
                .Define('U', StringSequence.Make("\\U"))
                .Define('d', FunctionParsableSequence.HexDigit)
                .Verify()))
        .AddValidation(UnicodeEscape)
        .Verify();

    private static bool UnicodeEscape(string arg1, Dictionary<string, object> arg2)
    {
        var trim = arg1[2..];
        var convertFromUtf32 = char.ConvertFromUtf32(int.Parse(trim, NumberStyles.HexNumber));
        arg2[ParsableSequence.ExtraParsedValue] = convertFromUtf32[0];
        if (convertFromUtf32.Length > 1)
            arg2[ParsableSequence.ExtraParsedValue + "2"] = convertFromUtf32[1];
        return true;
    }

    public static ParsableSequence Character =
        SwitchSequence.Make(UnicodeEscapeSequence, HexEscapeSequence, SimpleEscapeSequence, SingleCharacter);

    public static ParsableSequence CharLiteral = ParsableSequence.Make()
        .Add("qcq")
        .Define('q', StringSequence.Make("\'"))
        .Define('c', Character)
        .AddValidation(delegate(string arg1, Dictionary<string, object> arg2)
        {
            arg2[ParsableSequence.ExtraParsedValue] =
                ((Dictionary<string, object>)arg2["SEQUENCE_ID_c_0_@1"])[ParsableSequence.ExtraParsedValue];
            return true;
        })
        .Verify();

    public static ParsableSequence QuoteEscapeSequence = StringSequence.Make("\"\"").AddValidation(
        delegate(string arg1, Dictionary<string, object> arg2)
        {
            arg2[ParsableSequence.ExtraParsedValue] = "\"";
            return true;
        }).Verify();

    public static ParsableSequence VerbatimSingleCharacter = FunctionParsableSequence.Make(a => a != '\"').Verify();

    public static ParsableSequence VerbatimCharacter =
        SwitchSequence.Make(VerbatimSingleCharacter, QuoteEscapeSequence).Verify();

    public static ParsableSequence VerbatimStringLiteral = ParsableSequence.Make()
        .Add("@qc[]q")
        .Define('@', StringSequence.Make("@"))
        .Define('q', StringSequence.Make("\""))
        .Define('c', VerbatimCharacter)
        .AddValidation(VerbatimString)
        .Verify();

    public static bool VerbatimString(string arg1, Dictionary<string, object> arg2)
    {
        var trim = arg1[2..^1];
        arg2[ParsableSequence.ExtraParsedValue] = trim.Replace("\"\"", "\"");
        return true;
    }

    public static ParsableSequence SingleStringCharacter = FunctionParsableSequence.Make(c =>
        c != '\"' && c != '\\' && !NewLineCharacters.Contains(c)).AddValidation(
        delegate(string arg1, Dictionary<string, object> arg2)
        {
            arg2[ParsableSequence.ExtraParsedValue] = arg1[0];
            return true;
        }).Verify();

    public static ParsableSequence StringCharacter = SwitchSequence.Make(SingleStringCharacter, SimpleEscapeSequence,
        HexEscapeSequence, UnicodeEscapeSequence);
    
    public static ParsableSequence RegularStringLiteral=ParsableSequence.Make()
        .Add("qc[]q")
        .Define('q', StringSequence.Make("\""))
        .Define('c',StringCharacter )
        .AddValidation(RegularString)
        .Verify();
    
    public static bool RegularString(string arg1, Dictionary<string, object> arg2)
    {
        StringBuilder b = new();
        for (ulong i = 0; i < ulong.MaxValue; i++)
        {
            var c = $"SEQUENCE_ID_c_{i}_@1";
            if (!arg2.TryGetValue(c, out var value)) break;
            var v=(char)((Dictionary<string, object>)value)[ParsableSequence.ExtraParsedValue];
            b.Append(v);
        }
        arg2[ParsableSequence.ExtraParsedValue] = b.ToString();
        return true;
    }

    public static ParsableSequence StringLiteral =
        SwitchSequence.Make(VerbatimStringLiteral, RegularStringLiteral).Verify();

    public static ParsableSequence NullLiteral = StringSequence.Make("null").AddValidation(NullLit).Verify();

    private static bool NullLit(string arg1, Dictionary<string, object> arg2)
    {
        arg2[ParsableSequence.ExtraParsedValue] = null;
        return true;
    }

    public static ParsableSequence Literal = SwitchSequence.Make(BoolLiteral,IntLiteral,RealLiteral,CharLiteral,StringLiteral,NullLiteral).Verify();
}