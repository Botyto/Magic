using UnityEngine;
using UnityEngine.UI;

public class CodeSlot : MonoBehaviour
{
    /// <summary>
    /// Element index in owning piece
    /// </summary>
    public int index = -1;

    /// <summary>
    /// If a piece is attached to this slot
    /// </summary>
    public bool hasAttachment { get { return transform.childCount > 0; } }

    /// <summary>
    /// The piece attached to this slot
    /// </summary>
    public CodePiece attachedPiece { get { return hasAttachment ? transform.GetChild(0).GetComponent<CodePiece>() : null; } }

    /// <summary>
    /// Owning piece
    /// </summary>
    public CodePiece piece { get { return transform.parent.GetComponent<CodePiece>(); } }

    private void Awake()
    {
        var img = gameObject.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>("CodePieces/SlotHint");
        img.enabled = false;

        GetComponent<RectTransform>().sizeDelta = img.sprite.rect.size;
    }

    void OnDrawGizmos()
    {
        if (hasAttachment) { return; }

        Vector3 gizmoPos;

        var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        var canvasTransform = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasTransform, transform.position, null, out gizmoPos);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(gizmoPos, new Vector3(20, 20, 1));
    }

    private void PotentialChildEnter(CodePiece child)
    {
        GetComponent<Image>().enabled = true;
    }

    private void PotentialChildLeave(CodePiece child)
    {
        GetComponent<Image>().enabled = false;
    }

    private void ChildAttached(CodePiece child)
    {
        GetComponent<Image>().enabled = false;
    }
}
