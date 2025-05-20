using System.Collections.Generic;
using System;
using UnityEngine;

public enum AtkType
{
    Physical,
    Magic
}
public enum SubClassHeroMonster
{
    Knight,
    Warrior,
    Archer,
    Mechanic,
    Wizard,
    Assassin,
    Priest,

    FrostGiant,
    Harpy,
    Goblin,
    Dragon,
    Undead,
    Orc
}
public class Creature //attribute protection not complete, racial damage à faire
{
    //misc general stats
    public string Name { get; set; }
    public string Class { get; set; }
    public SubClassHeroMonster SubClass { get; set; }
    public AtkType AtkType { get; set; }
    public string AtkSubType { get; set; } // Melee or Ranged
    public string Position { get; set; }

    //main stats
    private double maxhpbase;
    public double MaxHpBase
    {
        get => this.maxhpbase;
        set
        {
            this.maxhpbase = Math.Max(value, 0);
            this.MaxHp = this.MaxHpBase * (1 + this.MaxHpLines) * (1 + this.MaxHpSkills);
            this.Hp += value * (1 + this.MaxHpLines) * (1 + this.MaxHpSkills);
        }
    }
    private double maxhplines;
    public double MaxHpLines
    {
        get => this.maxhplines;
        set
        {
            double deltaHp = this.MaxHpBase * (1 + this.MaxHpLines) * (1 + this.MaxHpSkills);
            this.maxhplines = Math.Max(value, 0);
            this.MaxHp = this.MaxHpBase * (1 + this.MaxHpLines) * (1 + this.MaxHpSkills);
            this.Hp += this.MaxHp - deltaHp;
        }
    }
    private double maxhpskills;
    public double MaxHpSkills
    {
        get => this.maxhpskills;
        set
        {
            double deltaHp = this.MaxHpBase * (1 + this.MaxHpLines) * (1 + this.MaxHpSkills);
            this.maxhpskills = Math.Max(value, 0);
            this.MaxHp = this.MaxHpBase * (1 + this.MaxHpLines) * (1 + this.MaxHpSkills);
            this.Hp += this.MaxHp - deltaHp;
        }
    }
    public double MaxHp { get; set; }
    private double hp;
    public double Hp
    {
        get => this.hp;
        set
        {
            this.hp = Math.Clamp(value, 0, this.MaxHp);
            if (this.hp == 0)
            {
                this.isDead = true;
            }
        }
    }
    private double atkbase;
    public double AtkBase
    {
        get => this.atkbase;
        set => this.atkbase = Math.Max(value, 0);
    }
    private double atklines;
    public double AtkLines
    {
        get => this.atklines;
        set => this.atklines = Math.Max(value, 0);
    }
    public double AtkSkills { get; set; }
    public double AtkFlat { get; set; }
    public double MaxMp { get; set; }
    private double mp;
    public double Mp
    {
        get => this.mp;
        set => this.mp = Math.Clamp(value, 0, this.MaxMp);
    }
    private double pdefbase;
    public double PDefBase
    {
        get => this.pdefbase;
        set => this.pdefbase = Math.Max(value, 0);
    }
    private double pdeflines;
    public double PDefLines
    {
        get => this.pdeflines;
        set => this.pdeflines = Math.Max(value, 0);
    }
    public double PDefSkills { get; set; }
    public double PDefFlat { get; set; }
    private double mdefbase;
    public double MDefBase
    {
        get => this.mdefbase;
        set => this.mdefbase = Math.Max(value, 0);
    }
    private double mdeflines;
    public double MDefLines
    {
        get => this.mdeflines;
        set => this.mdeflines = Math.Max(value, 0);
    }
    public double MDefSkills { get; set; }
    public double MDefFlat { get; set; }

