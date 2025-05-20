using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class NewGameLogic : MonoBehaviour
{
    public GameObject shea;
    public GameObject clause;
    public GameObject valance;
    public GameObject clauseTest;
    public List<GameObject> listGameObjectsHeroes, listGameObjectsEnemies;
    public float healTime = 0f;

    public void Awake()
    {
        shea = GameObject.Find("Shea");
        clause = GameObject.Find("Clause");
        valance = GameObject.Find("Valance");
        clauseTest = GameObject.Find("ClauseTest");

        ControllerCreature sheaController = shea.GetComponent<ControllerCreature>();
        ControllerCreature clauseController = clause.GetComponent<ControllerCreature>();
        ControllerCreature valanceController = valance.GetComponent<ControllerCreature>();
        ControllerCreature clauseTestController = clauseTest.GetComponent<ControllerCreature>();

        sheaController.creatureInstance = new Shea();
        clauseController.creatureInstance = new Clause();
        valanceController.creatureInstance = new Valance();
        clauseTestController.creatureInstance = new Clause();

        GameObject.Find("Character_Slot_1").GetComponent<ContainerHero>().hero = sheaController.creatureInstance;
        GameObject.Find("Character_Slot_2").GetComponent<ContainerHero>().hero = valanceController.creatureInstance;

        sheaController.creatureInstance.gameObject = shea;
        clauseController.creatureInstance.gameObject = clause;
        valanceController.creatureInstance.gameObject = valance;
        clauseTestController.creatureInstance.gameObject = clauseTest;

        Globals.listHeroes.Add(sheaController.creatureInstance);
        Globals.listHeroes.Add(valanceController.creatureInstance);
        Globals.listEnemies.Add(clauseController.creatureInstance);
        Globals.listEnemies.Add(clauseTestController.creatureInstance);

        Gear schwanheite = new Schwanheite();
        schwanheite.GearEquip(sheaController.creatureInstance);

        Gear exian = new Exian();
        exian.GearEquip(clauseController.creatureInstance);
        Rune rune1 = new RuneAtk();
        Rune rune2 = new RuneAtk();
        Rune rune3 = new RuneAtk();

        clauseTestController.creatureInstance.Name = "ClauseTest";

        /*rune1.RuneEquip(exian);
        rune2.RuneEquip(exian);
        rune3.RuneEquip(schwanheite);*/
    }
    public void Update()
    {
        healTime += Time.deltaTime;
        if (healTime >= 2f)
        {
            Globals.listEnemies[1].Hp += Globals.listEnemies[0].MaxHp / 10;
            healTime = 0f;
        }
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
static class Globals
{
    public static List<Creature> listHeroes = new List<Creature>();
    public static List<Creature> listEnemies = new List<Creature>();
    public static List<Hp_Mp_Bar> listHpBars = new List<Hp_Mp_Bar>();
    public static List<Hp_Mp_Bar> listMpBars = new List<Hp_Mp_Bar>();
    public static System.Random rand = new System.Random();
}
class DamageReceivedEventArgs : EventArgs
{
    public Creature Attacker { get; set; }
    public double DamageAmount { get; set; }
    public bool WasBlocked { get; set; }
    public bool IsHeroDamage { get; set; }
}

class BlockEventArgs : EventArgs
{
    public Creature Attacker { get; set; }
    public double BlockedDamage { get; set; }
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
        bool /*knightT2_1 = false,*/ knightT2_2 = false, knightT2_3 = false;
        bool archerT2_2 = false;
        bool mechanicT2_5 = false;
        bool wizardT2_2 = false;
        /*bool priestT2_2 = false, priestT2_3 = false, priestT2_5 = false;*/
        foreach (Creature hero in heroList)
        {
            //Apply T1 perks
            hero.AtkSkills += hero.listT1[0] ? 0.30 : 0;
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
                if (hero.listT2[1] == true && archerT2_2 == false)
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
            }
            for (int i = 0; i < 4; i += 1)
            {
                hero.SkillList[i].isDark = hero.listT3[i * 2 + 1];
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
    public string Name { get; set; }
    public double Multiplier { get; set; }
    public double FlatValue { get; set; }
    public Creature owner { get; set; }
    public void Use(Creature caster);
}
//delegate for skill events
public delegate void UseDelegate(Creature caster, EventArgs e);
public delegate void BlockDelegate(Creature blocker, EventArgs e);
public delegate void DamageReceivedDelegate(Creature receiver, EventArgs e);
class HpManager //to complete later with RacialDmg
{
    public static event BlockDelegate OnBlock; // Événement avec expéditeur
    public static event DamageReceivedDelegate OnDamageReceived;
    private static System.Random rand = new System.Random();
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

        if (target.isGuaranteeDodge != 0 || Dodge - caster.Acc >= rand.Next(0, 1000))
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
            //OnBlock?.Invoke(target, EventArgs.Empty);
            Console.WriteLine("\n BLOCK \n");
            Dmg *= BlockDefReduction(BlockDef);
        }

        else
        {
            Console.WriteLine("\n NON BLOCK \n");
        }

        Dmg *= (1 - ToughReduction(Tough)) * (1 + Weakness + Amp);
        //WriteLine($"Target receiving damage : {target.Name}");
        OnDamageReceived?.Invoke(target, EventArgs.Empty);
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

        Debug.Log($"Damage dealt : {Dmg} || From {caster.Name} to {target.Name}");

        return Dmg;
    }
    public static void SkillHeal(Creature caster, Creature target, Skill skill)
    {
        System.Random rand = new System.Random();

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
    public static void CooldownDurationDecrementation()
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            for (int i = 0; i < hero.SkillList.Count; i++)
            {
                var skill = hero.SkillList[i];
                if (skill.CurrentCooldown > 0)
                {
                    skill.CurrentCooldown -= Time.deltaTime;
                }
                if (skill.CurrentDuration > 0)
                {
                    skill.CurrentDuration -= Time.deltaTime;
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
                    skill.CurrentCooldown -= Time.deltaTime;
                }
                if (skill.CurrentDuration > 0)
                {
                    skill.CurrentDuration -= Time.deltaTime;
                    if (skill.CurrentDuration <= 0)
                    {
                        skill.Unuse();
                    }
                }
            }
        }
    }
    public static void ManaIncrementation()
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            hero.Mp += (600 * (1 + hero.MpRecovSecPercent) + hero.MpRecovSecFlat) * Time.deltaTime;
        }
        foreach (Creature enemy in Globals.listEnemies)
        {
            enemy.Mp += (600 * (1 + enemy.MpRecovSecPercent) + enemy.MpRecovSecFlat) * Time.deltaTime;
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

class GearAwakener
{
    private static List<Gear> ListGearToSacrifice = new List<Gear>();
    private static System.Random rand = new System.Random();
    public static void SacrificeAdd(Gear gear)
    {
        ListGearToSacrifice.Add(gear);
    }
    public static void SacrificeRemove(Gear gear)
    {
        ListGearToSacrifice.Remove(gear);
    }
    public static void SacrificeClear()
    {
        ListGearToSacrifice.Clear(); //ajouter autres nettoyages de l'inventaire plus tard
    }
    public static double ComputeAwakeningChance(Gear gearToAwaken)
    {
        int sum = 0;
        for (int i = 0; i < ListGearToSacrifice.Count; i++)
        {
            sum += (int)Math.Pow(2, Math.Max(4 - (gearToAwaken.AwakeningLvl - ListGearToSacrifice[i].AwakeningLvl), 0));
        }
        return sum switch
        {
            1 => 1,
            2 => 10,
            _ => Math.Min(100, 100 * sum / 16)
        };
    }
    public static void WeaponAwakening(Gear gearToAwaken)
    {
        if (gearToAwaken.AwakeningLvl == 5 || ListGearToSacrifice.Count == 0)
        {
            //WriteLine("Weapon at max awakening or no gear to sacrifice !");
            return;
        }
        double probability = ComputeAwakeningChance(gearToAwaken);
        if (rand.Next(0, 100) <= probability + gearToAwaken.FailureBonus)
        {
            //WriteLine("Success !");
            gearToAwaken.AwakeningLvl += 1;
            gearToAwaken.FailureBonus = 0;
        }
        else
        {
            //WriteLine("Failure !");
            gearToAwaken.FailureBonus += probability / 2;
        }
        ListGearToSacrifice.Clear(); //enlever toutes les futures références pour que le GC supprimme l'objet
    }
}
public class GearLine
{
    public GearLineType LineType { get; set; }
    public double LineValue { get; set; }
    public Gear EquippingGear { get; set; }
    public bool isEquippedLine { get; set; }
    private static readonly Dictionary<GearLineType, Action<Creature, double>> effectMap = new()
        {
            { GearLineType.MaxHp, (hero, v) => hero.MaxHpLines += v },
            { GearLineType.Atk, (hero, v) => hero.AtkLines += v },
            { GearLineType.PDef, (hero, v) => hero.PDefLines += v },
            { GearLineType.MDef, (hero, v) => hero.MDefLines += v },
            { GearLineType.Crit, (hero, v) => hero.Crit += v },
            { GearLineType.CritDmg, (hero, v) => hero.CritDmg += v },
            { GearLineType.Penetration, (hero, v) => hero.Penetration += v },
            { GearLineType.Acc, (hero, v) => hero.Acc += v },
            { GearLineType.PBlock, (hero, v) => hero.PBlock += v },
            { GearLineType.MBlock, (hero, v) => hero.MBlock += v },
            { GearLineType.PDodge, (hero, v) => hero.PDodge += v },
            { GearLineType.MDodge, (hero, v) => hero.MDodge += v },
            { GearLineType.Lifesteal, (hero, v) => hero.Lifesteal += v },
            { GearLineType.AtkSpd, (hero, v) => hero.AtkSpd += v },
            { GearLineType.CCAcc, (hero, v) => hero.CCAcc += v },
            { GearLineType.CCResist, (hero, v) => hero.CCResist += v },
            { GearLineType.MpRecovAtk, (hero, v) => hero.MpRecovAtk += v },
            { GearLineType.MpRecovSecPercent, (hero, v) => hero.MpRecovSecPercent += v },
            { GearLineType.PCritResist, (hero, v) => hero.PCritResist += v },
            { GearLineType.MCritResist, (hero, v) => hero.MCritResist += v },

            { GearLineType.Def, (hero, v) => { hero.PDefLines += v; hero.MDefLines += v; } },
            { GearLineType.Block, (hero, v) => { hero.PBlock += v; hero.MBlock += v; } },
            { GearLineType.Dodge, (hero, v) => { hero.PDodge += v; hero.MDodge += v; } },
            { GearLineType.CritResist, (hero, v) => { hero.PCritResist += v; hero.MCritResist += v; } }
        };
    public void LineAdd(Gear gear)
    {
        if (gear.LineList.Count() == gear.LineListLength)
        {
            return;
        }
        gear.LineList.Add(this);
        this.EquippingGear = gear;
        this.isEquippedLine = true;
        //WriteLine("Line Equipped !");
        if (gear.isEquippedGear)
        {
            LineEffect(true);
        }
    }
    public void LineRemove(Gear gear)
    {
        if (!this.isEquippedLine)
        {
            gear.LineList.Remove(this);
            //WriteLine("Line Unequipped !");
            this.EquippingGear = null;
            this.isEquippedLine = false;
        }

        if (gear.isEquippedGear)
        {
            LineEffect(false);
        }
    }
    public void LineEffect(bool apply)
    {
        double sign = apply ? 1 : -1;
        double value = sign * this.LineValue;
        var hero = this.EquippingGear.EquippingHero;

        if (effectMap.TryGetValue(this.LineType, out var applyEffect))
        {
            applyEffect(hero, value);
        }
    }
    public GearLine CloneLine()
    {
        GearLine gearLineClone = new GearLine();
        gearLineClone.LineType = this.LineType;
        gearLineClone.LineValue = this.LineValue;
        return gearLineClone;
    }
}
class GearLineManager
{
    private System.Random rand = new System.Random();
    private Dictionary<GearLineType, List<double>> TMLineValues = new Dictionary<GearLineType, List<double>>()
    {
        { GearLineType.MaxHp, new List<double> { 0.12, 0.14, 0.16 } },
        { GearLineType.Atk, new List<double> { 0.12, 0.14, 0.16 } },
        { GearLineType.PDef, new List<double> { 0.24, 0.28, 0.32 } },
        { GearLineType.MDef, new List<double> { 0.24, 0.28, 0.32 } },
        { GearLineType.Crit, new List<double> { 120, 140, 160 } },
        { GearLineType.CritDmg, new List<double> { 24, 28, 32 } },
        { GearLineType.Penetration, new List<double> { 120, 140, 160 } },
        { GearLineType.Acc, new List<double> { 120, 140, 160 } },
        { GearLineType.PBlock, new List<double> { 240, 280, 320 } },
        { GearLineType.MBlock, new List<double> { 240, 280, 320 } },
        { GearLineType.PDodge, new List<double> { 120, 140, 160 } },
        { GearLineType.MDodge, new List<double> { 120, 140, 160 } },
        { GearLineType.Lifesteal, new List<double> { 120, 140, 160 } },
        { GearLineType.AtkSpd, new List<double> { 120, 140, 160 } },
        { GearLineType.CCAcc, new List<double> { 120, 140, 160 } },
        { GearLineType.CCResist, new List<double> { 120, 140, 160 } },
        { GearLineType.MpRecovAtk, new List<double> { 240, 280, 320 } },
        { GearLineType.MpRecovSecPercent, new List<double> { 0.36, 0.42, 0.48 } },
        { GearLineType.PCritResist, new List<double> { 240, 280, 320 } },
        { GearLineType.MCritResist, new List<double> { 240, 280, 320 } },
    };
    public void GearLineReforge(GearLine Line)
    {
        int enumLength = Enum.GetNames(typeof(GearLineType)).Length;
        GearLineType newline;
        double newvalue;

        if (Line.EquippingGear.GearTier < 10)
        {
            newline = (GearLineType)rand.Next(0, enumLength);
            newvalue = ComputeNonTMLineValue(newline);
        }
        else
        {
            do
            {
                newline = (GearLineType)rand.Next(0, enumLength);
            }
            while (newline == GearLineType.Def || newline == GearLineType.Block
                || newline == GearLineType.Dodge || newline == GearLineType.CritResist);

            newvalue = ComputeTMLineValue(Line.LineType, newline, Line.LineValue);
        }

        Console.WriteLine($"New Line Type : {newline} with value : {newvalue}" +
                    $"\nAccept new line ? Yes = 1, No = 0");

        bool validInput = false;
        do
        {
            if (int.TryParse(Console.ReadLine(), out int i))
            {
                switch (i)
                {
                    case 0:
                        Console.WriteLine("\nNew line rejected");
                        validInput = true;
                        break;
                    case 1:
                        Console.WriteLine("\nNew line accepted");
                        Line.LineEffect(false);
                        Line.LineType = newline;
                        Line.LineValue = newvalue;
                        Line.LineEffect(true);
                        validInput = true;
                        break;
                    default:
                        Console.WriteLine("\nPlease enter a valid command (0 or 1)");
                        break;
                }
            }
            else
            {
                Console.WriteLine("\nInvalid input, please enter 0 or 1");
            }
        }
        while (!validInput);
    }
    public double ComputeNonTMLineValue(GearLineType LineType)
    {
        switch (LineType)
        {
            case GearLineType.MaxHp:
            case GearLineType.Atk:
            case GearLineType.Def:
                return (rand.NextDouble() * 6 + 6) / 100;
            case GearLineType.PDef:
            case GearLineType.MDef:
                return (rand.NextDouble() * 12 + 12) / 100;
            case GearLineType.Crit:
            case GearLineType.PDodge:
            case GearLineType.MDodge:
            case GearLineType.Lifesteal:
            case GearLineType.AtkSpd:
            case GearLineType.CCAcc:
            case GearLineType.CCResist:
            case GearLineType.Penetration:
            case GearLineType.Acc:
            case GearLineType.Block:
            case GearLineType.CritResist:
                return rand.NextDouble() * 60 + 60;
            case GearLineType.CritDmg:
                return rand.NextDouble() * 12 + 12;
            case GearLineType.PBlock:
            case GearLineType.MBlock:
            case GearLineType.MpRecovAtk:
            case GearLineType.PCritResist:
            case GearLineType.MCritResist:
                return rand.NextDouble() * 120 + 120;
            case GearLineType.Dodge:
                return rand.NextDouble() * 30 + 30;
            case GearLineType.MpRecovSecPercent:
                return (rand.NextDouble() * 18 + 18) / 100;
            default:
                return 0;
        }
    }
    public double ComputeTMLineValue(GearLineType FormerLineType, GearLineType NewLineType, double CurrentLineValue)
    {
        var possibleValues = TMLineValues[NewLineType];

        if (FormerLineType == NewLineType)
        {
            // Filtrer directement sans créer une liste intermédiaire inutile
            var validValues = possibleValues.Where(value => value > CurrentLineValue);

            // Prendre une nouvelle valeur ou garder l'ancienne si aucune n'est disponible
            return validValues.Any() ? validValues.ElementAt(rand.Next(validValues.Count())) : CurrentLineValue;
        }

        // Sélection aléatoire standard
        return possibleValues[rand.Next(possibleValues.Count)];
    }
}
class ArmorTest : Gear
{
    public ArmorTest() : base(1, 4)
    {
        this.Type = GearType.Armor;
        this.PDef = 32000;
        this.GearTier = 8;
    }
    public override void GearEffectApply() { }
    public override void GearEffectUnapply() { }
    public override Gear CloneGear()
    {
        return new ArmorTest();
    }
}
class RuneAtk : Rune //Rune test
{
    public RuneAtk() : base()
    {
        this.Name = "Attack Rune 25%";
        this.Rarity = 5;
        this.EquippableGear = GearType.Weapon;
    }

    public override void RuneEffectApply()
    {
        this.EquippingGear.EquippingHero.AtkLines += 0.25;
    }

    public override void RuneEffectUnapply()
    {
        this.EquippingGear.EquippingHero.AtkLines -= 0.25;
    }
    public override Rune CloneRune()
    {
        return new RuneAtk();
    }
}
class RuneCrit : Rune //Rune test
{
    public RuneCrit() : base()
    {
        this.Name = "Crit Rune 500";
        this.Rarity = 5;
        this.EquippableGear = GearType.Armor;
    }

    public override void RuneEffectApply()
    {
        EquippingGear.EquippingHero.Crit += 500;
    }

    public override void RuneEffectUnapply()
    {
        EquippingGear.EquippingHero.Crit -= 500;
    }
    public override Rune CloneRune()
    {
        return new RuneCrit();
    }
}
public class Fuck
{
    public void FuckThisShit()
    {

    }
}