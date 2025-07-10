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
        get
        {
            if (ImmutableString > 0)
                return Marshal.PtrToStringAuto(ImmutableString);
            return "";
        }
        private init
        {
            if (value.Length > 0)
                ImmutableString = Marshal.StringToHGlobalAuto(value);
            else ImmutableString = 0;
        }
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