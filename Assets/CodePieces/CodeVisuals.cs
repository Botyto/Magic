using UnityEngine;
using UnityEngine.UI;

public class CodeVisuals : MonoBehaviour
{
    #region Constants
    
    public const float sliceSizeLeft = 15;
    public const float sliceSizeTop = 10;
    public const float sliceSizeRight = 5;
    public const float sliceSizeBottom = 5;
    public const float extendedSliceSizeTop = 10;
    public const float extendedSliceSizeBottom = 10;

    public const float bottomIndentHeight = 5;
    public const float extendedCompensation = 5;
    public const float emptySlotHeight = 10;

    public const float narrowMinWidth = 20;
    public const float narrowMinHeight = 15;
    public const float extendedMinHeight = 20;
    public const float extendedMinWidth = narrowMinWidth + 20;

    private static Vector2 vec_0_1 = Vector2.up;

    #endregion

    #region Building

    /// <summary>
    /// Separate pieces are that cannot take part in a piece chain (see MainCodePiece)
    /// </summary>
    public bool isSeparatePiece = false;

    /// <summary>
    /// Part vertical position on the piece.
    /// </summary>
    private enum VPos { Top, Middle, Bottom }

    /// <summary>
    /// Builds a complex piece (as opposed to BuildContentOnly()).
    /// </summary>
    /// <returns>Piece total size</returns>
    private Vector2 BuildComplex()
    {
        var elements = m_Piece.elements;
        var elementTypes = m_Piece.elementTypes;
        var elementsCount = elements.Length;

        var iMax = elementsCount + ((m_Piece.bottomSlot != null) ? -1 : 0);

        int first = 0;
        int last = iMax - 1;

        //When there are extended parts, narrow parts above/below will compensate for it
        var topCompensation = false;
        var bottomCompensation = false;

        var totalSize = Vector2.zero;
        var currentY = 0.0f;
        for (int i = 0; i < iMax; ++i)
        {
            //Read element parameters
            var elem = elements[i];
            var elemSize = elem.sizeDelta;
            var isContent = (elementTypes[i] == CodePiece.ElementType.Content);
            var isExtended = isContent;

            //Read next element parameters (predict compensation
            var nextElem = ((i + 1) <= last) ? elements[i + 1] : null;
            if (nextElem != null)
            {
                var nextIsContent = (elementTypes[i + 1] == CodePiece.ElementType.Content);
                var nextIsExtended = nextIsContent;
                if (nextIsExtended) { bottomCompensation = true; }
            }

            //Figure out part position
            var vPos = VPos.Middle;
            if (i == first) { vPos = VPos.Top; }
            if (i == last) { vPos = VPos.Bottom; }

            //Figure out the part size
            var partSize = isContent ? elemSize : new Vector2(0, Mathf.Max(elemSize.y, emptySlotHeight));
            partSize.x += sliceSizeLeft + sliceSizeRight;
            if (isExtended)
            {
                partSize.y += extendedSliceSizeTop;
                partSize.y += extendedSliceSizeBottom;
                partSize.y += bottomIndentHeight;
            }
            else
            {
                partSize.y += (vPos == VPos.Top ? sliceSizeTop : 0);
                partSize.y += (vPos == VPos.Bottom ? sliceSizeBottom : 0);
                partSize.y += bottomIndentHeight;
            }
            var topCompensationSaved = topCompensation;
            if (topCompensation) { partSize.y -= extendedCompensation; }
            if (bottomCompensation) { partSize.y -= extendedCompensation; }
            topCompensation = false; //Reset after compensating
            bottomCompensation = false;

            //No point in creating empty parts
            if (partSize.y < float.Epsilon) { continue; }
            
            //Create the part
            GameObject obj = null;
            if (isExtended)
            {
                obj = CreateExtended(vPos, partSize.x, partSize.y);
            }
            else
            {
                obj = CreateNarrow(vPos, partSize.x, partSize.y);
            }
            var rt = obj.GetComponent<RectTransform>();
            rt.SetParent(transform);
            rt.localPosition = new Vector3(0, -currentY, 0);

            //Adjust element position
            var elemX = rt.localPosition.x + sliceSizeLeft + (topCompensationSaved ? extendedCompensation : 0);
            var elemY = rt.localPosition.y - (isExtended ? extendedSliceSizeTop : 0);
            if (topCompensationSaved) { elemY += extendedCompensation + bottomIndentHeight; }
            elem.localPosition = new Vector3(elemX, elemY, 0);

            //If this is an extended piece, the next one will have to compensate for it
            if (isExtended) { topCompensation = true; }

            //Advance on Y axis and recalculate total piece size
            currentY += partSize.y;
            totalSize.x = Mathf.Max(totalSize.x, partSize.x);
            totalSize.y = Mathf.Max(totalSize.y, currentY);
        }

        return totalSize;
    }
    
