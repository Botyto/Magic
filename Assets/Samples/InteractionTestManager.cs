using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class InteractionTestManager : MonoBehaviour
{
    #region Types

    public enum ManifestationProperty
    {
        Charge,
        Mass,
        Volume,
    }

    public struct ManifestationParams
    {
        public Vector3 position;
        public int charge;
        public Energy.Element element;
        public Energy.Shape shape;
    }

    #endregion

    #region Members

    public bool autoPause = true;
    public float amount = 10;
    public ManifestationProperty property = ManifestationProperty.Charge;
    public Energy.Shape shape;
    public float force = 10.0f;
    public Vector3 direction = Vector3.forward;
    public float spacing = 40.0f;

    private int m_Count = 0;

    #endregion

    #region Tools & helpers

    public EnergyManifestation CreateManifestation(ManifestationParams manifParams)
    {
        return CreateManifestation(manifParams, "TestEnergy" + m_Count);
    }

    public EnergyManifestation CreateManifestation(ManifestationParams manifParams, string name)
    {
        ++m_Count;

        var obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.hideFlags = HideFlags.DontSaveInBuild;
        obj.transform.position = manifParams.position;

        var holder = obj.AddComponent<EnergyHolder>();
        holder.energy = manifParams.charge;

        var manif = obj.AddComponent<EnergyManifestation>();
        manif.ChangeElementLater(manifParams.element);
        manif.ChangeShapeLater(manifParams.shape);

        return manif;
    }

    public void CreatePair(ManifestationParams first, ManifestationParams second, out EnergyManifestation outFirst, out EnergyManifestation outSecond)
    {
        CreatePair(first, second, "TestEnergyPair" + m_Count, out outFirst, out outSecond);
    }

    public void CreatePair(ManifestationParams first, ManifestationParams second, string name, out EnergyManifestation outFirst, out EnergyManifestation outSecond)
    {
        SetPairPositions(ref first, ref second, direction);
        outFirst = CreateManifestation(first,  name + "__first");
        outSecond = CreateManifestation(second, name + "__second");
    }

    public void CreateAllElementPairs(float value, Energy.Shape shape, float force, ForceMode forceMode)
    {
        var first = new ManifestationParams();
        first.shape = shape;

        var second = new ManifestationParams();
        second.shape = shape;

        var elements = Enum.GetValues(typeof(Energy.Element)).Cast<Energy.Element>().ToArray();
        int x = 0;
        foreach (var elem1 in elements)
        {
            first.element = elem1;
            first.charge = CalculateCharge(elem1, property, value);

            int z = 0;
            foreach (var elem2 in elements)
            {
                second.element = elem2;
                second.charge = CalculateCharge(elem2, property, value);

                var middle = new Vector3(spacing * x, 15.0f, spacing * z) + transform.position;
                first.position = middle;

                EnergyManifestation firstManif, secondManif;
                CreatePair(first, second, out firstManif, out secondManif);

                var orientation = new Orientation(firstManif, secondManif);
                if (Application.isPlaying)
                {
                    firstManif.ApplyForce(orientation.forward * force, forceMode);
                    secondManif.ApplyForce(orientation.back * force, forceMode);
                }
                else
                {
                    var firstForce = firstManif.gameObject.AddComponent<InitialForceApplier>();
                    firstForce.target = secondManif.transform;
                    firstForce.forceSize = force;
                    firstForce.mode = forceMode;

                    var secondForce = secondManif.gameObject.AddComponent<InitialForceApplier>();
                    secondForce.target = firstManif.transform;
                    secondForce.forceSize = force;
                    secondForce.mode = forceMode;
                }

                ++z;
            }

            ++x;
        }
    }

    private void SetPairPositions(ref ManifestationParams first, ref ManifestationParams second, Vector3 direction)
    {
        Vector3 middle = Vector3.zero;
        float distance = 5.0f;
        if (first.position == Vector3.zero)
        {
            middle = second.position;
        }
        else if (second.position == Vector3.zero)
        {
            middle = first.position;
        }
        else
        {
            middle = (first.position + second.position) / 2.0f;
            distance = first.position.DistanceTo(second.position);
        }

        direction.Normalize();

        first.position = middle + direction * distance;
        second.position = middle - direction * distance;
    }

    private int CalculateCharge(Energy.Element element, ManifestationProperty prop, float amount)
    {
        var info = Energy.GetElement(element);
        switch (prop)
        {
            case ManifestationProperty.Charge: return (int)amount;
            case ManifestationProperty.Mass: return (int)(amount / info.mass);
            case ManifestationProperty.Volume: return (int)((amount - info.baseVolume) / info.volume);

            default: return (int)amount;
        }
    }

    private void DestroyAllChildren()
    {
        var n = transform.childCount;
        for (int i = 0; i < n; ++i)
        {
            var child = transform.GetChild(i);
            Util.Destroy(child.gameObject);
        }
    }

    #endregion

    #region Tests

    private void TestElements()
    {
        DestroyAllChildren();
        CreateAllElementPairs(amount, shape, force, ForceMode.VelocityChange);
    }

    #endregion

    #region Unity interface

    private void Start()
    {
        if (autoPause && Application.isEditor)
        {
            EditorApplication.isPaused = true;
        }

        TestElements();
    }

    private void Reset()
    {
        m_Count = 0;
    }

    #endregion
}
