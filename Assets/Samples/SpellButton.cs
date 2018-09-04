using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Clickable))]
public class SpellButton : Dialog
{
    public Wizard wizard;
    public string id;

    private Image m_Icon;
    private Image m_ActiveIndicator;
    private bool m_isToggleSpell;

    private void Start()
    {
        if (wizard == null || string.IsNullOrEmpty(id))
        {
            Close();
            return;
        }

        var descr = wizard.FindSpellDescriptor(id);
        m_isToggleSpell = descr.isToggle;
        m_Icon = GetComponent<Image>();
        m_ActiveIndicator = FindRecursive<Image>("ActiveIndicator");
    }

    private void Update()
    {
        if (wizard == null)
        {
            Close();
            return;
        }

        bool active = wizard.IsSpellActive(id);
        if (m_isToggleSpell)
        {
            m_ActiveIndicator.gameObject.SetActive(active);
        }
        else
        {
            m_Icon.color = active ? Color.gray : Color.white;
        }
    }

    private void OnRightClick()
    {
        wizard.CancelSpell(id);
    }

    private void OnLeftClick()
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
