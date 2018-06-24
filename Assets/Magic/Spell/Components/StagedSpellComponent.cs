using System;
using UnityEngine;

/// <summary>
/// This is a base class for implementing more complex spells.
/// This type of spells contain two main steps: 'Casting' and 'Execution' (and some intermediate steps).
/// Cast() will be called regularly (each frame) until the NextStage() method has been called. The same is true for Execute().
/// </summary>
public abstract class StagedSpellComponent : SpellComponent
{
    #region Members
    
    /// <summary>
    /// Stages of a spell
    /// </summary>
    public enum Stage
    {
        /// <summary>
        /// The spell hasn't started execution
        /// </summary>
        Initial,
        /// <summary>
        /// Currently in the OnBegin() method
        /// </summary>
        Beginning,
        /// <summary>
        /// Currently looping over Cast()
        /// </summary>
        Casting,
        /// <summary>
        /// Currently looping over Execute()
        /// </summary>
        Executing,
        /// <summary>
        /// Currently in the OnFinish() method
        /// </summary>
        Finishing,
        /// <summary>
        /// The spell has finished execution
        /// </summary>
        Finished,
    }

    /// <summary>
    /// Current stage
    /// </summary>
    public Stage stage;

    [SerializeField]
    private bool m_increaseStage = false;

    #endregion

    #region Spell interface

    /// <summary>
    /// Use this method when your job in Cast() or Execute() is done.
    /// </summary>
    public void NextStage()
    {
        m_increaseStage = true;
    }

    /// <summary>
    /// Called First.
    /// </summary>
    public virtual void OnBegin() { }

    /// <summary>
    /// Called regularly (each frame) until NextStage() has been called.
    /// Cylce begins after OnBegin().
    /// </summary>
    public virtual void Cast(float dt) { NextStage(); }

    /// <summary>
    /// Caled regularly (each frame) until NextStage() has been called.
    /// Cycle begins after Cast() is done.
    /// </summary>
    public virtual void Execute(float dt) { NextStage(); }

    /// <summary>
    /// Called last.
    /// </summary>
    public virtual void OnFinish() { }

    #endregion

    #region Unity internals

    protected virtual void Start()
    {
        stage = Stage.Initial;
    }

    protected virtual void LateUpdate()
    {
        if (m_increaseStage)
        {
            if (stage == Stage.Casting || stage == Stage.Executing)
            {
                ++stage;
            }

            m_increaseStage = false;
        }

        try
        {
            switch (stage)
            {
                case Stage.Initial:
                    stage = Stage.Beginning;
                    OnBegin();
                    stage = Stage.Casting;
                    break;

                case Stage.Casting:
                    Cast(Time.deltaTime);
                    break;

                case Stage.Executing:
                    Execute(Time.deltaTime);
                    break;

                case Stage.Finishing:
                    OnFinish();
                    stage = Stage.Finished;
                    //Gameplay.Destroy(this, "finished");
                    Gameplay.Destroy(this);
                    break;
            }
        }
        catch (Exception e)
        {
            HandleException(e);
            return;
        }
    }

    #endregion
}
