using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    #region Gameplay

    public static Transform FindRecursive(this Transform self, string name)
    {
        var target = self.Find(name);
        if (target != null)
        {
            return target;
        }
        else
        {
            for (int i = 0; i < self.childCount; ++i)
            {
                var subTarget = FindRecursive(self.GetChild(i), name);
                if (subTarget != null)
                {
                    return subTarget;
                }
            }
        }

        return null;
    }

    #endregion

    #region Vectors

    public static float DistanceTo(this Vector3 self, Vector3 other)
    {
        return Vector3.Distance(self, other);
    }

    public static float SqrDistanceTo(this Vector3 self, Vector3 other)
    {
        return Vector3.SqrMagnitude(self - other);
    }

    public static Vector3 SetLength(this Vector3 self, float newLength)
    {
        return self.normalized * newLength;
        //return self * Mathf.Sqrt((newLength * newLength) / vec.sqrMagnitude); TODO test
    }

    public static Vector3 Multiply(this Vector3 self, Vector3 other)
    {
        return new Vector3(self.x * other.x, self.y * other.y, self.z * other.z);
    }

    public static Vector3 Divide(this Vector3 self, Vector3 other)
    {
        return new Vector3(self.x / other.x, self.y / other.y, self.z / other.z);
    }

    public static Vector3 MulDiv(this Vector3 self, Vector3 m, Vector3 d)
    {
        return new Vector3(
            (self.x * m.x) / d.x,
            (self.y * m.y) / d.y,
            (self.z * m.z) / d.z);
    }

    #endregion

    #region Collections

    public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
    {
        TValue result = default(TValue);
        if (dict.TryGetValue(key, out result))
        {
            return result;
        }

        return defaultValue;
    }

    #endregion
}
