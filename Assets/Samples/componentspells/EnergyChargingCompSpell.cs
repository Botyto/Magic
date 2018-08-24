public class EnergyChargingCompSpell : ContinuousSpellComponent
{
    int focus;

    public override void OnBegin()
    {
        var targetManifestation = target.GetComponent<EnergyManifestation>();
        if (SpellUtilities.IsManifestationHarmful(wizard, targetManifestation))
        {
            Cancel();
        }

        focus = AddFocus(targetManifestation);
    }

    public override void Activate(float dt)
    {
        if (controller.GetEnergy() < param.level * 10)
        {
            Finish();
        }
        else
        {
            Charge(focus, param.level * 10);
        }
    }

    public override void OnFocusLost(int handle)
    {
        Cancel();
    }

    public override void OnTargetLost()
    {
        Cancel();
    }
}
