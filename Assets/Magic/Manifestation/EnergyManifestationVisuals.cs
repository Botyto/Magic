using UnityEngine;

public partial class EnergyManifestation
{
    #region Members

    /// <summary>
    /// If visuals have been marked as dirty (energy/element/shape/temperature has changed)
    /// </summary>
    private bool m_VisualsDirty = false;

    #endregion

    #region Internals

    /// <summary>
    /// Marks visuals as dirty (will be update at end of frame - next LateUpdate)
    /// </summary>
    private void _Visuals_SetVisualsDirty()
    {
        m_VisualsDirty = true;
    }

    /// <summary>
    /// Update manifestation visualization (only visual parts)
    /// </summary>
    private void __Visuals_UpdateRenderingSettings()
    {
        GetComponent<MeshFilter>().sharedMesh = EnergyVisuals.FindMesh(shape);
        GetComponent<MeshRenderer>().sharedMaterial = EnergyVisuals.FindMaterial(element);
    }

    /// <summary>
    /// Updates manifestation visualization (both phyisical and visual parts)
    /// </summary>
    private void __Visuals_UpdateVisuals()
    {
        m_VisualsDirty = false;

        //Optimize
        _Collision_UpdateCollider();
        __Visuals_UpdateRenderingSettings();
        _Particles_UpdateParticles();
    }

    #endregion

    #region Unity interface

    private void _Visuals_Start()
    {
        __Visuals_UpdateVisuals();
    }

    private void _Visuals_FixedUpdate()
    {
        if (m_VisualsDirty)
        {
            __Visuals_UpdateVisuals();
        }
    }

    #endregion
}
