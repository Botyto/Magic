using UnityEngine;

public class CodeContainer : MonoBehaviour
{
    public static CodeContainer current { get { return FindObjectOfType<CodeContainer>(); } }
}
