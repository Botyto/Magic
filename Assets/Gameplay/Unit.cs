using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unit component
/// </summary>
[RequireComponent(typeof(Rigidbody))]
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
    /// Status effects applied to this unit
    /// </summary>
    [SerializeField]
    public Dictionary<StatusEffect.Type, StatusEffect> effects; //TODO redesign (make serializable)

    private void Awake()
    {
        effects = new Dictionary<StatusEffect.Type, StatusEffect>();
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
        StatusEffect effect;
        //Effect not applied
        if (!effects.TryGetValue(effectType, out effect))
        {
            return 0;
        }

        return effect.intensity;
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

        StatusEffect effect;
        //Adding the effect now
        if (!effects.TryGetValue(effectType, out effect))
        {
            effect.type = effectType;
        }

        effect.intensity = intensity; //TODO fix intensity recalculation to interpolate towards provided intensity depending on amounts (immitate thermodynamics?)
        effect.charge += amount; //TODO fix charge recalculation to depend on intensity sign difference (immitate thermodynamics?)
        effects[effectType] = effect;
    }

    /// <summary>
    /// Discharge a status effect for this unit
    /// </summary>
    public void StatusEffectDischarge(StatusEffect.Type effectType, int amount, bool negativeOnly)
    {
        Debug.Assert(amount >= 0);

        //No decrase
        if (amount == 0)
        {
            return;
        }

        StatusEffect effect;
        //Effect not applied
        if (!effects.TryGetValue(effectType, out effect))
        {
            return;
        }

        //Non-negative effect
        if (negativeOnly && effect.intensity > 0)
        {
            return;
        }

        //Effect fully removed
        if (amount >= effect.charge)
        {
            effects.Remove(effectType);
            return;
        }
        
        effect.charge -= amount;
        effects[effectType] = effect;
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
        var keys = new StatusEffect.Type[effects.Keys.Count];
        effects.Keys.CopyTo(keys, 0);
        foreach (var effectType in keys)
        {
            StatusEffectDischarge(effectType, (int)(Time.deltaTime*10.0f), false);
        }
    }

    /// <summary>
    /// Apply physical force to this unit (at center of mass)
    /// Any operation/action on a unit must be done through here, to account for status effects.
    /// </summary>
    public void ApplyForce(Vector3 force, ForceMode mode)
    {
        StatusEffect speed;
        if (effects.TryGetValue(StatusEffect.Type.Speed, out speed))
        {
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
        else
        {
            GetComponent<Rigidbody>().AddForce(force, mode);
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
}
