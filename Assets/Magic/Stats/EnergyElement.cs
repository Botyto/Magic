using System;
using UnityEngine;

/// <summary>
/// Data descriptor for an energy element.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Element", menuName = "Magic/Energy Element")]
public class EnergyElement : ScriptableObject
{
    #region Members

    /// <summary>
    /// Display name.
    /// </summary>
    public string displayName = "New Element";

    /// <summary>
    /// Manifestation material.
    /// </summary>
    public Material material;

    /// <summary>
    /// Mass (kg) per unit.
    /// </summary>
    public float mass = 1.0f;

    /// <summary>
    /// How much volume this element takeks up initially (not taking into account energy held).
    /// </summary>
    public float baseVolume = 0.0f;

    /// <summary>
    /// Volume (m^3) per unit.
    /// </summary>
    public float volume = 1.0f;

    /// <summary>
    /// If the manifestation is affected by gravity.
    /// </summary>
    public bool usesGravity = true;

    /// <summary>
    /// If the element is pass-through.
    /// </summary>
    public bool passThrough = false;
    
    /// <summary>
    /// Maximum impluse that can be applied to a solid manifestation (with unit mass) before it smashes violently
    /// </summary>
    public float smashImpulse = 10.0f;

    /// <summary>
    /// Normal temeprature.
    /// </summary>
    public int temperature = 25;

    /// <summary>
    /// Constant that affects the relation between density change and temperature change.
    /// </summary>
    public float temperatureConstant = 1.0f;

    /// <summary>
    /// Get physical elasticity of an element.
    /// Elasticity dictates how easily it is to deform a manifestation.
    /// </summary>
    public float elasticity = 1.0f;

    /// <summary>
    /// Base damage per unit.
    /// </summary>
    public float damage = 1.0f;

    /// <summary>
    /// If the element seals the energy from manipulation.
    /// </summary>
    public bool sealing = false;

    /// <summary>
    /// Particles used in idle state.
    /// </summary>
    public ElementParticles idleParticles;

    /// <summary>
    /// Particles used when the manifestation smashes.
    /// </summary>
    public ElementParticles smashParticles;
    
    /// <summary>
    /// Particles used when the manifestation is disposed of.
    /// </summary>
    public ElementParticles disposeParticles;

    #endregion

    #region Types

    [Serializable]
    public class ElementParticles
    {
        /// <summary>
        /// Prefab particle to be placed.
        /// </summary>
        public GameObject prefab = null;

        /// <summary>
        /// If the particles prefab will be attached to the manifestation.
        /// </summary>
        public bool attached = true;

        /// <summary>
        /// If the particles prefab will be scaled according to the manifestation.
        /// </summary>
        public bool scaled = true;

        /// <summary>
        /// If the particles prefab will be rotated in the same orientation as the manifestation.
        /// </summary>
        public bool rotated = true;
    }

    #endregion
}