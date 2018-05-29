using UnityEngine;

/// <summary>
/// This is a class with useful methods for using spells.
/// It adds nothing to the spell logic, but instead provides a useful interface for using focused manifestations and others.
/// See deriving classes for actual spell categories and implementations.
/// When implementing a new type of generic spell type - remember to catch exceptions!
/// </summary>
public class SpellComponent : SpellComponentBase
{
    #region Target

    /// <summary>
    /// Get spell target position
    /// If no target, the cursor position will be returned.
    /// </summary>
    public Vector3 GetTargetPosition()
    {
        if (target == null)
        {
            return CursorPosition;
        }

        return target.transform.position;
    }

    /// <summary>
    /// Position controlled by the player cursor
    /// </summary>
    public Vector3 CursorPosition
    {
        get
        {
            var playerMovement = wizard.GetComponent<PlayerMovement>();
            return playerMovement != null ? playerMovement.cursorPosition : wizard.transform.position;
        }
    }

    #endregion

    #region Focus

    /// <summary>
    /// Manifest energy and focus it
    /// </summary>
    /// <returns>Focus handle (-1 if failed)</returns>
    public EnergyActionResult ManifestEnergyAndFocusAbsolute(int amount, Vector3 position, out int focusHandle)
    {
        return ManifestEnergyAndFocus(amount, wizard.transform.InverseTransformPoint(position), out focusHandle);
    }

    /// <summary>
    /// Manifest energy and focus it
    /// </summary>
    /// <returns>Focus handle (-1 if failed)</returns>
    public EnergyActionResult ManifestEnergyAndFocusAbsolute(int amount, Vector3 position, Energy.Element element, Energy.Shape shape, out int focusHandle)
    {
        return ManifestEnergyAndFocus(amount, wizard.transform.InverseTransformPoint(position), element, shape, out focusHandle);
    }

    /// <summary>
    /// Manifest energy and focus it
    /// </summary>
    /// <returns>Focus handle (-1 if failed)</returns>
    public EnergyActionResult ManifestEnergyAndFocus(int amount, Vector3 wizardRelativePosition, out int focusHandle)
    {
        return ManifestEnergyAndFocus(amount, wizardRelativePosition, param.element, param.shape, out focusHandle);
    }

    /// <summary>
    /// Manifest energy and focus it
    /// </summary>
    /// <returns>Focus handle (-1 if failed)</returns>
    public EnergyActionResult ManifestEnergyAndFocus(int amount, Vector3 wizardRelativePosition, Energy.Element element, Energy.Shape shape, out int focusHandle)
    {
        //Check ahead of time
        if (!CanFocusMore())
        {
            focusHandle = -1;
            return EnergyActionResult.ForbiddenAction;
        }

        //Do manifest
        EnergyManifestation manifestation;
        var result = controller.ManifestEnergy(amount, wizardRelativePosition, element, shape, out manifestation);
        if (!TryStrict(result))
        {
            if (manifestation != null)
            {
                manifestation.Dispose();
            }

            focusHandle = -1;
            return result;
        }

        //Success
        focusHandle = AddFocus(manifestation);
        return result;
    }

    /// <summary>
    /// Get focused energy transform.
    /// If focus is invalid, null will be returned.
    /// </summary>
    public Transform GetFocusTransform(int focusHandle)
    {
        var manif = GetFocus(focusHandle);
        if (manif == null)
        {
            return null;
        }

        return manif.transform;
    }

    /// <summary>
    /// Get focused energy position.
    /// If focus is invalid, Vector3.zero will be returned.
    /// </summary>
    public Vector3 GetFocusPosition(int focusHandle)
    {
        var manif = GetFocus(focusHandle);
        if (manif == null)
        {
            return Vector3.zero;
        }

        return manif.transform.position;
    }

    /// <summary>
    /// Get focus velocity.
    /// If focus is invalid, Vector3.zero will be returned.
    /// </summary>
    public Vector3 GetFocusVelocity(int focusHandle)
    {
        var manif = GetFocus(focusHandle);
        if (manif == null)
        {
            return Vector3.zero;
        }

        return manif.rigidbody.velocity;
    }

    /// <summary>
    /// Remove a manifestation from the focus list & disowns it
    /// </summary>
    public bool DisownFocus(int focusHandle)
    {
        return DisownFocus(GetFocus(focusHandle));
    }
    
    #endregion

    #region Focus standard manipulation

    /// <summary>
    /// Sacrifice a manifestation to summon an object in it's place
    /// </summary>
    public EnergyActionResult Summon(int focusHandle, SummonRecipe recipe, out GameObject summonedObj)
    {
        return controller.Summon(GetFocus(focusHandle), recipe, out summonedObj);
    }

    /// <summary>
    /// Remotely charges an energy manifestation (cheaper than manifesting & merging).
    /// </summary>
    public EnergyActionResult Charge(int focusHandle, int amount)
    {
        return controller.Charge(GetFocus(focusHandle), amount);
    }

    /// <summary>
    /// Remotely discharges an energy manifestation.
    /// </summary>
    public EnergyActionResult Discharge(int focusHandle, int amount)
    {
        return controller.Discharge(GetFocus(focusHandle), amount);
    }

    /// <summary>
    /// Marge two manifestations
    /// </summary>
    public EnergyActionResult Merge(int focusHandleA, int focusHandleB)
    {
        return controller.Merge(GetFocus(focusHandleA), GetFocus(focusHandleB));
    }

