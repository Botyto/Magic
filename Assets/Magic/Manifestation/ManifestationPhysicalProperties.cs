using UnityEngine;

public struct ManifestationPhysicalProperties
{
    //Fields that encode this state (must match with the manifestation)
    public readonly int energy;
    public readonly Energy.Element element;
    public readonly Energy.Shape shape;
    
    public float mass;
    public float volume;
    public int temperature;

    public ManifestationPhysicalProperties(EnergyManifestation manifestation)
    {
        energy = manifestation.GetEnergy();
        element = manifestation.futureElement;
        shape = manifestation.futureShape;
        var energyScaled = manifestation.GetEnergyScaledf();

        mass = EnergyPhysics.MassPerUnit(manifestation.futureElement) * energyScaled * manifestation.lorentzFactor;
        volume = EnergyPhysics.BaseVolume(manifestation.futureElement) + EnergyPhysics.VolumePerUnit(manifestation.futureElement) * energyScaled;
        temperature = 0; //TODO calculate
    }

    public bool ApplyTo(EnergyManifestation manifestation)
    {
        if (energy != manifestation.GetEnergy())
        {
            return false;
        }

        manifestation.ChangeElementNow(element);
        manifestation.ChangeShapeNow(shape);

        manifestation.rigidbody.mass = mass;
        manifestation.originalVolume = volume;
        manifestation.transform.localScale = manifestation.deformation * Mathf.Pow(volume, 1.0f / 3.0f);

        return true;
    }
}
