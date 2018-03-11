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
        /// Swiftness(pos) / Slowness(neg)
        /// </summary>
        Speed,
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
}
