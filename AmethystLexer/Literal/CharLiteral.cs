using RuleLexer;
using RuleLexer.LexRules;

namespace AmethystLexer.Literal;

public struct CharLiteral : ILexRule<CharLiteral>.WithValue<char>
{
    public UnmanagedString AfterRead { get; set; }


    public static CharListRule SingleCharacter =
        ~new CharListRule('\'', '\\', '\r', '\n', '\u0085', '\u2028', '\u2029');

    public static
        OrRule<OrRule<
            OrRule<OrRule<
                OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>
                    , StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>
        SimpleEscapeSequence =
            "\\'".Convert<StringRule>()
                .Or((StringRule)"\\\"")
                .Or((StringRule)"\\\\")
                .Or((StringRule)"\\0")
                .Or((StringRule)"\\a")
                .Or((StringRule)"\\b")
                .Or((StringRule)"\\f")
                .Or((StringRule)"\\n")
                .Or((StringRule)"\\r")
                .Or((StringRule)"\\t")
                .Or((StringRule)"\\v");

    public static OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule> HexDigit =
        ('0'..'9').Convert<CharRangeRule>().Or((CharRangeRule)('A'..'F')).Or((CharRangeRule)('a'..'f'));

    public static AndRule<
        AndRule<AndRule<AndRule<StringRule, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>,
                OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>,
            OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>,
        OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>> HexadecimalEscapeSequence =
        "\\x".Convert<StringRule>().And(HexDigit).And(HexDigit.Optional()).And(HexDigit.Optional())
            .And(HexDigit.Optional());

    public static
        OrRule<OrRule<
                AndRule<AndRule<
                        AndRule<AndRule<StringRule, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>,
                            OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>,
                        OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>,
                    OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>, OrRule<
                    OrRule<OrRule<
                        OrRule<OrRule<
                            OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>,
                                StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>,
                    StringRule>>,
            CharListRule> Character = HexadecimalEscapeSequence.Or(SimpleEscapeSequence).Or(SingleCharacter);

    public static AndRule<AndRule<StringRule, OrRule<OrRule<
            AndRule<AndRule<
                    AndRule<AndRule<StringRule, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>,
                        OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>,
                    OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>,
                OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>, OrRule<
                OrRule<OrRule<
                    OrRule<OrRule<
                        OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>,
                                StringRule>,
                            StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>>,
        CharListRule>>,
        StringRule> CharLiteralRule => "\'".Convert<StringRule>().And(Character).And((StringRule)"\'");

    public AndRule<AndRule<StringRule, OrRule<OrRule<AndRule<AndRule<AndRule<AndRule<StringRule, OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>>, OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>, OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>, OrRule<OrRule<OrRule<CharRangeRule, CharRangeRule>, CharRangeRule>, AlwaysTrue>>, OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>>, CharListRule>>, StringRule> Rule = CharLiteralRule;

    public CharLiteral()
    {
        AfterRead = default;
        Value = null;
    }

    public bool Read(ref StringLexer reader, out string read)
    {
        return reader.Read(ref Rule, out read);
    }

    public void AfterReadMethod()
    {
        Value = Rule.Left.Right.AfterRead.Value[0];
    }

    public char? Value { get; set; }
}