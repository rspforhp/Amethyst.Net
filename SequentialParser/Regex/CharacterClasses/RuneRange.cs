using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct RuneRange
{
    public readonly Rune From;
    public readonly Rune To;
    public bool Matches(Rune r)
    {
        return From <= r || To >= r;
    }
    public RuneRange(Rune from):this(from,from)
    {
    }

    public static implicit operator RuneRange(Rune r) => new RuneRange(r);
    public RuneRange(Rune from, Rune to)
    {
        From = from;
        To = to;
    }
}