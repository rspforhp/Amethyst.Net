using System.Reflection;

namespace Parser.Attributes;

public static class AttributeHelpers
{
    public const AttributeTargets SimpleMembers = AttributeTargets.Field | AttributeTargets.Property |
                                                  AttributeTargets.Class | AttributeTargets.Struct;
    public const AttributeTargets SimpleMembersWithMethods = SimpleMembers | AttributeTargets.Method | AttributeTargets.Constructor;

    public static bool HasAttribute<T>(this MemberInfo t) where T : Attribute
    {
        return t.GetCustomAttribute(typeof(T)) != null;
    }
}
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class GenExRuleAttribute : Attribute;

[AttributeUsage(AttributeHelpers.SimpleMembers)]
public sealed class EndsWithWhiteSpaceAttribute : Attribute;
[AttributeUsage(AttributeHelpers.SimpleMembers)]
public sealed class EndsWithSemiColonAttribute : Attribute;
[AttributeUsage(AttributeHelpers.SimpleMembers)]
public sealed class RoundScopeAttribute : Attribute;
[AttributeUsage(AttributeHelpers.SimpleMembers)]
public sealed class CurveScopeAttribute : Attribute;
[AttributeUsage(AttributeHelpers.SimpleMembers)]
public sealed class SquareScopeAttribute : Attribute;
[AttributeUsage(AttributeHelpers.SimpleMembers)]
public sealed class BirdScopeAttribute : Attribute;