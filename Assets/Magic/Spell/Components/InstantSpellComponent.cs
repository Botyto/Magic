using System;
using UnityEngine;

/// <summary>
/// This is a base class for implementing an instant spell.
/// It only contains one method - Cast(), which is called only once, and then the spell is finished.
/// </summary>
public abstract class InstantSpellComponent : SpellComponent
{
    #region Spell interface

    /// <summary>
    /// Called when this spell is being cast.
    /// </summary>
    public abstract void Cast();

    #endregion

    #region Unity internals
    
    protected virtual void LateUpdate()
    {
        try
        {
            Cast();
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }

        //Gameplay.Destroy(this, "finished");
        Gameplay.Destroy(this);
    }

    #endregion
}
