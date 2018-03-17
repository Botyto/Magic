using System;
using UnityEngine;

/// <summary>
/// Data descriptor for an energy shape.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Shape", menuName = "Magic/Energy Shape")]
public class EnergyShape : ScriptableObject
{
    /// <summary>
    /// Display name.
    /// </summary>
    public string displayName = "New Shape";

    /// <summary>
    /// Visual mesh.
    /// </summary>
    public Mesh mesh;

    /// <summary>
    /// Mesh used for collider (might be overriden depending on the shape).
    /// </summary>
    public Mesh collider;

    /// <summary>
    /// Particle emitter shape.
    /// </summary>
    public ParticleSystemShapeType particlesShape = ParticleSystemShapeType.Sphere;

    /// <summary>
    /// Factor by which to scale particle emitters to fit the mesh.
    /// </summary>
    public float particlesScale = 1.0f;
}
