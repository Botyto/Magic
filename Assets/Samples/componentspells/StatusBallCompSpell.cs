using UnityEngine;

public class StatusBallCompSpell : ContinuousSpellComponent
{
    int ballHandle;

    public override void Activate(float dt) { }

    public new static GameObject TryFindTarget(Wizard wizard)
    {
        return SpellUtilities.FindClosestEnemy(wizard);
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
        if (!Try(ManifestEnergyAndFocus(30*param.level, Vector3.forward*3, param.element, param.shape, out ballHandle)))
        {
            Cancel();
            return;
        }

        OrientTowards(ballHandle, target);

        var forceDirection = TargetPosition - GetFocusPosition(ballHandle);
        if (!Try(ApplyForce(ballHandle, forceDirection.SetLength(10), ForceMode.Impulse)))
        {
            Cancel();
            return;
        }

        GetFocus(ballHandle).unitCollisionEvent.AddListener(HandleCollision);
    }

    public void HandleCollision(EnergyManifestation manif, Unit unit, EnergyManifestation.CollisionEventType ev)
    {
        if (ev != EnergyManifestation.CollisionEventType.Enter)
        {
            return;
        }
        
        StatusAura aura;
        if (!Try(ApplyAura(ballHandle, unit.gameObject, manif.GetEnergy(), out aura)))
        {
            Cancel();
            return;
        }

        aura.type = param.statusEffect;
    }
}
