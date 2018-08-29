using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Elements
    
    /// <summary>
    /// Element type.
    /// </summary>
    public enum ElementType
    {
        None,
        Content,
        Slot,
    }
    
    /// <summary>
    /// List of all chlid elements (content or slot).
    /// </summary>
    [HideInInspector]
    public RectTransform[] elements;

    /// <summary>
    /// Types of all child elements.
    /// </summary>
    [HideInInspector]
    public ElementType[] elementTypes;

    [HideInInspector]
    public CodeSlot bottomSlot;

    /// <summary>
    /// Initialize elements.
    /// </summary>
    protected virtual void InitElements()
    {
        //Initialize bottom slot
        var bottomSlotObj = new GameObject(name + "BottomSlot");
        bottomSlotObj.transform.SetParent(transform);
        bottomSlotObj.transform.SetAsLastSibling();
        bottomSlot = bottomSlotObj.AddComponent<CodeSlot>();
    }

    /// <summary>
    /// Find all child objects and resolve them as elements.
    /// </summary>
    protected void ResolveElements()
    {
        var lastType = ElementType.None;

        var vec_0_1 = new Vector2(0, 1);
        var n = transform.childCount;
        elements = new RectTransform[n];
        elementTypes = new ElementType[n];
        childPieces = new CodePiece[n];
        for (int idx = 0; idx < n; ++idx)
        {
            var elem = transform.GetChild(idx) as RectTransform;
            var slot = elem.GetComponent<CodeSlot>();

            elements[idx] = elem;
            if (slot != null)
            {
                elementTypes[idx] = ElementType.Slot;
                elem.sizeDelta = slot.hasAttachment ? (slot.attachedPiece.transform as RectTransform).sizeDelta : elem.sizeDelta;
                slot.index = idx;
            }
            else
            {
                elementTypes[idx] = ElementType.Content;
            }

            //Exclude last slot from consistency checks
            if (idx < n - 1)
            {
                if (idx == 0)
                {
                    Debug.Assert(elementTypes[idx] != ElementType.Slot, "First element cannot be a slot", this);
                }
                else
                {
                    Debug.Assert(elementTypes[idx] != lastType, "Two subsequent code piece elements of the same type", this);
                }
                lastType = elementTypes[idx];
            }

            elem.anchorMin = vec_0_1;
            elem.anchorMax = vec_0_1;
            elem.pivot = vec_0_1;
        }
    }

    #endregion

    #region Dragging
    
    /// <summary>
    /// If the pice is currently being dragged.
    /// </summary>
    [HideInInspector]
    public bool isDragged;

    /// <summary>
    /// Called when dragging begins.
    /// </summary>
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        isDragged = true;

        if (isAttached)
        {
            DetachFromParent();
        }

        transform.SetAsLastSibling();
    }

    /// <summary>
    /// Last found slot that this piece can potentially be attach to, when dragging finishes.
    /// </summary>
    private CodeSlot m_LastPotentialSlot;

    /// <summary>
    /// Child while dragging.
    /// </summary>
    public virtual void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0.0f);

        //Look for potential slots to attach to
        var newPotentialSlot = FindPotentialSlot();
        if (m_LastPotentialSlot != newPotentialSlot)
        {
            if (m_LastPotentialSlot != null) //Left last slot
            {
                m_LastPotentialSlot.SendMessage("PotentialChildLeave", this, SendMessageOptions.DontRequireReceiver);
            }

            if (newPotentialSlot != null) //Entered new slot
            {
                newPotentialSlot.SendMessage("PotentialChildEnter", this, SendMessageOptions.DontRequireReceiver);
            }

            m_LastPotentialSlot = newPotentialSlot;
        }
    }

    /// <summary>
    /// Called when dragging ends.
    /// </summary>
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        isDragged = false;
        if (!TryAttach())
        {
            //If not attached - try to destroy this piece
            var trashCan = FindObjectOfType<CodeTrashCan>();
            if (trashCan.transform.position.DistanceTo(transform.position) < trashCan.destroyDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion

    #region Attachment

    /// <summary>
    /// Maximum distance to attach two code pieces.
    /// </summary>
    public const float attachDistance = 25.0f;

    /// <summary>
    /// If this code piece is a child of another.
    /// </summary>
    public bool isAttached { get { return parentCode != null; } }
    
    /// <summary>
    /// The parent code piece, this one is attached to.
    /// </summary>
    public CodePiece parentCode = null;
    
    /// <summary>
    /// The parent code piece slot index, this one is attached to.
    /// </summary>
    public int slotIndex = -1;

    /// <summary>
    /// List of child code pieces, attached to this one.
    /// </summary>
    public CodePiece[] childPieces = null;
    
    /// <summary>
    /// Attach this code piece to another.
    /// </summary>
    /// <param name="slot">Slot to attach to.</param>
    public void AttachTo(CodeSlot slot)
    {
        Debug.Assert(slot != null, "Piece must be attached to a valid slot", this);
        Debug.Assert(!isAttached, "Piece is already attached", this);
        Debug.Assert(!slot.hasAttachment, "Slot already has code", slot);
        Debug.Assert(slot.piece != this, "Cannot attach code to itself", this);

        transform.SetParent(slot.transform);
        transform.SetAsLastSibling();
        transform.localPosition = Vector3.zero;
        
        var slotRectTransform = slot.transform as RectTransform;
        var rectTransform = transform as RectTransform;
        slotRectTransform.sizeDelta = rectTransform.sizeDelta;

        parentCode = slot.piece;
        slotIndex = slot.index;

        parentCode.childPieces[slotIndex] = this;
        parentCode.SendMessage("ChildAttached", this, SendMessageOptions.DontRequireReceiver);
        slotRectTransform.SendMessage("ChildAttached", this, SendMessageOptions.DontRequireReceiver);
        SendMessage("Attached", slot, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Detach this code piece from it's parent.
    /// </summary>
    public void DetachFromParent()
    {
        Debug.Assert(isAttached, "Piece is already detached", this);

        var slotRectTransform = parentCode.elements[slotIndex];
        slotRectTransform.sizeDelta = slotRectTransform.GetComponent<Image>().sprite.rect.size;

        parentCode.childPieces[slotIndex] = null;
        parentCode.SendMessage("ChildDetached", this, SendMessageOptions.DontRequireReceiver);
        slotRectTransform.SendMessage("ChildAttached", this, SendMessageOptions.DontRequireReceiver);
        SendMessage("Detached", parentCode, SendMessageOptions.DontRequireReceiver);

        parentCode = null;
        slotIndex = -1;

        transform.SetParent(CodeContainer.current.transform);
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// Look for nearby slots and try to attach to the closest one.
    /// Also look for child codes that can be attached to this one.
    /// </summary>
    public bool TryAttach()
    {
        var minSlotDistance = attachDistance;
        CodeSlot closestSlot = null;
        var minPieceDistance = attachDistance;
        CodePiece closestPiece = null;
        var candidateSlot = -1;

        var allPieces = FindObjectsOfType<CodePiece>();
        foreach (var piece in allPieces)
        {
            if (piece == this || piece.isDragged) { continue; }

            //Look for slots to attach to
            int n = piece.elements.Length;
            for (int i = 0; i < n; ++i)
            {
                if (piece.elementTypes[i] == ElementType.Content) { continue; }

                var slot = piece.elements[i].GetComponent<CodeSlot>();
                if (slot.hasAttachment) { continue; }

                var distance = Vector3.Distance(transform.position, slot.transform.position);
                if (distance < minSlotDistance)
                {
                    minSlotDistance = distance;
                    closestSlot = slot.GetComponent<CodeSlot>();
                }
            }

            if (piece.isAttached) { continue; }

            //Look to attach another piece to this one
            n = elements.Length;
            for (int i = 0; i < n; ++i)
            {
                if (elementTypes[i] == ElementType.Content) { continue; }

                var slot = elements[i].GetComponent<CodeSlot>();
                if (slot.hasAttachment) { continue; }

                var distance = Vector3.Distance(slot.transform.position, piece.transform.position);
                if (distance < minPieceDistance)
                {
                    minPieceDistance = distance;
                    closestPiece = piece;
                    candidateSlot = i;
                }
            }
        }

        //Try ot attach to another piece
        if (closestSlot != null && minSlotDistance < attachDistance)
        {
            AttachTo(closestSlot);
            return true;
        }
        //Try to attach another piece to this one
        else if (closestPiece != null && minPieceDistance < attachDistance)
        {
            closestPiece.AttachTo(elements[candidateSlot].GetComponent<CodeSlot>());
            return true;
        }

        return false;
    }

    /// <summary>
    /// Look for potential child pieces.
    /// </summary>
    public CodeSlot FindPotentialSlot()
    {
        var minSlotDistance = attachDistance;
        CodeSlot closestSlot = null;
        var minPieceDistance = attachDistance;
        CodePiece closestPiece = null;
        var candidateSlot = -1;

        var allPieces = FindObjectsOfType<CodePiece>();
        foreach (var piece in allPieces)
        {
            if (piece == this || piece.isDragged) { continue; }

            //Look for slots to attach to
            int n = piece.elements.Length;
            for (int i = 0; i < n; ++i)
            {
                if (piece.elementTypes[i] == ElementType.Content) { continue; }

                var pieceSlot = piece.elements[i].GetComponent<CodeSlot>();
                if (pieceSlot.hasAttachment) { continue; }

                var distance = Vector3.Distance(transform.position, pieceSlot.transform.position);
                if (distance < minSlotDistance)
                {
                    minSlotDistance = distance;
                    closestSlot = pieceSlot.GetComponent<CodeSlot>();
                }
            }

            if (piece.isAttached) { continue; }

            //Look to attach another piece to this one
            n = elements.Length;
            for (int i = 0; i < n; ++i)
            {
                if (elementTypes[i] == ElementType.Content) { continue; }

                var slot = elements[i].GetComponent<CodeSlot>();
                if (slot.hasAttachment) { continue; }

                var distance = Vector3.Distance(slot.transform.position, piece.transform.position);
                if (distance < minPieceDistance)
                {
                    minPieceDistance = distance;
                    closestPiece = piece;
                    candidateSlot = i;
                }
            }
        }

        if (closestSlot != null && minSlotDistance < attachDistance)
        {
            return closestSlot;
        }
        else if (closestPiece != null && minPieceDistance < attachDistance)
        {
            return elements[candidateSlot].GetComponent<CodeSlot>();
        }
        else
        {
            return null;
        }
    }

    #endregion

    #region Visuals

    /// <summary>
    /// Piece color.
    /// </summary>
    public Color color = Color.white;

    /// <summary>
    /// If this piece contains only content and no slots.
    /// </summary>
    public bool isContentOnlyPiece { get { return elements.Length == 2 && elementTypes[0] == ElementType.Content; } }

    /// <summary>
    /// Object that holds all visual elements.
    /// </summary>
    protected CodeVisuals m_Visuals;

    /// <summary>
    /// Initialize visuals.
    /// </summary>
    protected virtual void InitVisuals()
    {
        //initialize visuals object
        var visualsObj = new GameObject(name + "Visuals");
        visualsObj.transform.SetParent(transform);
        visualsObj.transform.SetAsFirstSibling();
        m_Visuals = visualsObj.AddComponent<CodeVisuals>();
    }

    /// <summary>
    /// Clear all visual objects and create new ones.
    /// Also reposition content and slots.
    /// </summary>
    protected void RebuildVisuals()
    {
        m_Visuals.RebuildVisuals();
        
        //Rebuild visuals for parent code piece
        if (parentCode != null && parentCode != this)
        {
            //First adjust the slot size
            var slotRectTransform = parentCode.elements[slotIndex].transform as RectTransform;
            var rectTransform = transform as RectTransform;
            slotRectTransform.sizeDelta = rectTransform.sizeDelta;

            //Then adjust everything in the parent
            parentCode.RebuildVisuals();
        }
    }
    
    /// <summary>
    /// Rebuild visuals when a child has been added.
    /// </summary>
    private void ChildAttached()
    {
        RebuildVisuals();
    }

    /// <summary>
    /// Rebuild visuals when a child has been removed.
    /// </summary>
    private void ChildDetached()
    {
        RebuildVisuals();
    }

    #endregion

    #region Unity Interface

    private void Awake()
    {
        Debug.Assert(transform.parent != null, "Puzzle piece without a parent");
        var vec_0_1 = new Vector2(0, 1);

        InitElements();
        ResolveElements();
        InitVisuals();
        RebuildVisuals();

        //setup own rect transform
        var rectTransform = transform as RectTransform;
        rectTransform.anchorMin = vec_0_1;
        rectTransform.anchorMax = vec_0_1;
        rectTransform.pivot = vec_0_1;
    }

    protected virtual void Start()
    {
        //Attach to parent
        if (slotIndex != -1)
        {
            if (parentCode != null && parentCode.elements.Length > slotIndex)
            {
                AttachTo(parentCode.elements[slotIndex].GetComponent<CodeSlot>());
            }
            else
            {
                slotIndex = -1;
            }
        }
        else
        {
            parentCode = null;
        }

        //Attach to children
        var n = childPieces.Length;
        for (int i = 0; i < n; ++i)
        {
            var child = childPieces[i];
            if (child == null) { continue; }
            child.AttachTo(elements[i].GetComponent<CodeSlot>());
        }
    }
    
    #endregion
}
