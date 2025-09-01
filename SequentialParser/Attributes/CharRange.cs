namespace SequentialParser.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public sealed class CharRangeAttribute : Attribute
{
    public readonly char MinInc;
    public readonly char MaxInc;

    public CharRangeAttribute(char minInc, char maxInc)
    {
        MinInc = minInc;
        MaxInc = maxInc;
    }
    public bool InRange(char v)
    {
        return char.IsBetween(v, MinInc, MaxInc);
    }
}
