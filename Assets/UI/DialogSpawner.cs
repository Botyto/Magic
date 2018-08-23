using UnityEngine;

public class DialogSpawner : MonoBehaviour
{
    public GameObject[] prefabs;

    void Start()
    {
        if (prefabs != null && prefabs.Length > 0)
        {
            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    var obj = Instantiate(prefab, transform);
                    obj.name = prefab.name;
                }
            }
        }

        Destroy(this);
    }
}
