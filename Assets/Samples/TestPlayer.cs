﻿using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(EnergyController))]
public class TestPlayer : MonoBehaviour
{
    public Wizard wizard { get { return GetComponent<Wizard>(); } }

    public void Update()
    {
        for (var k = KeyCode.Alpha1; k <= KeyCode.Alpha9; ++k)
        {
            if (Gameplay.GetKeyDown(k))
            {
                var spellIdx = wizard.spellOrdering[k - KeyCode.Alpha1];
                if (spellIdx >= 0)
                {
                    var spellDesc = wizard.spells[spellIdx];

                    GameObject target = null;
                    var playerMovement = GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        target = playerMovement.selectedObject;
                    }

                    wizard.CastSpell(spellDesc.id, target);
                }
            }
        }

        if (Gameplay.GetKeyDown(KeyCode.L))
        {
            var canvas = GameObject.Find("Canvas");
            var spellBook = Instantiate(Resources.Load<GameObject>("UI/Prefabs/SpellBook"), canvas.transform);
            spellBook.GetComponent<SpellBookManager>().wizard = GetComponent<Wizard>();
        }
    }

    public Unit FindEnemy()
    {
        return Util.FindClosestObject<Unit>(transform.position, u => u.CanAttack(GetComponent<Unit>()));
    }
}
