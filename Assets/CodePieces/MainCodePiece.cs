using UnityEngine;
using UnityEngine.EventSystems;

public class MainCodePiece : CodePiece
{
    #region Override dragging

    public override void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;
    }
    
    public override void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0.0f);
    }
    
    public override void OnEndDrag(PointerEventData eventData)
    {
        isDragged = false;
    }

    #endregion

    #region Override Setup

    protected override void InitElements()
    {
        parentCode = this; //TODO fix attaching this piece to another in reverse
        //No bottom slot :)
    }

    protected override void InitVisuals()
    {
        base.InitVisuals();
        m_Visuals.isSeparatePiece = true;
    }

    #endregion
}
