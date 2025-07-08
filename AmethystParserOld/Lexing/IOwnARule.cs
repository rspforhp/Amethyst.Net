namespace AmethystParser.Lexing;

public interface IOwnARule
{
    public abstract static LexRule Rule { get; set; }
}

public interface ICanBeRead
{
    public void Read(StringLexer l);
}