using UnityEngine;

public class InitialForceApplier : MonoBehaviour
{
    public Transform target;
    public float forceSize = 1.0f;
    public ForceMode mode = ForceMode.Impulse;

	void Start()
    {
		if (target != null)
        {
            var body = GetComponent<Rigidbody>();
            if (body != null)
            {
                var direction = target.transform.position - transform.position;
                body.AddForce(direction.SetLength(forceSize), mode);
            }
        }

        Gameplay.Destroy(this);
	}
	
}
