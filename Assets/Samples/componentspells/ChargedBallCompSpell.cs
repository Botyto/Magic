using UnityEngine;

public class ChargedBallCompSpell : StagedSpellComponent
{
    int handle;

    public new static GameObject TryFindTarget(Wizard wizard)
    {
        return SpellUtilities.FindClosestEnemy(wizard, 20.0f);
    }

    public override void OnFocusLost(int handle)
    {
        Cancel();
    }

    public override void OnTargetLost()
    {
        Cancel();
    }

    public override void OnBegin()
    {
        if (!Try(ManifestEnergyAndFocus(param.level * 5, Vector3.forward*3, param.element, param.shape, out handle)))
        {
            Cancel();
            return;
        }

        OrientTowards(handle, GetTargetPosition());
    }
    
    public override void Cast(float dt)
    {
        if (!Try(Charge(handle, param.level * 5)) || GetFocus(handle).GetEnergy() >= param.level * 50)
        {
            var forceDirection = GetTargetPosition() - GetFocusPosition(handle);
            ApplyForce(handle, forceDirection.SetLength(5 * param.level), ForceMode.Impulse);
            Finish();
            return;
        }
    }
}
