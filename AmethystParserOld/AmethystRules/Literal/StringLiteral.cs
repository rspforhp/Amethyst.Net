using System.Net.NetworkInformation;
using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Literal;

public class StringLiteral : AbstractLiteral<string>
{
    public LexRule SingleVerbatimStringLiteralCharacter = ~new CharListRule('\"');
    public LexRule QuoteEscapeSequence = "\"\"".Rule();
    public LexRule VerbatimStringLiteralCharacter;
    public LexRule VerbatimStringLiteral;
    public LexRule SingleRegularStringLiteralCharacter=~new CharListRule('\"', '\\','\r','\n','\u0085','\u2028','\u2029');
    public LexRule RegularStringLiteralCharacter;
    public LexRule RegularStringLiteral;
  
    
    public StringLiteral()
    {
        VerbatimStringLiteralCharacter = SingleVerbatimStringLiteralCharacter | QuoteEscapeSequence;
        VerbatimStringLiteral="@\"".Rule() & VerbatimStringLiteralCharacter.List() &  "\"";
        //TODO: unicode escape sequence
        RegularStringLiteralCharacter = SingleRegularStringLiteralCharacter | CharLiteral.SimpleEscapeSequence |
                                        CharLiteral.HexadecimalEscapeSequence;
        RegularStringLiteral = "\"".Rule() & RegularStringLiteralCharacter.List() & "\"";
        ComplexRule = RegularStringLiteral | VerbatimStringLiteral;
    }

    public override void AfterParse()
    {
        base.AfterParse();
        Value = RegularStringLiteral.MyString[1..^1] ?? VerbatimStringLiteral.MyString[2..^1];
    }
    //TODO: make the rules structs so it would be easier to handle shit
}