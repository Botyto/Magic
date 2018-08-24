using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellWatcher : Dialog
{
    public GameObject buttonPrefab;
    public Wizard wizard;
    private int serialNumber;

    private void Start()
    {
        UpdateSpells();
    }

    public void UpdateSpells()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Gameplay.Destroy(transform.GetChild(i).gameObject);
        }

        serialNumber = 1;
        foreach (var idx in wizard.spellOrdering)
        {
            if (idx >= 0)
            {
                var desc = wizard.spells[idx];
                AddSpell(desc);
            }
        }
    }

    public void AddSpell(SpellDescriptor desc)
    {
        var btn = Instantiate(buttonPrefab);
        btn.transform.SetParent(transform);
        var pos = transform.position;
        btn.transform.position = pos;

        var img = btn.GetComponent<Image>();
        img.sprite = desc.icon;

        var iconMainColor = GetMostUsedColor(img.sprite);
        var textColor = GetOppositeLightnessColor(iconMainColor);

        var spell = btn.GetComponent<SpellButton>();
        spell.wizard = wizard;
        spell.id = desc.id;

        var txt = btn.GetComponentsInChildren<Text>();

        var title = desc.displayName;
        if (desc.parameters.level > 1)
        {
            title += string.Format("({0}lvl)", desc.parameters.level);
        }
        txt[0].text = title;
        txt[0].color = textColor;

        txt[1].text = serialNumber.ToString();
        txt[1].color = textColor;

        serialNumber++;
    }

    static List<Color> m_PresetColors;
    public static Color GetNearestColor(Color inputColor)
    {
        var inputRed = inputColor.r;
        var inputGreen = inputColor.g;
        var inputBlue = inputColor.b;

        if (m_PresetColors == null)
        {
            m_PresetColors = new List<Color>()
            {
                Color.black,
                Color.blue,
                Color.cyan,
                Color.gray,
                Color.green,
                Color.grey,
                Color.magenta,
                Color.red,
                Color.white,
                Color.yellow,
            };
        }

        var nearestColor = Color.black;
        var distance = 500.0;
        foreach (var color in m_PresetColors)
        {
            // Compute Euclidean distance between the two colors
            var testRed = Mathf.Pow(color.r - inputRed, 2.0f);
            var testGreen = Mathf.Pow(color.g - inputGreen, 2.0f);
            var testBlue = Mathf.Pow(color.b - inputBlue, 2.0f);
            var tempDistance = Mathf.Sqrt(testBlue + testGreen + testRed);
            if (tempDistance == 0.0)
                return color;
            if (tempDistance < distance)
            {
                distance = tempDistance;
                nearestColor = color;
            }
        }
        return nearestColor;
    }

    static Dictionary<Sprite, Color> m_ColorsCache = new Dictionary<Sprite, Color>();
    public static Color GetMostUsedColor(Sprite sprite)
    {
        if (sprite == null)
        {
            return Color.white;
        }

        if (m_ColorsCache.ContainsKey(sprite))
        {
            return m_ColorsCache[sprite];
        }

        Color[] pixels;
        try
        {
            pixels = sprite.texture.GetPixels();
        }
        catch (UnityException)
        {
            return Color.white;
        }

        var colorIncidence = new Dictionary<Color, int>();
        foreach (var pixel in pixels)
        {
            if (colorIncidence.ContainsKey(pixel))
            {
                colorIncidence[pixel]++;
            }
            else
            {
                colorIncidence.Add(pixel, 1);
            }
        }
        
        var result = colorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value).First().Key;
        m_ColorsCache.Add(sprite, result);
        return result;
    }

    public static Color GetOppositeLightnessColor(Color source)
    {
        var lum = 0.2126 * source.r + 0.7152 * source.g + 0.0722 * source.b;
        return (lum > 0.5f) ? Color.black : Color.white;
    }
}
