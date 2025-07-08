using System.Net.NetworkInformation;
using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Literal;

public class CharLiteral : AbstractLiteral<char>
{
    public static LexRule SingleCharacter=~new CharListRule('\'', '\\','\r','\n','\u0085','\u2028','\u2029');

    public static LexRule SimpleEscapeSequence =
        "\\'".Rule() | "\\\"" | "\\\\" | "\\0" | "\\a" | "\\b" | "\\f" | "\\n" | "\\r" | "\\t" | "\\v";
    public static LexRule HexDigit = ('0'..'9').Rule() | 'A'..'F' | 'a'..'f';
    public static LexRule HexadecimalEscapeSequence = "\\x".Rule() & HexDigit & HexDigit.Optional() & HexDigit.Optional() & HexDigit.Optional();
    public static LexRule Character = SingleCharacter | SimpleEscapeSequence | HexadecimalEscapeSequence;
    public static RuleCombo CharLiteralRule=> "\'".Rule() & Character & "\'";
    public CharLiteral()
    {
        //TODO: unicode escape sequence 
        ComplexRule = CharLiteralRule;
    }

    public override void AfterParse()
    {
        base.AfterParse();
        Value = ((RuleCombo)this.ComplexRule).Elements[1].TheRule.MyString[0];
    }
}