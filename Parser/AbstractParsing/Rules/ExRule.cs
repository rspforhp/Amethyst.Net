using System.Numerics;
using System.Reflection;
using Parser.Attributes;

namespace Parser.Rules;


public class ExRule : AbstractRule
{
    private readonly Func<char, bool> _f;

    public ExRule(Func<char, bool> f)
    {
        _f = f;
    }

    public static implicit operator ExRule(Func<char, bool> f)
    {
        return new ExRule(f);
    }
    public static implicit operator Func<char, bool>(ExRule f)
    {
        return f._f;
    }
    public override bool IsValidChar(char ch)
    {
        return _f(ch);
    }
}