using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour
{
    public Wizard wizard;
    public string id;

	void Start ()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(HandleClick);
    }
	
	// Update is called once per frame
	void HandleClick()
    {
        wizard.CastSpell(id, null);
	}
}
