using System;
using UnityEngine.UI;

public class UISpellDescriptorCreation : Dialog
{
    public void Start()
    {
        UpdateSpellClasses();
    }

    public void UpdateSpellClasses()
    {
        var dropdown = FindRecursive<Dropdown>("SpellClass_Choice");
        dropdown.options.Clear();
        foreach (var entry in ScriptSpellDatabase.Spells)
        {
            dropdown.options.Add(new Dropdown.OptionData(entry.Key));
        }

        InitDropdownFromEnum("Element_Choice", typeof(Energy.Element));
        InitDropdownFromEnum("Shape_Choice", typeof(Energy.Shape));
    }

    public void InitDropdownFromEnum(string name, Type enumType)
    {
        var dropdown = FindRecursive<Dropdown>(name);
        dropdown.ClearOptions();
        foreach (var entry in Enum.GetNames(enumType))
        {
            dropdown.options.Add(new Dropdown.OptionData(entry));
        }
        
        dropdown.value = 0;
    }

    public void OnConfirm()
    {
        var spellClass = FindRecursive<Dropdown>("SpellClass_Choice").GetSelectionText();
        var spellInput = ScriptSpellDatabase.Spells[spellClass];

        var descr = new ScriptSpellDescriptor();
        descr.id = spellInput.id;
        descr.spellClass = spellInput.nativeBaseClass;
        descr.targetType = spellInput.targetType;
        descr.targetRequired = spellInput.targetRequired;
        descr.parameters = new SpellParameters
        {
            level = int.Parse(FindRecursive<InputField>("Level_Input").text),
            element = (Energy.Element)Enum.Parse(typeof(Energy.Element), FindRecursive<Dropdown>("Element_Choice").GetSelectionText()),
            shape = (Energy.Shape)Enum.Parse(typeof(Energy.Shape), FindRecursive<Dropdown>("Shape_Choice").GetSelectionText()),
        };
        descr.displayName = spellInput.displayName;
        descr.spellScriptClass = spellInput.className;

        ScriptSpellDatabase.SpellDescriptors.Add(descr);
    }
}
