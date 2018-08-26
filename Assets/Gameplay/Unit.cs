using System;
using UnityEngine;

/// <summary>
/// Unit component
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Selectable))]
public class Unit : MonoBehaviour
{
    /// <summary>
    /// Minimum impulse the unit has to receive to take damage
    /// </summary>
    public const float MinDamageImpulse = 5.0f;

    /// <summary>
    /// Damage dealt per impulse unit
    /// </summary>
    public const float DamagePerImpulseUnit = 1.0f;

    /// <summary>
    /// Current health
    /// </summary>
    public int health;

    /// <summary>
    /// Maximum health
    /// </summary>
    public int maxHealth;
    
    /// <summary>
    /// Status effects applied to this unit
    /// </summary>
    public StatusEffect[] effects; //TODO think about forward compatibility
    private float[] m_EffectDischargeAccumulator;

    private void Awake()
    {
        var n = Enum.GetValues(typeof(StatusEffect.Type)).Length;
        effects = new StatusEffect[n];
        m_EffectDischargeAccumulator = new float[n];
    }

    /// <summary>
    /// Check if this unit can attack another
    /// </summary>
    public bool CanAttack(Unit other)
    {
        return other != this;
    }

    /// <summary>
    /// Heal the unit
    /// </summary>
    public void Heal(int amount)
    {
        health += amount;
        SendMessage("HealthChanged", amount, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Deal damage to the unit
    /// </summary>
    public void DealDamage(int amount, GameObject damageDealer)
    {
        amount = Mathf.Min(amount, health);
        if (amount <= 0)
        {
            return;
        }

        health -= amount;

        string dealerName = damageDealer.name;
        var dealerManifestation = damageDealer.GetComponent<EnergyManifestation>();
        if (dealerManifestation != null) dealerName = dealerManifestation.holder.ResolveOwner().name;
        LogManager.LogMessage(LogManager.Combat, "'{0}' took {1} damage from {2}.", name, amount, dealerName);

        SendMessage("HealthChanged", -amount, SendMessageOptions.DontRequireReceiver);

        //Death...
        if (health <= 0)
        {
            SendMessage("HealthDepleted", SendMessageOptions.DontRequireReceiver);
            LogManager.LogMessage(LogManager.Combat, "'{0}' died.", name);
            Gameplay.Destroy(gameObject, "no health");
        }
    }
    
    /// <summary>
    /// Get how much this object is affected by a status effect (it's intesity)
    /// </summary>
    public int GetStatusEffect(StatusEffect.Type effectType)
    {
        return effects[(int)effectType].intensity;
    }

    /// <summary>
    /// Charge a status effect for this unit
    /// </summary>
    public void StatusEffectCharge(StatusEffect.Type effectType, int intensity, int amount)
    {
        //No increase
        if (amount == 0 || intensity == 0)
        { 
            return;
        }
        
        effects[(int)effectType].intensity = intensity; //TODO fix intensity recalculation to interpolate towards provided intensity depending on amounts (immitate thermodynamics?)
        effects[(int)effectType].charge += amount; //TODO fix charge recalculation to depend on intensity sign difference (immitate thermodynamics?)
        effects[(int)effectType].type = effectType;
    }

    /// <summary>
    /// Discharge a status effect for this unit
    /// </summary>
    public void StatusEffectDischarge(int effectIdx, int amount, bool negativeOnly)
    {
        Debug.Assert(amount >= 0);

        //No decrase
        if (amount == 0)
        {
            return;
        }
        
        //Effect not applied
        if (!effects[effectIdx].isActive)
        {
            return;
        }
        
        //Non-negative effect
        if (negativeOnly && effects[effectIdx].intensity > 0)
        {
            return;
        }
        
        //Effect fully discharged
        if (amount >= effects[effectIdx].charge)
        {
            effects[effectIdx].intensity = 0;
            return;
        }

        effects[effectIdx].charge -= amount;
    }

    public Unit FindClosestEnemy(float maxDistance = float.PositiveInfinity)
    {
        return Util.FindClosestObject<Unit>(transform.position, u => CanAttack(u), maxDistance);
    }

    public Unit FindClosestFriend(float maxDistance = float.PositiveInfinity)
    {
        return Util.FindClosestObject<Unit>(transform.position, u => !CanAttack(u), maxDistance);
    }

    private void Update()
    {
        var delta = Time.deltaTime * 10.0f;

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

    /// <summary>
    /// Apply physical force to this unit (at center of mass)
    /// Any operation/action on a unit must be done through here, to account for status effects.
    /// </summary>
    public void ApplyForce(Vector3 force, ForceMode mode)
    {
        var speed = effects[(int)StatusEffect.Type.Speed];
        if (speed.intensity < 0)
        {
            force /= -speed.intensity / 10.0f;
        }
        else if (speed.intensity > 0)
        {
            force *= speed.intensity / 10.0f;
        }

        GetComponent<Rigidbody>().AddForce(force, mode);
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
}
