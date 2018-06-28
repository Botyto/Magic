using UnityEngine;
using UnityEngine.UI;

public class UISpellBook : Dialog
{
    public Wizard wizard;

    public void Start()
    {
        UpdateSpellEntries();
    }

    private UISpellEntry SpawnEntry(SpellDescriptor spellDescriptor, RectTransform parent, ref float y)
    {
        var entry = Spawn<UISpellEntry>(parent);
        var pos = (entry.transform as RectTransform).anchoredPosition;
        pos.y -= y;
        (entry.transform as RectTransform).anchoredPosition = pos;
        entry.descriptor = spellDescriptor;
        entry.wizard = wizard;
        y += 80.0f;

        return entry;
    }

    public void UpdateSpellEntries()
    {
        var contentHolder = FindRecursive<ScrollRect>("Scroll View").content;

        while (contentHolder.childCount > 0)
        {
            Gameplay.Destroy(transform.GetChild(0));
        }

        var y = 0.0f;
        foreach (var spellDescriptor in wizard.spells)
        {
            SpawnEntry(spellDescriptor, contentHolder, ref y);
        }
        foreach (var spellDescriptor in ScriptSpellDatabase.spellDescriptors)
        {
            SpawnEntry(spellDescriptor, contentHolder, ref y);
        }

        var sizeDelta = contentHolder.sizeDelta;
        sizeDelta.y = y + 20;
        contentHolder.sizeDelta = sizeDelta;
    }
}
