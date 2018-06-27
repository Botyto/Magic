using UnityEngine;
using UnityEngine.UI;

public class UISpellBook : Dialog
{
    public Wizard wizard;

    public void Start()
    {
        UpdateSpellEntries();
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
            var entry = Spawn<UISpellEntry>(contentHolder);
            var pos = (entry.transform as RectTransform).anchoredPosition;
            pos.y -= y;
            (entry.transform as RectTransform).anchoredPosition = pos;
            entry.descriptor = spellDescriptor;
            entry.wizard = wizard;
            y += 80.0f;
        }

        var sizeDelta = contentHolder.sizeDelta;
        sizeDelta.y = y + 20;
        contentHolder.sizeDelta = sizeDelta;
    }
}
