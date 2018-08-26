using System;

/// <summary>
/// Descriptor of a status effect of a unit
/// </summary>
[Serializable]
public struct StatusEffect
{
    /// <summary>
    /// Status effect types
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// Swiftness (positive) / Slowness (negative)
        /// </summary>
        Speed,

        /// <summary>
        /// Healing (positive) / Damage (negative)
        /// </summary>
        Health,

        /// <summary>
        /// Increase regeneration (positive) / Decrease regeneration (negative)
        /// </summary>
        Energy,
    }

    /// <summary>
    /// Type of effect
    /// </summary>
    public Type type;

    /// <summary>
    /// Intensity with which the effect acts
    /// Can be positive and negative (negative values must affect the unit negatively!)
    /// </summary>
    public int intensity;

    /// <summary>
    /// Carge of the effect (decreases over time)
    /// </summary>
    public int charge;

    /// <summary>
    /// Returns if the status effect is currently active and effective
    /// </summary>
    public bool isActive { get { return intensity != 0; } }
}
