using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class HealthWatcher : MonoBehaviour
{
    public Unit[] units;
    private Text txt;

    void Start()
    {
        txt = GetComponent<Text>();
    }

    void Update()
    {
        var str = "";
        foreach (var unit in units)
        {
            if (unit != null)
            {
                str += unit.name;
                str += ",HP:" + unit.health;
                var en = unit.GetComponent<EnergyHolder>();
                if (en != null)
                {
                    str += ",EN:" + en.GetEnergyScaledf();
                }

                str += " | ";
            }
        }

        txt.text = str;
    }
}
