using UnityEngine;

public class UIKeyToggle : MonoBehaviour
{
    public GameObject[] toggledObjects;
    public KeyCode key;
    public bool active = true;

    void Start()
    {
        foreach (var obj in toggledObjects)
        {
            obj.SetActive(active);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            active = !active;

            foreach (var obj in toggledObjects)
            {
                obj.SetActive(active);
            }
        }
	}
}
