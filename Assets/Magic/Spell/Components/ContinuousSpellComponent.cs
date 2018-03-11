using System;
using UnityEngine;

/// <summary>
/// This is a base class for implementing contious spells.
/// Such spells are activated each frame until Finish()/Cancel() has been called.
/// This class is very similar to 'ToggleSpellComponent' but differs in the way Wizard handles it.
/// </summary>
public abstract class ContinuousSpellComponent : SpellComponent
{
    #region Spell interface

    /// <summary>
    /// Called when beginning the spell
    /// </summary>
    public virtual void OnBegin() { }

    /// <summary>
    /// Called regularly (each frame)
    /// </summary>
    public abstract void Activate(float dt);

    /// <summary>
    /// Called when finishing the spell
    /// </summary>
    public virtual void OnFinish() { }

    #endregion

    protected virtual void Start()
    {
        try
        {
            OnBegin();
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }
    }

    protected virtual void LateUpdate()
    {
        try
        {
            Activate(Time.deltaTime);
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }
    }

    protected override void OnDestroy()
    {
        try
        {
            OnFinish();
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }
    }
}
