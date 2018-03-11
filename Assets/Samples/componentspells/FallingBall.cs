using UnityEngine;

public class FallingBall : ContinuousSpellComponent
{
    int handle;

    public override void OnBegin()
    {
        if (!EnergyPhysics.BodyUsesGravity(param.element))
        {
            Cancel();
            return;
        }

        if (!Try(ManifestEnergyAndFocus(500 * param.level, 3 * Vector3.forward + 3 * Vector3.up, out handle)))
        {
            Cancel();
            return;
        }
    }

    public override void Activate(float dt)
    {
        if (!Try(ApplyForce(handle, -Physics.gravity * 0.5f, ForceMode.Acceleration)))
        {
            Finish();
            return;
        }
    }

    public override void OnFocusLost(int handle)
    {
        Finish();
    }
}
