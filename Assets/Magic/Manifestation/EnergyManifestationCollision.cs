using System.Collections.Generic;
using UnityEngine;

public partial class EnergyManifestation
{
    #region Collision info
    
    //All properties like: volume, mass, density, temperature, ...
    //  are kept the same from the start of the frame and are changed only at the end of the frame (LateUpdate).
    //(See the 'Charge' module for the implementation of this.)
    //This makes collision resolution deterministic even with non-deterministic resolution order.
    //Important: The 'charge' property is changed during the frame. To get the proper charge for collision resolution use 'm_PreviousEnergy'.

    #endregion

    #region Members

    /// <summary>
    /// Event type for collision events (see collision related events in this class)
    /// </summary>
    public enum CollisionEventType { Enter, Exit }

    /// <summary>
    /// Manifestation collider
    /// </summary>
    public new Collider collider;

    /// <summary>
    /// List of all objects currently colliding with the manifestation
    /// </summary>
    [SerializeField]
    public List<GameObject> collidedObjects = new List<GameObject>();

    #endregion

    #region Public events

    /// <summary>
    /// Event fired when any object collides with the manifestation.
    /// </summary>
    public SpellObjectCollisionEvent objectCollisionEvent = new SpellObjectCollisionEvent();

    /// <summary>
    /// Event fired when the manifestation collides with another manifestation.
    /// </summary>
    public SpellEnergyCollisionEvent energyCollisionEvent = new SpellEnergyCollisionEvent();

    /// <summary>
    /// Event fired when a unit collides with the manifestation.
    /// </summary>
    public SpellUnitCollisionEvent unitCollisionEvent = new SpellUnitCollisionEvent();

    #endregion

    #region Helpers & tools

    /// <summary>
    /// Returns if the manifestation is currently colliding with the object
    /// </summary>
    public bool IsColliding(GameObject obj)
    {
        return collidedObjects.Contains(obj);
    }

    /// <summary>
    /// Returns if the manifestation is currently colliding with the component
    /// </summary>
    public bool IsColliding<T>(T component) where T : MonoBehaviour
    {
        return IsColliding(component.gameObject);
    }

