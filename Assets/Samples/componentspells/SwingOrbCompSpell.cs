using UnityEngine;

public class SwingOrbCompSpell : InstantSpellComponent
{
    int h1, h2;

    public override void Cast()
    {
        ManifestEnergyAndFocus(100 * param.level, Vector3.forward * 3 + Vector3.left, out h1);
        ManifestEnergyAndFocus(100 * param.level, Vector3.forward * 3 + Vector3.up, out h2);
        CreateElasticConnection(h1, h2, 100 * param.level);

        var orient = new Orientation(controller, GetTargetPosition());
        ApplyForce(h1, orient.forward * 3 * param.level, ForceMode.Impulse);
        ApplyForce(h2, orient.back * 1 * param.level, ForceMode.Impulse);
    }
}   
