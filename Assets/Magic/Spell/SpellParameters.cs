using System;

/// <summary>
/// Parameters passed to a spell, as it is being cast.
/// </summary>
[Serializable]
public struct SpellParameters
{
    /// <summary>
    /// Spell level
    /// </summary>
    public int level;

    /// <summary>
    /// Element hint
    /// </summary>
    public Energy.Element element;

    /// <summary>
    /// Shape hint
    /// </summary>
    public Energy.Shape shape;

    /// <summary>
    /// Additional object
    /// </summary>
    public UnityEngine.Object obj;

    /// <summary>
    /// Status effect hint
    /// </summary>
    public StatusEffect.Type statusEffect;

    /// <summary>
    /// Integer number parameter
    /// </summary>
    public int integer;

    /// <summary>
    /// Real number parameter
    /// </summary>
    public float real;
}
