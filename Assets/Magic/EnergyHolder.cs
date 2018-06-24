using System.Collections.Generic;
using UnityEngine;

// Energy holder
public class EnergyHolder : MonoBehaviour
{
    #region Members

    /// <summary>
    /// Currently held energy
    /// </summary>
    public int energy = 1;

    /// <summary>
    /// Energy owner (parent)
    /// </summary>
    public EnergyHolder owner;

    /// <summary>
    /// Owned energies (children)
    /// </summary>
    public List<EnergyHolder> ownedEnergies = new List<EnergyHolder>();

    #endregion
    
    #region Ownership

    /// <summary>
    /// Resolve the absolute (top-most) owner of some energy holder
    /// </summary>
    public static EnergyHolder ResolveOwner(EnergyHolder target)
    {
        while (target.owner != null)
        {
            target = target.owner;
        }

        return target;
    }

    /// <summary>
    /// Resolve the absolute (top-most) owner of this energy holder
    /// </summary>
    public EnergyHolder ResolveOwner()
    {
        return ResolveOwner(this);
    }

    /// <summary>
    /// Change the owner (parent)
    /// </summary>
    public void SetOwner(EnergyHolder newOwner, bool keepObject = false)
    {
        //No change
        if (newOwner == owner)
        {
            return;
        }

        //Remove from previous owner
        if (owner != null)
        {
            owner.ownedEnergies.Remove(this);
        }

        //Set new owner
        owner = newOwner;

        //Setup if valid owner
        if (owner != null)
        {
            owner = newOwner.ResolveOwner();
            owner.ownedEnergies.Add(this);
            SendMessage("OwnerChanged", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            SendMessage("OwnerChanged", SendMessageOptions.DontRequireReceiver);

            //No longer has owner - destroy self
            if (!keepObject && gameObject != null)
            {
                Gameplay.Destroy(gameObject, "no owner");
            }
        }
    }

    /// <summary>
    /// Add child energy (owned)
    /// </summary>
    public void AddChild(EnergyHolder newChild)
    {
        newChild.SetOwner(ResolveOwner());
    }

    /// <summary>
    /// Remove child energy (owned)
    /// </summary>
    /// <param name="child"></param>
    public void RemoveChild(EnergyHolder child)
    {
        child.SetOwner(null);
    }

    #endregion

    #region Energy manipulation

    /// <summary>
    /// Currently held energy (unscaled)
    /// </summary>
    public int GetEnergy()
    {
        return energy;
    }

    /// <summary>
    /// Currently held energy (scaled, as integer)
    /// </summary>
    public int GetEnergyScaled()
    {
        return energy / Energy.scale;
    }

    /// <summary>
    /// Currently held energy (scaled, as float)
    /// </summary>
    public float GetEnergyScaledf()
    {
        return energy / Energy.scalef;
    }

    /// <summary>
    /// Increase the energy held (grant/gain)
    /// </summary>
    /// <returns>Actual increase</returns>
    public int Increase(int amount)
    {
        Debug.Assert(amount >= 0);
        
        if (amount == 0)
        {
            return 0;
        }

        if (amount > 0)
        {
            energy += amount;
            SendMessage("EnergyChanged", amount, SendMessageOptions.DontRequireReceiver);
        }

        return amount;
    }

    /// <summary>
    /// Decrease the energy held (consume)
    /// </summary>
    /// <returns>Actual decrease</returns>
    public int Decrease(int amount)
    {
        Debug.Assert(amount >= 0);

        if (amount == 0)
        {
            return 0;
        }

        amount = Mathf.Min(energy, amount);
        if (amount > 0)
        {
            energy -= amount;
            SendMessage("EnergyChanged", -amount, SendMessageOptions.DontRequireReceiver);

            if (energy == 0)
            {
                SendMessage("EnergyDepleted", SendMessageOptions.DontRequireReceiver);
            }
        }

        return amount;
    }

    #endregion

    #region Unity internals

    private void OnDestroy()
    {
        if (ownedEnergies.Count > 0)
        {
            var ownedEnergiesCopy = new HashSet<EnergyHolder>(ownedEnergies);
            foreach (var child in ownedEnergiesCopy)
            {
                child.SetOwner(owner);
            }
        }

        if (owner != null)
        {
            owner.ownedEnergies.Remove(this);
        }

    }

    #endregion
}
