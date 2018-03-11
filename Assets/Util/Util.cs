using System;
using UnityEngine;

public static class Util
{
    #region Math

    public static int Sign(int x)
    {
        return (x > 0) ? 1 : ((x < 0) ? -1 : 0);
    }
    
    #endregion

    #region Debug destroying

    /// <summary>
    /// Removes a gameobject, component or asset
    /// </summary>
    public static void Destroy(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj);
        }
    }

    /// <summary>
    /// Removes a gameobject, component or asset
    /// </summary>
    /// <param name="delay">Amount of time to delay before destroying the object</param>
    public static void Destroy(UnityEngine.Object obj, float delay)
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj, delay);
        }
    }

    /// <summary>
    /// Removes a gameobject, component or asset, leaving a debug note
    /// </summary>
    public static void Destroy(UnityEngine.Object obj, string note)
    {
        if (obj != null)
        {
#if DEBUG //TODO test if this works in release
            const int maxLen = 15;
            if (obj.name.Length > maxLen)
            {
                obj.name = string.Format("{0}..[{1}]", obj.name.Substring(0, maxLen - 2), note);
            }
            else
            {
                obj.name = string.Format("{0}[{1}]", obj.name, note);
            }
#endif
        
            UnityEngine.Object.Destroy(obj);
        }
    }

    /// <summary>
    /// Removes a gameobject, component or asset, leaving a debug note
    /// </summary>
    /// <param name="delay">Amount of time to delay before destroying the object</param>
    public static void Destroy(UnityEngine.Object obj, float delay, string note)
    {
        if (obj != null)
        {
#if DEBUG
            const int maxLen = 15;
            if (obj.name.Length > maxLen)
            {
                obj.name = string.Format("{0}..[{1}]", obj.name.Substring(0, maxLen - 2), note);
            }
            else
            {
                obj.name = string.Format("{0}[{1}]", obj.name, note);
            }
#endif
            UnityEngine.Object.Destroy(obj, delay);
        }
    }

    #endregion

    #region Parent resolution

    public static Transform ResolveTopmostParent(Transform obj)
    {
        while (obj.parent != null)
        {
            obj = obj.parent;
        }

        return obj;
    }

    public static GameObject ResolveTopmostParent(GameObject obj)
    {
        return ResolveTopmostParent(obj.transform).gameObject;
    }

    #endregion

    #region Object queries

    public static T FindClosestObject<T>(Vector3 worldPosition) where T : MonoBehaviour
    {
        float closestDistSqr = float.PositiveInfinity;
        T closestObj = null;
        
        foreach (T obj in UnityEngine.Object.FindObjectsOfType<T>())
        {
            var distSqr = worldPosition.SqrDistanceTo(obj.transform.position);
            if (distSqr <= closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestObj = obj;
            }
        }

        return closestObj;
    }

    public static T FindClosestObject<T>(Vector3 worldPosition, Predicate<T> filter) where T : MonoBehaviour
    {
        return FindClosestObject(worldPosition, filter, float.PositiveInfinity);
    }

    public static T FindClosestObject<T>(Vector3 worldPosition, Predicate<T> filter, float maxDistance) where T : MonoBehaviour
    {
        var sqrMaxDistance = maxDistance * maxDistance;
        float closestDistSqr = float.PositiveInfinity;
        T closestObj = null;

        foreach (T obj in UnityEngine.Object.FindObjectsOfType<T>())
        {
            if (obj.transform.position.SqrDistanceTo(worldPosition) > sqrMaxDistance) continue;
            if (!filter(obj)) continue;
            
            var distSqr = worldPosition.SqrDistanceTo(obj.transform.position);
            if (distSqr <= closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestObj = obj;
            }
        }

        return closestObj;
    }

    #endregion
}
