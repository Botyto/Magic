using System;
using UnityEngine;

[Serializable]
public class ManifestationPhysicalProperties
{
    //Fields that encode this state (must match with the manifestation)
    public int energy;
    public Energy.Element element;
    public Energy.Shape shape;

    public float mass;
    public float volume;
    public int temperature;

    public float density { get { return mass / volume; } }

    public ManifestationPhysicalProperties(EnergyManifestation manifestation)
    {
        ExtractProperties(manifestation);
    }

    public bool Update(EnergyManifestation manifestation)
    {
        if (energy == manifestation.GetEnergy() && element == manifestation.element && shape == manifestation.shape)
        {
            return false;
        }

        ExtractProperties(manifestation);
        return true;
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

    private void ExtractProperties(EnergyManifestation manifestation)
    {
        energy = manifestation.GetEnergy();
        element = manifestation.futureElement;
        shape = manifestation.futureShape;
        var energyScaled = manifestation.GetEnergyScaledf();

        var elementDef = Energy.GetElement(manifestation.futureElement);
        mass = elementDef.mass * energyScaled * manifestation.lorentzFactor;
        volume = elementDef.baseVolume + elementDef.volume * energyScaled;
        temperature = 0; //TODO calculate or get?
    }
}
