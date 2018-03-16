using UnityEngine;

public partial class EnergyManifestation
{
    #region Members

    /// <summary>
    /// Manifestation original volume (depends on element and energy held)
    /// This is the volume that would be reached if no deformation is applied.
    /// See currentVolume, for volume that takes into account the deformation.
    /// </summary>
    public float originalVolume;

    /// <summary>
    /// Manifestation current volume (depends on element, energy held and deformation)
    /// See originalVolume, for volume that would be reached if no deformation is applied.
    /// </summary>
    public float actualVolume { get { return originalVolume * deformation.x * deformation.y * deformation.z; } }

    /// <summary>
    /// Manifestation current density
    /// </summary>
    public float density { get { return rigidbody.mass / actualVolume; } }

    /// <summary>
    /// Phyisical deformation
    /// Note: Never write here from outside!
    /// </summary>
    public Vector3 deformation = Vector3.one;

    /// <summary>
    /// Rigidbody component
    /// </summary>
    public new Rigidbody rigidbody;

    /// <summary>
    /// This is where the Deform() manipulation will store all of it's input
    /// </summary>
    private Vector3 m_DeformationAccumulator = Vector3.zero;

    /// <summary>
    /// If orientation is locked to velocity or the body can rotate freely.
    /// Orientation can be unlocked by applying torque to the manifestation.
    /// </summary>
    private bool m_OrientationLocked = true;

    /// <summary>
    /// Lorentz factor for calculating the relativistic mass of this manifestation
    /// </summary>
    public float lorentzFactor
    {
        get
        {
            //https://en.wikipedia.org/wiki/Lorentz_factor
            var sqrSpeed = rigidbody.velocity.sqrMagnitude;
            return 1.0f / Mathf.Sqrt(1 - sqrSpeed / Energy.SqrSpeedLimit);
        }
    }

    /// <summary>
    /// Physical momentum
    /// </summary>
    public Vector3 momentum { get { return rigidbody.velocity * lastFrameProperties.mass; } }

    #endregion

    #region Internals & Unity interface
    
    private Vector3 __Physics_ResolveDeformation(Vector3 stress)
    {
        var elasticity_coef = Energy.GetElement(element).elasticity;
        return new Vector3(
            __Physics_ResolveDeformation(elasticity_coef, stress.x),
            __Physics_ResolveDeformation(elasticity_coef, stress.y),
            __Physics_ResolveDeformation(elasticity_coef, stress.z));
    }

    private float __Physics_ResolveDeformation(float k, float stress)
    {
        //stress == 0 - no stress
        if (stress == 0.0f)
        {
            return 1.0f;
        }
        //stress > 0 means compressive
        else if (stress > 0.0f)
        {
            //stress = k/deformation - k
            //s = k/d - k
            //d = k / (s + k)
            return k / (stress + k);
        }
        //stress < 0 means tensile
        else if (stress < 0.0f)
        {
            //stress = k*exp((deformation - 1) * 1.5) - k
            //s = k*exp((d - 1) * 1.5) - k
            //d = 2/3 * logn(s/k + 1) + 1
            return (2.0f / 3.0f) * Mathf.Log(-stress / k + 1) + 1;
        }

        //Will never reach here :)
        return 1.0f;
    }

    private void _Physics_UpdatePhysicalProperties()
    {
        var elementDef = Energy.GetElement(futureElement);
        rigidbody.mass = elementDef.mass * GetEnergyScaledf() * lorentzFactor;
        originalVolume = elementDef.baseVolume + elementDef.volume * GetEnergyScaledf();
        transform.localScale = deformation * originalVolume;
    }

    private void _Physics_OnEnable()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void _Physics_Start()
    { }

    private void _Physics_FixedUpdate()
    {
        if (m_OrientationLocked && rigidbody.velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rigidbody.velocity, Vector3.up);
        }
        
        const float DeformationTolerance = 0.01f;
        if (holder.ResolveOwner().GetComponent<Unit>() != null && //TODO: this should happen instantly, as the manipulation happens
            (m_DeformationAccumulator != Vector3.zero ||
            Mathf.Abs(deformation.x - 1) > DeformationTolerance ||
            Mathf.Abs(deformation.y - 1) > DeformationTolerance ||
            Mathf.Abs(deformation.z - 1) > DeformationTolerance))
        {
            var newDeformation = __Physics_ResolveDeformation(m_DeformationAccumulator);
            transform.localScale = transform.localScale.MulDiv(newDeformation, deformation);
            deformation = newDeformation;
            m_DeformationAccumulator = Vector3.zero;
        }
    }

    #endregion
}
