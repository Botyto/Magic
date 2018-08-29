using System.Text;
using UnityEngine;

public class CodeConcatFormatter : CodeFormatter
{
    public GameObject[] objects;
    public string bottomLine = "";

    public override string GetCode()
    {
        var strBuilder = new StringBuilder();

        //Append each object from the list
        foreach (var obj in objects)
        {
            strBuilder.Append(ExtractContent(obj) + " ");
        }

        //Append next object code
        var bottomSlot = GetComponent<CodePiece>().bottomSlot;
        if (bottomSlot != null)
        {
            var nextCode = GetPieceCode(bottomSlot.attachedPiece);
            strBuilder.AppendLine(nextCode);
        }

        //Bottom line
        strBuilder.AppendLine(bottomLine);

        return strBuilder.ToString();
    }
}
