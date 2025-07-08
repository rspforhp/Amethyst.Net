using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Member;

public sealed class Identifier : LexRule, IHasValue<string>
{
    public string Value { get; set; }

    public static LexRule IdStartCharacter = Letters.NotOptional() | "_";
    public static LexRule IdCharacter = Letters.NotOptional() | "_" | Digits;
    //TODO: generic and other shit
    //TODO: @keyword stuff for id's
    public Identifier() : base(null)
    {
        ComplexRule = IdStartCharacter & IdCharacter.List();
    }

    public override void AfterParse()
    {
        Value = "";
        Value += IdStartCharacter.MyString;
        Value += IdCharacter.MyString;
    }
}