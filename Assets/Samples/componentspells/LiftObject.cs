using UnityEngine;

public class LiftObject : InstantSpellComponent
{
    public override void Cast()
    {
        int handle;
        if (!Try(ManifestEnergyAndFocusAbsolute(param.level * 50, TargetPosition + Vector3.up * 3, out handle)))
        {
            Cancel();
            return;
        }

        if (!Try(CreateElasticConnection(handle, target, param.level * 200)))
        {
            Cancel();
            return;
        }

        if (!Try(ApplyForce(handle, Vector3.up * 200 * param.level, ForceMode.Impulse)))
        {
            Cancel();
            return;
        }

        Finish();
    }
}
