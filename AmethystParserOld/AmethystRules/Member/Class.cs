using System.Diagnostics;
using AmethystParser.Language;
using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Member;

public sealed class Class  : LexRule
{
    public static EnumRule<Accessibility> AccessibilityRule =EnumRule<Accessibility>.Make();
    public static SeparatedListRule ClassModifiersRule=EnumRule<ClassModifiers>.Make().ListWithSeparator(" ");
    public static Identifier IdentifierRule=new();
    
    public Accessibility Accessibility;
    public List<ClassModifiers> ClassModifiers = new();
    public string Identifier;
    public Class() : base(null)
    {
        ComplexRule=OptionalVariableWhitespace & AccessibilityRule & VariableWhitespace & ClassModifiersRule & VariableWhitespace & "class" & VariableWhitespace & IdentifierRule;
    }

    public override void AfterParse()
    {
        base.AfterParse();
        Console.WriteLine();
        //AccessibilityValue
    }
}