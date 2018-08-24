using System.Collections.Generic;
using UnityEngine.UI;

public class UIWizardStats : Dialog
{
    public Wizard wizard;

    public Dictionary<EnergyManifestation, UIFocusStats> focusWatchers;

    public void Start()
    {
        if (wizard == null)
        {
            Close();
            return;
        }

        focusWatchers = new Dictionary<EnergyManifestation, UIFocusStats>();

        FindRecursive<Text>("Name").text = wizard.name;

        var unit = wizard.GetComponent<Unit>();
        var holder = wizard.holder;

        var healthbar = FindRecursive<ProgressBar>("Healthbar");
        healthbar.gameObject.AddComponent<PropertyBinding>().Rebind(unit, "health", healthbar, "value");
        healthbar.gameObject.AddComponent<PropertyBinding>().Rebind(unit, "maxHealth", healthbar, "max");

        var energybar = FindRecursive<ProgressBar>("EnergyBar");
        energybar.gameObject.AddComponent<PropertyBinding>().Rebind(holder, "energy", energybar, "value");
        energybar.gameObject.AddComponent<PropertyBinding>().Rebind(wizard, "maxEnergy", energybar, "max");
    }

    public void Update()
    {
        if (wizard == null)
        {
            Close();
            return;
        }

        var activeSpells = wizard.GetComponents<SpellComponentBase>();
        foreach (var spell in activeSpells)
        {
            var maxFocus = spell.maxFocus;
            for (int i = 0; i < maxFocus; ++i)
            {
                var focus = spell.GetFocus(i);
                if (((object)focus) == null) { continue; } //no focus

                var watch = focusWatchers.TryGetValue(focus, null);
                if (watch == null)
                {
                    //add watch
                    watch = Spawn<UIFocusStats>(FindRecursive("Focuses").gameObject);
                    watch.manifestation = focus;
                    focusWatchers.Add(focus, watch);
                }
                else if(focus == null) //energy has died
                {
                    watch.Close();
                    focusWatchers.Remove(focus);
                }
            }
        }
    }
}
