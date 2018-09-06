public class StatusAura : AuraBase
{
    public StatusEffect.Type type;
    public int sign = 1;

    protected override void OnApply()
    {
        var intensity = (sign >= 0) ? 1 : -1;
        StatusEffectCharge(type, intensity, GetEnergy());
        Cancel();
    }

    protected override void Activate(float dt) { }
}
