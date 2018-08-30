using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputContentFitter : MonoBehaviour
{
    private RectTransform m_RT;
    private InputField m_Input;

    private void Start()
    {
        m_RT = GetComponent<RectTransform>();
        m_Input = GetComponent<InputField>();
        m_Input.onValueChanged.AddListener(Resize);
        Resize();
    }

    private void Resize(string str)
    {
        Resize();
    }

    private void Resize()
    {
        var txt = m_Input.textComponent;
        var placeholder = m_Input.placeholder;

        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings generationSettings = txt.GetGenerationSettings(txt.rectTransform.rect.size);
        float inputWidth = textGen.GetPreferredWidth(m_Input.text, generationSettings);

        var placeholderTxt = placeholder.GetComponent<Text>();
        var placeholderWidth = textGen.GetPreferredWidth(placeholderTxt.text, generationSettings);

        var newWidth = Mathf.Max(inputWidth, placeholderWidth) + 20;

        var sz = m_RT.sizeDelta;
        sz.x = Mathf.Max(30.0f, newWidth);
        m_RT.sizeDelta = sz;
    }
}