    /// <summary>
    /// Creates a narrow part for a complex piece.
    /// </summary>
    private GameObject CreateNarrow(VPos vPos, float width, float height)
    {
        //Fix width/height
        width = Mathf.Max(narrowMinWidth, width);
        height = Mathf.Max(narrowMinHeight, height);

        //Resolve sprite
        Sprite sprite = null;
        switch (vPos)
        {
            case VPos.Top:
                sprite = Resources.Load<Sprite>(isSeparatePiece ? "CodePieces/SepNarrowTop" : "CodePieces/NarrowTop");
                break;
            case VPos.Middle:
                sprite = Resources.Load<Sprite>("CodePieces/NarrowMiddle");
                break;
            case VPos.Bottom:
                sprite = Resources.Load<Sprite>(isSeparatePiece ? "CodePieces/SepNarrowBottom" : "CodePieces/NarrowBottom");
                break;
        }

        //Narrow parts are in a single object
        var go = new GameObject("Narrow" + vPos.ToString());
        var rt = go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        SetupPart(go);
        
        //Setup position, size and sprite
        rt.localPosition = new Vector3(0, 0, 0);
        rt.sizeDelta = new Vector2(width, height);
        img.sprite = sprite;

        return go;
    }

    /// <summary>
    /// Creates an extended part for a complex piece.
    /// </summary>
    private GameObject CreateExtended(VPos vPos, float width, float height)
    {
        //Resolve width and sprites of two halves
        width = Mathf.Max(extendedMinWidth, width);
        height = Mathf.Max(extendedMinHeight, height);
        var leftWidth = narrowMinWidth;
        var rightWidth = width - leftWidth;

        Sprite leftSprite = null;
        Sprite rightSprite = null;
        switch (vPos)
        {
            case VPos.Top:
                leftSprite = Resources.Load<Sprite>(isSeparatePiece ? "CodePieces/SepExtendedLTop" : "CodePieces/ExtendedLTop");
                rightSprite = Resources.Load<Sprite>("CodePieces/ExtendedRTop");
                break;
            case VPos.Middle:
                leftSprite = Resources.Load<Sprite>("CodePieces/ExtendedLMiddle");
                rightSprite = Resources.Load<Sprite>("CodePieces/ExtendedRMiddle");
                break;
            case VPos.Bottom:
                leftSprite = Resources.Load<Sprite>(isSeparatePiece ? "CodePieces/SepExtendedLBottom" : "CodePieces/ExtendedLBottom");
                rightSprite = Resources.Load<Sprite>("CodePieces/ExtendedRBottom");
                break;
        }

        //Extended parts are in two halves in two separate objects
        var go = new GameObject("Extended" + vPos.ToString());
        var rt = go.AddComponent<RectTransform>();
        rt.localPosition = new Vector3(0, 0, 0);
        rt.sizeDelta = new Vector2(width, height);
        SetupPart(go);

        //Create left half
        var goL = new GameObject("Left");
        goL.transform.SetParent(go.transform);
        var lrt = goL.AddComponent<RectTransform>();
        var limg = goL.AddComponent<Image>();
        SetupPart(goL);

        //Create right half
        var goR = new GameObject("Right");
        goR.transform.SetParent(go.transform);
        var rrt = goR.AddComponent<RectTransform>();
        var rimg = goR.AddComponent<Image>();
        SetupPart(goR);
        
        //Setup halves positions, sizes and images
        lrt.localPosition = new Vector3(0, 0, 0);
        lrt.sizeDelta = new Vector2(leftWidth, height);
        limg.sprite = leftSprite;

        rrt.localPosition = new Vector3(leftWidth, 0, 0);
        rrt.sizeDelta = new Vector2(rightWidth, height);
        rimg.sprite = rightSprite;

        return go;
    }

