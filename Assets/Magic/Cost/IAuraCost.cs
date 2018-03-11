/// <summary>
/// Base interface for calculating costs of specific aura operations/actions
/// </summary>
public interface IAuraCost
{
    int InflictDamage(AuraBase aura, Unit target, int damage, Energy.Element element);
    int StatusEffectCharge(AuraBase aura, Unit target, StatusEffect.Type effectType, int intensity, int amount);
}
