using UnityEngine;

public class ProtectiveWall : InstantSpellComponent
{
    public override void Cast()
    {
        int handle;
        if (!Try(ManifestEnergyAndFocus(100 * param.level, Vector3.forward * 3, param.element, param.shape, out handle)))
        {
            Cancel();
            return;
        }

        if (!Try(Deform(handle, new Vector3(0, 0, 20.0f))))
        {
            Cancel();
            return;
        }

        DisownFocus(handle);
    }
}
