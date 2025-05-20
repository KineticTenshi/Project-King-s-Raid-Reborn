using System.Linq;
using UnityEngine;

public abstract class Rune
{
    //misc stats
    public string Name { get; set; }
    public int Rarity { get; set; }
    //logic stats
    public GearType EquippableGear { get; set; }
    public Gear EquippingGear { get; set; }
    public bool isEquippedRune { get; set; }
    public void RuneEquip(Gear gear)
    {
        if (gear.Type != this.EquippableGear || gear.RuneList.Count() == gear.RuneListLength || this.isEquippedRune)
        {
            return;
        }

        gear.RuneList.Add(this);
        this.EquippingGear = gear;
        this.isEquippedRune = true;

        if (gear.isEquippedGear)
        {
            RuneEffectApply();
        }
    }

    public void RuneUnequip(Gear gear)
    {
        if (!this.isEquippedRune)
        {
            gear.RuneList.Remove(this);
            this.EquippingGear = null;
            this.isEquippedRune = false;
        }

        if (gear.isEquippedGear)
        {
            RuneEffectUnapply();
        }
    }
    public abstract void RuneEffectApply();
    public abstract void RuneEffectUnapply();
    public abstract Rune CloneRune();
}

