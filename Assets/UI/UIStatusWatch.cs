using UnityEngine.UI;

public class UIStatusWatch : Dialog
{
    public Unit unit;
    public StatusEffect.Type type;

    private Image m_Image;
    private Text m_ChargeText;

    void Start()
    {
        m_Image = GetComponent<Image>();
        m_ChargeText = FindRecursive<Text>("Charge");
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        StatusEffect effect;
        if (unit == null || !unit.effects.TryGetValue(type, out effect))
        {
            Close();
        }
        else
        {
            var info = Energy.GetStatusEffect(type);
            var details = effect.intensity > 0 ? info.positive : info.negative;

            m_Image.sprite = details.icon;
            var iconMainColor = SpellWatcher.GetMostUsedColor(details.icon);
            var textColor = SpellWatcher.GetOppositeLightnessColor(iconMainColor);

            m_ChargeText.color = textColor;
            m_ChargeText.text = effect.intensity.ToString();
        }
    }
}
