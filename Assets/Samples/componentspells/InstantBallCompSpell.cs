using UnityEngine;

public class InstantBallCompSpell : InstantSpellComponent
{
    int handle;

    public new static GameObject TryFindTarget(Wizard wizard)
    {
        return SpellUtilities.FindClosestEnemy(wizard, 20.0f);
    }

    public override void Cast()
    {
        if (!Try(ManifestEnergyAndFocus(param.level * 50, Vector3.forward * 3, param.element, param.shape, out handle)))
        {
            Cancel();
            return;
        }

        OrientTowards(handle, GetTargetPosition());

        var forceDirection = GetTargetPosition() - GetFocusPosition(handle);
        ApplyForce(handle, forceDirection.SetLength(10 * param.level), ForceMode.Impulse);
    }
}
