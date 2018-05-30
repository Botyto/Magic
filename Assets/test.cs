using UnityEngine;

public class test : MonoBehaviour
{
    private void Start()
    {
        var dirs = FileUtility.GetAllFiles("Scripts/", ".txt");
        foreach (var dir in dirs)
            print(dir);
    }
}