    //sub stats
    public double Crit { get; set; }
    public double CritDmg { get; set; }
    public double Penetration { get; set; }
    public double Acc { get; set; }
    public double PBlock { get; set; }
    public double MBlock { get; set; }
    public double PBlockDef { get; set; }
    public double MBlockDef { get; set; }
    public double PTough { get; set; }
    public double MTough { get; set; }
    public double PDodge { get; set; }
    public double MDodge { get; set; }
    public double Lifesteal { get; set; }
    public double AtkSpd { get; set; }
    public double PWeakness { get; set; }
    public double MWeakness { get; set; }
    public double Recovery { get; set; }
    public double HealRate { get; set; }
    public double CCAcc { get; set; }
    public double CCResist { get; set; }
    public double MpRecovAtk { get; set; }
    public double MpRecovSecFlat { get; set; }
    public double MpRecovSecPercent { get; set; }
    public double MpRecovDmg { get; set; }
    public double PCritResist { get; set; }
    public double MCritResist { get; set; }
    public double PAmp { get; set; }
    public double MAmp { get; set; }
    public double BossAmp { get; set; }
    public double BossResist { get; set; }
    public double NonHeroAmp { get; set; }
    public double NonHeroResist { get; set; }
    public double TrueDmgAmp { get; set; }

    //equipment and logic stats
    public Gear Weapon { get; set; }
    public Dictionary<GearType, Gear> Gears = new Dictionary<GearType, Gear>();
    public List<Gear> TreasureList = new List<Gear>(4);
    public List<Skill> SkillList { get; set; } = new List<Skill>();
    public AutoAttack AutoAttack { get; set; }
    public Skill isLightSkill { get; set; }
    public Skill isDarkSkill { get; set; }
    public List<Skill> AbilityApplied { get; set; } = new List<Skill>();
    public Dictionary<Skill, int> StackableSkills { get; set; } = new Dictionary<Skill, int>();
    public List<KeyValuePair<Skill, double>> AppliedShields { get; set; } = new List<KeyValuePair<Skill, double>>();
    public Creature EnemyTarget { get; set; }
    public GameObject gameObject { get; set; }
    public float range {  get; set; }
    //status effect
    public int isImmuneDmg { get; set; }
    public int isIgnoreImmuneDmg { get; set; }
    public int isImmuneCC { get; set; }
    public int isIgnoreImmuneCC { get; set; }
    public int isIgnoreDef { get; set; }
    public int isIgnoreBlock { get; set; }
    public int isGuaranteeCrit { get; set; }
    public int isGuaranteeDodge { get; set; }
    public int isManaImmune { get; set; }
    public int isBuffImmune { get; set; }
    public int isDebuffImmune { get; set; }
    public bool isLight { get; set; }
    public bool isDark { get; set; }
    public bool isSilenced { get; set; }
    public bool isStunned { get; set; }
    public bool isFrozen { get; set; }
    public bool isDead { get; set; }
    //progression stats
    public int Level;
    public int Experience;
    public int Rarity;
    //transcendence
    public List<bool> listT1 = new List<bool>(5);
    public List<bool> listT2 = new List<bool>(5);
    public List<bool> listT3 = new List<bool>(8);
    public List<bool> listT5 = new List<bool>(2);
    public int MaxTP { get; set; }
    public int CurrentTP { get; set; }

