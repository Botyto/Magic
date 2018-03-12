
/// <summary>
/// Energy stats related to Rigidbody and Collider properties
/// </summary>
public static class EnergyPhysics
{
    #region Interaction
    
    /// <summary>
    /// Maximum impluse that can be applied to a solid manifestation (with unit mass) before it smashes violently
    /// </summary>
    public static float SmashImpulse(Energy.Element element)
    {
        return 10.0f;
    }

    #endregion

    #region Collider & shape properties

    /// <summary>
    /// If collider of manifestation should be a trigger or not (solid or transparent/pass-through)
    /// </summary>
    public static bool ElementIsPassThrough(Energy.Element element)
    {
        switch (element)
        {
            //Pass through
            case Energy.Element.Raw:
            case Energy.Element.Mana:
            case Energy.Element.Fire:
            case Energy.Element.Wind:
            case Energy.Element.Water:
            case Energy.Element.Electricity:
                return true;

            //Solids
            case Energy.Element.Dirt:
            case Energy.Element.Wood:
            case Energy.Element.Ice:
            case Energy.Element.Ritual:
                return false;

            default: return true;
        }
    }

    /// <summary>
    /// How much volume a single (scaled) energy unit takes up
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static float VolumePerUnit(Energy.Element element)
    {
        switch (element)
        {
            case Energy.Element.Raw: return 1.0f;
            case Energy.Element.Mana: return 1.0f;
            case Energy.Element.Fire: return 0.8f;
            case Energy.Element.Ice: return 1.1f;
            case Energy.Element.Ritual: return 0.01f;
            default: return 1.0f;
        }
    }

    /// <summary>
    /// How much volume this element takeks up initially (not taking into account energy held)
    /// </summary>
    public static float BaseVolume(Energy.Element element)
    {
        if (element == Energy.Element.Ritual)
        {
            return 1.0f;
        }
        else
        {
            return 0.0f;
        }
    }

    #endregion

    #region Rigidbody

    /// <summary>
    /// If body of manifestation should use gravity
    /// </summary>
    public static bool BodyUsesGravity(Energy.Element element)
    {
        switch (element)
        {
            case Energy.Element.Raw:
            case Energy.Element.Mana:
            case Energy.Element.Ice:
            case Energy.Element.Water:
            case Energy.Element.Wood:
            case Energy.Element.Dirt:
                return true;

            case Energy.Element.Fire:
            case Energy.Element.Ritual:
            case Energy.Element.Electricity:
            case Energy.Element.Wind:
                return false;

            default: return true;
        }
    }
    
    /// <summary>
    /// How much mass a single (scaled) energy unit makes up
    /// </summary>
    public static float MassPerUnit(Energy.Element element)
    {
        switch (element)
        {
            case Energy.Element.Raw:
            case Energy.Element.Mana: return 1.0f;
            case Energy.Element.Ice: return 1.0f;
            case Energy.Element.Fire: return 0.1f;

            case Energy.Element.Ritual: return 10.0f;

            default:
                return 1.0f;
        }
    }

    #endregion
}
