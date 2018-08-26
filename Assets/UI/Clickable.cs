using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SendMessage("OnLeftClick", SendMessageOptions.DontRequireReceiver);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            SendMessage("OnRightClick", SendMessageOptions.DontRequireReceiver);
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            SendMessage("OnMiddleClick", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            SendMessage("OnTap", eventData.pointerId, SendMessageOptions.DontRequireReceiver);
        }
    }
}
