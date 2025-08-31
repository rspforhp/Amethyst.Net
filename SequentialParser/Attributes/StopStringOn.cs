namespace SequentialParser.Attributes;

[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class StopStringOnAttribute : Attribute
{
    public readonly char[] StopAt;

    public StopStringOnAttribute(params char[] stopAt)
    {
        StopAt = stopAt;
    }
}