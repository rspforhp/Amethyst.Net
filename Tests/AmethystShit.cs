using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using SequentialParser;

namespace Tests;

public static class AmethystShit
{
    public static ParsableSequence BoolLiteral = StringSequence.Make("true", "false")
        .AddValidation(BoolValidation)
        .Verify();

    public static bool BoolValidation(string str, SequenceDictionary d)
    {
        bool? Value = str switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };
        if (d != null) d.ParsedValue = Value;
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

    private static bool IntValidation(string arg1, SequenceDictionary arg2)
    {
        //TODO: suffix handling
        switch (arg1)
        {
            case { Length: > 0 } when arg1.StartsWith("0b", true, CultureInfo.InvariantCulture):
            {
                var trimmed = arg1[2..];
                arg2.ParsedValue = ulong.Parse(trimmed, NumberStyles.BinaryNumber);
            }
                //bin
                break;
            case { Length: > 0 } when arg1.StartsWith("0x", true, CultureInfo.InvariantCulture):
            {
                var trimmed = arg1[2..];
                arg2.ParsedValue = ulong.Parse(trimmed, NumberStyles.HexNumber);
            }
                //hex
                break;
            case { Length: > 0 }:
                arg2.ParsedValue = ulong.Parse(arg1, NumberStyles.Integer);
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

    public static bool ValidateReal(string arg1, SequenceDictionary arg2)
    {
        var removeSuffix = arg1.Replace("d", "", StringComparison.InvariantCultureIgnoreCase)
            .Replace("f", "", StringComparison.InvariantCultureIgnoreCase)
            .Replace("m", "", StringComparison.InvariantCultureIgnoreCase);
        arg2.ParsedValue = double.Parse(removeSuffix, NumberStyles.Float);
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
            delegate(string arg1, SequenceDictionary arg2)
            {
                arg2.ParsedValue = arg1[0];
                return true;
            }).Verify();


    public static ParsableSequence SimpleEscapeSequence = StringSequence
        .Make(@"\'", "\\\"", @"\\", @"\0", @"\a", @"\b", @"\f", @"\n", @"\r", @"\t", @"\v").AddValidation(SimpleEscape)
        .Verify();

    public static bool SimpleEscape(string arg1, SequenceDictionary arg2)
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

        arg2.ParsedValue = value;
        return true;
    }

    public static ParsableSequence HexEscapeSequence = ParsableSequence.Make()
        .Add("xdd[0.1]d[0.1]d[0.1]")
        .Define('x', StringSequence.Make("\\x"))
        .Define('d', FunctionParsableSequence.HexDigit)
        .AddValidation(HexEscape)
        .Verify();

    public static bool HexEscape(string arg1, SequenceDictionary arg2)
    {
        var trim = arg1[2..];
        var ch = (char)uint.Parse(trim, NumberStyles.HexNumber);
        arg2.ParsedValue = ch;
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

    private static bool UnicodeEscape(string arg1, SequenceDictionary arg2)
    {
        var trim = arg1[2..];
        var convertFromUtf32 = char.ConvertFromUtf32(int.Parse(trim, NumberStyles.HexNumber));
        arg2.ParsedValue = convertFromUtf32[0];
        if (convertFromUtf32.Length > 1)
            arg2.ParsedValue2 = convertFromUtf32[1];
        return true;
    }

    public static ParsableSequence Character =
        SwitchSequence.Make(UnicodeEscapeSequence, HexEscapeSequence, SimpleEscapeSequence, SingleCharacter);

    public static ParsableSequence CharLiteral = ParsableSequence.Make()
        .Add("qcq")
        .Define('q', StringSequence.Make("\'"))
        .Define('c', Character)
        .AddValidation(delegate(string arg1, SequenceDictionary arg2)
        {
            arg2.ParsedValue =
                arg2.GetSeqElement('c', 0, 1).ParsedValue;
            return true;
        })
        .Verify();

    public static ParsableSequence QuoteEscapeSequence = StringSequence.Make("\"\"").AddValidation(
        delegate(string arg1, SequenceDictionary arg2)
        {
            arg2.ParsedValue = "\"";
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

    public static bool VerbatimString(string arg1, SequenceDictionary arg2)
    {
        var trim = arg1[2..^1];
        arg2.ParsedValue = trim.Replace("\"\"", "\"");
        return true;
    }

    public static ParsableSequence SingleStringCharacter = FunctionParsableSequence.Make(c =>
        c != '\"' && c != '\\' && !NewLineCharacters.Contains(c)).AddValidation(
        delegate(string arg1, SequenceDictionary arg2)
        {
            arg2.ParsedValue = arg1[0];
            return true;
        }).Verify();

    public static ParsableSequence StringCharacter = SwitchSequence.Make(SingleStringCharacter, SimpleEscapeSequence,
        HexEscapeSequence, UnicodeEscapeSequence);

    public static ParsableSequence RegularStringLiteral = ParsableSequence.Make()
        .Add("qc[]q")
        .Define('q', StringSequence.Make("\""))
        .Define('c', StringCharacter)
        .AddValidation(RegularString)
        .Verify();

    public static bool RegularString(string arg1, SequenceDictionary arg2)
    {
        StringBuilder b = new();
        var allChars = arg2.GetSeqElements('c', 1);
        foreach (var ch in allChars)
        {
            b.Append(ch.ParsedValue);
        }

        arg2.ParsedValue = b.ToString();
        return true;
    }

    public static ParsableSequence StringLiteral =
        SwitchSequence.Make(VerbatimStringLiteral, RegularStringLiteral).Verify();

    public static ParsableSequence NullLiteral = StringSequence.Make("null").AddValidation(NullLit).Verify();

    private static bool NullLit(string arg1, SequenceDictionary arg2)
    {
        arg2.ParsedValue = null;
        return true;
    }

    public static ParsableSequence Literal = SwitchSequence
        .Make(BoolLiteral, IntLiteral, RealLiteral, CharLiteral, StringLiteral, NullLiteral).Verify();

    //TODO:id's and shit

    public static ParsableSequence FormattingCharacter = SwitchSequence
        .Make(UnicodeEscapeSequence, FunctionParsableSequence.AnyChar).AddValidation(FormattingCharacter_).Verify();

    public static bool FormattingCharacter_(string arg1, SequenceDictionary arg2) =>
        char.GetUnicodeCategory((char)arg2.ParsedValue) == UnicodeCategory.Format;

    public static ParsableSequence ConnectingCharacter = SwitchSequence
        .Make(UnicodeEscapeSequence, FunctionParsableSequence.AnyChar).AddValidation(ConnectingCharacter_).Verify();

    public static bool ConnectingCharacter_(string arg1, SequenceDictionary arg2) =>
        char.GetUnicodeCategory((char)arg2.ParsedValue) == UnicodeCategory.ConnectorPunctuation;

    public static ParsableSequence DecimalDigitCharacter = SwitchSequence
        .Make(UnicodeEscapeSequence, FunctionParsableSequence.AnyChar).AddValidation(DecimalDigitCharacter_).Verify();

    public static bool DecimalDigitCharacter_(string arg1, SequenceDictionary arg2) =>
        char.GetUnicodeCategory((char)arg2.ParsedValue) == UnicodeCategory.DecimalDigitNumber;

    public static ParsableSequence CombiningCharacter = SwitchSequence
        .Make(UnicodeEscapeSequence, FunctionParsableSequence.AnyChar).AddValidation(CombiningCharacter_).Verify();

    public static bool CombiningCharacter_(string arg1, SequenceDictionary arg2) =>
        char.GetUnicodeCategory((char)arg2.ParsedValue) is (UnicodeCategory.SpacingCombiningMark
            or UnicodeCategory.NonSpacingMark);

    public static ParsableSequence LetterCharacter = SwitchSequence
        .Make(UnicodeEscapeSequence, FunctionParsableSequence.AnyChar).AddValidation(LetterCharacter_).Verify();

    public static bool LetterCharacter_(string arg1, SequenceDictionary arg2) =>
        char.GetUnicodeCategory((char)arg2.ParsedValue) is UnicodeCategory.LetterNumber
            or UnicodeCategory.LowercaseLetter or UnicodeCategory.UppercaseLetter;

    public static ParsableSequence IdentifierPartCharacter = SwitchSequence.Make(FormattingCharacter,
        CombiningCharacter, ConnectingCharacter, DecimalDigitCharacter, LetterCharacter).Verify();

    public static ParsableSequence UnderscoreCharacter = StringSequence.Make("_", "\\u005", "\\U0000005").Verify();

    public static ParsableSequence IdentifierStartCharacter =
        SwitchSequence.Make(LetterCharacter, UnderscoreCharacter).Verify();

    public static ParsableSequence BasicIdentifier = ParsableSequence.Make()
        .Add("sp[]")
        .Define('s', IdentifierStartCharacter)
        .Define('p', IdentifierPartCharacter)
        .Verify();


    public static string[] Keywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
        "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
        "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null",
        "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly",
        "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
        "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    ];

    public static ParsableSequence Keyword = StringSequence.Make(Keywords).Verify();

    public static string[] ContextualKeywords =
    [
        "add", "alias", "ascending", "async", "await", "by", "descending", "dynamic", "equals", "from", "get",
        "global", "group", "into", "join", "let", "nameof", "notnull", "on", "orderby", "partial", "remove", "select",
        "set", "unmanaged", "value", "var", "when", "where", "yield"
    ];

    public static ParsableSequence ContextualKeyword = StringSequence.Make(ContextualKeywords).Verify();


    // Includes keywords and contextual keywords prefixed by '@'.
    public static ParsableSequence EscapedIdentifier = ParsableSequence.Make()
        .Add("@i")
        .Define('@', StringSequence.Make("@"))
        .Define('i', BasicIdentifier)
        .AddValidation(EscapedIdentifier_)
        .Verify();

    public static bool EscapedIdentifier_(string arg1, SequenceDictionary arg2)
    {
        if (Keywords.Contains(arg1) || ContextualKeywords.Contains(arg1))
        {
            arg2.ReadString = arg2.ReadString[1..];
            return true;
        }

        return true;
    }

    // excluding keywords or contextual keywords
    public static ParsableSequence AvailableIdentifer =
        SwitchSequence.Make(BasicIdentifier).AddValidation(AvailableIdentifier_).Verify();

    public static bool AvailableIdentifier_(string arg1, SequenceDictionary arg2)
    {
        return !Keywords.Contains(arg1) && !ContextualKeywords.Contains(arg1);
    }

    public static ParsableSequence SimpleIdentifier =
        SwitchSequence.Make(AvailableIdentifer, EscapedIdentifier).Verify();

    public static ParsableSequence Identifier = SwitchSequence.Make(SimpleIdentifier, ContextualKeyword).Verify();

    public static ParsableSequence Whitespace = FunctionParsableSequence.Make(a =>
        char.GetUnicodeCategory(a) == UnicodeCategory.SpaceSeparator || a == '\u0009' || a == '\u000B' ||
        a == '\u000C').Verify();

    //TODO: 
    public static ParsableSequence TypeParam = SwitchSequence.Make(Identifier).Verify();

    public static ParsableSequence TypeArgumentList = ParsableSequence.Make()
        .Add("<ts[]>")
        .Define('<', StringSequence.Make("<"))
        .Define('>', StringSequence.Make(">"))
        .Define('t', TypeArgument)
        .Define('s', ParsableSequence.Make()
            .Add(",t")
            .Define('t', TypeArgument)
            .Define(',', StringSequence.Make(","))
            .Verify())
        .Verify();

    public static ParsableSequence TypeArgument = SwitchSequence.Make(Type, ParsableSequence.Make()
        .Add("ta[0.1]")
        .Define('t', TypeParam)
        .Define('a', NullableTypeAnnotation)
        .Verify()).Verify();

    public static ParsableSequence NamespaceOrTypeName = SwitchSequence.Make(
        (ParsableSequence.Make()
            .Add("il[0.1]s[]")
            .Define('i', Identifier)
            .Define('l', TypeArgumentList)
            .Define('s', ParsableSequence.Make()
                .Add(".il[0.1]")
                .Define('i', Identifier)
                .Define('l', TypeArgumentList)
                .Verify())
            .Verify()),
        (ParsableSequence.Make()
            .Add("as[]")
            .Define('a', QualifiedAliasMember)
            .Define('s', ParsableSequence.Make()
                .Add(".il[0.1]")
                .Define('i', Identifier)
                .Define('l', TypeArgumentList)
                .Verify())
            .Verify())
    ).Verify();


    public static ParsableSequence NamespaceName = SwitchSequence.Make(NamespaceOrTypeName).Verify();
    public static ParsableSequence TypeName = SwitchSequence.Make(NamespaceOrTypeName).Verify();


    public static ParsableSequence DynamicTypeKeyword =
        StringSequence.Make("dynamic").AddValidation(delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Dynamic";
            return true;
        }).Verify();

    public static ParsableSequence ObjectTypeKeyword = StringSequence.Make("object").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Object";
            return true;
        }).Verify();

    public static ParsableSequence BoolTypeKeyword = StringSequence.Make("bool").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Boolean";
            return true;
        }).Verify();

    public static ParsableSequence StringTypeKeyword = StringSequence.Make("string").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.String";
            return true;
        }).Verify();

    public static ParsableSequence SByteTypeKeyword = StringSequence.Make("sbyte").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.SByte";
            return true;
        }).Verify();


    public static ParsableSequence ByteTypeKeyword = StringSequence.Make("byte").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Byte";
            return true;
        }).Verify();


    public static ParsableSequence ShortTypeKeyword = StringSequence.Make("short").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Int16";
            return true;
        }).Verify();

    public static ParsableSequence UShortTypeKeyword = StringSequence.Make("ushort").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.UInt16";
            return true;
        }).Verify();

    public static ParsableSequence IntTypeKeyword = StringSequence.Make("int").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Int32";
            return true;
        }).Verify();

    public static ParsableSequence UIntTypeKeyword = StringSequence.Make("uint").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.UInt32";
            return true;
        }).Verify();

    public static ParsableSequence LongTypeKeyword = StringSequence.Make("long").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Int64";
            return true;
        }).Verify();

    public static ParsableSequence ULongTypeKeyword = StringSequence.Make("ulong").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.UInt64";
            return true;
        }).Verify();

    public static ParsableSequence CharTypeKeyword = StringSequence.Make("char").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Char";
            return true;
        }).Verify();

    public static ParsableSequence DecimalTypeKeyword = StringSequence.Make("decimal").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Decimal";
            return true;
        }).Verify();

    public static ParsableSequence FloatTypeKeyword = StringSequence.Make("float").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Single";
            return true;
        }).Verify();

    public static ParsableSequence DoubleTypeKeyword = StringSequence.Make("double").AddValidation(
        delegate(string s, SequenceDictionary d)
        {
            d.ReadString = "System.Double";
            return true;
        }).Verify();


    //TODO: validation for all the c# sequences!

    public static ParsableSequence Type = SwitchSequence.Make(
        LazySequence.Make(() => RefType),
        LazySequence.Make(() => ValType),
        LazySequence.Make(() => TypeParam),
        LazySequence.Make(() => PointerType)
    ).Verify();

    public static ParsableSequence RefType = SwitchSequence.Make(NNRefType, NRefType).Verify();

    public static ParsableSequence NNRefType =
        SwitchSequence.Make(ClassType, InterfaceType, ArrayType, DelegateType, DynamicTypeKeyword).Verify();

    public static ParsableSequence ClassType =
        SwitchSequence.Make(TypeName, ObjectTypeKeyword, StringTypeKeyword).Verify();

    public static ParsableSequence InterfaceType = SwitchSequence.Make(TypeName).Verify();
    public static ParsableSequence DelegateType = SwitchSequence.Make(TypeName).Verify();

    public static ParsableSequence ArrayType = ParsableSequence.Make()
        .Add("tr[1.]")
        .Define('t', LazySequence.Make(() => NonArrayType))
        .Define('r', RankSpecifier)
        .Verify();

    public static ParsableSequence NonArrayType = SwitchSequence.Make(ValType, ClassType, InterfaceType, DelegateType,
        DynamicTypeKeyword, TypeParam, PointerType).Verify();

    public static ParsableSequence RankSpecifier = ParsableSequence.Make()
        .Add("o,[]c")
        .Define('o', StringSequence.Make("["))
        .Define(',', StringSequence.Make(","))
        .Define('c', StringSequence.Make("]"))
        .Verify();

    public static ParsableSequence NullableTypeAnnotation = StringSequence.Make("?").Verify();

    public static ParsableSequence NRefType = ParsableSequence.Make()
        .Add("na")
        .Define('n', NNRefType)
        .Define('a', NullableTypeAnnotation)
        .Verify();

    public static ParsableSequence ValType = SwitchSequence.Make(LazySequence.Make(() => NNValType), NValType).Verify();

    public static ParsableSequence NNValType =
        SwitchSequence.Make(LazySequence.Make(() => StructType), EnumType).Verify();

    public static ParsableSequence StructType =
        SwitchSequence.Make(TypeName, LazySequence.Make(() => SimpleType), TupleType).Verify();

    public static ParsableSequence SimpleType =
        SwitchSequence.Make(LazySequence.Make(() => NumericType), BoolTypeKeyword).Verify();


    public static ParsableSequence IntegralType = SwitchSequence.Make(SByteTypeKeyword, ByteTypeKeyword,
        ShortTypeKeyword, UShortTypeKeyword, IntTypeKeyword, UIntTypeKeyword, LongTypeKeyword, ULongTypeKeyword,
        CharTypeKeyword).Verify();

    public static ParsableSequence FloatingType = SwitchSequence.Make(FloatTypeKeyword, DoubleTypeKeyword).Verify();

    public static ParsableSequence NumericType =
        SwitchSequence.Make(IntegralType, FloatingType, DecimalTypeKeyword).Verify();

    public static ParsableSequence TupleType = ParsableSequence.Make()
        .Add("(el[1.])")
        .Define('(', StringSequence.Make("("))
        .Define(')', StringSequence.Make(")"))
        .Define('e', TupleTypeElement)
        .Define('l', ParsableSequence.Make()
            .Add(",e")
            .Define('e', TupleTypeElement)
            .Define(',', StringSequence.Make(","))
            .Verify())
        .Verify();

    public static ParsableSequence TupleTypeElement = ParsableSequence.Make()
        .Add("tn[0.1]")
        .Define('t', Type)
        .Define('n', Identifier)
        .Verify();

    public static ParsableSequence EnumType = SwitchSequence.Make(TypeName).Verify();

    public static ParsableSequence NValType = ParsableSequence.Make()
        .Add("na")
        .Define('n', NNValType)
        .Define('a', NullableTypeAnnotation)
        .Verify();


    public static ParsableSequence UnmanagedType = SwitchSequence.Make(ValType, PointerType).Verify();

    public static ParsableSequence VarReference = SwitchSequence.Make(Expression).Verify();

    public static ParsableSequence Pattern =
        SwitchSequence.Make(DeclarationPattern, ConstantPattern, VarPattern).Verify();

    public static ParsableSequence SingleVariableDesignation = SwitchSequence.Make(Identifier).Verify();

    public static ParsableSequence SimpleDesignation = SwitchSequence.Make(SingleVariableDesignation).Verify();

    public static ParsableSequence DeclarationPattern = ParsableSequence.Make()
        .Add("td")
        .Define('t', Type)
        .Define('d', SimpleDesignation)
        .Verify();

    public static ParsableSequence ConstantPattern = SwitchSequence.Make(ConstantExpression).Verify();

    public static ParsableSequence Designation = SwitchSequence.Make(SimpleDesignation).Verify();

    public static ParsableSequence VarPattern = ParsableSequence.Make()
        .Add("vd")
        .Define('v', StringSequence.Make("var"))
        .Define('d', Designation)
        .Verify();


    public static ParsableSequence ArgumentList = ParsableSequence.Make()
        .Add("as[]")
        .Define('a', Argument)
        .Define('s', ParsableSequence.Make()
            .Add(",a")
            .Define(',', StringSequence.Make(","))
            .Define('a', Argument)
            .Verify())
        .Verify();

    public static ParsableSequence Argument = ParsableSequence.Make()
        .Add("n[0.1]v")
        .Define('n', ArgumentName)
        .Define('v', ArgumentValue)
        .Verify();

    public static ParsableSequence ArgumentName = ParsableSequence.Make()
        .Add("n:")
        .Define('n', Identifier)
        .Define(':', StringSequence.Make(":"))
        .Verify();

    public static ParsableSequence ArgumentValue = SwitchSequence.Make(Expression,
            ParsableSequence.Make().Add("pr").Define('p', StringSequence.Make("in")).Define('r', VariableReference)
                .Verify(),
            ParsableSequence.Make().Add("pr").Define('p', StringSequence.Make("ref")).Define('r', VariableReference)
                .Verify(),
            ParsableSequence.Make().Add("pr").Define('p', StringSequence.Make("out")).Define('r', VariableReference)
                .Verify()
        )
        .Verify();

    //ValueOfExpression for const generics which i plant for amethyst
    //Questionable?
    public static ParsableSequence PrimaryExpression = SwitchSequence.Make(Literal, InterpolatedStringExpression,
        SimpleName, ParenthesizedExpression, TupleExpression, MemberAccess, NullConditionalMemberAccess,
        InvocationExpression, ElementAccess, NullConditionalElementAccess, ThisAccess, BaseAccess,
        PostIncrementExpression, PostDecrementExpression,
        NullForgivingExpression, ArrayCreationExpression, ObjectCreationExpression, DelegateCreationExpression,
        AnonymousObjectCreationExpression,
        TypeOfExpression, SizeOfExpression, CheckedExpression, UncheckedExpression, DefaultValueExpression,
        NameOfExpression, AnonymousMethodExpression, PointerMemberAccess, PointerElementAccess, StackAllocExpression
    ).Verify();

    public static ParsableSequence InterpolatedStringExpression = SwitchSequence
        .Make(InterpolatedRegularStringExpression, InterpolatedVerbatimStringExpression)
        .Verify();

    public static ParsableSequence InterpolatedRegularStringExpression = ParsableSequence.Make()
        .Add("sm[0.1]s[]e")
        .Define('s', InterpolatedRegularStringStart)
        .Define('m', InterpolatedRegularStringMid)
        .Define('s', ParsableSequence.Make()
            .Add("{r}m[0.1]")
            .Define('{', StringSequence.Make("{"))
            .Define('}', StringSequence.Make("}"))
            .Define('r', RegularInterpolation)
            .Define('m', InterpolatedRegularStringMid)
            .Verify())
        .Define('e', InterpolatedRegularStringEnd)
        .Verify();

    public static ParsableSequence InterpolationMinimumWidth = SwitchSequence.Make(ConstantExpression).Verify();


    public static ParsableSequence RegularInterpolation = ParsableSequence.Make()
        .Add("es[0.1]f[0.1]")
        .Define('e', Expression)
        .Define('s', ParsableSequence.Make()
            .Add(",w")
            .Define(',', StringSequence.Make(","))
            .Define('w', InterpolationMinimumWidth)
            .Verify())
        .Define('f', RegularInterpolationFormat)
        .Verify();

    public static ParsableSequence InterpolatedRegularStringStart = StringSequence.Make("$\"").Verify();

    public static ParsableSequence InterpolatedRegularStringMid = ParsableSequence.Make()
        .Add("e[1.]")
        .Define('e', InterpolatedRegularStringElement)
        .Verify();

    public static ParsableSequence RegularInterpolationFormat = ParsableSequence.Make()
        .Add(":e[1.]")
        .Define(':', StringSequence.Make(":"))
        .Define('e', InterpolatedRegularStringElement)
        .Verify();

    public static ParsableSequence InterpolatedRegularStringEnd = StringSequence.Make("\"").Verify();

    public static ParsableSequence InterpolatedRegularStringElement = SwitchSequence
        .Make(InterpolatedRegularStringCharacter, SimpleEscapeSequence, HexEscapeSequence, UnicodeEscapeSequence,
            OpenBraceEscapeSequence, CloseBraceEscapeSequence).Verify();

    public static ParsableSequence InterpolatedRegularStringCharacter = FunctionParsableSequence.Make(c =>
        c != '\"' && c != '\\' && c != '{' && c != '}' && !NewLineCharacters.Contains(c)).Verify();

    
    //TODO: CONTINUE
// interpolated verbatim string expressions
    public static ParsableSequence InterpolatedVerbatimStringExpression;
}