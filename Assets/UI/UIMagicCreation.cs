using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class UIMagicCreation : Dialog
{
    private ScriptSpellInput m_EditedSpellInput = null;
    private Dictionary<string, string> m_MethodImplementaions = new Dictionary<string, string>();
    private string m_LastMethod = "";
    
    public void Start()
    {
        var dropdown = FindRecursive<Dropdown>("ScriptsDropdown");
        dropdown.AddOptions(ScriptSpellDatabase.spells.Keys.ToList());
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
        var input = FindRecursive<InputField>("CodeField");
        return input.text;
    }

    public void SetCode(string newCode)
    {
        var input = FindRecursive<InputField>("CodeField");
        input.text = newCode;
    }
    
    public void OnEndEditCodeField(string code)
    {
        //methodImplementaions[lastMethod] = GetCode();
        //var input = GenerateScriptInput();
        //var classCode = input.GenerateCode();

        //var env = new ScriptEnvironment(code);
    }

    public void UpdateMethodsDropdown()
    {
        var dropdown = FindRecursive<Dropdown>("MethodDropDown");
        dropdown.AddOptions(ScriptSpellDatabase.GetMethodsList(GetSpellClass()).ConvertAll(md => md.signature));
    }
    
    public void OnUpdateSpellType()
    {
        var newImplementations = new Dictionary<string, string>();

        foreach (var method in ScriptSpellDatabase.GetMethodsList(GetSpellClass()))
        {
            newImplementations.Add(method.name, m_MethodImplementaions.TryGetValue(method.name, ""));
        }

        m_MethodImplementaions = newImplementations;
        UpdateMethodsDropdown();
    }

    public void OnUpdateMethod()
    {
        m_MethodImplementaions[m_LastMethod] = GetCode();
        var currMethod = GetMethod();
        m_LastMethod = currMethod;
        SetCode(m_MethodImplementaions.TryGetValue(currMethod, ""));
    }

    public ScriptSpellInput GenerateScriptInput()
    {
        var spellInput = new ScriptSpellInput();
        FillScriptInput(spellInput);
        return spellInput;
    }

    public void FillScriptInput(ScriptSpellInput spellInput)
    {
        m_MethodImplementaions.Remove(""); //TODO fix this
        var targetString = FindRecursive<Dropdown>("Input_TargetType").GetSelectionText();
        var targetType = SpellDescriptor.SpellTargetType.None;
        switch (targetString)
        {
            case "Unit": targetType = SpellDescriptor.SpellTargetType.Unit; break;
            case "Manifestation": targetType = SpellDescriptor.SpellTargetType.Manifestation; break;
        }

        spellInput.id = FindRecursive<InputField>("Input_ID").text;
        spellInput.targetRequired = FindRecursive<Toggle>("Input_RequireTarget").isOn;
        spellInput.displayName = FindRecursive<InputField>("Input_DisplayName").text;
        spellInput.targetType = targetType;
        spellInput.type = GetSpellType();
        spellInput.variables = new Dictionary<string, object>();
        spellInput.methods = m_MethodImplementaions;
    }

    public void OnConfirm()
    {
        m_MethodImplementaions[m_LastMethod] = GetCode();

        if (m_EditedSpellInput == null)
        {
            ScriptSpellDatabase.AddSpellImplementation(GenerateScriptInput());
        }
        else
        {
            FillScriptInput(m_EditedSpellInput);
            ScriptSpellDatabase.SaveSpellImplementation(m_EditedSpellInput);
        }
    }

    public void OnDeleteScript()
    {
        var dropdown = FindRecursive<Dropdown>("ScriptsDropdown");
        ScriptSpellDatabase.RemoveSpellImplementation(dropdown.GetSelectionText());
        dropdown.options.RemoveAt(dropdown.value);
    }

    public void OnEditScript()
    {
        FindRecursive("ManageScripts").gameObject.SetActive(false);
        FindRecursive("NewScript").gameObject.SetActive(true);

        var dropdown = FindRecursive<Dropdown>("ScriptsDropdown");
        m_EditedSpellInput = ScriptSpellDatabase.spells[dropdown.options[dropdown.value].text];

        FindRecursive<InputField>("Input_ID").text = m_EditedSpellInput.id;
        FindRecursive<Toggle>("Input_RequireTarget").isOn = m_EditedSpellInput.targetRequired;
        FindRecursive<InputField>("Input_DisplayName").text = m_EditedSpellInput.displayName;

        var targetDropdown = FindRecursive<Dropdown>("Input_TargetType");
        switch (m_EditedSpellInput.targetType)
        {
            case SpellDescriptor.SpellTargetType.Manifestation: targetDropdown.value = targetDropdown.options.FindIndex(o => o.text == "Manifestation"); break;
            case SpellDescriptor.SpellTargetType.Unit: targetDropdown.value = targetDropdown.options.FindIndex(o => o.text == "Unit"); break;
            case SpellDescriptor.SpellTargetType.None: targetDropdown.value = targetDropdown.options.FindIndex(o => o.text == "None"); break;
            default: targetDropdown.value = targetDropdown.options.FindIndex(o => o.text == "None"); break;
        }

        var spellTypeDropdown = FindRecursive<Dropdown>("SpellTypeDropdown");
        spellTypeDropdown.value = spellTypeDropdown.options.FindIndex(o => o.text == m_EditedSpellInput.type);

        //new Dictionary<string, object>() = m_EditedSpellInput.variables;
        m_MethodImplementaions = m_EditedSpellInput.methods;
    }

    public void OnNewScript()
    {
        FindRecursive("ManageScripts").gameObject.SetActive(false);
        FindRecursive("NewScript").gameObject.SetActive(true);
        UpdateMethodsDropdown();
    }
}
