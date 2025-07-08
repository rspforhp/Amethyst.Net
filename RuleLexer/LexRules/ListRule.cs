using System.Text;
using UnmanageUtility;

namespace RuleLexer.LexRules;

public struct ListRule<T1,T2> : ILexRule<ListRule<T1,T2>> where T1 : unmanaged,ILexRule<T1> where T2 : unmanaged, ILexRule<T2>
{
    
    public override string ToString()
    {
        char a = '*';
        if (!CanHaveZero) a = '+';
        return $"({ElementRule}){a}";
    }

    public readonly bool CanHaveZero;
    public T1 ElementRule;
    public  T2 KillRule;
    public ListRule(T1 rule,bool canHaveZero)
    {
        CanHaveZero = canHaveZero;
        ElementRule = rule;
        KillRule = new();
    }
    public ListRule(T1 rule,T2 kill,bool canHaveZero)
    {
        CanHaveZero = canHaveZero;
        KillRule = kill;
        ElementRule = rule;
    }
    public UnmanagedList<T1> Elements = new();

    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        StringBuilder b = new();
        bool hasElements = false;
        string sOut;
        if (reader.Read(ref KillRule,out var k))
        {
            read = k;
            return CanHaveZero;
        }
        var e = ElementRule;
        //e.Read(ref reader,out sOut)
        while (reader.Read(ref e,out sOut))
        {
            Elements.Add(e);
            
            hasElements = true;
            b.Append(sOut);
            if (reader.Read(ref KillRule,out var killed))
            {
                b.Append(killed);
                break;
            }
            e=ElementRule;
        }
        read= b.ToString();
        return hasElements  || CanHaveZero;
    }
}