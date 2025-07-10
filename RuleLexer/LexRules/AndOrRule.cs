namespace RuleLexer.LexRules;


public struct AndRule<T1,T2> : ILexRule<AndRule<T1,T2>> where T1 : unmanaged,ILexRule<T1> where T2 : unmanaged,ILexRule<T2>
{
    public T1 Left;
    public T2 Right;
    public AndRule(T1 l,T2 r) 
    {
        Left = l;
        Right = r;
    }
    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        var lb = reader.Read(ref Left,out var ls);
        if (!lb)
        {
            read="";
            return false;
        }
        var rb = reader.Read(ref Right,out var rs);
        if (rb)
        {
            read = ls+rs;
            return true;
        }
        read = "";
        return false;
    }
    public override string ToString()
    {
        return $"{Left} & {Right}";
    }
}

public struct OrRule<T1,T2> : ILexRule<OrRule<T1,T2>> where T1 : unmanaged,ILexRule<T1> where T2 : unmanaged,ILexRule<T2>
{
    public T1 Left;
    public T2 Right;
    public OrRule(T1 l,T2 r) 
    {
        Left = l;
        Right = r;
    }

    public override string ToString()
    {
        return $"{Left} | {Right}";
    }

    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        var lb = reader.Read(ref Left,out var ls);
        if (lb)
        {
            read=ls;
            return true;
        }
        var rb = reader.Read(ref Right,out var rs);
        if (rb)
        {
            read = rs;
            return true;
        }
        read = "";
        return false;
    }
}
