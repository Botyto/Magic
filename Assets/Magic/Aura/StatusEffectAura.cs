/// <summary>
/// Standard aura that applies a status effect to its target.
/// </summary>
public class StatusEffectAura : AuraBase
{
    public StatusEffect.Type effect;
    public int intensitySign = 1;

    public int charge { get { return GetEnergy(); } }
    public int intensity { get { return sign; } }
    private int sign { get { return (intensitySign >= 0) ? 1 : -1; } }

    protected override void OnApply()
    {
        StatusEffectCharge(effect, intensity, charge);
        Cancel();
    }

    protected override void Activate(float dt) { }
}
