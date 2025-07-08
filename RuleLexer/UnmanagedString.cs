using System.Runtime.InteropServices;

namespace RuleLexer;

public struct UnmanagedString
{
    public IntPtr ImmutableString;

    public override string ToString()
    {
        return Value;
    }

    //~UnmanagedString()
    //{
    //    Marshal.FreeCoTaskMem(ImmutableString);
    //}

    public string Value
    {
        get => Marshal.PtrToStringAuto(ImmutableString);
        private init => ImmutableString = Marshal.StringToHGlobalAuto(value);
    }

    public static implicit operator UnmanagedString(string str) => new(str);
    public static implicit operator string(UnmanagedString str) => str.Value;

    public UnmanagedString(IntPtr immutableString)
    {
        ImmutableString = immutableString;
    }
    public UnmanagedString(string immutableString)
    {
        Value = immutableString;
    }
}