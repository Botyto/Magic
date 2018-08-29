using UnityEngine;

[RequireComponent(typeof(CodePiece))]
public abstract class CodeFormatter : MonoBehaviour
{
    public abstract string GetCode();
}
