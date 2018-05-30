using UnityEngine;

public class InstantBallCompSpell : InstantSpellComponent
{
    int handle;

    public new static GameObject TryFindTarget(Wizard wizard)
    {
        return SpellUtilities.FindClosestEnemy(wizard, Wizard.TargetSearchDistance);
    }

    public override void Cast()
    {
        if (!Try(ManifestEnergyAndFocus(param.level * 50, Vector3.forward * 3, out handle)))
        {
            Cancel();
            return;
        }

        OrientTowards(handle, TargetPosition);

        var forceDirection = TargetPosition - GetFocusPosition(handle);
        ApplyForce(handle, forceDirection.SetLength(5 * param.level), ForceMode.Impulse);
    }
}
