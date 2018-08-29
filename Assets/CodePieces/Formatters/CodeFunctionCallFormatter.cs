using System.Text;
using UnityEngine.UI;

public class CodeFunctionCallFormatter : CodeFormatter
{
    public InputField functionInput;
    public CodeSlot argumentsSlot;

    public override string GetCode()
    {
        var strBuilder = new StringBuilder();

        strBuilder.Append(ExtractContent(functionInput));
        strBuilder.Append("(");

        var args = GetPieceCode(argumentsSlot.attachedPiece, ", ");
        strBuilder.Append(args);

        strBuilder.Append(")");

        return strBuilder.ToString();
    }
}
