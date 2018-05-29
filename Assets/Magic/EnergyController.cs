using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component is used to manipulate on a physically manifested energy
/// For adding new manipulation procedures see comments in IEnergyCost
/// </summary>
public class EnergyController : EnergyUser
{
    #region Members

    /// <summary>
    /// Owned energies (children; see holder)
    /// </summary>
    public List<EnergyHolder> ownedEnergies { get { return holder.ownedEnergies; } }

    /// <summary>
    /// Maximum range at which energy manifestations are controllable
    /// </summary>
    public float controlRange = 50;

    /// <summary>
    /// Used for calculating action costs
    /// </summary>
    public IEnergyCost cost { get; private set; }

    #endregion
    
    #region Helpers & tools

    /// <summary>
    /// Unified way of strictly checking if an energy operation/action has succeeded
    /// </summary>
    public static bool TryStrict(EnergyActionResult actionResult)
    {
        return actionResult == EnergyActionResult.Success;
    }

    /// <summary>
    /// Unified way of checking if an energy operation/action has succeeded (redundant actions are accepted)
    /// </summary>
    public static bool Try(EnergyActionResult actionResult)
    {
        return actionResult == EnergyActionResult.Success || actionResult == EnergyActionResult.RedundantAction;
    }

    /// <summary>
    /// Check if a target is reachable (thus energy can be controlled there)
    /// </summary>
    public bool IsWithinRange<T>(T target) where T : MonoBehaviour
    {
        return IsWithinRange(target.transform.position);
    }

    /// <summary>
    /// Check if a point is reachable (thus energy can be controlled there)
    /// </summary>
    public bool IsWithinRange(Vector3 absolutePosition)
    {
        return transform.position.SqrDistanceTo(absolutePosition) <= controlRange * controlRange;
    }

    #endregion

    #region Manipulations

