﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookEntry : MonoBehaviour
{
    public Wizard wizard;
    public SpellDescriptor descriptor;
    
    private int m_SpellIdx = -1;

    public void Start()
    {
        SetDescriptor(descriptor);
    }
    
    public void SetDescriptor(SpellDescriptor newDescriptor)
    {
        descriptor = newDescriptor;

        transform.FindRecursive("Icon").GetComponent<Image>().sprite = descriptor.icon;
        transform.FindRecursive("Name").GetComponent<Text>().text = descriptor.displayName + " " + descriptor.parameters.level;
        var className = (descriptor is ScriptSpellDescriptor) ? ((descriptor as ScriptSpellDescriptor).spellScriptClass) : descriptor.spellClass;
        transform.FindRecursive("Class").GetComponent<Text>().text = className;
        
        for (int i = 0; i < wizard.spells.Length; ++i)
        {
            if (wizard.spells[i] == descriptor)
            {
                m_SpellIdx = i;
                break;
            }
        }

        bool found = false;
        for (int i = 0; i < wizard.spellOrdering.Length; ++i)
        {
            if (wizard.spellOrdering[i] == m_SpellIdx)
            {
                found = true;
                break;
            }
        }
        transform.FindRecursive("Toggle").GetComponent<Toggle>().isOn = found;
    }

    public void OnToggle()
    {
        if (transform.FindRecursive("Toggle").GetComponent<Toggle>().isOn)
        {
            Add();
        }
        else
        {
            Remove();
        }
    }

    private void Add()
    {
        if (m_SpellIdx != -1)
        {
            for (int i = 0; i < wizard.spellOrdering.Length; ++i)
            {
                if (wizard.spellOrdering[i] == m_SpellIdx)
                {
                    return;
                }
            }

            var newOrdering = new int[wizard.spellOrdering.Length + 1];
            wizard.spellOrdering.CopyTo(newOrdering, 0);
            newOrdering[newOrdering.Length - 1] = m_SpellIdx;
            wizard.spellOrdering = newOrdering;

            var watcher = FindObjectOfType<SpellWatcher>();
            watcher.UpdateSpells();
        }
    }

    private void Remove()
    {
        if (m_SpellIdx != -1)
        {
            bool found = false;
            for (int i = 0; i < wizard.spellOrdering.Length; ++i)
            {
                if (wizard.spellOrdering[i] == m_SpellIdx)
                {
                    found = true;
                    break;
                }
            }
            if (!found) { return; }

            var currentArrayIdx = 0;
            var newOrdering = new int[wizard.spellOrdering.Length - 1];
            for (int i = 0; i < wizard.spellOrdering.Length; ++i)
            {
                if (wizard.spellOrdering[i] != m_SpellIdx)
                {
                    newOrdering[currentArrayIdx] = wizard.spellOrdering[i];
                    ++currentArrayIdx;
                }
            }
            wizard.spellOrdering = newOrdering;

            var watcher = FindObjectOfType<SpellWatcher>();
            watcher.UpdateSpells();
        }
    }
}
