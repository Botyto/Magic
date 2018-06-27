using System.Collections.Generic;
using UnityEngine.UI;

public class UIMagicCreation : Dialog
{
    Dictionary<string, string> methodImplementaions = new Dictionary<string, string>();
    string lastMethod = "";
    
    public void Start()
    {
        UpdateMethodsDropdown();
    }

    public string GetSpellType()
    {
        return FindRecursive<Dropdown>("SpellTypeDropdown").GetSelectionText();
    }

    public string GetSpellClass()
    {
        return GetSpellType() + "Spell";
    }

    public string GetMethod()
    {
        var signature = FindRecursive<Dropdown>("MethodDropDown").GetSelectionText();
        return signature.Substring(0, signature.IndexOf('('));
    }

    public string GetCode()
    {
        var input = transform.Find("CodeField").GetComponent<InputField>();
        return input.text;
    }

    public void SetCode(string newCode)
    {
        var input = transform.Find("CodeField").GetComponent<InputField>();
        input.text = newCode;
    }
    
    public void UpdateMethodsDropdown()
    {
        var dropdown = transform.Find("MethodDropDown").GetComponent<Dropdown>();
        dropdown.options = new List<Dropdown.OptionData>();

        foreach (var method in ScriptSpellDatabase.GetMethodsList(GetSpellClass()))
        {
            dropdown.options.Add(new Dropdown.OptionData(method.Signature));
        }

        dropdown.value = 0;
    }
    
    public void OnUpdateSpellType()
    {
        var newImplementations = new Dictionary<string, string>();

        foreach (var method in ScriptSpellDatabase.GetMethodsList(GetSpellClass()))
        {
            newImplementations.Add(method.name, methodImplementaions.TryGetValue(method.name, ""));
        }

        methodImplementaions = newImplementations;
        UpdateMethodsDropdown();
    }

    public void OnUpdateMethod()
    {
        methodImplementaions[lastMethod] = GetCode();
        var currMethod = GetMethod();
        lastMethod = currMethod;
        SetCode(methodImplementaions.TryGetValue(currMethod, ""));
    }

    public void OnConfirm()
    {
        methodImplementaions[lastMethod] = GetCode();
        
        var targetString = FindRecursive<Dropdown>("Input_TargetType").GetSelectionText();
        var targetType = SpellDescriptor.SpellTargetType.None;
        switch (targetString)
        {
            case "Unit":          targetType = SpellDescriptor.SpellTargetType.Unit;          break;
            case "Manifestation": targetType = SpellDescriptor.SpellTargetType.Manifestation; break;
        }

        var spellInput = new ScriptSpellInput();
        spellInput.id = FindRecursive<InputField>("Input_ID").text;
        spellInput.targetRequired = FindRecursive<Toggle>("Input_RequireTarget").isOn;
        spellInput.displayName = FindRecursive<InputField>("Input_DisplayName").text;
        spellInput.targetType = targetType;
        spellInput.type = GetSpellType();
        spellInput.variables = new Dictionary<string, object>();
        spellInput.methods = new Dictionary<string, string>();

        ScriptSpellDatabase.Spells.Add(spellInput.id, spellInput);
    }
}
