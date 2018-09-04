using UnityEngine;

public class CodePieceSpawner : MonoBehaviour
{
    public GameObject prefab;

    public void Spawn()
    {
        Instantiate(prefab, FindObjectOfType<CodeContainer>().transform);
    }
}