    /// <summary>
    /// Builds a simple, content-only, piece.
    /// </summary>
    /// <returns>Piece total size.</returns>
    private Vector2 BuildContentOnly()
    {
        var elements = m_Piece.elements;
        var elementsCount = elements.Length;

        var content = elements[0];
        content.localPosition = new Vector3(sliceSizeLeft, -sliceSizeTop, 0.0f);
        var contentSize = content.rect.size;

        var visualWidth = sliceSizeLeft + contentSize.x + sliceSizeRight;
        var visualHeight = sliceSizeTop + contentSize.y + sliceSizeBottom;
        var totalSize = new Vector2(visualWidth, visualHeight);

        var visual = new GameObject("ContentOnly");
        visual.transform.SetParent(transform);
        var rt = visual.AddComponent<RectTransform>();
        var img = visual.AddComponent<Image>();
        SetupPart(visual);

        rt.localPosition = new Vector3(0, 0, 0);
        rt.sizeDelta = totalSize;
        img.sprite = Resources.Load<Sprite>(isSeparatePiece ? "CodePieces/SepContentOnly" : "CodePieces/ContentOnly");
        
        return totalSize;
    }

    /// <summary>
    /// For internal use.
    /// </summary>
    private void SetupPart(GameObject part)
    {
        var img = part.GetComponent<Image>();
        if (img != null)
        {
            img.type = Image.Type.Sliced;
            img.color = m_Piece.color;
        }

        var rt = part.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = vec_0_1;
            rt.anchorMax = vec_0_1;
            rt.pivot = vec_0_1;
        }
    }
    
    #endregion
    
    #region Public interface

    /// <summary>
    /// Clears all visual parts and builds new ones.
    /// Also rearragnes piece elements
    /// </summary>
    public void RebuildVisuals()
    {
        ClearVisuals();
        
        //Build the visuals and assign sizes to visuals and piece
        var totalSize = m_Piece.isContentOnlyPiece ? BuildContentOnly() : BuildComplex();
        
        //Adjust bottom slot
        var bottomSlot = m_Piece.bottomSlot;
        if (bottomSlot != null)
        {
            var bottomSlotRT = bottomSlot.transform as RectTransform;
            bottomSlotRT.localPosition = new Vector3(0.0f, -totalSize.y + bottomIndentHeight, 0.0f);

            //Adjust total size for bottom slot
            totalSize.y += bottomSlot.hasAttachment ? (bottomSlotRT.sizeDelta.y - bottomIndentHeight) : 0;
        }

        //Apply new total size
        rt.sizeDelta = totalSize;
        (m_Piece.transform as RectTransform).sizeDelta = totalSize;
    }

    /// <summary>
    /// Clears all visual parts.
    /// </summary>
    public void ClearVisuals()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion

    #region Unity
    
    private CodePiece m_Piece;
    private RectTransform rt;

    void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        if (rt == null) { rt = gameObject.AddComponent<RectTransform>(); }
        m_Piece = transform.parent.GetComponent<CodePiece>();

        SetupPart(gameObject);
        rt.localPosition = Vector3.zero;
    }

    #endregion
}
