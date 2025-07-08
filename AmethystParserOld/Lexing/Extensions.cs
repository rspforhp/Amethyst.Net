namespace AmethystParser.Lexing;

public static class Extensions
{
    public static StringRule Rule(this string s) => s;
    public static CharRangeRule Rule(this Range s) => s;
    public static CharListRule Rule(this char[] s) => s;
}