using UnityEngine;

public class CodePieceSpawner : MonoBehaviour
{
    public GameObject prefab;

    public void Spawn()
    {
        var container = FindObjectOfType<CodeContainer>();
        var crt = container.transform as RectTransform;
        var csize = crt.rect.size;

        var newPiece = Instantiate(prefab, crt);

        var rt = prefab.transform as RectTransform;
        var size = rt.rect.size;
        //rt.position = (crt.sizeDelta / 2) - (size / 2);
    }
}
