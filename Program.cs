global using GearSpace;
global using static System.Console;

using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using HeroSpace;

namespace BaseLogic
{
    enum AtkType
    {
        Physical,
        Magic
    }
    enum SubClassHeroMonster
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
    static class Globals
    {
        public static List<Creature> listHeroes = new List<Creature>();
        public static List<Creature> listEnemies = new List<Creature>();
    }
    class Creature //attribute protection not complete, racial damage à faire
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
        public Dictionary<Skill, int> AppliedBuffs { get; set; } = new Dictionary<Skill, int>();
        public Dictionary<Skill, int> AppliedDebuffs { get; set; } = new Dictionary<Skill, int>();
        public List<KeyValuePair<Skill, double>> AppliedShields { get; set; } = new List<KeyValuePair<Skill, double>>();
        public Creature EnemyTarget { get; set; }
        //public GameObject gameObject { get; set; }
        public float range { get; set; }
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
            this.BossResist = 1; // note to self : boss resist and other racial resist must be in the type of : (1 - modifier) => 10% NH resist : NHresist *= (1 - 0.1)
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
        }
    }
    public class BattleEventManager
    {
        private static BattleEventManager _instance;
        public static BattleEventManager _Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BattleEventManager();
                }
                return _instance;
            }
        }
        private List<Action> cleanupActions = new();

        public void Register<T>(T target, Action<T> subscribe, Action<T> unsubscribe)
        {
            subscribe(target);
            cleanupActions.Add(() => unsubscribe(target));
        }
        public void ResetAll()
        {
            foreach (var action in cleanupActions)
                action.Invoke();

            cleanupActions.Clear();
        }
    }
    interface IHeroClone
    {
        public Creature CloneHero();
    }
    class HeroGearCloner
    {
        public static Creature HeroGearClone(Creature hero)
        {
            // Clone base hero
            Creature heroClone = ((IHeroClone)hero).CloneHero();

            // Copy simple properties
            heroClone.isLight = hero.isLight;
            heroClone.isDark = hero.isDark;
            heroClone.listT1 = new List<bool>(hero.listT1);
            heroClone.listT2 = new List<bool>(hero.listT2);
            heroClone.listT3 = new List<bool>(hero.listT3);
            heroClone.listT5 = new List<bool>(hero.listT5);

            // Temporary storage for cloned gears
            List<Gear> tempGears = new List<Gear>();
            List<Gear> tempTreasures = new List<Gear>(4);
            Gear weaponClone = null;

            // 1. First clone all gears without equipping
            foreach (var kvp in hero.Gears)
            {
                if (kvp.Value != null)
                {
                    List<GearLine> tempLines = new List<GearLine>();
                    List<Rune> tempRunes = new List<Rune>();
                    Gear gearClone = kvp.Value.CloneGear();
                    tempLines = kvp.Value.LineList?.Select(l => l.CloneLine()).ToList();
                    tempRunes = kvp.Value.RuneList?.Select(r => r.CloneRune()).ToList();

                    foreach (GearLine line in tempLines)
                    {
                        line.LineAdd(gearClone);
                    }
                    foreach (Rune rune in tempRunes)
                    {
                        rune.RuneEquip(gearClone);
                    }

                    tempGears.Add(gearClone);
                }
            }

            // Clone treasures
            for (int i = 0; i < hero.TreasureList.Count; i++)
            {
                if (hero.TreasureList[i] != null)
                {
                    List<GearLine> tempLines = new List<GearLine>();
                    var treasureClone = hero.TreasureList[i].CloneGear();
                    tempLines = hero.TreasureList[i].LineList?.Select(l => l.CloneLine()).ToList();

                    foreach (GearLine line in tempLines)
                    {
                        line.LineAdd(treasureClone);
                    }

                    tempTreasures.Add(treasureClone);
                }
                else
                {
                    tempTreasures.Add(null);
                }
            }

            // Clone weapon
            if (hero.Weapon != null)
            {
                List<Rune> tempRunes = new List<Rune>();
                weaponClone = hero.Weapon.CloneGear();
                tempRunes = hero.Weapon.RuneList.Select(r => r.CloneRune()).ToList();

                foreach (Rune rune in tempRunes)
                {
                    rune.RuneEquip(weaponClone);
                }
            }

            // 2. Then equip all cloned gears to activate effects
            // Equip dictionary gears
            foreach (var gearClone in tempGears)
            {
                gearClone.GearEquip(heroClone);
            }

            // Equip treasures
            for (int i = 0; i < tempTreasures.Count; i++)
            {
                if (tempTreasures[i] != null)
                {
                    tempTreasures[i].GearEquip(heroClone);
                }
            }
            weaponClone?.GearEquip(heroClone);

            return heroClone;
        }
    }
    class TranscendenceManager
    {
        public static void TranscendenceEffectApply(List<Creature> heroList)
        {
            bool knightT2_1 = false, knightT2_2 = false, knightT2_3 = false;
            bool archerT2_2 = false;
            bool mechanicT2_5 = false;
            bool wizardT2_2 = false;
            bool priestT2_2 = false, priestT2_3 = false, priestT2_5 = false;
            foreach (Creature hero in heroList)
            {
                //Apply T1 perks
                hero.AtkSkills += hero.listT1[0] ?  0.30 : 0;
                hero.MaxHpSkills += hero.listT1[1] ? 0.30 : 0;
                hero.PDefSkills += hero.listT1[2] ? 0.35 : 0;
                hero.MDefSkills += hero.listT1[2] ? 0.35 : 0;
                hero.PCritResist += hero.listT1[3] ? 250 : 0;
                hero.MCritResist += hero.listT1[3] ? 250 : 0;
                hero.NonHeroAmp += hero.listT1[4] ? 0.1 : 0;
                hero.NonHeroResist += hero.listT1[4] ? 0.1 : 0;
                //Apply T2 perks
                if (hero.SubClass == SubClassHeroMonster.Knight) //T2 pour Knight
                {
                    //trouver moyen pour appliquer T2_1. Ok en fait c'est easy j'ai juste la flemme
                    if (hero.listT2[1] == true && knightT2_2 == false) //T2_2
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.CCAcc += 150;
                        }
                        knightT2_2 = true;
                    }
                    else if (hero.listT2[1] == true && knightT2_2 == true)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.CCAcc += 50;
                        }
                    }
                    if (hero.listT2[2] == true && knightT2_3 == false) //T2_3
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.MaxHpSkills += 0.15;
                        }
                        knightT2_2 = true;
                    }
                    else if (hero.listT2[2] == true && knightT2_3 == true)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.MaxHpSkills += 0.05;
                        }
                    }
                    if (hero.listT2[3] == true) //T2_4
                    {
                        hero.PBlock += 200;
                        hero.MBlock += 200;
                        hero.CCResist += 300;
                    }
                    if (hero.listT2[3] == true) //T2_5
                    {
                        hero.AtkSpd += 400;
                        hero.Acc += 200;
                    }
                }
                if (hero.SubClass == SubClassHeroMonster.Warrior)
                {
                    if (hero.listT2[0] == true)
                    {
                        hero.Crit += 150;
                        hero.CritDmg += 30;
                    }
                    //Créer skill pour T2_2 warrior
                    //Idem pour T2_3
                    if (hero.listT2[3] == true)
                    {
                        hero.PDodge += 200;
                        hero.MDodge += 200;
                        hero.PTough += 100;
                        hero.MTough += 100;
                    }
                    if (hero.listT2[4] == true)
                    {
                        hero.Lifesteal += 200;
                        hero.AtkSpd += 200;
                    }
                }
                if (hero.SubClass == SubClassHeroMonster.Archer)
                {
                    if (hero.listT2[0] == true)
                    {
                        hero.AtkSkills += 0.2;
                        hero.Acc += 400;
                    }
                    if (hero.listT2[1]  == true && archerT2_2 == false)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.Penetration += 150;
                            ally.Acc += 75;
                        }
                        archerT2_2 = true;
                    }
                    else if (hero.listT2[1] == true && archerT2_2 == true)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.Penetration += 50;
                            ally.Acc += 25;
                        }
                    }
                    //Créer skill pour T2_3 archer
                    if (hero.listT2[3])
                    {
                        hero.Crit += 150;
                        hero.CritDmg += 30;
                    }
                    //Créer skill pour T2_5 archer
                }
                if (hero.SubClass == SubClassHeroMonster.Mechanic)
                {
                    if (hero.listT2[0])
                    {
                        hero.AtkSkills += 0.20;
                        hero.Penetration += 200;
                    }
                    //Créer skill pour T2_2 mechanic
                    hero.Crit += hero.listT2[2] ? 400 : 0;
                    //Créer skill pour T2_4 mechanic
                    if (hero.listT2[4] == true && mechanicT2_5 == false)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.CritDmg += 30;
                        }
                        mechanicT2_5 = true;
                    }
                    else if (hero.listT2[4] && mechanicT2_5 == true)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.CritDmg += 10;
                        }
                    }
                }
                if (hero.SubClass == SubClassHeroMonster.Wizard)
                {
                    //Créer skill pour T2_1 wizard
                    if (hero.listT2[1] == true && wizardT2_2 == false)
                    {
                        foreach (Creature ally in heroList)
                        {
                            ally.AtkSkills += 0.15;
                        }
                    }
                    else if (hero.listT2[1] == true && wizardT2_2 == true)
                    {
                        foreach (Creature all in heroList)
                        {
                            all.AtkSkills += 0.03;
                        }
                    }
                    //Créer skill ou attribut pour Blessing of mana
                    if (hero.listT2[3])
                    {
                        hero.PWeakness += 0.20;
                        hero.MWeakness += 0.20;
                        hero.Acc += 200;
                        hero.AtkSkills += 0.40;
                    }
                    if (hero.listT2[4])
                    {
                        hero.Crit += 200;
                        hero.Penetration += 200;
                    }
                }
                if (hero.SubClass == SubClassHeroMonster.Assassin)
                {
                    if (hero.listT2[0])
                    {
                        hero.AtkSkills += 0.20;
                        hero.Penetration += 200;
                    }
                    //Créer skill pour T2_2 assassin
                    if (hero.listT2[2])
                    {
                        hero.PDodge += 200;
                        hero.MDodge += 200;
                        hero.PTough += 100;
                        hero.MTough += 100;
                    }
                    if (hero.listT2[3])
                    {
                        hero.Crit += 150;
                        hero.CritDmg += 30;
                    }
                    //Créer skill pour T2_5 assassin
                }
                //Apply T3 perks
                for (int i = 0; i < 4; i += 1)
                {
                    hero.SkillList[i].isLight = hero.listT3[i * 2];
                    if (hero.SkillList[i].isLight)
                    {
                        hero.SkillList[i].LightEffectApply();
                    }
                }
                for (int i = 0; i < 4; i += 1)
                {
                    hero.SkillList[i].isDark = hero.listT3[i * 2 + 1];
                    if (hero.SkillList[i].isLight)
                    {
                        hero.SkillList[i].DarkEffectApply();
                    }
                }
                //Apply T5 perks
                if (hero.isLight)
                {
                    hero.isLightSkill.Use(hero);
                }
                if (hero.isDark)
                {
                    hero.isDarkSkill.Use(hero);
                }
            }
        }
        public static void TranscendenceEditor(Creature hero, int tier, int index)
        {
            if (tier == 1)
            {
                if (hero.listT1[index] || hero.CurrentTP < 10)
                {
                    return;
                }
                hero.listT1[index] = true;
                hero.CurrentTP -= 10;
            }
            if (tier == 2 || hero.CurrentTP < 15)
            {
                if (hero.listT2[index])
                {
                    return;
                }
                hero.listT1[index] = true;
                hero.CurrentTP -= 15;
            }
            if (tier == 3 && hero.CurrentTP >= 15)
            {
                if (index % 2 == 0 && hero.listT3[index + 1] == false)
                {
                    hero.listT3[index] = true;
                    hero.CurrentTP -= 15;
                }
                else if (index % 2 == 1 && hero.listT3[index - 1] == false)
                {
                    hero.listT3[index] = true;
                    hero.CurrentTP -= 15;
                }
            }
            if (tier == 5 && hero.CurrentTP >= 15)
            {
                if (hero.listT5[index])
                {
                    return;
                }
                hero.listT1[index] = true;
                hero.CurrentTP -= 15;
            }
        }
        public static void TranscendenceReset(Creature hero)
        {
            int TPrecovered = 0;
            for (int i = 0; i < hero.listT1.Count; i++)
            {
                TPrecovered += hero.listT1[i] ? 10 : 0;
                hero.listT1[i] = false;
            }
            for (int i = 0; i < hero.listT2.Count; i++)
            {
                TPrecovered += hero.listT2[i] ? 15 : 0;
                hero.listT2[i] = false;
            }
            for (int i = 0; i < hero.listT3.Count; i++)
            {
                TPrecovered += hero.listT3[i] ? 15 : 0;
                hero.listT3[i] = false;
            }
            for (int i = 0; i < hero.listT5.Count; i++)
            {
                TPrecovered += hero.listT5[i] ? 15 : 0;
                hero.listT5[i] = false;
            }
            hero.CurrentTP = TPrecovered;
        }
        public static void TranscendencePointAdd()
        {

        }
    }
    interface IAttackSource //Puisque Skill et AutoAttack paratagent des attributs commun, on les rassemble dans une interface pour ensuite permettre de réutiliser SkillDamage => ComputeDamage
    {
        public double Multiplier { get; set; }
        public double FlatValue { get; set; }
        public Creature owner { get; set; }
    }
    abstract class Skill : IAttackSource
    {
        //misc general stats
        public string Name { get; set; }
        public string Description { get; set; }

        //main stats
        public double Multiplier { get; set; }
        public double FlatValue { get; set; }
        private double mpcost;
        public double MpCost
        {
            get => mpcost;
            set => mpcost = Math.Clamp(value, 0, 6000);
        }
        public double Cooldown { get; set; } //in second
        private double currentcooldown;
        public double CurrentCooldown
        {
            get => currentcooldown;
            set => currentcooldown = Math.Max(value, 0);
        }
        public double Duration { get; set; }
        private double currentduration;
        public double CurrentDuration
        {
            get => currentduration;
            set => currentduration = Math.Max(value, 0);
        }
        public double CastingTime { get; set; }

        //logic stats
        public Creature owner { get; set; }
        public bool isIrremovable { get; set; }
        public bool isLight { get; set; }
        public bool isDark { get; set; }
        public bool isChanneling { get; set; }
        public bool isLocked { get; set; }
        public bool isCleanse { get; set; }
        public Skill(string Name, string Description, double Multiplier, double FlatValue, int MpCost, double Cooldown, double Duration, Creature owner)
        {
            this.Name = Name;
            this.Description = Description;
            this.Multiplier = Multiplier;
            this.FlatValue = FlatValue;
            this.MpCost = MpCost;
            this.Cooldown = Cooldown;
            this.Duration = Duration;
            this.owner = owner;
        }
        //other
        public Random rand { get; set; } = new Random();
        public List<Skill> ListPool { get; set; } = new();
        public void Use(Creature caster)
        {
            if (this.isChanneling) this.isChanneling = false;
            if (!isReadySkill(caster)) return;

            else
            {
                caster.Mp -= this.MpCost;
                this.CurrentDuration = this.Duration;
                this.CurrentCooldown = this.Cooldown;
                BattleEvent._Instance.TriggerOnUseSkill(caster, this);
                ApplyEffect(caster);
            }
        }
        private void RemoveFromCreature(Creature creature)
        {
            if (creature.AppliedBuffs.ContainsKey(this))
            {
                PositiveEffectRemove(creature);
                creature.AppliedBuffs.Remove(this);
            }
            if (creature.AppliedDebuffs.ContainsKey(this))
            {
                NegativeEffectRemove(creature);
                creature.AppliedDebuffs.Remove(this);
            }
        }
        public void Unuse()
        {
            foreach (Creature hero in Globals.listHeroes) RemoveFromCreature(hero);
            foreach (Creature enemy in Globals.listEnemies) RemoveFromCreature(enemy);
        }
        public void Cleanse(Creature target)
        {
            if (target?.AppliedDebuffs == null) return;

            this.ListPool.Clear();
            foreach (var kvp in target.AppliedDebuffs)
            {
                if (!kvp.Key.isIrremovable) ListPool.Add(kvp.Key);
            }
            foreach (var skill in ListPool)
            {
                skill.NegativeEffectRemove(target);
                target.AppliedDebuffs.Remove(skill);
            }
        }
        public void Dispel(Creature target)
        {
            if (target?.AppliedBuffs == null) return;

            this.ListPool.Clear();
            foreach (var kvp in target.AppliedBuffs)
            {
                if (!kvp.Key.isIrremovable) ListPool.Add(kvp.Key);
            }
            foreach (var skill in ListPool)
            {
                skill.PositiveEffectRemove(target);
                target.AppliedBuffs.Remove(skill);
            }
        }
        public void DispelShield(Creature target, Skill skill)
        {
            if (target.AppliedBuffs.ContainsKey(skill))
            {
                skill.PositiveEffectRemove(target);
                target.AppliedBuffs.Remove(skill);
            }
        }
        public bool isReadySkill(Creature caster)
        {
            if (this.isLocked) return false;
            if (caster.Mp < this.MpCost || this.CurrentCooldown != 0) return false;
            if ((caster.isSilenced || caster.isStunned || caster.isFrozen) && !this.isCleanse) return false;
            return true;
        }
        public abstract void LightEffectApply();
        public abstract void DarkEffectApply();
        protected abstract void ApplyEffect(Creature caster);
        protected abstract void PositiveEffectRemove(Creature caster);
        protected abstract void NegativeEffectRemove(Creature caster);
    }

    class BattleEvent
    {
        private static BattleEvent _instance;
        public static BattleEvent _Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BattleEvent();
                }
                return _instance;
            }
        }
        public event EventHandler OnBattleStart;
        public event EventHandler OnBlock;
        public event EventHandler OnDamageReceived;
        public event EventHandler OnUseSkill;

        public void TriggerOnBattleStart()
        {
            OnBattleStart?.Invoke(this, EventArgs.Empty);
        }
        public void TriggerOnBlock(Creature attacker, Creature blocker)
        {
            OnBlock?.Invoke(this, new AttackerReceiverEventArgs(attacker, blocker));
        }
        public void TriggerOnDamageReceived(Creature attacker, Creature receiver)
        {
            OnDamageReceived?.Invoke(this, new AttackerReceiverEventArgs(attacker, receiver));
        }
        public void TriggerOnUseSkill(Creature caster, Skill skill)
        {
            OnUseSkill?.Invoke(this, new SkillCasterEventArgs(caster, skill));
        }
    }
    class AttackerReceiverEventArgs : EventArgs
    {
        public Creature attacker { get; }
        public Creature receiver { get; }
        public AttackerReceiverEventArgs(Creature attacker, Creature receiver)
        {
            this.attacker = attacker;
            this.receiver = receiver;
        }
    }
    class SkillCasterEventArgs : EventArgs
    {
        public Creature caster { get; }
        public Skill skill { get; }
        public SkillCasterEventArgs(Creature caster, Skill skill)
        {
            this.caster = caster;
            this.skill = skill;
        }
    }
    class HpManager //to complete later with RacialDmg
    {
        private static Random rand = new Random();
        public static double BaseDamage(Creature caster, IAttackSource attackSource)
        {
            return attackSource.Multiplier * Math.Max((caster.AtkBase * (1 + caster.AtkLines) * (1 + caster.AtkSkills)) + caster.AtkFlat, 0) + attackSource.FlatValue;
        }
        public static double BaseDamageCritical(Creature caster, IAttackSource attackSource)
        {
            return (attackSource.Multiplier * Math.Max((caster.AtkBase * (1 + caster.AtkLines) * (1 + caster.AtkSkills)) + caster.AtkFlat, 0) + attackSource.FlatValue) * (2 + caster.CritDmg / 100);
        }
        public static double Penetration(Creature caster)
        {
            if (1000 < caster.Penetration)
            {
                return 0.9 - 900000 / (2 * Math.Pow(caster.Penetration, 2) + (1000 * caster.Penetration + 1000000));
            }

            if (450 < caster.Penetration && caster.Penetration <= 1000)
            {
                return 0.45 + ((caster.Penetration - 450) / 550) * 0.225;
            }

            return caster.Penetration / 1000;
        }
        public static double DefReduction(Creature caster, double def)
        {
            return 1 - (0.98 * def / (def + 19205));
        }
        public static double BlockChance(double Block)
        {
            if (Block > 1690)
            {
                return 0.9;
            }

            if (1000 < Block && Block <= 1690)
            {
                double a = 2.07e-8;
                double b = -3.82e-5;
                double c = 3.82e-2;
                double d = 1.8e-2;

                double BlockAbove1000 = Block - 1000;

                return 0.75 + (a * Math.Pow(BlockAbove1000, 3) + b * Math.Pow(BlockAbove1000, 2) + c * BlockAbove1000 + d) / 100;
            }

            if (500 < Block && Block <= 1000)
            {
                return 0.5 + ((Block - 500) / 2);
            }

            return Block / 1000;
        }
        public static double BlockDefReduction(double BlockDef)
        {
            if (BlockDef > 1025)
            {
                return 0.885;
            }

            if (225 < BlockDef && BlockDef <= 1025)
            {
                return 0.725 + (BlockDef - 225) * 0.0002;
            }

            return 0.5 + BlockDef / 1000;
        }
        public static double ToughReduction(double Tough)
        {
            if (Tough >= 1500)
            {
                return 0.75;
            }

            if (900 < Tough && Tough < 1500)
            {
                return 0.63 + (Tough - 900) * 0.0002;
            }

            if (450 < Tough && Tough <= 900)
            {
                return 0.45 + (Tough - 450) * 0.0004;
            }

            return Tough / 1000;
        }

        public static double ComputeDamage(Creature caster, Creature target, IAttackSource attackSource)
        {
            double Dmg;
            double Dodge;
            double CritResist;
            double Def;
            double block;
            double BlockDef;
            double Tough;
            double Weakness;
            double Amp;

            if (target.isImmuneDmg != 0 && caster.isIgnoreImmuneDmg == 0)
            {
                Console.WriteLine("\n INVINCIBILITY \n");
                return 0;
            }

            if (caster.AtkType == AtkType.Physical)
            {
                Dodge = target.PDodge;
                CritResist = target.PCritResist;
                Def = (target.PDefBase * (1 + target.PDefLines) * (1 + target.PDefSkills) + target.PDefFlat) * (1 - Penetration(caster));
                block = target.PBlock;
                BlockDef = target.PBlockDef;
                Tough = target.PTough;
                Weakness = target.PWeakness;
                Amp = caster.PAmp;
            }

            else
            {
                Dodge = target.MDodge;
                CritResist = target.MCritResist;
                Def = (target.MDefBase * (1 + target.MDefLines) * (1 + target.MDefSkills) + target.MDefFlat) * (1 - Penetration(caster));
                block = target.MBlock;
                BlockDef = target.MBlockDef;
                Tough = target.MTough;
                Weakness = target.MWeakness;
                Amp = caster.MAmp;
            }

            if (target.isGuaranteeDodge !=0 || Dodge - caster.Acc >= rand.Next(0, 1000))
            {
                Console.WriteLine("DODGE");
                return 0;
            }

            if ((caster.Crit - CritResist) >= rand.Next(0, 1000) || caster.isGuaranteeCrit != 0)
            {
                Console.WriteLine("\n CRITICAL \n");
                Dmg = BaseDamageCritical(caster, attackSource);
            }

            else
            {
                Console.WriteLine("\n NON CRITICAL \n");
                Dmg = BaseDamage(caster, attackSource);
            }

            if (caster.isIgnoreDef == 0)
            {
                Console.WriteLine("\n NON DEF IGNORE \n");
                Dmg *= DefReduction(caster, Def);
            }

            else
            {
                Dmg *= (1 + caster.TrueDmgAmp);
                Console.WriteLine("\n DEF IGNORE \n");
            }

            if (BlockChance(block) * 100 >= rand.Next(0, 100) && caster.isIgnoreBlock == 0)
            {
                BattleEvent._Instance.TriggerOnBlock(caster, target);
                Console.WriteLine("\n BLOCK \n");
                Dmg *= BlockDefReduction(BlockDef);
            }

            else
            {
                Console.WriteLine("\n NON BLOCK \n");
            }

            Dmg *= (1 - ToughReduction(Tough)) * (1 + Weakness + Amp);
            WriteLine($"Target receiving damage : {target.Name}");
            BattleEvent._Instance.TriggerOnDamageReceived(caster, target);
            if (target.Class != "Hero")
            {
                Dmg *= caster.NonHeroAmp;
            }
            if (caster.Class != "Hero")
            {
                Dmg *= target.NonHeroResist;
            }

            if (target.Class == "Boss")
            {
                Dmg *= caster.BossAmp;
            }
            if (caster.Class == "Boss")
            {
                Dmg *= target.BossResist;
            }

            int nShield = target.AppliedShields.Count;
            if (nShield > 0)
            {
                while (nShield > 0 && Dmg > 0)
                {
                    var shield = target.AppliedShields[nShield - 1];
                    if (shield.Value > Dmg)
                    {
                        target.AppliedShields[nShield - 1] = new KeyValuePair<Skill, double>(shield.Key, shield.Value - Dmg);
                        Dmg = 0;
                    }
                    else
                    {
                        Dmg -= shield.Value;
                        shield.Key.DispelShield(target, shield.Key);
                    }
                    nShield = target.AppliedShields.Count;
                }
            }
            target.Hp -= Convert.ToInt64(Dmg);
            caster.Hp += Convert.ToInt64(Dmg * caster.Lifesteal / 1000);

            WriteLine($"\n Damage dealt : {Dmg}\n");

            return Dmg;
        }
        public static void SkillHeal(Creature caster, Creature target, Skill skill)
        {
            double healAmount = (skill.Multiplier * ((caster.AtkBase * (1 + caster.AtkLines) * (1 + caster.AtkSkills)) + caster.AtkFlat) + skill.FlatValue) * (1 + caster.HealRate) * (1 + target.Recovery);

            if (caster.Crit < rand.Next(1, 1000))
            {
                target.Hp += Convert.ToInt64(healAmount);
                return;
            }
            target.Hp += Convert.ToInt64(healAmount * (1 + caster.CritDmg / 100));
        }
        public static void SkillHealNonCrit(Creature caster, Creature target, Skill skill)
        {
            double healAmount = (skill.Multiplier * ((caster.AtkBase * (1 + caster.AtkLines) * (1 + caster.AtkSkills)) + caster.AtkFlat) + skill.FlatValue) * (1 + caster.HealRate) * (1 + target.Recovery);
            target.Hp += Convert.ToInt64(healAmount);
        }
        public static double ShieldAmount(Creature caster, Skill skill)
        {
            return skill.Multiplier * (caster.AtkBase * (1 + caster.AtkLines) * (1 + caster.AtkSkills) + caster.AtkFlat) + skill.FlatValue;
        }
    }
    class TimeManager
    {
        public void CooldownDurationDecrementation()
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                for (int i = 0; i < hero.SkillList.Count; i++)
                {
                    var skill = hero.SkillList[i];
                    if (skill.CurrentCooldown > 0)
                    {
                        //skill.CurrentCooldown -= Time.deltaTime;
                    }
                    if (skill.CurrentDuration > 0)
                    {
                        //skill.CurrentDuration -= Time.deltaTime;
                        if (skill.CurrentDuration <= 0)
                        {
                            skill.Unuse();
                        }
                    }
                }
            }
            foreach (Creature enemi in Globals.listEnemies)
            {
                for (int i = 0; i < enemi.SkillList.Count; i++)
                {
                    var skill = enemi.SkillList[i];
                    if (skill.CurrentCooldown > 0)
                    {
                        //skill.CurrentCooldown -= Time.deltaTime;
                    }
                    if (skill.CurrentDuration > 0)
                    {
                        //skill.CurrentDuration -= Time.deltaTime;
                        if (skill.CurrentDuration <= 0)
                        {
                            skill.Unuse();
                        }
                    }
                }
            }
        }
    }
    class ListParser
    {
        public static List<Creature> ParserHP(List<Creature> ListToParse, bool isMax, int n)
        {
            return isMax
                    ? ListToParse.OrderByDescending(c => c.Hp).Take(n).ToList()  // Trier décroissant et prendre les n premiers
                    : ListToParse.OrderBy(c => c.Hp).Take(n).ToList();           // Trier croissant et prendre les n premiers
        }
        public static List<Creature> ParserAtk(List<Creature> ListToParse, bool isMax, int n)
        {
            return isMax
                    ? ListToParse.OrderByDescending(c => (c.AtkBase * (1 + c.AtkLines) * (1 + c.AtkSkills) + c.AtkFlat)).Take(n).ToList()  // Trier décroissant et prendre les n premiers
                    : ListToParse.OrderBy(c => (c.AtkBase * (1 + c.AtkLines) * (1 + c.AtkSkills) + c.AtkFlat)).Take(n).ToList();           // Trier croissant et prendre les n premiers
        }
    }
    abstract class AutoAttack : IAttackSource
    {
        public HpManager hpManager;
        public string Name { get; set; }
        public int AutoAtkNumberPerCycle { get; set; }
        public double Multiplier { get; set; }
        public double FlatValue { get; set; }
        public double Delay { get; set; }
        public Creature owner { get; set; }
        public AutoAttack(int AutoAtkNumberPerCycle, double Multiplier, double Flatvalue, Creature owner) 
        {
            this.AutoAtkNumberPerCycle = AutoAtkNumberPerCycle;
            this.Multiplier = Multiplier;
            this.FlatValue = Flatvalue;
            this.owner = owner;
        }
        public virtual void Use(Creature attacker)
        {
            if (attacker.isStunned || attacker.isFrozen)
            {
                return;
            }
            for (int i = 0; i < this.AutoAtkNumberPerCycle; i++)
            {
                HpManager.ComputeDamage(attacker, attacker.EnemyTarget, this);
                attacker.Mp += attacker.MpRecovAtk / this.AutoAtkNumberPerCycle;

                //time Delay à implémenter ou ailleurs jsp
            }
            AutoAtkEffect();
        }
        public abstract void AutoAtkEffect();
    }
    class Program
    {
        static void Main() //Test de tout le système
        {
            //mesure temps de calcul, start
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Kasel kasel = new Kasel();
            Shea shea = new Shea();

            BattleEvent._Instance.TriggerOnBattleStart();

            Globals.listHeroes.Add(kasel);
            Globals.listHeroes.Add(shea);

            Clause clause = new Clause();
            Globals.listHeroes.Add(clause);

            kasel.EnemyTarget = clause;
            shea.EnemyTarget = clause;
            clause.EnemyTarget = kasel;

            Gear armor = new ArmorTest();
            Aea aea = new Aea();

            RuneCrit critRune1 = new RuneCrit();
            RuneAtk atkRune2 = new RuneAtk();
            RuneAtk atkRune3 = new RuneAtk();
            RuneAtk atkRune4 = new RuneAtk();

            critRune1.RuneEquip(armor);
            atkRune2.RuneEquip(aea);
            atkRune3.RuneEquip(aea);
            atkRune4.RuneEquip(aea);

            kasel.Status();
            shea.Status();
            clause.Status();
            
            WriteLine("\nCréation copie Kasel");

            Creature kaselCopy = ((IHeroClone)Globals.listHeroes[0]).CloneHero();
            kaselCopy.Status();

            kasel.Mp += 10000;
            kaselCopy.Mp += 10000;

            kasel.SkillList[0].Use(kasel);

            kasel.Status();
            kaselCopy.Status();

            WriteLine(kasel.SkillList[0].CurrentCooldown);
            WriteLine(kaselCopy.SkillList[0].CurrentCooldown);

            WriteLine($"\n Abilities applied on Kasel: {string.Join(", ", kasel.AppliedBuffs.Select(skill => skill.Key.Name))}");
            WriteLine($"\n Abilities applied on KaselCopy: {string.Join(", ", kaselCopy.AppliedBuffs.Select(skill => skill.Key.Name))}");

            armor.GearEquip(kasel);
            aea.GearEquip(kasel);
            WriteLine("\nEquip gear and weapon on kasel");
            kasel.Status();

            WriteLine("\nMake another copy of kasel");
            Creature kaselCopy2 = HeroGearCloner.HeroGearClone(kasel);

            kaselCopy2.Status();

            kasel.EnemyTarget = shea;
            kasel.SkillList[0].CurrentCooldown = 0;
            kasel.SkillList[0].Use(kasel);

            kasel.Status();
            shea.Status();

            WriteLine("\nTest Soul Weapon Exian");

            //set up exian SW
            Exian exian = new();
            exian.SoulWeapon.AdvancementLvl = 0;
            exian.SoulWeapon.CurrentCharges = exian.SoulWeapon.MaxCharges;

            //equip UW/SW
            WriteLine("\nBefore equipping UW w/ SoulWeapon");
            clause.Status();
            exian.GearEquip(clause);
            armor.GearUnequip(kasel);
            WriteLine("\nAfter equipping UW w/ SoulWeapon");
            clause.Status();
            kasel.Status();

            WriteLine("\nUse SW on Kasel");
            if (clause.Weapon is Exian exianTemp)
            {
                exianTemp.SoulWeapon.Skill.Use(clause);
            }

            clause.Status();
            kasel.Status();

            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);

            clause.Status();
            kasel.Status();

            ((IWeapon)clause.Weapon).SoulWeapon.Skill.Use(clause);

            clause.Status();
            kasel.Status();

            WriteLine($"\n Abilities applied on Kasel: {string.Join(", ", kasel.AppliedBuffs.Select(skill => skill.Key.Name))}");
            WriteLine($"\n Abilities applied on Clause: {string.Join(", ", clause.AppliedBuffs.Select(skill => skill.Key.Name))}");

            ((IWeapon)clause.Weapon).SoulWeapon.AdvancementLvl = 1;

            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);
            clause.SkillList[0].Use(clause);

            ((IWeapon)clause.Weapon).SoulWeapon.Skill.Use(clause);

            clause.Status();
            kasel.Status();

            /*
            Console.WriteLine($"\n Abilities applied on Kasel: {string.Join(", ", kasel.AbilityApplied.Select(skill => skill.Name))}");
            Console.WriteLine($"\n Abilities applied on Clause: {string.Join(", ", clause.AbilityApplied.Select(skill => skill.Name))}");
            Console.WriteLine($"\n Abilities applied on Shea: {string.Join(", ", shea.AbilityApplied.Select(skill => skill.Name))}");

            Console.WriteLine("\n Shea S2 activation without UT2 (first time)");
            shea.SkillList[1].Use(shea);

            kasel.Status();
            shea.Status();
            clause.Status();

            Console.WriteLine($"\n Abilities applied on Kasel: {string.Join(", ", kasel.AbilityApplied.Select(skill => skill.Name))}");
            Console.WriteLine($"\n Abilities applied on Clause: {string.Join(", ", clause.AbilityApplied.Select(skill => skill.Name))}");
            Console.WriteLine($"\n Abilities applied on Shea: {string.Join(", ", shea.AbilityApplied.Select(skill => skill.Name))}");

            Console.WriteLine("\n Add 10 000 MP to Kasel and allow him to hit clause with S1");
            kasel.Mp += 10000;
            kasel.SkillList[0].Use(kasel);

            kasel.Status();
            shea.Status();
            clause.Status();

            Console.WriteLine("\n UT2 Equipped on Shea");

            WindyFlower windyflower = new WindyFlower();
            windyflower.GearEquip(shea);
            */

            stopwatch.Stop();
            Console.WriteLine($"Temps écoulé: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}