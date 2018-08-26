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
