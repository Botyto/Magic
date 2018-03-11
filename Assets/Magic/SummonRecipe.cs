using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Recipe", menuName = "Magic/Summon Recipe")]
public class SummonRecipe : ScriptableObject
{
    /// <summary>
    /// Object to be summoned
    /// </summary>
    public GameObject template;

    /// <summary>
    /// Energy required to summon that object
    /// </summary>
    public int requiredEnergy;
}
