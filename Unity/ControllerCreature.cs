using TMPro.EditorUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

class ControllerCreature : MonoBehaviour
{
    // Reference to the logic-side Creature (Shea/Clause)
    public Creature creatureInstance;
    public double atkSpdBonus = 0;

    // Link to UI (health bars, etc.)
    public Hp_Mp_Bar HpBar;
    public Hp_Mp_Bar MpBar;

    public List<CooldownScript> listCooldowns;
    void Start()
    {
        // Link UI
        if (HpBar != null && MpBar != null)
        {
            HpBar.SetMaxHealth();
            HpBar.SetHealth(creatureInstance);
            MpBar.SetMP(creatureInstance);
        }
    }

    void Update()
    {
        creatureInstance.AtkSpd = 1000 + atkSpdBonus; //pour le fun
        if (HpBar != null && MpBar != null)
        {
            HpBar.SetHealth(creatureInstance);
            MpBar.SetMP(creatureInstance);
        }
        for (int i = 0; i < listCooldowns.Count; i++)
        {
            listCooldowns[i].SetText(creatureInstance.SkillList[i]);
            listCooldowns[i].SetImage(creatureInstance.SkillList[i]);
        }
    }
}
