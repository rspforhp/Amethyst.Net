using AmethystParser.Lexing;

namespace AmethystParser.AmethystRules.Literal;

public abstract class AbstractLiteral<T> : LexRule,IHasValue<T>
{
    public AbstractLiteral() : base(null)
    {
    }

    public T? Value { get;  set; }

}