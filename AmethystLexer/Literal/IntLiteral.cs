using System.Diagnostics;
using RuleLexer;
using RuleLexer.LexRules;

namespace AmethystLexer.Literal;

public struct IntLiteral : ILexRule<IntLiteral>.WithValue<Int64>
{
    public IntLiteral()
    {
    }

    public UnmanagedString AfterRead { get; set; }
    public static StringRule Underscore = "_";


    public static OrRule<StringRule, StringRule> BinaryDigit = "0".As<StringRule>().Or("1");

    public static OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<StringRule, StringRule>> DecoratedBinaryDigit =
        Underscore.List().Or(BinaryDigit);

    public static CharRangeRule DecimalDigit = ('0'..'9').Convert<CharRangeRule>();

    public static OrRule<ListRule<StringRule, AlwaysFalse>, CharRangeRule> DecoratedDecimalDigit =
        Underscore.List().Or(DecimalDigit);

    public static OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule> HexDigit =
        ('0'..'9').As<CharRangeRule>().Or('a'..'f').Or('A'..'F');

    public static OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>
        DecoratedHexDigit = Underscore.List().Or(HexDigit);

    public static
        OrRule<OrRule<
            OrRule<OrRule<
                OrRule<OrRule<
                    OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>,
                        StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>
        TypeSuffix = "U".Convert<StringRule>().Or("u").Or("L").Or("l").Or("UL").Or("Ul").Or("uL").Or("ul")
            .Or("LU").Or("Lu").Or("lU").Or("lu");

    public static
        AndRule<
            AndRule<OrRule<StringRule, StringRule>, ListRule<
                OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<StringRule, StringRule>>, AlwaysFalse>>, OrRule<
                OrRule<OrRule<
                    OrRule<OrRule<
                        OrRule<OrRule<
                            OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>,
                                StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>,
                    StringRule>, StringRule>, AlwaysTrue>> BinaryLiteral =
            ("0b".As<StringRule>().Or("0B"))
            .And(DecoratedBinaryDigit.List(false))
            .And(TypeSuffix.Optional());

    public static AndRule<
        AndRule<OrRule<StringRule, StringRule>, ListRule<
            OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>,
            AlwaysFalse>>, OrRule<OrRule<
            OrRule<OrRule<
                OrRule<OrRule<
                    OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>,
                        StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>,
            StringRule>, AlwaysTrue>> HexLiteral =
        ("0x".As<StringRule>().Or("0X"))
        .And(DecoratedHexDigit.List(false))
        .And(TypeSuffix.Optional());

    public static
        AndRule<AndRule<CharRangeRule, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, CharRangeRule>, AlwaysFalse>>,
            OrRule<OrRule<
                OrRule<OrRule<
                    OrRule<OrRule<
                        OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>,
                            StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>,
                StringRule>, AlwaysTrue>> DecimalLiteral =
            DecimalDigit.And(DecoratedDecimalDigit.List()).And(TypeSuffix.Optional());

    public static OrRule<OrRule<AndRule<AndRule<CharRangeRule, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, CharRangeRule>, AlwaysFalse>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, AlwaysTrue>>, AndRule<AndRule<OrRule<StringRule, StringRule>, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>, AlwaysFalse>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, AlwaysTrue>>>, AndRule<AndRule<OrRule<StringRule, StringRule>, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<StringRule, StringRule>>, AlwaysFalse>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, AlwaysTrue>>> IntRule = DecimalLiteral.Or(HexLiteral).Or(BinaryLiteral);
    public OrRule<OrRule<AndRule<AndRule<CharRangeRule, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, CharRangeRule>, AlwaysFalse>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, AlwaysTrue>>, AndRule<AndRule<OrRule<StringRule, StringRule>, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>, AlwaysFalse>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, AlwaysTrue>>>, AndRule<AndRule<OrRule<StringRule, StringRule>, ListRule<OrRule<ListRule<StringRule, AlwaysFalse>, OrRule<StringRule, StringRule>>, AlwaysFalse>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, AlwaysTrue>>> Rule = IntRule;

    public bool Read(ref StringLexer reader, out string read)
    {
        return reader.Read(ref Rule, out read);
    }

    public void AfterReadMethod()
    {
        
        Debugger.Break();
    }

    public bool Long;
    public bool Unsigned;
    public Int64? Value { get; set; }
}