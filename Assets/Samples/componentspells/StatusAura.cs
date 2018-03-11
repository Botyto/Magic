public class StatusAura : AuraBase
{
    public StatusEffect.Type type;
    
    protected override void OnApply()
    {
        StatusEffectCharge(type, -GetEnergy(), 5);
        Cancel();
    }

    protected override void Activate(float dt) { }
}
