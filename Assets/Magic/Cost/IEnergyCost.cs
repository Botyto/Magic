using UnityEngine;

/// <summary>
/// Base interface for calculating costs of casting a specific spell
/// </summary>
public interface IEnergyCost
{
    int ManifestEnergy(EnergyController user, int amount, Vector3 relativePosition);
    int Summon(EnergyController user, EnergyManifestation target, SummonRecipe recipe);
    int Charge(EnergyController user, EnergyManifestation target, int amount);
    int Discharge(EnergyController user, EnergyManifestation target, int amount);
    int ChangeElement(EnergyController user, EnergyManifestation target, Energy.Element newElement);
    int ChangeShape(EnergyController user, EnergyManifestation target, Energy.Shape newShape);
    int Deform(EnergyController user, EnergyManifestation target, Vector3 stress);
    int CreateElasticConnection(EnergyController user, EnergyManifestation target, EnergyManifestation other, int connectionCharge);
    int CreateElasticConnection(EnergyController user, EnergyManifestation target, GameObject other, int connectionCharge);
    int ApplyForce(EnergyController user, EnergyManifestation target, Vector3 force, ForceMode mode);
    int ApplyTorque(EnergyController user, EnergyManifestation target, Vector3 torque, ForceMode mode);
    int OrientTowards(EnergyController user, EnergyManifestation target, Vector3 lookat);
    int ProbePoint(EnergyController user, Vector3 relativePoint);
    int Merge(EnergyController user, EnergyManifestation targetA, EnergyManifestation targetB);
    int Separate(EnergyController user, EnergyManifestation target, int amount, Vector3 force);
    int ApplyAura<T>(EnergyController user, EnergyManifestation target, GameObject obj, int extractedEnergy);
    int Substitution(EnergyController user, EnergyManifestation target, GameObject first, GameObject second);

    //Adding new manipulation procedure & cost:
    //1) Implement the actual manipulation in EnergyManifestationManipulation.cs
    //2) Add a function in EnergyController that must:
    //2.1) Check redundancy
    //2.2) Check range and possibility of execution
    //2.3) Check cost here (see step 3). Note: cost for extracting energy from the controller is not included in this class!
    //2.4) Implement the actual implementation of the manipulation
    //2.5) Consume the energy from the controller's holder
    //3) Add a function here that returns int, takes EnergyController and all arguments the function in step 2 takes
    //4) Add an interface for the controller function in SpellComponent.cs
}
