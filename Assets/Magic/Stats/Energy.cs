
/// <summary>
/// Energy helpers related to fundamental energy properties
/// </summary>
public static class Energy //TODO organize/spearate into classes
{
    /// <summary>
    /// List of all elements an energy manifestation can be
    /// </summary>
    public enum Element
    {
        //Classical
        /// <summary>
        /// Raw energy
        /// </summary>
        Raw,
        /// <summary>
        /// Mana
        /// </summary>
        Mana,

        //Elemental
        /// <summary>
        /// Fire energy
        /// </summary>
        Fire,
        /// <summary>
        /// Water energy
        /// </summary>
        Water,
        /// <summary>
        /// Wind element
        /// </summary>
        Wind,
        /// <summary>
        /// Dirt (natural) energy
        /// </summary>
        Dirt,
        /// <summary>
        /// Wood (natural) energy
        /// </summary>
        Wood,
        /// <summary>
        /// Electrical element
        /// </summary>
        Electricity,
        /// <summary>
        /// Ice energy (frozen water)
        /// </summary>
        Ice,
        
        //Special
        /// <summary>
        /// Special element for doing rituals
        /// </summary>
        Ritual,
    }

    /// <summary>
    /// List of all shape an energy manifestation can take
    /// </summary>
    public enum Shape
    {
        /// <summary>
        /// Shperical.
        /// Origin is at center.
        /// Uses built in collider.
        /// </summary>
        Sphere,
        /// <summary>
        /// Half sphere.
        /// Origin is at canter of base.
        /// Uses specialized collider.
        /// </summary>
        HemiSphere,
        /// <summary>
        /// Cube.
        /// Origin is at center.
        /// Uses built in collider.
        /// </summary>
        Cube,
        /// <summary>
        /// Cone.
        /// Origin is at "around" center of mass.
        /// Uses mesh as collider.
        /// </summary>
        Cone,
        /// <summary>
        /// Capsule.
        /// Origin is at center of mass.
        /// Uses built in collider.
        /// </summary>
        Capsule,
    }

    /// <summary>
    /// Default element, wherever needed
    /// </summary>
    public const Element DefaultElement = Element.Raw;

    /// <summary>
    /// Default shape, wherever needed
    /// </summary>
    public const Shape DefaultShape = Shape.Sphere;

    /// <summary>
    /// Energy scale (for display purposes)
    /// </summary>
    public const int Scale = 100;

    /// <summary>
    /// Energy scale as floating point number (for display purposes)
    /// </summary>
    public const float Scalef = Scale;

    /// <summary>
    /// Minimum temperature any material can reach
    /// </summary>
    public const int MinTemperature = -273;

    /// <summary>
    /// Global speed limit
    /// </summary>
    public const float SpeedLimit = 1000.0f;

    /// <summary>
    /// Global speed limit squared
    /// </summary>
    public const float SqrSpeedLimit = SpeedLimit * SpeedLimit;

    /// <summary>
    /// How much damage a single (scaled) enery unit deals (TODO redesign)
    /// </summary>
    public static float DamagePerUnit(Element element)
    {
        return 10.0f;
    }

    /// <summary>
    /// Get physical elasticity of an element.
    /// Elasticity dictates how easily it is to deform a manifestation.
    /// </summary>
    public static float ElementElasticity(Element element)
    {
        return 1f;
    }

    /// <summary>
    /// If this element seals the manifestation.
    /// Most manipulations cannot be done on sealed energies.
    /// </summary>
    public static bool IsSealingElement(Element element)
    {
        return element == Element.Ritual;
    }

    /// <summary>
    /// Temperature of element in non-deformed state.
    /// Density changes temperature.
    /// </summary>
    public static int NormalTemperature(Element element)
    {
        switch (element)
        {
            case Element.Mana:
            case Element.Raw:
                return 25;

            case Element.Fire: return 500;
            case Element.Electricity: return 800;
            case Element.Ice: return -5;

            case Element.Wind: return 15;

            default: return 25;
        }
    }

    /// <summary>
    /// Constant that affects the relation between density change and temperature change
    /// </summary>
    public static float TemperatureConstant(Element element) //TODO: rename me
    {
        return 1.0f;
    }
}
