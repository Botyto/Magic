using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIFocusStats : Dialog
{
    public EnergyManifestation manifestation;

    private Image m_Image;

    void Start()
    {
        m_Image = GetComponent<Image>();
        Update();
    }

    void Update()
    {
        if (manifestation == null)
        {
            Close();
        }
        else
        {
            m_Image.color = Energy.GetElement(manifestation.element).keyColor;
        }
    }
}
