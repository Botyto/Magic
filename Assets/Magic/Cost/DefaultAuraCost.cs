using UnityEngine;

public class DefaultAuraCost : IAuraCost //TODO balance energy
{
    public int StatusEffectCharge(AuraBase aura, Unit target, StatusEffect.Type effectType, int intensity, int amount)
    {
        return Mathf.Abs(intensity);
    }

    public int InflictDamage(AuraBase aura, Unit target, int damage, Energy.Element element)
    {
        return damage;
    }
}
