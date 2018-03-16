using System;
using UnityEngine;

public partial class EnergyManifestation
{
    #region Members

    /// <summary>
    /// Particles object
    /// </summary>
    public GameObject particles;

    #endregion

    #region Particle triggers

    /// <summary>
    /// Triggers particles for when smashing
    /// </summary>
    private void _Particles_TriggerSmashParticles()
    {
        __Particles_ChangeParticlePrefab(SmashParticlesGetter, false);
    }

    /// <summary>
    /// Triggers particles for when disposing
    /// </summary>
    private void _Particles_TriggerDisposeParticles()
    {
        __Particles_ChangeParticlePrefab(DisposeParticlesGetter, false);
    }
    
    #endregion

    #region Internals & particles management
    
    //TODO: see if future properties or current properties should be used

    private void __Particles_ClearParticles()
    {
        if (particles != null)
        {
            Util.Destroy(particles);
            particles = null;
        }
    }

    private void __Particles_UpdateParticlesShapes()
    {
        if (particles == null)
        {
            return;
        }

        //Set up all particle systems' shapes
        var particleSystems = particles.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var shapeObj = ps.shape;
            if (shapeObj.enabled)
            {
                shapeObj.shapeType = Energy.GetShape(shape).particlesShape;
                shapeObj.scale = transform.localScale;
            }
        }
    }

    private void __Particles_CreateParticles(GameObject prefab, bool attach = true)
    {
        //Instanciate prefab
        if (attach)
        {
            particles = Instantiate(prefab, transform);
        }
        else
        {
            particles = Instantiate(prefab, transform.position, transform.rotation);
            particles.transform.localScale = transform.localScale;
        }

        particles.name = prefab.name;

        __Particles_UpdateParticlesShapes();
    }

    private bool __Particles_ChangeParticlePrefab(Func<Energy.Element, GameObject> fn, bool attach = true)
    {
        var prefab = fn(element);

        //No prefab found (element doesn't have such particles)
        if (prefab == null)
        {
            __Particles_ClearParticles();
            return false;
        }

        //We have particles already
        if (particles != null)
        {
            //Check if they need to be changed
            if (prefab.name != particles.name)
            {
                __Particles_ClearParticles();
                __Particles_CreateParticles(prefab, attach);
                return true;
            }
        }
        else //We don't have particles
        {
            __Particles_CreateParticles(prefab, attach);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Update manifestation visualization (attached particles only)
    /// </summary>
    private void _Particles_UpdateParticles()
    {
        var newParticlesCreated = __Particles_ChangeParticlePrefab(IdleParticlesGetter, true);
        if (!newParticlesCreated)
        {
            //New particles would have already had their shapes set
            __Particles_UpdateParticlesShapes();
        }
    }

    private void _Particles_Start()
    { }

    private void _Particles_FixedUpdate()
    { }

    #endregion

    #region Particle prefab getters

    private static GameObject SmashParticlesGetter(Energy.Element element)
    {
        return Energy.GetElement(element).smashParticles.prefab;
    }

    private static GameObject DisposeParticlesGetter(Energy.Element element)
    {
        return Energy.GetElement(element).disposeParticles.prefab;
    }

    private static GameObject IdleParticlesGetter(Energy.Element element)
    {
        return Energy.GetElement(element).idleParticles.prefab;
    }

    #endregion
}
