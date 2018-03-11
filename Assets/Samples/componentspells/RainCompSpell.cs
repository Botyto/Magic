using UnityEngine;

public class RainCompSpell : ContinuousSpellComponent
{
    public float interval = 0.3f;
    private float m_lastSpawn;
    public const int baseHits = 6;
    public const int hitsPerLevel = 2;
    public int hits = 0;

    public new static GameObject TryFindTarget(Wizard wizard)
    {
        return SpellUtilities.FindClosestEnemy(wizard, 20.0f);
    }

    public override void OnTargetLost()
    {
        Cancel();
    }

    public override void Activate(float dt)
    {
        if (Time.time - m_lastSpawn > interval)
        {
            m_lastSpawn = Time.time;
            if (CanFocusMore())
            {
                var center = GetTargetPosition() + Vector3.up * 30;
                var angle = Random.Range(0, 360);
                var len = Random.Range(5.0f, 15.0f);
                var spawnPos = center + new Vector3(Mathf.Sin(angle) * len, 0, Mathf.Cos(angle) * len);
                int handle;
                var result = ManifestEnergyAndFocus(10 + 5 * param.level, wizard.transform.InverseTransformPoint(spawnPos), out handle);
                if (!Try(result))
                {
                    if (result == EnergyActionResult.NotEnoughEnergy)
                    {
                        Finish();
                        return;
                    }
                }
                else
                {
                    OrientTowards(handle, target);

                    var forceDirection = GetTargetPosition() - GetFocusPosition(handle);
                    if (!Try(ApplyForce(handle, forceDirection.SetLength(300 + 50 * param.level), ForceMode.Impulse)))
                    {
                        Finish();
                        return;
                    }

                    ++hits;
                    if (hits > baseHits + hitsPerLevel * param.level)
                    {
                        Finish();
                        return;
                    }
                }
            }
        }
    }
}
