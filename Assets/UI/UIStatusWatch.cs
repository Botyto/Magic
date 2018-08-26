using UnityEngine;
using UnityEngine.UI;

public class UIStatusWatch : Dialog
{
    public Unit unit;
    public StatusEffect.Type type;

    private Image m_Image;
    private Text m_IntensityText;
    private ProgressBar m_ChargeProgress;

    void Start()
    {
        m_Image = GetComponent<Image>();
        m_IntensityText = FindRecursive<Text>("Intensity");
        m_ChargeProgress = FindRecursive<ProgressBar>("ChargeProgress");
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (unit == null)
        {
            Close();
            return;
        }

        var effect = unit.effects[(int)type];
        if (effect.isActive)
        {
            return;
        }

        var info = Energy.GetStatusEffect(type);
        var details = effect.intensity > 0 ? info.positive : info.negative;

        m_Image.sprite = details.icon;
        var iconMainColor = SpellWatcher.GetMostUsedColor(details.icon);
        var textColor = SpellWatcher.GetOppositeLightnessColor(iconMainColor);

        m_IntensityText.color = textColor;
        m_IntensityText.text = effect.intensity.ToString();

        m_ChargeProgress.max = Mathf.Max(m_ChargeProgress.max, effect.charge);
        m_ChargeProgress.value = effect.charge;
    }
}
