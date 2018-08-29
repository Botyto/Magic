using System.Text;
using UnityEngine.UI;

public class CodeLuaFunctionDefFormatter : CodeFormatter
{
    public Toggle localToggle;
    public InputField functionInput;
    public CodeSlot argumentsSlot;
    public CodeSlot bodySlot;

    public override string GetCode()
    {
        var strBuilder = new StringBuilder();

        //local function
        strBuilder.Append(ExtractContent(localToggle));
        strBuilder.Append(" function");

        //functionn name
        strBuilder.Append(ExtractContent(functionInput));

        //arguments
        strBuilder.Append("(");
        var args = GetPieceCode(argumentsSlot.attachedPiece, ", ");
        strBuilder.Append(args);
        strBuilder.AppendLine(")");

        //body
        var body = GetPieceCode(bodySlot.attachedPiece, "\n");
        strBuilder.AppendLine(body);

        //end
        strBuilder.Append("end");

        return strBuilder.ToString();
    }
}
