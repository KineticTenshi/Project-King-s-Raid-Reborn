using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering;
public class CooldownScript : MonoBehaviour
{
    public Image CooldownImage;
    public TMP_Text CooldownCounter;
    internal void SetText(Skill skill)
    {
        if (skill.CurrentCooldown > 0)
        {
            CooldownCounter.text = Math.Floor(skill.CurrentCooldown).ToString();
        }
        else
        {
            CooldownCounter.text = null;
        }
    }
    internal void SetImage(Skill skill)
    {
        CooldownImage.fillAmount = (float)skill.CurrentCooldown / (float)skill.Cooldown;
    }
}
