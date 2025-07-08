using System.Diagnostics;
using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Literal;

public sealed class BooleanLiteral : AbstractLiteral<bool>
{
    public static new readonly StringRule True = "true";
    public static readonly StringRule False = "false";
    public BooleanLiteral()
    {
        ComplexRule = True | False;
    }

    public override void AfterParse()
    {
        base.AfterParse();
        if (!string.IsNullOrWhiteSpace(True.MyString)) this.Value = true;
        if (!string.IsNullOrWhiteSpace(False.MyString)) this.Value = false;
    }
}