using UnityEngine;
using UnityEngine.EventSystems;

public class SpellButton : MonoBehaviour, IPointerClickHandler
{
    public Wizard wizard;
    public string id;

    void OnRightClick()
    {
        wizard.CancelSpell(id);
    }

    void OnLeftClick()
    {
        GameObject target = null;
        var playerMovement = wizard.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            target = playerMovement.selectedObject;
        }

        wizard.CastSpell(id, target);
	}

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
    }
}
