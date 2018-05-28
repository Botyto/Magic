using UnityEngine;

public static class Gameplay
{
    #region Input

    public static bool GetKey(KeyCode key)
    {
        return Input.GetKey(key);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }

    public static bool GetKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key);
    }
    

    public static bool GetMouseButton(int button)
    {
        return Input.GetMouseButton(button);
    }
    
    public static bool GetMouseButtonDown(int button)
    {
        return Input.GetMouseButtonDown(button);
    }
    
    public static bool GetMouseButtonUp(int button)
    {
        return Input.GetMouseButtonUp(button);
    }

    
    public static float GetAxis(string axisName)
    {
        return Input.GetAxis(axisName);
    }

    #endregion
}
