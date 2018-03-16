using UnityEngine;

/// <summary>
/// A physically manifested energy
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public partial class EnergyManifestation : EnergyUser
{
    #region Organization info

    //This calls is too big for one file, thus it's separated into 'modules'
    //Each module has Start() and LateUpdate() methods and mostly works on itself
    //Methods like _Unity_Start() are accessible through other manifestation modules
    //Methods like __Collision_EstimateIntersectionVolume() are only accessible through the same module

#if DEBUG

    public ManifestationDebugOverview __dbgOverview { get { return new ManifestationDebugOverview(this); } }

#endif

    #endregion

    #region Unity initialization

    private void _Unity_Start()
    { }

    private void _Unity_FixedUpdate()
    { }

    #endregion

    #region Unity interface

    protected void OnEnable()
    {
        //_Unity_OnEnable();

        _Physics_OnEnable();
        //_Manipulation_OnEnable();
        _Charge_OnEnable();
        //_Collision_OnEnable();

        //_Particles_OnEnable();
        //_Visuals_OnEnable();
    }

    protected void Start()
    {
        _Unity_Start();

        _Physics_Start();
        _Manipulation_Start();
        _Charge_Start();
        _Collision_Start();

        _Particles_Start();
        _Visuals_Start();
    }

    protected void FixedUpdate()
    {
        _Unity_FixedUpdate();

        _Physics_FixedUpdate();
        _Manipulation_FixedUpdate();
        _Charge_FixedUpdate();
        _Collision_FixedUpdate();

        _Particles_FixedUpdate();
        _Visuals_FixedUpdate();
    }

    #endregion
}

#if DEBUG
public struct ManifestationDebugOverview
{
    public int energy;
    public Energy.Element element;
    public Energy.Shape shape;
    public EnergyHolder owner;

    public ManifestationDebugOverview(EnergyManifestation manif)
    {
        energy = manif.GetEnergy();
        element = manif.element;
        shape = manif.shape;
        owner = manif.holder.ResolveOwner();
    }
}

#endif
