using UnityEngine;

public class DodgeBehaviour : MonoBehaviour
{
    public float force = 10.0f;
    public float cycleDuration = 5.0f;
    
    private float time = 0.0f;
    private float dir = 1.0f;
    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    private void Start()
    {
        time = cycleDuration / 3;
    }

    private void FixedUpdate()
    {
        time = time + Time.fixedDeltaTime;
        if (time >= cycleDuration)
        {
            dir *= -1;
            time = 0;
        }

        unit.ApplyForce(Vector3.forward * force * dir, ForceMode.Acceleration);
    }
}
