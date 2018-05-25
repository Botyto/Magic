
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

        //Primary elements
        /// <summary>
        /// Fire energy (beats air, loses to water)
        /// </summary>
        Fire,
        /// <summary>
        /// Air element (beats electricity, loses to fire)
        /// </summary>
        Air,
        /// <summary>
        /// Electrical element (beats earth, loses to air)
        /// </summary>
        Electricity,
        /// <summary>
        /// Dirt (natural) energy (beats water, loses to electricity)
        /// </summary>
        Earth,
        /// <summary>
        /// Water energy (beats fire, loses to earth)
        /// </summary>
        Water,

        //Secondary elements
        /// <summary>
        /// Ice energy (water + air)
        /// </summary>
        Ice,
        /// <summary>
        /// Wood energy (earth + water)
        /// </summary>
        Wood,
        /// <summary>
        /// Lava energy (fire + earth)
        /// </summary>
        //Lava,
        /// <summary>
        /// Storm energy (electricity + water)
        /// </summary>
        //Storm,
        /// <summary>
        /// Boil energy (fire + water)
        /// </summary>
        //Boil,
        /// <summary>
        /// Dust energy (fire + earth + air)
        /// </summary>
        //Dust,
        /// <summary>
        /// Explosion energy (fire + electricity)
        /// </summary>
        //Explosion,
        /// <summary>
        /// Scorch energy (fire + wind)
        /// </summary>
        //Scorch,
        /// <summary>
        /// Magnetic energy (earth + air)
        /// </summary>
        //Magnetic,
        
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
        Hemisphere,
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
    
    #region Universal constants

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

    #endregion

    #region Global getters

    /// <summary>
    /// Default element, wherever needed
    /// </summary>
    public static Element DefaultElement { get { return EnergyGlobals.instance.DefaultElement; } }

    /// <summary>
    /// Default shape, wherever needed
    /// </summary>
    public static Shape DefaultShape { get { return EnergyGlobals.instance.DefaultShape; } }

    /// <summary>
    /// Returns the definition of the specified energy element
    /// </summary>
    public static EnergyElement GetElement(Element element)
    {
        return EnergyGlobals.instance.GetElement(element);
    }

    /// <summary>
    /// Returns the defintion of the specified energy shape
    /// </summary>
    public static EnergyShape GetShape(Shape shape)
    {
        return EnergyGlobals.instance.GetShape(shape);
    }

    #endregion
}
