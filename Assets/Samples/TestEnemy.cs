using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public float castInterval = 20.0f;
    private float lastCast = 0.0f;

    private void Start()
    {
        lastCast = Time.time;
    }

    void Update()
    {
        if (Time.time - lastCast > castInterval)
        {
            var wiz = GetComponent<Wizard>();
            wiz.CastSpell("ChargedFireball3", null);
            lastCast = Time.time;
        }
    }
}
