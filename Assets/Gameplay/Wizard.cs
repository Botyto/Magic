using UnityEngine;

public class Wizard : MonoBehaviour
{
    /// <summary>
    /// Maximum distance for finding a spell target
    /// </summary>
    public const float TargetSearchDistance = 20.0f;

    #region Members

    /// <summary>
    /// List of spells this wizard knows.
    /// </summary>
    public SpellDescriptor[] spells;

    /// <summary>
    /// Amount of energy regenerated each frame.
    /// </summary>
    public float energyRegenerationPerFrame = 0.5f;

    /// <summary>
    /// Maximum energy.
    /// </summary>
    public int maxEnergy;
    
    /// <summary>
    /// UI ordering of the spells.
    /// </summary>
    [HideInInspector]
    public int[] spellOrdering;
    
    /// <summary>
    /// Wizard's energy holder.
    /// </summary>
    [HideInInspector]
    public EnergyHolder holder;

    /// <summary>
    /// In case 'energyRegenerationSpeed' is too small to add at least 1 energy per frame, this accumulator fixes that case.
    /// </summary>
    private float m_EnergyRegenerationAccumulator = 0.0f;

    #endregion
    
    #region Spell casting

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

    /// <summary>
    /// Cancels a currently active spell.
    /// </summary>
    /// <returns>If the spell has been interrupted.</returns>
    public bool CancelSpell(string id)
    {
        //Try finding spell descriptor
        var descriptor = FindSpellDescriptor(id);
        if (descriptor == null)
        {
            return false;
        }

        //Spell already executing
        var spell = GetComponent(descriptor.spellType) as SpellComponent;
        if (spell == null)
        {
            return false;
        }

        //Cancel the spell
        Gameplay.Destroy(spell);

        return true;
    }

    /// <summary>
    /// Cast a spell.
    /// </summary>
    /// <returns>The casted spell or null.</returns>
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
            if (oldSpell is ToggleSpellComponent)
            {
                //Destroying a toggle spell will internally call its OnToggle()
                Gameplay.Destroy(oldSpell);
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
        else //error casting the spell
        {
            LogManager.LogMessage(LogManager.Combat, "'{0}' failed to cast '{1}': {2}", name, descriptor.displayName, result.ToString());
            return null;
        }
    }
    
    /// <summary>
    /// Returns true if the spell is currently active.
    /// </summary>
    public bool IsSpellActive(string id)
    {
        //Try to find the spell descriptor
        var descriptor = FindSpellDescriptor(id);
        if (descriptor == null)
        {
            return false;
        }

        //Get the currently active spell component
        var spell = GetComponent(descriptor.spellType) as SpellComponent;
        if (spell == null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Unity internals

    private void OnEnable()
    {
        holder = GetComponent<EnergyHolder>();

        var visibleCount = 0;
        foreach (var spell in spells)
        {
            if (spell.visible)
            {
                ++visibleCount;
            }
        }

        spellOrdering = new int[visibleCount];
        int idx = 0;
        for (int i = 0; i < spells.Length; ++i)
        {
            if (idx >= visibleCount) { break; }
            if (spells[i].visible)
            {
                spellOrdering[idx] = i;
                ++idx;
            }
        }

        for (; idx < visibleCount; ++idx)
        {
            spellOrdering[idx] = -1;
        }
    }

    private void FixedUpdate()
    {
        //Regenerate energy
        if (holder.GetEnergy() < maxEnergy)
        {
            m_EnergyRegenerationAccumulator += energyRegenerationPerFrame;
            if (m_EnergyRegenerationAccumulator > 1.0f)
            {
                var energyAdded = Mathf.FloorToInt(m_EnergyRegenerationAccumulator);
                holder.Increase(energyAdded);
                m_EnergyRegenerationAccumulator -= energyAdded;
            }
        }
    }

    private void OnDestroy()
    {
        //Cancel all spells and disown all focused energies
        var spells = GetComponents<SpellComponent>();
        foreach (var spell in spells)
        {
            spell.Cancel(false);
        }
    }

    #endregion
}
