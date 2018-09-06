using System;
using UnityEngine;

/// <summary>
/// Unit component
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Selectable))]
public class Unit : MonoBehaviour
{
    #region Settings

    /// <summary>
    /// Minimum impulse the unit has to receive to take damage.
    /// </summary>
    public const float MinDamageImpulse = 5.0f; //TODO balance gameplay

    /// <summary>
    /// Damage dealt per impulse unit.
    /// </summary>
    public const float DamagePerImpulseUnit = 1.0f; //TODO balance energy

    #endregion

    #region Members

    /// <summary>
    /// Current health.
    /// </summary>
    public int health;

    /// <summary>
    /// Maximum health.
    /// </summary>
    public int maxHealth;
    
    /// <summary>
    /// Status effects applied to this unit.
    /// </summary>
    public StatusEffect[] effects; //TODO think about forward compatibility

    /// <summary>
    /// In case status effect discharge amount is too small to discharge at least 1 energy per frame, these accumulators fix that case.
    /// </summary>
    private float[] m_EffectDischargeAccumulator;

    /// <summary>
    /// The units wizard (if any).
    /// </summary>
    private Wizard m_Wizard;

    #endregion

    #region Combat

    /// <summary>
    /// Check if this unit can attack another.
    /// </summary>
    public bool CanAttack(Unit other)
    {
        return other != this;
    }

