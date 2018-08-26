using System.Collections.Generic;
using UnityEngine.UI;

public class UIWizardStats : Dialog
{
    public Wizard wizard;

    public Dictionary<EnergyManifestation, UIFocusStats> focusWatchers;
    public Dictionary<StatusEffect.Type, UIStatusWatch> statusWatchers;

    public void Awake()
    {
        if (wizard == null)
        {
            Close();
        }
    }

    public void OnEnable()
    {
        focusWatchers = new Dictionary<EnergyManifestation, UIFocusStats>();
        statusWatchers = new Dictionary<StatusEffect.Type, UIStatusWatch>();
    }

    public void Start()
    {
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

        var focuses = FindRecursive("Focuses").gameObject;
        var auras = FindRecursive("Auras").gameObject;
        var unit = wizard.GetComponent<Unit>();

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
                    watch = Spawn<UIFocusStats>(focuses);
                    watch.manifestation = focus;
                    focusWatchers.Add(focus, watch);
                }
                else if (focus == null) //energy has died
                {
                    watch.Close();
                    focusWatchers.Remove(focus);
                }
            }
        }

        var effects = unit.effects;
        foreach (var effect in effects)
        {
            if (effect.isActive)
            {
                var watch = statusWatchers.TryGetValue(effect.type, null);
                if (watch == null)
                {
                    //add watch
                    watch = Spawn<UIStatusWatch>(auras);
                    watch.unit = unit;
                    watch.type = effect.type;
                    statusWatchers[effect.type] = watch;
                }
            }
        }
    }
}
