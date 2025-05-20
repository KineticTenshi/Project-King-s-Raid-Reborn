using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[Flags]
public enum HeroClassLimit
{
    Knight = 1 << 0,
    Warrior = 1 << 1,
    Assassin = 1 << 2,
    Archer = 1 << 3,
    Mechanic = 1 << 4,
    Wizard = 1 << 5,
    Priest = 1 << 6,
    EarthClass = Knight | Warrior,
    SkyClass = Archer | Assassin | Mechanic,
    FlowClass = Priest | Wizard
}
public enum GearType
{
    Weapon,
    Armor,
    SubArmor,
    Earrings,
    Bracelet,
    Necklace,
    Ring,
    Orb,
    Treasure
}
public enum GearLineType
{
    MaxHp,
    Atk,
    Def,
    PDef,
    MDef,
    Crit,
    CritDmg,
    Penetration,
    Acc,
    Block,
    PBlock,
    MBlock,
    Dodge,
    PDodge,
    MDodge,
    Lifesteal,
    AtkSpd,
    CCAcc,
    CCResist,
    MpRecovAtk,
    MpRecovSecPercent,
    CritResist,
    PCritResist,
    MCritResist
}
public abstract class Gear //add set effect and enchants
{
    //misc stats
    public string Name { get; set; }
    public GearType Type { get; set; }
    private int awakeninglvl;
    public int AwakeningLvl
    {
        get => awakeninglvl;
        set => Math.Clamp(value, 0, 5);
    }
    public int GearTier { get; set; }
    //main stats
    public int LimitLvl { get; set; }
    public HeroClassLimit LimitClass { get; set; }
    public string LimitHero { get; set; }
    public double Hp { get; set; }
    public double Atk { get; set; }
    public double PDef { get; set; }
    public double MDef { get; set; }
    public List<Rune> RuneList { get; set; }
    public List<GearLine> LineList { get; set; }
    public int RuneListLength { get; set; }
    public int LineListLength { get; set; }
    public bool isLocked;
    //other stats
    public double FailureBonus { get; set; }
    public bool isEquippedGear { get; set; }
    public Creature EquippingHero { get; set; }

    public Gear(int RuneListLength, int LineListLength)
    {
        this.RuneListLength = RuneListLength;
        this.LineListLength = LineListLength;
        this.RuneList = new List<Rune>(this.RuneListLength);
        this.LineList = new List<GearLine>(this.LineListLength);
    }
    public void GearEquip(Creature creature)
    {
        if (this.isEquippedGear || creature.Gears.ContainsKey(this.Type))
        {
            return;
        }
        if (this.Type == GearType.Weapon)
        {
            creature.Weapon = this;
        }
        else if (this.Type == GearType.Treasure && !creature.TreasureList.Any(gear => gear.Name == this.Name))
        {
            creature.TreasureList.Add(this);
        }
        else
        {
            creature.Gears[this.Type] = this;
        }
        this.isEquippedGear = true;
        this.EquippingHero = creature;

        creature.MaxHpBase += this.Hp;
        creature.AtkBase += this.Atk;
        creature.PDefBase += this.PDef;
        creature.MDefBase += this.MDef;

        creature.Hp = creature.MaxHp;

        foreach (Rune rune in this.RuneList)
        {
            rune.RuneEffectApply();
        }
        foreach (GearLine line in this.LineList)
        {
            line.LineEffect(true);
        }
        GearEffectApply();
    }
    public void GearUnequip(Creature creature)
    {
        if (!this.isEquippedGear || !creature.Gears.ContainsKey(this.Type))
        {
            return;
        }
        if (this.Type == GearType.Weapon)
        {
            creature.Weapon = null;
        }
        else if (this.Type == GearType.Treasure)
        {
            creature.TreasureList.Remove(this);
        }
        else
        {
            creature.Gears.Remove(this.Type);
        }
        this.isEquippedGear = false;

        creature.MaxHpBase -= this.Hp;
        creature.AtkBase -= this.Atk;
        creature.PDefBase -= this.PDef;
        creature.MDefBase -= this.MDef;

        foreach (Rune rune in this.RuneList)
        {
            rune.RuneEffectUnapply();
        }
        foreach (GearLine Line in this.LineList)
        {
            Line.LineEffect(false);
        }
        this.EquippingHero = null;
        GearEffectUnapply();
    }
    public abstract void GearEffectApply();
    public abstract void GearEffectUnapply();
    public abstract Gear CloneGear();
}

