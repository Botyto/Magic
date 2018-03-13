using UnityEngine;

public partial class EnergyManifestation
{
    #region Members

    /// <summary>
    /// An energy manifestation can be sealed and cannot be modified in most ways anymore.
    /// </summary>
    public bool @sealed;

    #endregion

    #region Lifetime

    /// <summary>
    /// Violently smash self
    /// </summary>
    public void Smash()
    {
        _Particles_TriggerSmashParticles();
        Util.Destroy(gameObject, "smash");
    }

    /// <summary>
    /// Dispose self (non-violent, opposite to smashing)
    /// </summary>
    public void Dispose()
    {
        _Particles_TriggerDisposeParticles();
        Util.Destroy(gameObject, "dispose");
    }

    /// <summary>
    /// Sacrifice self and summon an object
    /// </summary>
    public GameObject Summon(GameObject original)
    {
        var summonedObj = Instantiate(original, transform.position, transform.rotation);

        //Transfer some qualities & properties
        var summonedBody = summonedObj.GetComponent<Rigidbody>();
        if (summonedBody != null)
        {
            summonedBody.AddForce(rigidbody.velocity, ForceMode.VelocityChange);
        }

        Dispose();

        return summonedObj;
    }

    #endregion

    #region Merge & Separate

    /// <summary>
    /// Check if merging can be done with another manifestation
    /// </summary>
    public bool CanMergeWith(EnergyManifestation other)
    {
        Debug.Assert(other != null);
        return element == other.element && IsColliding(other.gameObject);
    }

    /// <summary>
    /// Marge self with another manifestation
    /// </summary>
    public bool Merge(EnergyManifestation other)
    {
        Debug.Assert(other != null);

        //Check conditions (TODO decide whether to uncomment)
        //if (!CanMergeWith(other))
        //{
        //    return false;
        //}

        IncreaseEnergy(other.GetEnergy());
        Util.Destroy(other.gameObject, "merged");

        //TODO recalculate velocity

        return true;
    }

    /// <summary>
    /// Split self into two manifestation
    /// </summary>
    /// <param name="force">Force applied (as impulse) to the separated manifestation</param>
    public EnergyManifestation Separate(int amount, Vector3 force, ForceMode forceMode)
    {
        amount = DecreaseEnergy(amount);
        if (amount == 0)
        {
            return null;
        }

        var newObj = new GameObject(gameObject.name + "(sep)");

        var newManifest = newObj.AddComponent<EnergyManifestation>();
        newManifest.element = element;
        newManifest.shape = shape;

        var newHolder = newManifest.holder;
        newHolder.energy = amount;
        newHolder.SetOwner(holder.ResolveOwner());

        var halfForce = force / 2.0f;
        newManifest.ApplyForce(halfForce, forceMode);
        ApplyForce(-halfForce, forceMode);

        return newManifest;
    }

    #endregion

    #region Element & Shape

    /// <summary>
    /// Get current element
    /// </summary>
    public Energy.Element GetElement()
    {
        return futureElement;
    }

    /// <summary>
    /// Change current element (at end of frame)
    /// </summary>
    public void ChangeElementLater(Energy.Element newElement)
    {
        futureElement = newElement;
        ChangeElementNow(newElement);
    }