    /// <summary>
    /// Split manifestation into two manifestations
    /// </summary>
    public EnergyActionResult Separate(int focusHandle, int amount, Vector3 force, ForceMode forceMode, out int separatedEnergyFocusHandle)
    {
        if (!CanFocusMore())
        {
            separatedEnergyFocusHandle = -1;
            return EnergyActionResult.ForbiddenAction;
        }

        EnergyManifestation separatedManif;
        var result = controller.Separate(GetFocus(focusHandle), amount, force, forceMode, out separatedManif);
        if (!TryStrict(result))
        {
            separatedEnergyFocusHandle = -1;
            return result;
        }

        separatedEnergyFocusHandle = AddFocus(separatedManif);
        return result;
    }

    /// <summary>
    /// Change manifestations' element
    /// </summary>
    public EnergyActionResult ChangeElement(int focusHandle, Energy.Element newElement)
    {
        return controller.ChangeElement(GetFocus(focusHandle), newElement);
    }

    /// <summary>
    /// Change manifestations' shape
    /// </summary>
    public EnergyActionResult ChangeShape(int focusHandle, Energy.Shape newShape)
    {
        return controller.ChangeShape(GetFocus(focusHandle), newShape);
    }

    /// <summary>
    /// Deform a manifestation
    /// </summary>
    public EnergyActionResult Deform(int focusHandle, Vector3 stress)
    {
        return controller.Deform(GetFocus(focusHandle), stress);
    }

    /// <summary>
    /// Create an elastic joint between this and another manifestation.
    /// </summary>
    public EnergyActionResult CreateElasticConnection(int focusHandle1, int focusHandle2, int connectionCharge)
    {
        return controller.CreateElasticConnection(GetFocus(focusHandle1), GetFocus(focusHandle2), connectionCharge);
    }

    /// <summary>
    /// Apply physical force to manifestation (at center of mass). Force vector is relaive to the controller's transform.
    /// </summary>
    public EnergyActionResult ApplyForceRelative(int focusHandle, Vector3 relativeForce, ForceMode mode)
    {
        return ApplyForce(focusHandle, transform.TransformDirection(relativeForce), mode);
    }

    /// <summary>
    /// Apply physical force to manifestation (at center of mass)
    /// </summary>
    public EnergyActionResult ApplyForce(int focusHandle, Vector3 force, ForceMode mode)
    {
        return controller.ApplyForce(GetFocus(focusHandle), force, mode);
    }

    /// <summary>
    /// Apply physical torque to manifestation (at center of mass)
    /// </summary>
    public EnergyActionResult ApplyTorque(int focusHandle, Vector3 torque, ForceMode mode)
    {
        return controller.ApplyTorque(GetFocus(focusHandle), torque, mode);
    }

    /// <summary>
    /// Forcefully and instantly orient a manifestation to look at a specific point.
    /// This does not affect angular momentum.
    /// </summary>
    public EnergyActionResult OrientTowards(int focusHandle, Vector3 lookat)
    {
        return controller.OrientTowards(GetFocus(focusHandle), lookat);
    }

    /// <summary>
    /// Forcefully and instantly orient a manifestation to look at a specific object.
    /// This does not affect angular momentum.
    /// </summary>
    public EnergyActionResult OrientTowards(int focusHandle, GameObject lookat)
    {
        return controller.OrientTowards(GetFocus(focusHandle), lookat.transform.position);
    }

    /// <summary>
    /// Apply an aura to some object
    /// </summary>
    public EnergyActionResult ApplyAura<T>(int focusHandle, GameObject obj, int extractedEnergy, out T aura) where T : AuraBase
    {
        return controller.ApplyAura(GetFocus(focusHandle), obj, extractedEnergy, out aura);
    }

    /// <summary>
    /// Substitute self with an object
    /// </summary>
    public EnergyActionResult Substitute(int focusHandle, GameObject obj)
    {
        return Substitute(focusHandle, controller.gameObject, obj);
    }

    /// <summary>
    /// Substitute two objects
    /// </summary>
    public EnergyActionResult Substitute(int focusHandle, GameObject first, GameObject second)
    {
        return controller.Substitute(GetFocus(focusHandle), first, second);
    }

    #endregion

    #region Focus extended manipulation

    /// <summary>
    /// Apply a force on a manifestation, such that it will keep the manifestation from falling down (gravity + down motion).
    /// </summary>
    public EnergyActionResult CounterFalling(int focusHandle)
    {
        var manif = GetFocus(focusHandle);
        if (manif == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        var velocity = manif.rigidbody.velocity;
        if (velocity.y >= 0.0f)
        {
            return EnergyActionResult.RedundantAction;
        }

        return ApplyForce(focusHandle, Vector3.up * velocity.y, ForceMode.Force); //TODO check if this is correct
    }

    /// <summary>
    /// Apply a force on a manifestation, such that it will make the manifestation seem as if it were not affected by gravity.
    /// </summary>
    public EnergyActionResult CounterGravity(int focusHandle)
    {
        var manif = GetFocus(focusHandle);
        if (manif == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        if (!manif.rigidbody.useGravity)
        {
            return EnergyActionResult.RedundantAction;
        }

        return ApplyForce(focusHandle, -Physics.gravity, ForceMode.Acceleration);
    }

    /// <summary>
    /// Apply a force on a manifestation, such that it will counter any motion it currently has
    /// </summary>
    public EnergyActionResult CounterMotion(int focusHandle)
    {
        var manif = GetFocus(focusHandle);
        if (GetFocus(focusHandle) == null)
        {
            return EnergyActionResult.InvalidManifestation;
        }

        return ApplyForce(focusHandle, manif.rigidbody.velocity * manif.rigidbody.mass, ForceMode.Impulse);
    }

    #endregion
}
