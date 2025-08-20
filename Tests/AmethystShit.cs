using SequentialParser;
using SequentialParser.Regex.CharacterClasses;

namespace Tests;

public static class AmethystShit
{
    public static AdvancedStringReader CopyHandlersAndNamesHere = new("");

    public static void DoSetup()
    {
        foreach (var fieldInfo in typeof(AmethystShit).GetFields())
        {
            if (fieldInfo.FieldType != typeof(GroupConstruct)) continue;
            GroupConstruct v = (GroupConstruct)fieldInfo.GetValue(null);
            CopyHandlersAndNamesHere = CopyHandlersAndNamesHere.AddGroup(fieldInfo.Name, v);
        }
    }
    static AmethystShit()
    {
       DoSetup();
    }


    public static GroupConstruct DEFAULT = ClassCharacter.Get<GroupConstruct>(@"(default)");
    public static GroupConstruct NULL = ClassCharacter.Get<GroupConstruct>(@"(null)");
    public static GroupConstruct TRUE = ClassCharacter.Get<GroupConstruct>(@"(true)");
    public static GroupConstruct FALSE = ClassCharacter.Get<GroupConstruct>(@"(false)");
    public static GroupConstruct ASTERISK = ClassCharacter.Get<GroupConstruct>(@"(\*)");
    public static GroupConstruct SLASH = ClassCharacter.Get<GroupConstruct>(@"(\\)");
    public static GroupConstruct input = ClassCharacter.Get<GroupConstruct>(
        @"(@input_section@?)"
    );
    public static GroupConstruct input_section = ClassCharacter.Get<GroupConstruct>(
        @"(@input_section_part@+)"
    );
    public static GroupConstruct input_section_part = ClassCharacter.Get<GroupConstruct>(
        @"((@input_element@*@New_Line@)|@PP_Directive@)"
    );
    public static GroupConstruct input_element = ClassCharacter.Get<GroupConstruct>(
        @"(@Whitespace@|@Comment@|@token@)"
    );
    public static GroupConstruct New_Line = ClassCharacter.Get<GroupConstruct>(
        @"(@New_Line_Character@|(\r\n))"
    );
    //TODO: continue parsing and fixing bugs
}