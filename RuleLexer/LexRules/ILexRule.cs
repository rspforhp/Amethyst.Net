namespace RuleLexer.LexRules;

public interface ILexRule<TSelf> : ILexRule where TSelf : unmanaged, ILexRule<TSelf>
{
    public interface WithValue<TValue> : ILexRule<TSelf> where TValue:unmanaged
    {
        public TValue? Value { get; set; }
    }
}

public interface ILexRule
{
    public UnmanagedString AfterRead { get; set; }

    public virtual void AfterReadMethod()
    {
    }

    public virtual void BeforeRead()
    {
    }

    [Obsolete("DONT USE THIS, USE STRINGLEXER INSTEAD",false)]
    public bool Read(ref StringLexer reader, out string read);
    public static readonly Dictionary<Type, Type> ConversionList = new()
    {
        {typeof(string),typeof(StringRule)},
        {typeof(Range),typeof(CharRangeRule)},
        {typeof(char[]),typeof(CharListRule)},
    };

    public static ILexRule Convert(object obj)
    {
        var t = obj.GetType();
        if (t.IsAssignableTo(typeof(ILexRule))) return (ILexRule)obj;
        if (ConversionList.TryGetValue(t, out var ot))
        {
            return (ILexRule)Activator.CreateInstance(ot, obj);
        }
        else return null;
    }
}

public static partial class Ext
{
    public static ILexRule Convert(this object obj)=>ILexRule.Convert(obj);
    public static T Convert<T>(this object obj) where T : unmanaged => (T)obj.Convert();
    public static T As<T>(this object obj) where T : unmanaged => (T)obj.Convert();

    public static ListRule<T1, T2> List<T1, T2>(this T1 r,T2 kill,bool canHaveZero=true) where T1 : unmanaged, ILexRule<T1> where T2 : unmanaged, ILexRule<T2>
    {
        return new ListRule<T1, T2>(r, kill,canHaveZero);
    }
    public static ListRule<T1, AlwaysFalse> List<T1>(this T1 r,bool canHaveZero=true) where T1 : unmanaged, ILexRule<T1>
    {
        return new ListRule<T1, AlwaysFalse>(r, canHaveZero);
    }
    public static SeparatedListRule<T1,T2> ListWithSeparator<T1,T2>(this T1 r, T2 sep) where T1 : unmanaged, ILexRule<T1> where T2 : unmanaged, ILexRule<T2>
    {
        return new SeparatedListRule<T1,T2>(r, sep);
    }
    public static T1 Copy<T1>(this T1 l) where T1 : unmanaged, ILexRule<T1> 
    {
        T1 t = l;
        return t;
    }
    
    public static OrRule<T1, AlwaysTrue> Optional<T1>(this T1 l) where T1 : unmanaged, ILexRule<T1> 
    {
        return new OrRule<T1, AlwaysTrue>(l, new());
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public static OrRule<T1, StringRule> Or<T1>(this T1 l, string r) where T1 : unmanaged, ILexRule<T1> 
    {
        return new OrRule<T1, StringRule>(l, (StringRule)r);
    }
    public static AndRule<T1, StringRule> And<T1>(this T1 l, string r) where T1 : unmanaged, ILexRule<T1> 
    {
        return new AndRule<T1, StringRule>(l, (StringRule)r);
    }
    public static OrRule<T1, CharRangeRule> Or<T1>(this T1 l, Range r) where T1 : unmanaged, ILexRule<T1> 
    {
        return new OrRule<T1, CharRangeRule>(l, (CharRangeRule)r);
    }
    public static AndRule<T1, CharRangeRule> And<T1>(this T1 l, Range r) where T1 : unmanaged, ILexRule<T1> 
    {
        return new AndRule<T1, CharRangeRule>(l, (CharRangeRule)r);
    }
    public static OrRule<T1, CharListRule> Or<T1>(this T1 l, char[] r) where T1 : unmanaged, ILexRule<T1> 
    {
        return new OrRule<T1, CharListRule>(l, (CharListRule)r);
    }
    public static AndRule<T1, CharListRule> And<T1>(this T1 l, char[] r) where T1 : unmanaged, ILexRule<T1> 
    {
        return new AndRule<T1, CharListRule>(l, (CharListRule)r);
    }
    
    
    public static OrRule<T1, T2> Or<T1, T2>(this T1 l, T2 r) where T1 : unmanaged, ILexRule<T1> where T2 : unmanaged, ILexRule<T2>
    {
        return new OrRule<T1, T2>(l, r);
    }
    public static AndRule<T1, T2> And<T1, T2>(this T1 l, T2 r) where T1 : unmanaged, ILexRule<T1> where T2 : unmanaged, ILexRule<T2>
    {
        return new AndRule<T1, T2>(l, r);
    }
}