    /// <summary>
    /// Returns if the manifestation or any child of it is currently colliding with the object
    /// </summary>
    public bool IsCollidingRecursive(GameObject obj)
    {
        if (IsColliding(obj))
        {
            return true;
        }

        foreach (var child in holder.ownedEnergies)
        {
            var childManifestation = child.GetComponent<EnergyManifestation>();
            if (childManifestation != null && childManifestation.IsCollidingRecursive(obj))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns if the manifestation or any child of it is currently colliding with the component
    /// </summary>
    public bool IsCollidingRecursive<T>(T component) where T : MonoBehaviour
    {
        return IsCollidingRecursive(component.gameObject);
    }

    /// <summary>
    /// Esimate the amount of volume that intersects during a collision
    /// </summary>
    private float __Collision_EstimateIntersectionVolume(EnergyCollisionInfo info)
    {
        //If the other collider is big enough intersecting volume is estimated by simulating a AABB (with the same volume) colliding with AA wall (at the same speed)
        //Otherwise it's the other way around
        
        var myVolume = lastFrameProperties.volume;
        var smallerVolume = Mathf.Min(myVolume, info.EstimateVolume());
        var intersectionDepth = info.relativeVelocity.magnitude * Time.fixedDeltaTime;
        var newCubeWidth = Mathf.Pow(smallerVolume, 1.0f / 3.0f) - intersectionDepth * 2;
        return myVolume - Mathf.Clamp(newCubeWidth * newCubeWidth * newCubeWidth, 0.0f, myVolume);
    }

    /// <summary>
    /// Esimate the amount of energy that intersects during a collision
    /// </summary>
    private int __Collision_EstimeIntersectionEnergy(EnergyCollisionInfo info)
    {
        var intersectingVolume = __Collision_EstimateIntersectionVolume(info);
        
        var lesserDensity = Mathf.Min(lastFrameProperties.density, info.EstimateDensity());
        var intersectingMass = intersectingVolume * lesserDensity;
        if (float.IsNaN(intersectingMass) || float.IsInfinity(intersectingMass))
        {
            return 0;
        }
        else
        {
            var intersectingEnergy = (intersectingMass * Energy.Scalef) / (EnergyPhysics.MassPerUnit(lastFrameProperties.element) * lorentzFactor);
            return (int)intersectingEnergy;
        }
    }

    #endregion

    #region Per-object-type collision handling

    /// <summary>
    /// Applies the current elemental effect to a unit during collision
    /// </summary>
    private void __Collision_ApplyElementalEffectToUnit(Unit unit)
    { }

    /// <summary>
    /// Handle collision with unit
    /// </summary>
    private void __Collision_CollideWithUnit(EnergyCollisionInfo info)
    {
        var myOwnerUnit = holder.ResolveOwner().GetComponent<Unit>();
        if (myOwnerUnit != null && !myOwnerUnit.CanAttack(info.unit))
        {
            //Cannot attack unit
            return;
        }

        __Collision_ApplyElementalEffectToUnit(info.unit);

        var passThrough = EnergyPhysics.ElementIsPassThrough(lastFrameProperties.element);
        if (!passThrough)
        {
            //If I am solid - nothing happens
            return;
        }

        //Unfriendly unit - deal damage, lose energy
        var energyLost = __Collision_EstimeIntersectionEnergy(info);
        var damage = Energy.DamagePerUnit(lastFrameProperties.element) * energyLost / Energy.Scalef;
        info.unit.DealDamage((int)damage, gameObject);
        DecreaseEnergy(energyLost);
    }

    /// <summary>
    /// Handle collision with another manifestation
    /// </summary>
    /// <param name="info"></param>
    private void __Collision_CollideWithManifestation(EnergyCollisionInfo info)
    {
        var myOwner = holder.ResolveOwner();
        var otherOwner = info.manifestation.holder.ResolveOwner();
        if (myOwner == otherOwner)
        {
            //Same owner - cannot attack
            return;
        }

        var myOwnerUnit = myOwner.GetComponent<Unit>();
        if (myOwnerUnit != null)
        {
            var otherOwnerUnit = otherOwner.GetComponent<Unit>();
            if (!myOwnerUnit.CanAttack(otherOwnerUnit))
            {
                //Cannot attack other
                return;
            }
        }

        //Destructive interaction
        var passThrough = EnergyPhysics.ElementIsPassThrough(lastFrameProperties.element);
        var otherPassThrough = info.IsPassThrough();

        if (passThrough)
        {
            //I am pass through - lose energy
            var energyLost = __Collision_EstimeIntersectionEnergy(info);
            DecreaseEnergy(energyLost);
        }
        else if (otherPassThrough) //&& !passThrough)
        {
            //I am solid & other is pass though - apply force by the interaction
            var newVelocity = __Collision_CalculateNewVelocity(info);
            var impulseReceived = lastFrameProperties.mass * (newVelocity - rigidbody.velocity);
            var smashImpulse = EnergyPhysics.SmashImpulse(element) * lastFrameProperties.mass;
            if (impulseReceived.sqrMagnitude > smashImpulse * smashImpulse)
            {
                Smash();
            }
            else
            {
                ApplyForce(impulseReceived, ForceMode.Impulse);
            }
        }
    }

    /// <summary>
    /// Handle collision with solid body (eg. movable rock)
    /// </summary>
    private void __Collision_CollideWithSolidBody(EnergyCollisionInfo info)
    {
        __Collision_CollideWithStatic(info);
    }

    /// <summary>
    /// Handle collision with static body (eg. terrain)
    /// </summary>
    private void __Collision_CollideWithStatic(EnergyCollisionInfo info)
    {
        //Destructive interaction with random solid
        //If I am solid:
        //1) smashing is handled in calling fn.
        //2) collision is resolved by the physics engine
        //If I am pass-through:
        //Calculate volume lost & lose that energy instantly

        var passThrough = EnergyPhysics.ElementIsPassThrough(lastFrameProperties.element);
        if (!passThrough)
        {
            return;
        }
        
        var energyLost = __Collision_EstimeIntersectionEnergy(info);
        DecreaseEnergy(energyLost);
    }

    #endregion

    #region General collision handling

    /// <summary>
    /// Calculate the velocity after the collision has been resolved
    /// </summary>
    private Vector3 __Collision_CalculateNewVelocity(EnergyCollisionInfo info)
    {
        if (info.manifestation != null)
        {
            var otherBody = info.rigidbody;
            var otherMass = info.manifestation.lastFrameProperties.mass;
            return (rigidbody.velocity * (rigidbody.mass - otherMass) + (2 * otherMass * otherBody.velocity)) / (rigidbody.mass + otherMass);
        }
        else if (info.rigidbody != null)
        {
            var otherBody = info.rigidbody;
            var otherMass = otherBody.mass;
            return (rigidbody.velocity * (rigidbody.mass - otherMass) + (2 * otherMass * otherBody.velocity)) / (rigidbody.mass + otherMass);
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Handle general collision (start)
    /// </summary>
    private void __Collision_HandleCollisionEnter(EnergyCollisionInfo info)
    {
        //Note: See https://answers.unity.com/questions/696068/difference-between-forcemodeforceaccelerationimpul.html

        //Add to collided list & emit events
        collidedObjects.Add(info.collider.gameObject);
        SendMessage("OnObjectEnterCollision", info.collider.gameObject, SendMessageOptions.DontRequireReceiver);
        objectCollisionEvent.Invoke(this, info.collider.gameObject, CollisionEventType.Enter);

        //Check smashing conditions
        if (!collider.isTrigger && !info.collider.isTrigger && info.rigidbody != null)
        {
            var newVelocity = __Collision_CalculateNewVelocity(info);
            var impulseReceived = rigidbody.mass * (newVelocity - rigidbody.velocity);
            var smashImpulse = EnergyPhysics.SmashImpulse(element) * rigidbody.mass;
            if (impulseReceived.sqrMagnitude > smashImpulse * smashImpulse)
            {
                Smash();
                return;
            }
        }
        
        if (info.unit != null)
        {
            SendMessage("OnUnitEnterCollision", info.unit, SendMessageOptions.DontRequireReceiver);
            unitCollisionEvent.Invoke(this, info.unit, CollisionEventType.Enter);
        }
        else if (info.manifestation != null)
        {
            SendMessage("OnEnergyEnterCollision", info.manifestation, SendMessageOptions.DontRequireReceiver);
            energyCollisionEvent.Invoke(this, info.manifestation, CollisionEventType.Enter);
        }
    }

    /// <summary>
    /// Handle general collision (stay)
    /// </summary>
    private void __Collision_HandleCollisionStay(EnergyCollisionInfo info)
    {
        if (info.unit != null)
        {
            __Collision_CollideWithUnit(info);
        }
        else if (info.manifestation != null)
        {
            __Collision_CollideWithManifestation(info);
        }
        else if (!info.IsPassThrough())
        {
            if (info.rigidbody != null)
            {
                __Collision_CollideWithSolidBody(info);
            }
            else
            {
                __Collision_CollideWithStatic(info);
            }
        }
    }

    /// <summary>
    /// Handle general collision (finish)
    /// </summary>
    private void __Collision_HandleCollisionExit(EnergyCollisionInfo info)
    {
        //Remove from collided lists & emit events
        collidedObjects.Remove(info.collider.gameObject);
        SendMessage("OnObjectExitCollision", info.collider.gameObject, SendMessageOptions.DontRequireReceiver);
        objectCollisionEvent.Invoke(this, info.collider.gameObject, CollisionEventType.Exit);
        
        if (info.unit != null)
        {
            SendMessage("OnUnitExitCollision", info.unit, SendMessageOptions.DontRequireReceiver);
            unitCollisionEvent.Invoke(this, info.unit, CollisionEventType.Exit);
        }
        else if (info.manifestation != null)
        {
            SendMessage("OnEnergyExitCollision", info.manifestation, SendMessageOptions.DontRequireReceiver);
            energyCollisionEvent.Invoke(this, info.manifestation, CollisionEventType.Exit);
        }
    }

    #endregion

    #region Internals & Unity interface

    /// <summary>
    /// Update collider according to shape & element
    /// </summary>
    private void _Collision_UpdateCollider()
    {
        switch (shape)
        {
            case Energy.Shape.Sphere:
                if (collider is SphereCollider) break; //Already has correct collider
                if (collider != null) Util.Destroy(collider); //Destroy incorrect collider

                { //Attach & setup correct collider
                    var sphere = gameObject.AddComponent<SphereCollider>();
                    sphere.radius = 0.62035f; //sphere with volume 1 cubic meter
                    collider = sphere;
                }
                break;
            case Energy.Shape.Cube:
                if (collider is BoxCollider) break; //Already has correct collider
                if (collider != null) Util.Destroy(collider); //Destroy incorrect collider

                { //Attach & setup correct collider
                    var box = gameObject.AddComponent<BoxCollider>();
                    box.size = Vector3.one;
                    collider = box;
                }
                break;

            case Energy.Shape.Capsule:
                if (collider is CapsuleCollider) break; //Already has correct collider
                if (collider != null) Util.Destroy(collider);

                { //Attach & setup correct collider
                    var capsule = gameObject.AddComponent<CapsuleCollider>();
                    capsule.radius = 0.45708f;
                    capsule.height = 2 * capsule.radius;
                    collider = capsule;
                }
                break;

            default:
                if (collider is MeshCollider)
                {
                    //Already has correct collider
                    (collider as MeshCollider).sharedMesh = EnergyVisuals.FindCollider(shape);
                    break;
                }
                if (collider != null) Util.Destroy(collider); //Destroy incorrect collider

                { //Attach & setup correct collider
                    var meshc = gameObject.AddComponent<MeshCollider>();
                    meshc.sharedMesh = EnergyVisuals.FindCollider(shape);
                    meshc.convex = true;
                    collider = meshc;
                }
                break;
        }

        collider.isTrigger = EnergyPhysics.ElementIsPassThrough(element);
        rigidbody.useGravity = EnergyPhysics.BodyUsesGravity(element);
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; //TODO decide if this is needed
    }

    private void OnTriggerEnter(Collider other) { __Collision_HandleCollisionEnter(new EnergyCollisionInfo(this, other)); }
    private void OnTriggerStay(Collider other) { __Collision_HandleCollisionStay(new EnergyCollisionInfo(this, other)); }
    private void OnTriggerExit(Collider other) { __Collision_HandleCollisionExit(new EnergyCollisionInfo(this, other)); }
    private void OnCollisionEnter(Collision collision) { __Collision_HandleCollisionEnter(new EnergyCollisionInfo(this, collision.collider, collision.relativeVelocity)); }
    private void OnCollisionStay(Collision collision) { __Collision_HandleCollisionStay(new EnergyCollisionInfo(this, collision.collider, collision.relativeVelocity)); }
    private void OnCollisionExit(Collision collision) { __Collision_HandleCollisionExit(new EnergyCollisionInfo(this, collision.collider, collision.relativeVelocity)); }

    void _Collision_Start()
    { }
	
	void _Collision_FixedUpdate()
    { }

    #endregion
}

/// <summary>
/// Container for collision data.
/// </summary>
class EnergyCollisionInfo
{
    public Collider collider;
    public EnergyManifestation manifestation;
    public GameObject gameObject;
    public Transform transform;
    public Rigidbody rigidbody;
    public Vector3 relativeVelocity;
    public Unit unit;

    public EnergyCollisionInfo(EnergyManifestation originalManifestation, Collider collider)
    {
        FetchComponents(collider);
        relativeVelocity = CalculateRelativeVelocity(originalManifestation);
    }

    public EnergyCollisionInfo(EnergyManifestation originalManifestation, Collider collider, Vector3 relativeVelocity)
    {
        FetchComponents(collider);
        this.relativeVelocity = relativeVelocity;
    }

    public float EstimateVolume()
    {
        if (manifestation != null)
        {
            return manifestation.lastFrameProperties.volume;
        }
        else if (collider != null)
        {
            var colliderSize = collider.bounds.size;
            return colliderSize.x * colliderSize.y * colliderSize.z;
        }
        else
        {
            return 0.0f;
        }
    }

    public float EstimateDensity()
    {
        if (manifestation != null)
        {
            return manifestation.lastFrameProperties.density;
        }
        else if (rigidbody != null)
        {
            return rigidbody.mass / EstimateVolume();
        }
        else
        {
            return float.PositiveInfinity;
        }
    }

    public bool IsPassThrough()
    {
        if (manifestation != null)
        {
            return EnergyPhysics.ElementIsPassThrough(manifestation.lastFrameProperties.element);
        }
        else if (collider != null)
        {
            return collider.isTrigger;
        }
        else
        {
            return true;
        }
    }

    private void FetchComponents(Collider collider)
    {
        gameObject = collider.gameObject;
        this.collider = collider;
        manifestation = gameObject.GetComponent<EnergyManifestation>();
        transform = gameObject.transform;
        rigidbody = gameObject.GetComponent<Rigidbody>();
        unit = gameObject.GetComponent<Unit>();
    }

    private Vector3 CalculateRelativeVelocity(EnergyManifestation originalManifestation)
    {
        if (rigidbody == null)
        {
            return originalManifestation.rigidbody.velocity;
        }

        return originalManifestation.rigidbody.velocity - rigidbody.velocity;
    }
}
