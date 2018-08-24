using System;
using UnityEngine;

/// <summary>
/// Descriptor of a status effect
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New Status Effect", menuName = "Magic/Status Effect")]
public class StatusEffectInfo : ScriptableObject
{
    [Serializable]
    public struct Details
    {
        /// <summary>
        /// Name displayed to the user
        /// </summary>
        public string displayName;

        /// <summary>
        /// Description displayed to the user
        /// </summary>
        public string description;

        /// <summary>
        /// Icon displayed to the user
        /// </summary>
        public Sprite icon;
    }

    /// <summary>
    /// Type of effect
    /// </summary>
    public StatusEffect.Type type;

    /// <summary>
    /// Positive details
    /// </summary>
    public Details positive;

    /// <summary>
    /// Negative details
    /// </summary>
    public Details negative;
}

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
