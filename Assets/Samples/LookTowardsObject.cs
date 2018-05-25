using UnityEngine;

public class LookTowardsObject : MonoBehaviour
{
    public GameObject target;

    void Update()
    {
        if (target == null)
        {
            Util.Destroy(this);
            return;
        }
            
        transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);
    }
}
