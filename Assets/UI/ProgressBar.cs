using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    public enum TextFormat
    {
        CurrentMax,
        Current,
        Percent,
    }

    public int value = 0;
    public int max = 100;
    public TextFormat textFormat = TextFormat.Percent;

    public Image infill;
    public Text text;

    private int m_PreviousValue;
    
    private void UpdateVisuals()
    {
        if (text != null)
        {
            switch (textFormat)
            {
                case TextFormat.CurrentMax:
                    text.text = string.Format("{0}/{1}", value, max);
                    break;
                case TextFormat.Current:
                    text.text = string.Format("{0}", value);
                    break;
                case TextFormat.Percent:
                    text.text = string.Format("{0}%", (value * 100) / max);
                    break;
            }
        }

        if (infill != null)
        {
            var percent = ((float)value) / max;
            var infillRect = (infill.transform as RectTransform);
            var infillAnchorMax = infillRect.anchorMax;
            infillAnchorMax.x = percent;
            infillRect.anchorMax = infillAnchorMax;
        }
    }

    private void Start()
    {
        UpdateVisuals();   
    }

    private void Update()
    {
        if (m_PreviousValue != value)
        {
            value = Mathf.Clamp(value, 0, max);

            UpdateVisuals();
            m_PreviousValue = value;
        }
    }
}