    /// <summary>
    /// Change current element (at this instant)
    /// </summary>
    public void ChangeElementNow(Energy.Element newElement)
    {
        if (element == newElement)
        {
            return;
        }

        element = newElement;
        @sealed = Energy.IsSealingElement(element);
        _Visuals_SetVisualsDirty();

        //Conserve momentum
        if (rigidbody.velocity.sqrMagnitude > float.Epsilon * float.Epsilon)
        {
            _Physics_UpdatePhysicalProperties();
            rigidbody.velocity = momentum / rigidbody.mass;
        }
        else
        {
            _Physics_UpdatePhysicalProperties();
        }

        //Possibly unlock rotation
        m_OrientationLocked = m_OrientationLocked && EnergyPhysics.ElementIsPassThrough(futureElement);

        SendMessage("ElementChanged", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Get current shape
    /// </summary>
    public Energy.Shape GetShape()
    {
        return futureShape;
    }

    /// <summary>
    /// Change current shape (at end of frame)
    /// </summary>
    public void ChangeShapeLater(Energy.Shape newShape)
    {
        futureShape = newShape;
        ChangeShapeNow(newShape);
    }

    /// <summary>
    /// Change current shape (at this instant)
    /// </summary>
    public void ChangeShapeNow(Energy.Shape newShape)
    {
        if (shape == newShape)
        {
            return;
        }

        shape = newShape;
        _Visuals_SetVisualsDirty();
        SendMessage("ShapeChanged", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Deform manifestation
    /// </summary>
    public void Deform(Vector3 stress)
    {
        m_DeformationAccumulator += stress;
    }

    #endregion

    #region Physical forces

    /// <summary>
    /// Apply physical force (at center of mass)
    /// </summary>
    public void ApplyForce(Vector3 force, ForceMode mode)
    {
        rigidbody.AddForce(force, mode);
    }

    /// <summary>
    /// Apply physical torque (at center of mass)
    /// </summary>
    public void ApplyTorque(Vector3 torque, ForceMode mode)
    {
        //Adjust torque due to collision mechanics
        if (mode == ForceMode.Force || mode == ForceMode.Impulse)
        {
            torque *= rigidbody.mass / lastFrameProperties.mass;
        }

        rigidbody.AddTorque(torque, mode);
        m_OrientationLocked = false;
    }

    #endregion

    #region Aura & unit related manipulations

    /// <summary>
    /// Check if an aura can be applied to another object
    /// </summary>
    public bool CanApplyAura<T>(GameObject target)
    {
        return IsColliding(target);
    }

    /// <summary>
    /// Apply an aura to some object
    /// </summary>
    public T ApplyAura<T>(GameObject target, int extractedEnergy) where T : AuraBase
    {
        Debug.Assert(target != null, "No target to apply aura to");
        Debug.Assert(target.GetComponent<Unit>() != null, "Aura target must be a unit");

        //Check conditions (TODO decide whether to uncomment)
        //if (!CanApplyAura<T>(target))
        //{
        //    return null;
        //}

        //Target already has this aura
        if (target.gameObject.GetComponentInChildren<T>() != null)
        {
            return null;
        }

        //"Extract" energy
        extractedEnergy = DecreaseEnergy(extractedEnergy);
        if (extractedEnergy == 0)
        {
            //No energy extracted?
            return null;
        }

        //Create new aura
        var auraObject = new GameObject(typeof(T).Name);
        auraObject.transform.SetParent(target.transform);
        var newAura = auraObject.AddComponent<T>();
        newAura.holder.energy = extractedEnergy;
        newAura.target = target;
        newAura.holder.SetOwner(holder.ResolveOwner());

        return newAura;
    }

    /// <summary>
    /// Check if an object can take part in a substituition
    /// </summary>
    public bool CanSubstitute(GameObject obj)
    {
        //My owner
        var wizard = obj.GetComponent<Wizard>();
        if (wizard != null)
        {
            return wizard.holder == holder.ResolveOwner();
        }

        //Energy holder or unit
        if (obj.GetComponent<EnergyHolder>() != null || obj.GetComponent<Unit>() != null)
        {
            return false;
        }

        //Massless object
        var rigidbody = obj.GetComponent<Rigidbody>();
        if (rigidbody == null || rigidbody.mass <= 0.0f)
        {
            return false;
        }

        //Self or a child must collide with obj
        return IsCollidingRecursive(obj);
    }

    /// <summary>
    /// Substitute the position of two objects.
    /// </summary>
    public bool Substitute(GameObject targetA, GameObject targetB)
    {
        //Same object - ignore
        if (targetA == targetB)
        {
            return true;
        }

        //Any non-substitutable object
        if (!CanSubstitute(targetA) || !CanSubstitute(targetB))
        {
            return false;
        }

        //Both cannot be Wizards, because if one is a Wizard it must be my owner - thus they would be the same (see above)
        //var wizardA = targetA.GetComponent<Wizard>();
        //var wizardB = targetB.GetComponent<Wizard>();
        //if (wizardA != null && wizardB != null)
        //{
        //    return false;
        //}

        //Do position substitution
        var positionA = targetA.transform.position;
        targetA.transform.position = targetB.transform.position;
        targetB.transform.position = positionA;

        //Destroy self
        Dispose();

        //Succcess
        return true;
    }

    #endregion

    #region Unity interface

    private void _Manipulation_Start()
    { }

    private void _Manipulation_FixedUpdate()
    { }

    #endregion
}
