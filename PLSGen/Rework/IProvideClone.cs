namespace PLGSGen.Rework;

public interface IProvideClone<T> : IProvideClone
{
}
public interface IProvideClone
{
    public Func<object> CloningProp { get; set; }
}

public static class IProvideCloneExtension
{
    public static T Clone<T>(this IProvideClone<T> c)
    {
        return (T)c.CloningProp();
    }

    public static T Generate<T>(this Func<object> cloning) where T : IProvideClone<T>
    {
        var c = (T)cloning();
        c.CloningProp = cloning;
        return c;
    }
}