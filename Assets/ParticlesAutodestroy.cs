using UnityEngine;

public class ParticlesAutodestroy : MonoBehaviour
{
    public static ParticleSystem FindParticleSystem(Transform obj)
    {
        return obj.GetComponentInChildren<ParticleSystem>();
    }

    public ParticleSystem FindParticleSystem()
    {
        return FindParticleSystem(transform);
    }

    private void OnEnable()
    {
        var ps = FindParticleSystem();
        if (ps == null)
        {
            Util.Destroy(this);
        }
        else if (!ps.main.loop)
        {
            Util.Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax, "ps finished");
        }
    }
}
