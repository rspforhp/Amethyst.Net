using System.Text;
using UnmanageUtility;

namespace RuleLexer.LexRules;

public struct SeparatedListRule<T1,T2> : ILexRule<SeparatedListRule<T1,T2>> where T1 : unmanaged, ILexRule<T1>  where T2 : unmanaged, ILexRule<T2> 
{
    public override string ToString()
    {
        return $"{FirstElement}[{Separator}]";
    }

    public T1 FirstElement;
    public T2 Separator;
    public AndRule<T2,T1> ElementRule;
    
    
    public SeparatedListRule(T1 r,T2 sep)
    {
        FirstElement = r;
        Separator = sep;
        ElementRule = (sep.And(FirstElement));
    }

    public UnmanagedList<T1> Elements = new();
    
    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        StringBuilder b = new();
        if (!reader.Read(ref FirstElement,out var s))
        {
            read = "";
            return false;
        }
        Elements.Add(FirstElement);
        b.Append(s);
        string sOut;
        
        var e = ElementRule;
        while (reader.Read(ref e,out sOut))
        {
            Elements.Add(e.Right);
            b.Append(sOut);
            e=ElementRule;
        }
        read= b.ToString();
        return true;
    }
}