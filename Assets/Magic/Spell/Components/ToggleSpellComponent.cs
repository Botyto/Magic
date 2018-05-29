using System;
using UnityEngine;

/// <summary>
/// This is a base class for implementing spells that can be toggled on and off.
/// Such a spell is activated at a regular interval, util it has been turned off.
/// Casting this spell once will turn it on. Casting it a second time will turn it off.
/// This class is very similar to 'ContinousSpellComponent' but differs in the way Wizard handles it.
/// </summary>
public abstract class ToggleSpellComponent : SpellComponent
{
    #region Members

    /// <summary>
    /// Interval between two spell activations
    /// </summary>
    /// [SerializeField]
    public float interval = 1.0f;

    /// <summary>
    /// Keeps track of when the last activation happened
    /// </summary>
    [SerializeField]
    private float m_LastActivation;

    #endregion

    #region Spell interface

    /// <summary>
    /// Called periodically
    /// </summary>
    public abstract void Activate(float dt);

    /// <summary>
    /// Called when toggling on or off this spell
    /// </summary>
    public virtual void OnToggle(bool active) { }

    #endregion

    #region Unity internals

    protected virtual void Start()
    {
        try
        {
            OnToggle(true);
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }
    }

    protected virtual void LateUpdate()
    {
        if (Time.time - m_LastActivation >= interval)
        {
            try
            {
                Activate(interval);
            }
            catch (Exception e)
            {
                HandleException(e);
                return;
            }

            m_LastActivation = Time.time;
        }
    }

    protected override void OnDestroy()
    {
        try
        {
            OnToggle(false);
            base.OnDestroy();
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }
    }

    #endregion
}
