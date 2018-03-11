using UnityEngine;
using UnityEngine.UI;

public class SpellWatcher : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Wizard wizard;
    private float x = 0.0f;
    int key = 1;

    void Start()
    {
        foreach (var idx in wizard.spellOrdering)
        {
            if (idx >= 0)
            {
                var desc = wizard.spells[idx];
                AddSpell(wizard, desc);
            }
        }
    }

    void AddSpell(Wizard wizard, SpellDescriptor desc)
    {
        var btn = Instantiate(buttonPrefab);
        btn.transform.SetParent(transform);
        var pos = transform.position;
        pos.x += x;
        btn.transform.position = pos;
        x += (btn.transform as RectTransform).rect.width + 1;

        var img = btn.GetComponent<Image>();
        img.sprite = desc.icon;

        var spell = btn.GetComponent<SpellButton>();
        spell.wizard = wizard;
        spell.id = desc.id;
        
        var txt = btn.GetComponentsInChildren<Text>();

        var title = desc.displayName;
        if (desc.parameters.level > 1)
        {
            title += string.Format("({0}lvl)", desc.parameters.level);
        }
        txt[0].text = title;


        txt[1].text = key.ToString();
        key++;
    }
}
