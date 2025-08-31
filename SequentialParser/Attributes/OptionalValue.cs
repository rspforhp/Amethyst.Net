namespace SequentialParser.Attributes;

[System.AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]

public sealed class OptionalValueAttribute : Attribute
{
    public OptionalValueAttribute()
    {
    }
}