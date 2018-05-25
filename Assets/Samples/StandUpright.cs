using UnityEngine;

public class StandUpright : MonoBehaviour
{
    public const float StandUpSpeed = 1.0f;

    void Update()
    {
        if (Vector3.Dot(transform.up, Vector3.up) >= 1.0f)
        {
            Util.Destroy(this);
        }
        else
        {
            var normal = Vector3.Cross(transform.up, Vector3.up);
            var maxAngle = Vector3.Angle(transform.up, Vector3.up);
            transform.Rotate(normal, Mathf.Min(maxAngle, StandUpSpeed), Space.World);
        }
    }
}
