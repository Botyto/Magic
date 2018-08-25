using UnityEngine;
using UnityEngine.EventSystems;

public class Dialog : MonoBehaviour
{
    #region Standard Dialog Methdods

    public void Close()
    {
        Gameplay.Destroy(gameObject, "close");

        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    
    protected virtual void OnOpen() { }
     
    protected virtual void OnClose() { }

    #endregion

    #region Input Handling
    
    public virtual bool OnKeyDown(KeyCode keyCode) { return false; }

    #endregion

    #region Child Lookup

    public Transform FindRecursive(string name)
    {
        return transform.FindRecursive(name);
    }

    public T FindRecursive<T>(string name) where T : MonoBehaviour
    {
        var child = FindRecursive(name);
        if (child == null)
        {
            return null;
        }

        return child.GetComponent<T>();
    }

    #endregion

    #region Dialog Spawning

    public static T Spawn<T>(bool single = false) where T : Dialog
    {
        return Spawn<T>(FindObjectOfType<Canvas>().transform as RectTransform, single);
    }

    public static T Spawn<T>(GameObject parent, bool single = false) where T : Dialog
    {
        return Spawn<T>(parent.transform as RectTransform, single);
    }

    public static T Spawn<T>(RectTransform parent, bool single = false) where T : Dialog
    {
        if (single)
        {
            var alreadySpawned = FindObjectOfType<T>();
            if (alreadySpawned != null)
            {
                return alreadySpawned;
            }
        }

        if (parent == null)
        {
            return Spawn<T>(single);
        }

        var prefab = Resources.Load<GameObject>("UI/Prefabs/" + typeof(T).Name);
        if (prefab == null)
        {
            Debug.LogErrorFormat("Cannot find prefab for Dialog '{0}'!", typeof(T).Name);
            return null;
        }
        
        if (prefab.GetComponent<T>() == null)
        {
            Debug.LogErrorFormat("Dialog '{0}' doesn't contain its component!", typeof(T).Name);
            return null;
        }

        var dlg = Instantiate(prefab, parent.transform);
        return dlg.GetComponent<T>();
    }

    #endregion

    #region Unity Internals

    private void Start() { OnOpen(); }

    private void OnDestroy() { OnClose(); }

    protected virtual void OnGUI()
    {
        var e = Event.current;
        if (e == null) { return; }

        if (e.type == EventType.KeyDown && OnKeyDown(e.keyCode)) { e.Use(); }
    }

    #endregion
}
