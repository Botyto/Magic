using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMagicCreation : MonoBehaviour
{
    Dictionary<string, string> methodImplementaions = new Dictionary<string, string>();
    string lastMethod = "";

    

    public void Start()
    {
        UpdateMethodsDropdown();
    }

    public string GetSpellType()
    {
        var dropdown = transform.Find("SpellTypeDropdown").GetComponent<Dropdown>();
        return dropdown.options[dropdown.value].text;
    }

    public string GetSpellClass()
    {
        return GetSpellType() + "Class";
    }

    public string GetMethod()
    {
        var dropdown = transform.Find("MethodDropDown").GetComponent<Dropdown>();
        var signature = dropdown.options[dropdown.value].text;
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

        var spellInput = new ScriptSpellInput();
        spellInput.name = "TestSpellName";
        spellInput.type = GetSpellType();
        spellInput.variables = new Dictionary<string, object>();
        spellInput.methods = new Dictionary<string, string>();
    }
}