    /// <summary>
    /// Heal the unit.
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        amount = Mathf.Min(amount, maxHealth - health);
        health += amount;
        SendMessage("HealthChanged", amount, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Deal damage to the unit.
    /// </summary>
    public void DealDamage(int amount, GameObject damageDealer)
    {
        string dealerName = damageDealer.name;
        var dealerManifestation = damageDealer.GetComponent<EnergyManifestation>();
        if (dealerManifestation != null) dealerName = dealerManifestation.holder.ResolveOwner().name;
        DealDamage(amount, dealerName);
    }

    /// <summary>
    /// Deal damage to the unit.
    /// </summary>
    public void DealDamage(int amount, string dealerName)
    {
        amount = Mathf.Min(amount, health);
        if (amount <= 0)
        {
            return;
        }

        health -= amount;
        LogManager.LogMessage(LogManager.Combat, "'{0}' took {1} damage from {2}.", name, amount, dealerName);
        SendMessage("HealthChanged", -amount, SendMessageOptions.DontRequireReceiver);

        //Death...
        if (health <= 0)
        {
            health = 0;
            SendMessage("HealthDepleted", SendMessageOptions.DontRequireReceiver);
            LogManager.LogMessage(LogManager.Combat, "'{0}' died.", name);
            Gameplay.Destroy(gameObject, "no health");
        }
    }

    #endregion

    #region Helpers

    public Unit FindClosestEnemy(float maxDistance = float.PositiveInfinity)
    {
        return Util.FindClosestObject<Unit>(transform.position, u => CanAttack(u), maxDistance);
    }

    public Unit FindClosestFriend(float maxDistance = float.PositiveInfinity)
    {
        return Util.FindClosestObject<Unit>(transform.position, u => !CanAttack(u), maxDistance);
    }

    #endregion

    #region Status effects

    /// <summary>
    /// Get how much this object is affected by a status effect (its intesity).
    /// </summary>
    public int GetStatusEffect(StatusEffect.Type effectType)
    {
        return effects[(int)effectType].intensity;
    }

    /// <summary>
    /// Charge a status effect for this unit.
    /// </summary>
    public void StatusEffectCharge(StatusEffect.Type effectType, int intensity, int amount)
    {
        //No increase
        if (amount == 0 || intensity == 0)
        {
            return;
        }

        //TODO balance energy
        effects[(int)effectType].intensity = intensity; //TODO fix intensity recalculation to interpolate towards provided intensity depending on amounts (immitate thermodynamics?)
        effects[(int)effectType].charge += amount; //TODO fix charge recalculation to depend on intensity sign difference (immitate thermodynamics?)
        effects[(int)effectType].type = effectType;
    }

    /// <summary>
    /// Discharge a status effect for this unit.
    /// </summary>
    public void StatusEffectDischarge(int effectIdx, int amount, bool negativeOnly)
    {
        Debug.Assert(amount >= 0);

        //No decrase
        if (amount == 0)
        {
            return;
        }

        var effect = effects[effectIdx];

        //Effect not applied
        if (!effect.isActive)
        {
            return;
        }

        //Non-negative effect
        if (negativeOnly && effect.intensity > 0)
        {
            return;
        }

        amount = Mathf.Min(amount, effect.intensity);

        switch ((StatusEffect.Type)effectIdx)
        {
            case StatusEffect.Type.Speed: break; //Speed effect is handled in ApplyForce()
            case StatusEffect.Type.Health: ApplyHealthStatusEffect(effect, amount); break;
            case StatusEffect.Type.Energy: ApplyEnergyStatusEffect(effect, amount); break;
        }

        //Effect fully discharged
        if (amount >= effect.charge)
        {
            effect.intensity = 0;
            return;
        }

        effect.charge -= amount;
    }


    private Vector3 ApplySpeedStatusEffect(Vector3 force)
    {
        return ApplySpeedStatusEffect(effects[(int)StatusEffect.Type.Speed], force);
    }

    private Vector3 ApplySpeedStatusEffect(StatusEffect speedEffect, Vector3 force)
    {
        Debug.Assert(speedEffect.type == StatusEffect.Type.Speed);

        if (speedEffect.intensity < 0)
        {
            return force / (Mathf.Abs(speedEffect.intensity) / 10.0f); //TODO balance energy
        }
        else if (speedEffect.intensity > 0)
        {
            return force * (speedEffect.intensity / 10.0f); //TODO balance energy
        }

        return force;
    }

    private void ApplyHealthStatusEffect(StatusEffect healthEffect, int energyAmountUsed)
    {
        Debug.Assert(healthEffect.type == StatusEffect.Type.Health);

        if (healthEffect.intensity < 0)
        {
            DealDamage(Mathf.Abs(healthEffect.intensity) * energyAmountUsed, "Health Status Effect"); //TODO balance energy
        }
        else
        {
            Heal(healthEffect.intensity * energyAmountUsed); //TODO balance energy
        }
    }

    private void ApplyEnergyStatusEffect(StatusEffect energyEffect, int energyAmountUsed)
    {
        Debug.Assert(energyEffect.type == StatusEffect.Type.Energy);

        if (m_Wizard == null) { return; }
        var holder = m_Wizard.holder;

        if (energyEffect.intensity < 0)
        {
            var energyChange = Mathf.Min(Mathf.Abs(energyEffect.intensity) * energyAmountUsed, holder.GetEnergy()); //TODO balance energy
            holder.Decrease(energyChange);
        }
        else
        {
            var energyChange = Mathf.Min(energyEffect.intensity * energyAmountUsed, m_Wizard.maxEnergy - holder.GetEnergy()); //TODO balance energy
            holder.Increase(energyChange);
        }
    }

    #endregion

    #region Unity internal

    /// <summary>
    /// Apply physical force to this unit (at center of mass)
    /// Any operation/action on a unit must be done through here, to account for status effects.
    /// </summary>
    public void ApplyForce(Vector3 force, ForceMode mode)
    {
        force = ApplySpeedStatusEffect(force);
        GetComponent<Rigidbody>().AddForce(force, mode);
    }

    private void Awake()
    {
        var n = Enum.GetValues(typeof(StatusEffect.Type)).Length;
        effects = new StatusEffect[n];
        m_EffectDischargeAccumulator = new float[n];
    }

    private void OnEnable()
    {
        m_Wizard = GetComponent<Wizard>();
    }
    
    private void FixedUpdate()
    {
        var delta = Time.fixedDeltaTime * 10.0f;

        int n = m_EffectDischargeAccumulator.Length;
        for (int i = 0; i < n; ++i)
        {
            var amount = m_EffectDischargeAccumulator[i];
            amount += delta;
            if ((int)amount > 1)
            {
                StatusEffectDischarge(i, (int)amount, false);
                amount -= (int)amount;
            }

            m_EffectDischargeAccumulator[i] = amount;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Try take damage if the impulse is too high
        var sqrImpulseSize = collision.impulse.sqrMagnitude;
        if (sqrImpulseSize > MinDamageImpulse * MinDamageImpulse)
        {
            var impulseSize = Mathf.Sqrt(sqrImpulseSize);
            var damage = (impulseSize - MinDamageImpulse) * DamagePerImpulseUnit;
            DealDamage((int)damage, collision.gameObject);
        }
    }

    #endregion
}
