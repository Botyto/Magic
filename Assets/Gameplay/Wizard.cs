using UnityEngine;

public class Wizard : MonoBehaviour
{
    public const float TargetSearchDistance = 20.0f;

    public SpellDescriptor[] spells;

    [HideInInspector]
    public int[] spellOrdering;
    
    [HideInInspector]
    public EnergyHolder holder;

    public SpellDescriptor FindSpellDescriptor(string id)
    {
        foreach (var desc in spells)
        {
            if (desc.id == id)
            {
                return desc;
            }
        }

        return null;
    }

    private void OnEnable()
    {
        holder = GetComponent<EnergyHolder>();

        spellOrdering = new int[9];
        int idx = 0;
        for (int i = 0; i < spells.Length; ++i)
        {
            if (spells[i].visible)
            {
                spellOrdering[idx] = i;
                ++idx;
                if (idx > 9)
                {
                    break;
                }
            }
        }

        for (; idx < 9; ++idx)
        {
            spellOrdering[idx] = -1;
        }
    }

    private void OnDestroy()
    {
        var spells = GetComponents<SpellComponent>();
        foreach (var spell in spells)
        {
            spell.Cancel(false);
        }
    }

    public SpellComponent CastSpell(string id, GameObject target)
    {
        //Try finding spell descriptor
        var descriptor = FindSpellDescriptor(id);
        if (descriptor == null)
        {
            return null;
        }

        //Spell already executing
        var oldSpell = GetComponent(descriptor.spellType) as SpellComponent;
        if (oldSpell != null)
        {
            //Deactivate toggle spells
            if ((oldSpell as ToggleSpellComponent) != null)
            {
                Util.Destroy(oldSpell);
            }
            return null;
        }

        //Cast the spell
        SpellComponent spell;
        var result = descriptor.Cast(this, target, out spell);
        if (result == SpellDescriptor.SpellCastResult.Success)
        {
            if (spell.target)
            {
                LogManager.LogMessage(LogManager.Combat, "'{0}' casted '{1}' at '{2}'", name, descriptor.displayName, spell.target.name);
            }
            else
            {
                LogManager.LogMessage(LogManager.Combat, "'{0}' casted '{1}'", name, descriptor.displayName);
            }

            return spell;
        }
        else //error casting the spell :)
        {
            LogManager.LogMessage(LogManager.Combat, "'{0}' failed casting '{1}': {2}", name, descriptor.displayName, result.ToString());
            return null;
        }
    }

    public void InterruptSpell(string id)
    {
        var desc = FindSpellDescriptor(id);
        if (desc == null)
        {
            return;
        }

        var comp = GetComponent(desc.spellType) as SpellComponent;
        comp.Cancel();
    }
}
