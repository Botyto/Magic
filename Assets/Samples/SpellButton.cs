using UnityEngine;

[RequireComponent(typeof(Clickable))]
public class SpellButton : MonoBehaviour
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
}
