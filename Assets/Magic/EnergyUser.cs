using UnityEngine;

/// <summary>
/// Base class for using the energy held by this object.
/// </summary>
/// <article>Energy</article>
/// <seealso cref="EnergyHolder"/>
/// <seealso cref="EnergyController"/>
[RequireComponent(typeof(EnergyHolder))]
public class EnergyUser : MonoBehaviour
{
    #region Members

    /// <summary>
    /// The energy holder
    /// </summary>
    [HideInInspector]
    public EnergyHolder holder;

    #endregion

    #region Energy manipulation (direct interface to EnergyHolder)

    /// <summary>
    /// Currently held energy (unscaled)
    /// </summary>
    public int GetEnergy()
    {
        return holder.GetEnergy();
    }

    /// <summary>
    /// Currently held energy (scaled, as integer)
    /// </summary>
    public int GetEnergyScaled()
    {
        return holder.GetEnergyScaled();
    }

    /// <summary>
    /// Currently held energy (scaled, as float)
    /// </summary>
    public float GetEnergyScaledf()
    {
        return holder.GetEnergyScaledf();
    }

    /// <summary>
    /// Increase the energy held (grant/gain)
    /// </summary>
    /// <returns>Actual increase</returns>
    public int IncreaseEnergy(int amount)
    {
        return holder.Increase(amount);
    }

    /// <summary>
    /// Decrease the energy held (consume)
    /// </summary>
    /// <returns>Actual decrease</returns>
    public int DecreaseEnergy(int amount)
    {
        return holder.Decrease(amount);
    }

    #endregion

    #region Unity internals

    protected virtual void Awake()
    {
        holder = GetComponent<EnergyHolder>();
    }

    #endregion
}
