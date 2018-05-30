﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all types of spells.
/// Spells, when cast, are attached as components to the caster.
/// This class provides common memers and tools for implementing spells.
/// This class does implements base spell logic. See class SpellComponent.
/// When implementing a new type of generic spell - remember to catch exceptions!
/// </summary>
public class SpellComponentBase : MonoBehaviour
{
    #region Members
    
    /// <summary>
    /// Caster
    /// </summary>
    public Wizard wizard;

    /// <summary>
    /// Energy controller of caster
    /// </summary>
    public EnergyController controller;

    /// <summary>
    /// Unit of caster
    /// </summary>
    public Unit unit;

    /// <summary>
    /// Spell target
    /// </summary>
    public GameObject target;

    /// <summary>
    /// Spell parameters.
    /// Useful for tweaking reusable spell logic.
    /// This is assigned by the SpellDescriptor and should not be modified.
    /// </summary>
    public SpellParameters param;

    /// <summary>
    /// List of focused manifestations
    /// </summary>
    [SerializeField]
    private List<EnergyManifestation> m_Focus = new List<EnergyManifestation>();

    [SerializeField]
    private int m_ActiveFocusNum = 0;

    public int maxFocus = 10;

    #endregion

    #region Target interface
    
    /// <summary>
    /// Assign a new target
    /// </summary>
    public void SetTarget(GameObject newTarget)
    {
        if (target != null && newTarget == null)
        {
            target = newTarget;
            OnTargetLost();
            return;
        }

        target = newTarget;
    }

    /// <summary>
    /// Called if the spell's target has been lost
    /// </summary>
    public virtual void OnTargetLost() { }

    /// <summary>
    /// Try to find a target for this spell
    /// </summary>
    public static GameObject TryFindTarget(Wizard wizard)
    {
        return null;
    }

    #endregion

    #region Focus interface

    /// <summary>
    /// Add a manifestation to the focus list
    /// </summary>
    /// <returns>Focus handle, or -1 if not successful</returns>
    public int AddFocus(EnergyManifestation manifestation)
    {
        if (manifestation == null)
        {
            return -1;
        }

        if (!CanFocusMore())
        {
            return -1;
        }

        m_Focus.Add(manifestation);
        m_ActiveFocusNum++;
        return m_Focus.Count - 1;
    }

    /// <summary>
    /// Remove a manifestation from the focus list & disowns it
    /// </summary>
    public bool DisownFocus(EnergyManifestation manifestation)
    {
        if (manifestation == null)
        {
            return false;
        }

        return m_Focus.Remove(manifestation);
    }

    /// <summary>
    /// If another manifestation can be added to the manifestation list
    /// </summary>
    public bool CanFocusMore()
    {
        return m_ActiveFocusNum < maxFocus;
    }

    /// <summary>
    /// Check if a focused energy is still valid and can be manipulated on
    /// </summary>
    public bool IsFocusValid(int handle)
    {
        if (handle >= m_Focus.Count)
        {
            return false;
        }

        var manif = m_Focus[handle];
        return manif != null && manif.gameObject != null;
    }
    
    /// <summary>
    /// Get a focused energy by it's focus handle
    /// </summary>
    public EnergyManifestation GetFocus(int handle)
    {
        return IsFocusValid(handle) ? m_Focus[handle] : null;
    }

    /// <summary>
    /// Cancel all focused manifestations
    /// </summary>
    public void DisposeAllFocused()
    {
        foreach (var manif in m_Focus)
        {
            if (manif != null && manif.gameObject != null) //Check validity
            {
                manif.Dispose();
            }
        }

        m_ActiveFocusNum = 0;
        m_Focus.Clear();
    }

    /// <summary>
    /// Set owner of all focused energies to null
    /// </summary>
    public void DisownAllFocused()
    {
        foreach (var manif in m_Focus)
        {
            if (manif != null && manif.gameObject != null) //Check validity
            {
                manif.holder.SetOwner(null, true);
            }
        }

        m_ActiveFocusNum = 0;
        m_Focus.Clear();
    }

    /// <summary>
    /// Called if a focused energy has been lost
    /// </summary>
    public virtual void OnFocusLost(int handle) { }

    #endregion

    #region Spell tools & helpers

    /// <summary>
    /// Manually cancel/interrupt the spell.
    /// Also cancels all focused energies.
    /// </summary>
    public void Cancel(bool disposeFocused = true)
    {
        if (disposeFocused)
        {
            DisposeAllFocused();
        }

        //Util.Destroy(this, "cancelled");
        Util.Destroy(this);
    }

    /// <summary>
    /// Manually finishes the spell.
    /// Doesn't cancel any focused energies.
    /// </summary>
    public void Finish()
    {
        //Util.Destroy(this, "manual finish");
        Util.Destroy(this);
    }

    /// <summary>
    /// Unified way of strictly checking if an energy operation/action has succeeded.
    /// Strictly meaning it has fully executed and has succeeded in it's execution.
    /// </summary>
    public bool TryStrict(EnergyActionResult actionResult)
    {
        return EnergyController.TryStrict(actionResult);
    }

    /// <summary>
    /// Unified way of checking if an energy operation/action has succeeded (redundant actions are accepted).
    /// This is has a more looses meaning, compared to TryStrict(). Try() says true, in case of redundant actions.
    /// </summary>
    public bool Try(EnergyActionResult actionResult)
    {
        return EnergyController.Try(actionResult);
    }
    
    /// <summary>
    /// Handles exceptions in a uniform fashion.
    /// The calling function should assume the spell was cancelled here and should return immediately after.
    /// In all base classes, when running spell-specific code it must be wrapped in a try/catch construct
    /// </summary>
    protected virtual void HandleException(Exception exception)
    {
        MagicLog.LogErrorFormat("Spell '{0}' failed due to an exception: '{1}' (see below)", GetType().Name, exception.Message);
        MagicLog.LogException(exception);
        Cancel();
    }

    #endregion
    
    #region Unity internals

    protected virtual void OnEnable()
    {
        wizard = GetComponent<Wizard>();
        controller = GetComponent<EnergyController>();
        unit = GetComponent<Unit>();
    }
    
    protected virtual void Update()
    {
        //Handle lost target
        if ((target as object) != null && target == null)
        {
            OnTargetLost();
            target = null;
        }

        //Handle lost focus
        var sqControlRange = controller.controlRange * controller.controlRange;
        for (int handle = 0; handle < m_Focus.Count; ++handle)
        {
            //Pointers are set to null after OnFocusLost() has been called for them.
            //Otherwise they would still point to the EnergyManifestation, which is destroyed.
            if ((m_Focus[handle] as object) == null)
            {
                continue;
            }

            if (m_Focus[handle] == null)
            {
                --m_ActiveFocusNum;
                m_Focus[handle] = null;
                OnFocusLost(handle);
            }
            else
            {
                //Focused manifestation hasn't disappeared - check if still within range
                var manif = m_Focus[handle];
                if (manif.transform.position.SqrDistanceTo(controller.transform.position) > sqControlRange)
                {
                    --m_ActiveFocusNum;
                    manif.holder.SetOwner(null, true);
                    m_Focus[handle] = null;
                    OnFocusLost(handle);
                }
            }
        }
    }

    protected virtual void OnDestroy()
    {
        DisownAllFocused();
    }

    #endregion
}
