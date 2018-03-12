#define DISOWNED_DISAPPEAR

using UnityEngine;

public partial class EnergyManifestation
{
    #region Charge info

    //All properties of an energy manifestation are kept unchanged since the beginning of the frame until the end.
    //At the end of each frame (LateUpdate) all properties are updated according to the new energy held.

    #endregion

    #region Members

    /// <summary>
    /// Manifestation element
    /// </summary>
    public Energy.Element element = Energy.DefaultElement;

    /// <summary>
    /// Manifestation shape
    /// </summary>
    public Energy.Shape shape = Energy.DefaultShape;

    /// <summary>
    /// Manifestation temperature (°C)
    /// </summary>
    public int temperature;

    /// <summary>
    /// Manifestation physical properties cache.
    /// They are updated each time energy changes and are actually applied at the end of each frame.
    /// </summary>
    public ManifestationPhysicalProperties lastFrameProperties;

    /// <summary>
    /// Element to which to switch at end of this frame (LateUpdate)
    /// </summary>
    public Energy.Element futureElement = Energy.DefaultElement;

    /// <summary>
    /// Shape to which to switch at end of this frame (LateUpdate)
    /// </summary>
    public Energy.Shape futureShape = Energy.DefaultShape;

    #endregion

    #region Unity interface & internals
    
    /// <summary>
    /// Callback for when energy held has been depleted
    /// </summary>
    private void EnergyDepleted()
    {
        Util.Destroy(gameObject, "energy depleted");
    }

    private void EnergyChanged(int delta)
    {
        _Physics_UpdatePhysicalProperties();
    }

    private void _Charge_OnEnable()
    {
        lastFrameProperties = new ManifestationPhysicalProperties(this);
    }

    private void _Charge_Start()
    { }

    private void _Charge_FixedUpdate()
    {
#if DISOWNED_DISAPPEAR
        //Energies without an owner lose charge over time
        if (holder.owner == null)
        {
            DecreaseEnergy(Mathf.Max(1, GetEnergy() / (60 * Energy.Scale)));
        }
#endif

        if (futureElement != element)
        {
            ChangeElementNow(futureElement);
        }
        if (futureShape != shape)
        {
            ChangeShapeNow(futureShape);
        }

        lastFrameProperties.Update(this);
    }

#endregion
}
