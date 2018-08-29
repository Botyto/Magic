using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CodePiece))]
public abstract class CodeFormatter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Code generation

    public abstract string GetCode();

    public virtual string GetPieceCode(CodePiece piece, string separator = "\n")
    {
        if (piece == null) { return ""; }

        var strBuilder = new StringBuilder();

        var list = GetPieceCodeInList(piece);
        var last = list.Count - 1;
        for (int i = 0; i <= last; ++i)
        {
            strBuilder.Append(list[i]);
            if (i != last) { strBuilder.Append(separator); }
        }

        return strBuilder.ToString();
    }

    public virtual List<string> GetPieceCodeInList(CodePiece piece)
    {
        var list = new List<string>();

        if (piece == null) { return list; }
        list.Add(ExtractContent(piece.gameObject));

        while (piece != null)
        {
            var bottomSlot = piece.bottomSlot;
            if (bottomSlot != null && bottomSlot.hasAttachment)
            {
                piece = bottomSlot.attachedPiece;
                list.Add(ExtractContent(piece.gameObject));
            }
            else
            {
                break;
            }
        }

        return list;
    }

    public void ShowTooltip()
    {
        print(name + ": " + GetCode());
    }

    public void HideTooltip()
    { }

    #endregion

    #region Helpers

    public static string ExtractContent(GameObject obj)
    {
        string str = null;

        str = ExtractContent(obj.GetComponent<CodeFormatter>());
        if (!string.IsNullOrEmpty(str)) { return str; }

        str = ExtractContent(obj.GetComponent<CodeSlot>());
        if (!string.IsNullOrEmpty(str)) { return str; }

        str = ExtractContent(obj.GetComponent<Text>());
        if (!string.IsNullOrEmpty(str)) { return str; }

        str = ExtractContent(obj.GetComponent<InputField>());
        if (!string.IsNullOrEmpty(str)) { return str; }

        str = ExtractContent(obj.GetComponent<Toggle>());
        if (!string.IsNullOrEmpty(str)) { return str; }

        return "";
    }

    public static string ExtractContent(CodeFormatter code)
    {
        return (code != null) ? code.GetCode() : "";
    }

    public static string ExtractContent(CodeSlot slot)
    {
        return (slot != null && slot.hasAttachment) ? ExtractContent(slot.attachedPiece.gameObject) : "";
    }

    public static string ExtractContent(Text text)
    {
        return (text != null) ? text.text : "";
    }

    public static string ExtractContent(InputField input)
    {
        return (input != null) ? input.text : "";
    }

    public static string ExtractContent(Toggle toggle)
    {
        if (toggle == null || !toggle.isOn) { return ""; }
        var label = toggle.transform.Find("Label");
        if (label == null) { return ""; }
        return ExtractContent(label.GetComponent<Text>());
    }

    #endregion

    #region Mouse over

    public const float tooltipTime = 1.0f;
    private float m_MouseEnterTime = 0;
    private bool m_MouseOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_MouseEnterTime = Time.time;
        m_MouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_MouseOver = false;
        HideTooltip();
    }

    private void Update()
    {
        if (!m_MouseOver) { return; }

        if (Time.time - m_MouseEnterTime > tooltipTime)
        {
            if (!GetComponent<CodePiece>().isDragged)
            {
                ShowTooltip();
            }

            m_MouseOver = false;
        }
    }

    #endregion
}
