using UnityEngine;

public struct Orientation
{
    public Vector3 from;
    public Vector3 to;
    public float distance;

    public Vector3 forward, back;
    public Vector3 right, left;
    public Vector3 up, down;

    public Orientation(MonoBehaviour from, MonoBehaviour to)
        : this(from.transform.position, to.transform.position)
    { }

    public Orientation(MonoBehaviour from, Vector3 to)
        : this(from.transform.position, to)
    { }

    public Orientation(Transform from, Transform to)
        : this(from.position, to.position)
    { }

    public Orientation(Transform from, Vector3 to)
        : this(from.position, to)
    { }

    public Orientation(Vector3 from, Vector3 to)
    {
        //Points & distance
        this.from = from;
        this.to = to;
        distance = from.DistanceTo(to);

        //Directions
        forward = (to - from).normalized;
        back = -forward;
        right = new Vector3(forward.z, forward.y, -forward.x);
        left = -right;
        up = Vector3.Cross(forward, right);
        down = -up;
    }
}
