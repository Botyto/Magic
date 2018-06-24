using UnityEngine;
using UnityEngine.UI;

public class SpellBookManager : MonoBehaviour
{
    public Wizard wizard;
    public GameObject spellEntryPrefab;

    public void Start()
    {
        UpdateSpellEntries();
    }

    public void UpdateSpellEntries()
    {
        var contentHolder = transform.Find("Scroll View").GetComponent<ScrollRect>().content;

        while (contentHolder.childCount > 0)
        {
            Gameplay.Destroy(transform.GetChild(0));
        }

        var y = 0.0f;
        foreach (var spellDescriptor in wizard.spells)
        {
            var entry = Instantiate(spellEntryPrefab, contentHolder);
            var pos = (entry.transform as RectTransform).anchoredPosition;
            pos.y -= y;
            (entry.transform as RectTransform).anchoredPosition = pos;
            var entryComponent = entry.GetComponent<SpellBookEntry>();
            entryComponent.descriptor = spellDescriptor;
            entryComponent.wizard = wizard;
            y += 80.0f;
        }

        var sizeDelta = contentHolder.sizeDelta;
        sizeDelta.y = y + 20;
        contentHolder.sizeDelta = sizeDelta;
    }

    public void CloseSpellBook()
    {
        Gameplay.Destroy(gameObject);
    }
}
