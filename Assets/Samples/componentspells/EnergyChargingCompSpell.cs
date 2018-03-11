public class EnergyChargingCompSpell : ContinuousSpellComponent
{
    public override void Activate(float dt)
    {
        if (controller.GetEnergy() < param.level * 10)
        {
            Finish();
        }
        else
        {
            controller.Charge(target.GetComponent<EnergyManifestation>(), param.level * 10);
        }
    }

    public override void OnTargetLost()
    {
        Cancel();
    }
}
