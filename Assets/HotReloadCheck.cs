using UnityEngine;
using UnityEngine.SceneManagement;

public class HotReloadCheck : MonoBehaviour
{
    [HideInInspector]
    public bool firstLoad = true;

    void OnEnable()
    {
        if (firstLoad)
        {
            firstLoad = false;
            return;
        }

        Debug.Log(">>>>> Hot Reload Begin >>>>>", this);

        int totalObjects = 0;
        int totalScripts = 0;
        int totalMissing = 0;

        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            CheckMissingScripts(obj, ref totalObjects, ref totalScripts, ref totalMissing);
        }

        Debug.Log("<<<<< Hot Reload End <<<<<");
    }

    private void CheckMissingScripts(GameObject obj, ref int objectsNum, ref int scriptsNum, ref int missingNum)
    {
        objectsNum++;
        Component[] components = obj.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            scriptsNum++;
            if (components[i] == null)
            {
                missingNum++;
                string s = obj.name;
                Transform t = obj.transform;
                while (t.parent != null)
                {
                    s = t.parent.name + "/" + s;
                    t = t.parent;
                }
                Debug.Log(s + " has an empty script attached in position: " + i, obj);
            }
        }
        foreach (Transform childT in obj.transform)
        {
            CheckMissingScripts(childT.gameObject, ref objectsNum, ref scriptsNum, ref missingNum);
        }
    }
}
