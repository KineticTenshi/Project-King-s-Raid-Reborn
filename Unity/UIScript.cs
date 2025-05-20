using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Hp_Mp_Bar : MonoBehaviour
{
    public List<Image> ListMPBarImage = new List<Image>();
    internal void SetMP(Creature creature)
    {
        for (int i = 0; i < ListMPBarImage.Count; i++)
        {
            ListMPBarImage[i].fillAmount = ((float)creature.Mp - (float)creature.MaxMp + (6 - i) * 1000) / 1000;
        }
    }
    public Image healthBarImage;
    public void SetMaxHealth()
    {
        healthBarImage.fillAmount = 1;
    }
    internal void SetHealth(Creature creature)
    {
        healthBarImage.fillAmount = (float)creature.Hp / (float)creature.MaxHp;
    }
}


