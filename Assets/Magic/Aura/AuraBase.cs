using UnityEngine;

public abstract class AuraBase : EnergyUser
{
    #region Members

    /// <summary>
    /// Interval (in seconds) at which Activte() will be called
    /// </summary>
    public float interval = 1.0f;

    /// <summary>
    /// Aura target (read-only)
    /// </summary>
    public GameObject target;

    /// <summary>
    /// Used for calculating action costs
    /// </summary>
    public IAuraCost cost { get; private set; }

    /// <summary>
    /// The target's unit
    /// </summary>
    public Unit unit { get { return target.GetComponent<Unit>(); } }

    #endregion

    #region Aura interface

    /// <summary>
    /// Called once when the aura is initially applied
    /// </summary>
    protected virtual void OnApply() { }

    /// <summary>
    /// Called once when the aura expires (or is cancelled manually).
    /// Aura expires when it's out of energy or when it doesn't consume any energy on activation.
    /// </summary>
    protected virtual void OnExpire() { }

    /// <summary>
    /// Called before destroying this object, if that target has been lost.
    /// Aura object will be destroyed immediately after.
    /// </summary>
    protected virtual void OnTargetLost() { }

    /// <summary>
    /// Called at regular intervals (see 'interval' member)
    /// Energy must be used on each call to keep the aura from expiring
    /// </summary>
    /// <param name="dt">Delta time before last call (in seconds)</param>
    protected abstract void Activate(float dt);

    /// <summary>
    /// Instantly cancel the aura, causing it to expire
    /// </summary>
    public void Cancel()
    {
        Gameplay.Destroy(gameObject, "canceled");
    }

    #endregion

    #region Aura actions

    /// <summary>
    /// Inflict damage to a unit
    /// </summary>
    public EnergyActionResult InflictDamage(int damage, Energy.Element element)
    {
        //Check cost
        int totalCost = cost.InflictDamage(this, unit, damage, element);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }
        
        //Actual implementation
        switch (element)
        {
            case Energy.Element.Raw:
            case Energy.Element.Mana:
                unit.DealDamage(damage, gameObject);
                break;

            case Energy.Element.Fire:
                break;

            case Energy.Element.Ice:
                unit.DealDamage(damage / 5, gameObject);
                unit.StatusEffectCharge(StatusEffect.Type.Speed, -100, damage);
                break;
        }

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    public EnergyActionResult StatusEffectCharge(StatusEffect.Type effectType, int intensity, int amount)
    {
        //Check cost
        int totalCost = cost.StatusEffectCharge(this, unit, effectType, intensity, amount);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }
        
        //Actual implementation
        unit.StatusEffectCharge(effectType, intensity, amount);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    #endregion

    #region Unity internals

    /// <summary>
    /// Last activateion time
    /// </summary>
    private float m_LastActivation = 0.0f;

    protected override void Awake()
    {
        base.Awake();
        cost = new DefaultAuraCost();
    }

    private void Start()
    {
        m_LastActivation = Time.time;
        OnApply();
        Debug.Assert(target != null);
    }

    private void EnergyDepleted()
    {
        Gameplay.Destroy(gameObject, "energy depleted");
    }
    
    private void Update()
    {
        //TODO implement limits on interval?

        //Target lost
        if (target == null)
        {
            OnTargetLost();
            Gameplay.Destroy(gameObject, "target lost");
            return;
        }
        
        if (Time.time - m_LastActivation >= interval)
        {
            //TODO implement energy decrease if interval is too long

            var oldEnergy = GetEnergy();
            Activate(interval);
            m_LastActivation = Time.time;
            if (GetEnergy() == oldEnergy)
            {
                //Every aura tick must use energy
                Gameplay.Destroy(gameObject, "expired");
            }
        }
    }

    private void OnDestroy()
    {
        OnExpire();
    }

    #endregion
}
