using System;
using UnityEngine;
using UnityEngine.UI;

public class UISpellDescriptorCreation : Dialog
{
    public Text codeReviewText;

    public void Start()
    {
        UpdateSpellClasses();
    }

    public void UpdateSpellClasses()
    {
        var dropdown = FindRecursive<Dropdown>("SpellClass_Choice");
        dropdown.options.Clear();
        foreach (var entry in ScriptSpellDatabase.spells)
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
        var spellInput = ScriptSpellDatabase.spells[spellClass];

        var descr = ScriptableObject.CreateInstance<ScriptSpellDescriptor>();
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

        ScriptSpellDatabase.AddSpellDescriptor(descr);
    }

    public void OnViewCode()
    {
        var spellClass = FindRecursive<Dropdown>("SpellClass_Choice").GetSelectionText();
        var spellDef = ScriptSpellDatabase.spells.TryGetValue(spellClass, null);
        if (spellDef == null)
        {
            OnCloseCode();
            return;
        }

        FindRecursive("CodeViewOverlay").gameObject.SetActive(true);
        var code = spellDef.GenerateCode();

        TextGenerator textGenenerator = new TextGenerator();
        TextGenerationSettings generationSettings = codeReviewText.GetGenerationSettings(codeReviewText.rectTransform.rect.size);
        float height = textGenenerator.GetPreferredHeight(code, generationSettings);
        var sz = codeReviewText.rectTransform.sizeDelta;
        sz.y = height;
        codeReviewText.rectTransform.sizeDelta = sz;
        codeReviewText.text = code;
    }

    public void OnCloseCode()
    {
        FindRecursive("CodeViewOverlay").gameObject.SetActive(false);
    }
}
