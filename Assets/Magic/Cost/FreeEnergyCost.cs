using UnityEngine;

public class FreeEnergyCost : IEnergyCost
{
    public bool freeManifest = false;

    public int ManifestEnergy(EnergyController user, int amount, Vector3 relativePosition)
    {
        return freeManifest ? -amount : 0;
    }

    public int Charge(EnergyController user, EnergyManifestation target, int amount)
    {
        return 0;
    }

    public int Discharge(EnergyController user, EnergyManifestation target, int amount)
    {
        return 0;
    }

    public int Merge(EnergyController user, EnergyManifestation targetA, EnergyManifestation targetB)
    {
        return 0;
    }

    public int Separate(EnergyController user, EnergyManifestation target, int amount, Vector3 force)
    {
        return 0;
    }

    public int Summon(EnergyController user, EnergyManifestation target, SummonRecipe recipe)
    {
        return 0;
    }

    public int ChangeElement(EnergyController user, EnergyManifestation target, Energy.Element newElement)
    {
        return 0;
    }

    public int ChangeShape(EnergyController user, EnergyManifestation target, Energy.Shape newShape)
    {
        return 0;
    }

    public int Deform(EnergyController user, EnergyManifestation target, Vector3 stress)
    {
        return 0;
    }

    public int ProbePoint(EnergyController user, Vector3 relativePoint)
    {
        return 0;
    }

    public int CreateElasticConnection(EnergyController user, EnergyManifestation target, EnergyManifestation other, int connectionCharge)
    {
        return 0;
    }

    public int CreateElasticConnection(EnergyController user, EnergyManifestation target, GameObject other, int connectionCharge)
    {
        return 0;
    }

    public int ApplyForce(EnergyController user, EnergyManifestation target, Vector3 force, ForceMode mode)
    {
        return 0;
    }

    public int ApplyTorque(EnergyController user, EnergyManifestation target, Vector3 toruqe, ForceMode mode)
    {
        return 0;
    }

    public int OrientTowards(EnergyController user, EnergyManifestation target, Vector3 lookat)
    {
        return 0;
    }

    public int ApplyAura<T>(EnergyController user, EnergyManifestation target, GameObject obj, int extractedEnergy)
    {
        return 0;
    }

    public int Substitution(EnergyController user, EnergyManifestation target, GameObject first, GameObject second)
    {
        return 0;
    }
}
