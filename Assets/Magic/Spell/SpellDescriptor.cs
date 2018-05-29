using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Object used to describe a spell.
/// A wizard contains such objects in a list and needs one to cast it.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Spell", menuName = "Magic/Spell Descriptor")]
public class SpellDescriptor : ScriptableObject
{
    #region Types

    public enum SpellTargetType
    {
        /// <summary>
        /// Works with any or no target
        /// </summary>
        None,
        /// <summary>
        /// Requires a Unit as a target
        /// </summary>
        Unit,
        /// <summary>
        /// Requires an EnergyManifestation as a target
        /// </summary>
        Manifestation,
    }

    public enum SpellCastResult
    {
        /// <summary>
        /// Spell successfully cast
        /// </summary>
        Success,
        /// <summary>
        /// Invalid spell descriptor
        /// </summary>
        InvalidDescriptor,
        /// <summary>
        /// No suitable target provided/found for the spell
        /// </summary>
        NoTarget,
    }

    #endregion

    #region Technical fields

    /// <summary>
    /// Universally unique identifier
    /// </summary>
    public readonly Guid guid;

    /// <summary>
    /// Unique spell identifier
    /// </summary>
    public string id;

    /// <summary>
    /// Name of the spell class
    /// </summary>
    public string spellClass;

    /// <summary>
    /// The type of target this spell requires
    /// </summary>
    public SpellTargetType targetType;

    /// <summary>
    /// If a target is required or optional
    /// </summary>
    public bool targetRequired;

    /// <summary>
    /// Parameters passed to the spell
    /// </summary>
    public SpellParameters parameters;
    
    #endregion

    #region Visual properties

    /// <summary>
    /// Visual name of the spell
    /// </summary>
    public string displayName;

    /// <summary>
    /// Visual icon of the spell
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// If the spell is visible by default
    /// </summary>
    public bool visible = true;

    #endregion

    #region Spell info
    
    /// <summary>
    /// Get the class type of the spell to be cast
    /// </summary>
    public virtual Type spellType { get { return Type.GetType(spellClass); } }

    /// <summary>
    /// If this spell is a toggel spell type
    /// </summary>
    public bool isToggle { get { var ty = spellType; return (ty != null) ? (ty.IsSubclassOf(typeof(ToggleSpellComponent))) : false; } }

    #endregion

    #region Spell casting

    /// <summary>
    /// Cast this spell
    /// </summary>
    public virtual SpellCastResult Cast(Wizard wizard, GameObject target, out SpellComponent spell)
    {
        Debug.AssertFormat(parameters.level >= 1, "Invalid 'Level' parameter of spell '{0}'!", id);

        //Get class
        var type = spellType;
        if (type == null)
        {
            //No such class - invalid descriptor
            Debug.LogErrorFormat("Casting spell '{0}' failed! Class '{1}' not found!", id, type.Name);
            spell = null;
            return SpellCastResult.InvalidDescriptor;
        }
        
        //Try find target, if required
        if (!CheckTargetType(target))
        {
            var tryFindTargetMethod = type.GetMethod("TryFindTarget", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            target = tryFindTargetMethod != null ? tryFindTargetMethod.Invoke(null, new[] { wizard }) as GameObject : null;

            //Can't find target
            if (!CheckTargetType(target) && targetRequired)
            {
                Debug.LogErrorFormat("Casting spell '{0}' failed! Requires target, but none was found!", id);
                spell = null;
                return SpellCastResult.NoTarget;
            }
        }

        //Cast successfully
        spell = wizard.gameObject.AddComponent(type) as SpellComponent;
        spell.param = parameters;
        spell.target = target;
        return SpellCastResult.Success;
    }

    public virtual bool CheckTargetType(GameObject target)
    {
        //Anything goes
        if (targetType == SpellTargetType.None)
        {
            return target != null;
        }

        //No target - since requirement isn't 'None', then we fail
        if (target == null)
        {
            return false;
        }

        //Check different requirements
        switch (targetType)
        {
            case SpellTargetType.Unit:
                return target.GetComponent<Unit>() != null || !targetRequired;

            case SpellTargetType.Manifestation:
                return target.GetComponent<EnergyManifestation>() != null || !targetRequired;
        }

        //Invalid requirement? Fail...
        return false;
    }

    #endregion

    #region Internals

    public SpellDescriptor()
    {
        guid = Guid.NewGuid();
    }

    #endregion
}
