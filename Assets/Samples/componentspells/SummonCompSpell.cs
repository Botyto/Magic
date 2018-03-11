using UnityEngine;

public class SummonCompSpell : ContinuousSpellComponent
{
    int handle;

    public override void OnBegin()
    {
        if (!Try(ManifestEnergyAndFocus(50, Vector3.forward * 15, Energy.Element.Ritual, Energy.Shape.Cube, out handle)))
        {
            Cancel();
            return;
        }
        
        wizard.CastSpell("EnergyCharging", GetFocus(handle).gameObject);
    }

    public override void Activate(float dt)
    {
        GameObject obj;
        var result = Summon(handle, param.obj as SummonRecipe, out obj);
        var success = Try(result);
        if (result != EnergyActionResult.ExtractingTooMuch && !success)
        {
            Cancel();
        }
        else if (success)
        {
            Finish();
        }
    }
}