    public Creature()
    {
        this.TreasureList.AddRange(new List<Gear> { null, null, null, null });
        this.BossAmp = 1;
        this.BossResist = 1; // note to self : boss resist and other racial resist must be in the sort of : (1 - modifier) => 10% NH resist : NHresist *= (1 - 0.1)
        this.NonHeroAmp = 1;
        this.NonHeroResist = 1;
    }
    public void Status()
    {
        double finalAtk = (this.AtkBase * (1 + this.AtkLines) * (1 + this.AtkSkills) + this.AtkFlat);
        double finalPDef = (this.PDefBase * (1 + this.PDefLines) * (1 + this.PDefSkills) + this.PDefFlat);
        double finalMDef = (this.MDefBase * (1 + this.MDefLines) * (1 + this.MDefSkills) + this.MDefFlat);

        Console.WriteLine($"\n Name : {this.Name}, Class : {this.Class}, SubClass : {this.SubClass}, AtkType : {this.AtkType}, Position : {this.Position} \n " +
            $"Hp : {this.Hp}/{this.MaxHp}, FinalAtk : {finalAtk}, Mp : {this.Mp}/{this.MaxMp}, PDef : {finalPDef}, MDef : {finalMDef} \n " +
            $"Crit : {this.Crit}, CritDmg : {this.CritDmg}, Penetration : {this.Penetration}, Acc : {this.Acc}, PBlock : {this.PBlock} " +
            $"PCrit Resist : {this.PCritResist}");
    }
    public void BaseKnightStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Knight;
        this.MaxHpBase = 1_706_672;
        this.AtkBase = 19_792;
        this.PDefBase = 8792;
        this.MDefBase = 6840;
        this.MaxMp = 6000;
        this.Crit = 50;
        this.PBlock = 200;
        this.PTough = 250;
        this.MTough = 250;
        this.AtkSpd = 1000;
        this.range = 2.5f;
    }
    public void BaseWarriorStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Warrior;
        this.MaxHpBase = 1_449_520;
        this.AtkBase = 22_488;
        this.PDefBase = 7328;
        this.MDefBase = 8792;
        this.MaxMp = 6000;
        this.Crit = 150;
        this.Penetration = 150;
        this.Acc = 100;
        this.PDodge = 100;
        this.MDodge = 100;
        this.PTough = 100;
        this.MTough = 100;
        this.CCResist = 150;
        this.AtkSpd = 1000;
        this.range = 2.5f;
    }
    public void BaseAssassinStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Assassin;
        this.MaxHpBase = 1_384_992;
        this.AtkBase = 24_688;
        this.PDefBase = 7816;
        this.MDefBase = 6840;
        this.MaxMp = 6000;
        this.Crit = 200;
        this.CritDmg = 30;
        this.Acc = 100;
        this.PDodge = 200;
        this.MDodge = 200;
        this.AtkSpd = 1000;
        this.range = 1;
    }
    public void BaseArcherStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Archer;
        this.MaxHpBase = 1_066_728;
        this.AtkBase = 27_864;
        this.PDefBase = 5376;
        this.MDefBase = 4392;
        this.MaxMp = 6000;
        this.Crit = 150;
        this.Penetration = 250;
        this.Acc = 100;
        this.AtkSpd = 1000;
        this.range = 5;
    }
    public void BaseMechanicStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Mechanic;
        this.MaxHpBase = 1_157_176;
        this.AtkBase = 25_416;
        this.PDefBase = 5376;
        this.MDefBase = 4392;
        this.MaxMp = 6000;
        this.Crit = 150;
        this.CritDmg = 50;
        this.Penetration = 100;
        this.Acc = 200;
        this.AtkSpd = 1000;
        this.range = 5;
    }
    public void BaseWizardStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Wizard;
        this.MaxHpBase = 985_328;
        this.AtkBase = 29_328;
        this.PDefBase = 3904;
        this.MDefBase = 5864;
        this.MaxMp = 6000;
        this.Crit = 100;
        this.Penetration = 150;
        this.Acc = 100;
        this.MBlock = 250;
        this.MDodge = 200;
        this.AtkSpd = 1000;
        this.range = 5;
    }
    public void BasePriestStatsApply()
    {
        this.SubClass = SubClassHeroMonster.Priest;
        this.MaxHpBase = 1_104_864;
        this.AtkBase = 23_216;
        this.PDefBase = 4888;
        this.MDefBase = 6104;
        this.MaxMp = 6000;
        this.Crit = 100;
        this.Acc = 100;
        this.MBlock = 500;
        this.MBlockDef = 250;
        this.MTough = 150;
        this.AtkSpd = 1000;
        this.range = 5;
    }
    // Event for when damage is received
    event EventHandler<DamageReceivedEventArgs> DamageReceived;

    // Event for when blocking occurs
    event EventHandler<BlockEventArgs> Blocked;
    internal virtual void OnDamageReceived(DamageReceivedEventArgs e)
    {
        DamageReceived?.Invoke(this, e);
    }
    internal virtual void OnBlocked(BlockEventArgs e)
    {
        Blocked?.Invoke(this, e);
    }
}