    /// <summary>
    /// Manifest own energy into the real world
    /// </summary>
    public EnergyActionResult ManifestEnergy(int amount, Vector3 relativePosition, out EnergyManifestation manifestation)
    {
        //Check range
        if (!IsWithinRange(transform.position + relativePosition))
        {
            manifestation = default(EnergyManifestation);
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = amount + cost.ManifestEnergy(this, amount, relativePosition);
        if (totalCost > GetEnergy())
        {
            manifestation = default(EnergyManifestation);
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        var newObj = new GameObject(gameObject.name + "(energy)");
        if (newObj == null)
        {
            manifestation = null;
            return EnergyActionResult.SubactionFailed;
        }

        newObj.transform.position = transform.TransformPoint(relativePosition);
        newObj.transform.rotation = transform.rotation;
        
        manifestation = newObj.AddComponent<EnergyManifestation>();
        if (manifestation == null)
        {
            return EnergyActionResult.SubactionFailed;
        }

        var newHolder = manifestation.holder;
        newHolder.energy = amount;
        newHolder.SetOwner(holder.ResolveOwner());

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Manifest own energy into the real world (initalizing shape and element)
    /// </summary>
    public EnergyActionResult ManifestEnergy(int amount, Vector3 relativePosition, Energy.Element element, Energy.Shape shape, out EnergyManifestation manifestation)
    {
        //Execute sub action
        var result = ManifestEnergy(amount, relativePosition, out manifestation);
        if (!Try(result))
        {
            return result;
        }

        //Check energy cost
        var totalCost = cost.ChangeElement(this, manifestation, element) + cost.ChangeShape(this, manifestation, shape);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        manifestation.ChangeElementLater(element);
        manifestation.ChangeShapeLater(shape);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Sacrifice a manifestation to summon an object in it's place
    /// </summary>
    public EnergyActionResult Summon(EnergyManifestation target, SummonRecipe recipe, out GameObject summonedObj)
    {
        //Check validity
        if (target == null)
        {
            summonedObj = default(GameObject);
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            summonedObj = default(GameObject);
            return EnergyActionResult.OutsideRange;
        }

        //Must have element 'Ritual' to summon objects
        if (target.element != Energy.Element.Ritual)
        {
            summonedObj = default(GameObject);
            return EnergyActionResult.ForbiddenAction;
        }

        //Target must hold enough energy
        if (target.GetEnergy() < recipe.requiredEnergy)
        {
            summonedObj = default(GameObject);
            return EnergyActionResult.ExtractingTooMuch;
        }

        //Check energy
        var totalCost = cost.Summon(this, target, recipe);
        if (totalCost > GetEnergy())
        {
            summonedObj = default(GameObject);
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        summonedObj = target.Summon(recipe.template);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Remotely charges an energy manifestation (cheaper than manifesting & merging).
    /// </summary>
    public EnergyActionResult Charge(EnergyManifestation target, int amount)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = amount + cost.Charge(this, target, amount);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.IncreaseEnergy(amount);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Remotely discharges an energy manifestation.
    /// </summary>
    public EnergyActionResult Discharge(EnergyManifestation target, int amount)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check if discharging a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check energy cost
        var totalCost = cost.Discharge(this, target, amount);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.DecreaseEnergy(amount);
        IncreaseEnergy(amount); 

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Marge two manifestations
    /// </summary>
    public EnergyActionResult Merge(EnergyManifestation targetA, EnergyManifestation targetB)
    {
        //Check validity
        if (targetA == null || targetB == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(targetA) || !IsWithinRange(targetB))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check merge conditions
        if (!targetA.CanMergeWith(targetB))
        {
            return EnergyActionResult.NoContact;
        }

        //Check if merging sealed manifestations, which is forbidden
        if (targetA.@sealed || targetB.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check energy cost
        var totalCost = cost.Merge(this, targetA, targetB);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        if (!targetA.Merge(targetB))
        {
            return EnergyActionResult.SubactionFailed;
        }

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Split manifestation into two manifestations
    /// </summary>
    public EnergyActionResult Separate(EnergyManifestation target, int amount, Vector3 force, ForceMode forceMode, out EnergyManifestation separatedEnergy)
    {
        //Check validity
        if (target == null)
        {
            separatedEnergy = default(EnergyManifestation);
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            separatedEnergy = default(EnergyManifestation);
            return EnergyActionResult.OutsideRange;
        }

        //Check if separating a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            separatedEnergy = default(EnergyManifestation);
            return EnergyActionResult.ForbiddenAction;
        }

        //Check if extracting too much
        if (target.GetEnergy() <= amount + 1) //+1 because we need to have at least one left
        {
            separatedEnergy = default(EnergyManifestation);
            return EnergyActionResult.ExtractingTooMuch;
        }

        //Check energy cost
        var totalCost = cost.Separate(this, target, amount, force);
        if (totalCost > GetEnergy())
        {
            separatedEnergy = default(EnergyManifestation);
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        separatedEnergy = target.Separate(amount, force, forceMode);
        if (separatedEnergy == null)
        {
            return EnergyActionResult.SubactionFailed;
        }

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Change manifestations' element
    /// </summary>
    public EnergyActionResult ChangeElement(EnergyManifestation target, Energy.Element newElement)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check if action is needed at all
        if (target.element == newElement)
        {
            return EnergyActionResult.RedundantAction;
        }

        //Check if changing of a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = cost.ChangeElement(this, target, newElement);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.ChangeElementLater(newElement);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Change manifestations' shape
    /// </summary>
    public EnergyActionResult ChangeShape(EnergyManifestation target, Energy.Shape newShape)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check if action is needed at all
        if (target.shape == newShape)
        {
            return EnergyActionResult.RedundantAction;
        }

        //Check if changing shape of a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = cost.ChangeShape(this, target, newShape);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.ChangeShapeLater(newShape);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Deform a manifestation
    /// </summary>
    public EnergyActionResult Deform(EnergyManifestation target, Vector3 stress)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = cost.Deform(this, target, stress);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.Deform(stress);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Create an elastic joint between this and another manifestation.
    /// </summary>
    public EnergyActionResult CreateElasticConnection(EnergyManifestation target, EnergyManifestation other, int connectionCharge)
    {
        //Check validity.
        if (target == null || other == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //If there is a joint already, it's either a redundant or a forbidden action (Only one joint is allowed per manifestation).
        var joint = target.GetComponent<SpringJoint>();
        if (joint != null)
        {
            return (joint.connectedBody == other.rigidbody) ? EnergyActionResult.RedundantAction : EnergyActionResult.ForbiddenAction;
        }

        //Check range.
        if (!IsWithinRange(target) || !IsWithinRange(other))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = connectionCharge + cost.CreateElasticConnection(this, target, other, connectionCharge);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation.
        target.CreateElasticConnection(other, connectionCharge);

        //Consume energy.
        DecreaseEnergy(totalCost);

        //Success.
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Apply physical force to manifestation (at center of mass). Force vector is relaive to the controller's transform.
    /// </summary>
    public EnergyActionResult ApplyForceRelative(EnergyManifestation target, Vector3 relativeForce, ForceMode mode)
    {
        return ApplyForce(target, transform.TransformDirection(relativeForce), mode);
    }
    
    /// <summary>
    /// Apply physical force to manifestation (at center of mass)
    /// </summary>
    public EnergyActionResult ApplyForce(EnergyManifestation target, Vector3 force, ForceMode mode)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check if applying force to a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check energy cost
        var totalCost = cost.ApplyForce(this, target, force, mode);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.ApplyForce(force, mode);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Apply physical torque to manifestation (at center of mass)
    /// </summary>
    public EnergyActionResult ApplyTorque(EnergyManifestation target, Vector3 torque, ForceMode mode)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check if applying torque to a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check energy cost
        var totalCost = cost.ApplyTorque(this, target, torque, mode);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.ApplyTorque(torque, mode);

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Forcefully and instantly orient a manifestation to look at a specific point.
    /// This does not affect angular momentum.
    /// </summary>
    public EnergyActionResult OrientTowards(EnergyManifestation target, Vector3 lookat)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        //Check if orienting a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            return EnergyActionResult.ForbiddenAction;
        }

        //Check energy cost
        var totalCost = cost.OrientTowards(this, target, lookat);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        target.transform.rotation = Quaternion.LookRotation(lookat - target.transform.position, Vector3.up);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Apply an aura to some object
    /// </summary>
    public EnergyActionResult ApplyAura<T>(EnergyManifestation target, GameObject obj, int extractedEnergy, out T aura) where T : AuraBase
    {
        //Check validity
        if (target == null)
        {
            aura = default(T);
            return EnergyActionResult.InvalidManifestation;
        }
        if (obj == null)
        {
            aura = default(T);
            return EnergyActionResult.InvalidObject;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            aura = default(T);
            return EnergyActionResult.OutsideRange;
        }

        //Check "appply aura" conditions
        if (!target.CanApplyAura<T>(obj))
        {
            aura = default(T);
            return EnergyActionResult.NoContact;
        }

        //Check if applying aura from a sealed manifestation, which is forbidden
        if (target.@sealed)
        {
            aura = default(T);
            return EnergyActionResult.ExtractingTooMuch;
        }

        //Check if extracting too much
        if (target.GetEnergy() < extractedEnergy)
        {
            aura = default(T);
            return EnergyActionResult.ExtractingTooMuch;
        }
        
        //Check energy cost
        var totalCost = cost.ApplyAura<T>(this, target, obj, extractedEnergy);
        if (totalCost > GetEnergy())
        {
            aura = default(T);
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        aura = target.ApplyAura<T>(obj, extractedEnergy);
        if (aura == null)
        {
            return EnergyActionResult.SubactionFailed;
        }

        //Consume energy
        DecreaseEnergy(totalCost);

        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Substitute self with an object
    /// </summary>
    public EnergyActionResult Substitute(EnergyManifestation target, GameObject obj)
    {
        return Substitute(target, gameObject, obj);
    }

    /// <summary>
    /// Substitute two objects
    /// </summary>
    public EnergyActionResult Substitute(EnergyManifestation target, GameObject first, GameObject second)
    {
        //Check validity
        if (target == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }
        if (first == null || second == null)
        {
            return EnergyActionResult.InvalidObject;
        }

        //Check range
        if (!IsWithinRange(target))
        {
            return EnergyActionResult.OutsideRange;
        }

        if (first == second)
        {
            return EnergyActionResult.RedundantAction;
        }
        
        //Check energy
        var totalCost = cost.Substitution(this, target, first, second);
        if (totalCost > GetEnergy())
        {
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        if (!target.Substitute(first, second))
        {
            return EnergyActionResult.SubactionFailed;
        }

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    /// <summary>
    /// Probe spatial point for manifested energy
    /// </summary>
    public EnergyActionResult ProbePoint(Vector3 relatitvePosition, out EnergyProbeResult probeResult)
    {
        //Check range
        if (!IsWithinRange(transform.position + relatitvePosition))
        {
            probeResult = default(EnergyProbeResult);
            return EnergyActionResult.OutsideRange;
        }

        //Check energy cost
        var totalCost = cost.ProbePoint(this, relatitvePosition);
        if (totalCost > GetEnergy())
        {
            probeResult = default(EnergyProbeResult);
            return EnergyActionResult.NotEnoughEnergy;
        }

        //Actual implementation
        probeResult = new EnergyProbeResult(Energy.Element.Raw, 0); //TODO implement

        //Consume energy
        DecreaseEnergy(totalCost);

        //Success
        return EnergyActionResult.Success;
    }

    #endregion

    #region Types

    /// <summary>
    /// Result from probing a spatial point for energy
    /// </summary>
    public struct EnergyProbeResult
    {
        /// <summary>
        /// Energy element at the probed point
        /// </summary>
        public Energy.Element element;
        /// <summary>
        /// Amount of energy at the probed point
        /// </summary>
        public int amount;

        public EnergyProbeResult(Energy.Element element, int amount)
        {
            this.element = element;
            this.amount = amount;
        }
    }

    #endregion

    #region Unity internals

    protected override void Awake()
    {
        base.Awake();
        cost = new DefaultEnergyCost();
    }

    #endregion
}

// Result from executing an energy related operation/action
public enum EnergyActionResult
{
    /// <summary>
    /// Action succeeded
    /// </summary>
    Success,

    // Standard errors

    /// <summary>
    /// Target is too far away
    /// </summary>
    OutsideRange,
    /// <summary>
    /// User doesn't hold enough energy
    /// </summary>
    NotEnoughEnergy,
    /// <summary>
    /// Trying to use non-existing energy
    /// </summary>
    InvalidManifestation,
    /// <summary>
    /// Trying to use non-existing object
    /// </summary>
    InvalidObject,

    // Specific case errors

    /// <summary>
    /// Example: transform into fire a manifestation that is already a fire element
    /// </summary>
    RedundantAction,
    /// <summary>
    /// The action is forbidden and cannot be executed
    /// </summary>
    ForbiddenAction,
    /// <summary>
    /// Internally the action was not successful (unknown error - see actual implementation section)
    /// </summary>
    SubactionFailed,
    /// <summary>
    /// Two manifestations/objects are not colliding
    /// </summary>
    NoContact,
    /// <summary>
    /// Trying to extract too much from manifestation
    /// </summary>
    ExtractingTooMuch,

    // Unknown error

    /// <summary>
    /// Undefined error occurred
    /// </summary>
    UndefinedError,
}
