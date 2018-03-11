using System.Collections.Generic;
using UnityEngine;

public static class EnergyParticles
{
    #region Public getters

    public static GameObject SmashParticles(Energy.Element element)
    {
        return m_SmashParticles.TryGetValue(element, default(GameObject));
    }

    public static GameObject DisposeParticles(Energy.Element element)
    {
        return m_DisposeParticles.TryGetValue(element, default(GameObject));
    }

    public static GameObject IdleParticles(Energy.Element element)
    {
        return m_IdleParticles.TryGetValue(element, default(GameObject));
    }

    public static float EmitterScaleMultiplier(Energy.Shape shape)
    {
        switch (shape)
        {
            case Energy.Shape.Sphere: return 0.62035f;
            case Energy.Shape.Cube: return 1.0f;

            default: return 1.0f;
        }
    }

    public static ParticleSystemShapeType ResolveEmitterShape(Energy.Shape manifestationShape)
    {
        switch (manifestationShape)
        {
            case Energy.Shape.Sphere: return ParticleSystemShapeType.Sphere;
            case Energy.Shape.Cube: return ParticleSystemShapeType.Box;

            default: return ParticleSystemShapeType.Sphere;
        }
    }

    #endregion

    #region Initialization

    private static GameObject FindParticlePrefab(string prefabPath)
    {
        return Resources.Load<GameObject>(prefabPath);
    }

    static EnergyParticles()
    {
        //Smash particle prefabs by element
        m_SmashParticles = new Dictionary<Energy.Element, GameObject>();
        m_SmashParticles[Energy.Element.Mana] = FindParticlePrefab("Particles/Energy/Smash/SmashMana");

        //Dispose particle prefabs by element
        m_DisposeParticles = new Dictionary<Energy.Element, GameObject>();

        //Idle particle prefabs by element
        m_IdleParticles = new Dictionary<Energy.Element, GameObject>();
        m_IdleParticles[Energy.Element.Fire] = FindParticlePrefab("Particles/Energy/Idle/IdleFire");
    }

    #endregion

    #region Members

    private static Dictionary<Energy.Element, GameObject> m_SmashParticles;
    private static Dictionary<Energy.Element, GameObject> m_DisposeParticles;
    private static Dictionary<Energy.Element, GameObject> m_IdleParticles;

    #endregion
}
