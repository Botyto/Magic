using UnityEngine;
using UnityEngine.EventSystems;

public static class Gameplay
{
    #region Input

    public static bool IsUIFocused()
    {
        return EventSystem.current.currentSelectedGameObject != null;
    }

    public static bool CanHandleInput()
    {
        return !IsUIFocused();
    }

    public static bool GetKey(KeyCode key)
    {
        return CanHandleInput() && Input.GetKey(key);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        return CanHandleInput() && Input.GetKeyDown(key);
    }

    public static bool GetKeyUp(KeyCode key)
    {
        return CanHandleInput() && Input.GetKeyUp(key);
    }
    

    public static bool GetMouseButton(int button)
    {
        return CanHandleInput() && Input.GetMouseButton(button);
    }
    
    public static bool GetMouseButtonDown(int button)
    {
        return CanHandleInput() && Input.GetMouseButtonDown(button);
    }
    
    public static bool GetMouseButtonUp(int button)
    {
        return CanHandleInput() && Input.GetMouseButtonUp(button);
    }

    
    public static float GetAxis(string axisName)
    {
        return CanHandleInput() ? Input.GetAxis(axisName) : 0.0f;
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
            RenameDestroyedObject(obj, note);
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
            RenameDestroyedObject(obj, note);
#endif
            UnityEngine.Object.Destroy(obj, delay);
        }
    }

    /// <summary>
    /// Internal fn. used for renaming objects destroyed using the debug destory funcs above.
    /// </summary>
    private static void RenameDestroyedObject(UnityEngine.Object obj, string note)
    {
        if (string.IsNullOrEmpty(note))
        {
            return;
        }

        const int maxLen = 15;
        if (obj.name.Length > maxLen)
        {
            obj.name = string.Format("{0}..[{1}]", obj.name.Substring(0, maxLen - 2), note);
        }
        else
        {
            obj.name = string.Format("{0}[{1}]", obj.name, note);
        }
    }

    #endregion
}
