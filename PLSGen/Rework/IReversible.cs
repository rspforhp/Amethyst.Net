namespace PLGSGen.Rework;

public interface IReversible<T>
{
    public bool Reverse { get; set; } 
}

public static class IReversibleExtension
{
    public static T Reverse<T>(ref T val) where T :IProvideClone<T>, IReversible<T>
    {
         T reversedVal =  val.Clone();
         reversedVal.Reverse = !reversedVal.Reverse;
         return reversedVal;
    }
}