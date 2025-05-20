using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

class SheaGlobals
{
    public delegate void SheaEndDelegate(Creature caster, EventArgs e);
    public static event SheaEndDelegate OnSheaEnd;
    public static void TriggerOnSheaEnd(Creature sender)
    {
        OnSheaEnd?.Invoke(sender, EventArgs.Empty); //on ne peut pas utiliser OnSheaEnd directement dans les skill... mais on peut l'appeler via une méthode globale !
    }
}

class SheaCoroutine : MonoBehaviour
{
    private static SheaCoroutine _instance;

    public static SheaCoroutine Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SheaCoroutineRunner");
                _instance = go.AddComponent<SheaCoroutine>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public void RunSkillCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }

    public void StopSkillCoroutine(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }
}
public class Shea : Creature, IHeroClone
{
    public Shea() : base()
    {
        this.Name = "Shea";
        this.Class = "Hero";
        this.AtkType = AtkType.Magic;
        this.AtkSubType = "Ranged";
        this.Position = "Back";
        this.BasePriestStatsApply();
        this.Hp = this.MaxHp;

        this.SkillList.AddRange(new List<Skill> { new SheaS1(this), new SheaS2(this), new SheaS3(this), new SheaS4(this) });
        this.AutoAttack = new SheaAA(this);

        if (this.isLight)
        {
            this.AtkSkills += 0.15;
            this.PDefSkills += 0.15;
            this.MDefSkills += 0.15;
            this.MaxHpSkills += 0.15;
            this.CCResist += 100;
        }
        if (this.isDark)
        {
            //dafuk is this T5D ????
        }
    }
    public Creature CloneHero()
    {
        return new Shea();
    }
}
public class SheaAA : AutoAttack
{
    public SheaAA(Creature owner) : base()
    {
        this.owner = owner;
        this.AutoAtkNumberPerCycle = 1;
        this.Multiplier = 1;
    }
    public override void AutoAtkEffect()
    {
        //UnityEngine.Debug.Log($"Shea AA ! Target : {owner.EnemyTarget}");
    }
}
public class SheaS1 : Skill
{
    private bool tranceStorage { get; set; }
    SheaS4 sheaS4Instance {  get; set; }
    public SheaS1(Creature owner) : base("Green Nature's Melody", "Hp battery", 1.2, 54143, 2000, 28, 14)
    {
        this.owner = owner;
    }
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster) 
    {
        SheaCoroutine.Instance.RunSkillCoroutine(PositiveEffectApplyCoroutine(caster));
    }

    private IEnumerator PositiveEffectApplyCoroutine(Creature caster)
    {
        this.isChanneling = true;
        if (this.isLight)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                hero.Hp += hero.MaxHp * 0.15;
            }
        }
        
        this.tranceStorage = caster.AbilityApplied.Contains(this.owner.SkillList[3]);
        
        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.isBuffImmune != 0) continue;

            if (hero.AbilityApplied.Contains(this))
            {
                this.PositiveEffectRemove(hero);
                hero.AbilityApplied.Remove(this);
            }

            if (this.isDark) hero.NonHeroAmp *= 1.2f;
            hero.PTough += this.tranceStorage ? 700 : 350;
            hero.MTough += this.tranceStorage ? 700 : 350;
            
            hero.AbilityApplied.Add(this);
        }

        int nTicks = 0;

        this.Multiplier = this.tranceStorage ? 2.4 : 1.2;
        this.FlatValue = this.tranceStorage ? 108286 : 54143;

        while (this.isChanneling && nTicks < 8)
        {
            foreach (Creature hero in Globals.listHeroes) HpManager.SkillHeal(caster, hero, this);
            nTicks += 1;
            if (nTicks < 8) yield return new WaitForSeconds(1000f / (float)caster.AtkSpd);
        }
        this.isChanneling = false;
        if (this.owner.SkillList[3] is SheaS4 sheaS4) sheaS4.TriggerStacksUp(caster);
    }
    protected override void PositiveEffectRemove(Creature target)
    {
        if (this.isDark) target.NonHeroAmp /= 1.2;
        target.PTough -= this.tranceStorage ? 700 : 350;
        target.MTough -= this.tranceStorage ? 700 : 350;
    }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void NegativeEffectRemove(Creature caster) { }
}
public class SheaS2 : Skill
{
    private bool tranceStorage { get; set; }
    public SheaS2(Creature owner) : base("Blue Wind's Song", "Mana battery", 1, 45118, 3000, 28, 14)
    {
        this.owner = owner;
    }
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster)
    {
        SheaCoroutine.Instance.StartCoroutine(PositiveEffectApplyCoroutine(caster));
    }
    private IEnumerator PositiveEffectApplyCoroutine(Creature caster)
    {
        this.isChanneling = true;
        if (this.isLight)
        {
            foreach (Creature hero in Globals.listHeroes) hero.Mp += 500;
        }

        this.tranceStorage = caster.AbilityApplied.Contains(this.owner.SkillList[3]);

        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.isBuffImmune != 0) continue;
            if (hero.AbilityApplied.Contains(this))
            {
                this.PositiveEffectRemove(hero);
                hero.AbilityApplied.Remove(this);
            }
            if (this.isDark) hero.AtkSpd += 250;
            if (this.tranceStorage) hero.isImmuneCC += 1;

            hero.AbilityApplied.Add(this);
        }
        int nTicks = 0;

        this.Multiplier = this.tranceStorage ? 1.5 : 1;
        this.FlatValue = this.tranceStorage ? 67677 : 45118;
        double mpValue = this.tranceStorage ? 160 : 120;

        while (this.isChanneling && nTicks < 8)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                hero.Mp += mpValue;
                if (hero.Mp == hero.MaxMp) HpManager.SkillHeal(caster, hero, this);
            }
            nTicks += 1;
            if (nTicks < 8) yield return new WaitForSeconds(1000f / (float)caster.AtkSpd);
        }
        this.isChanneling = false;
        if (this.owner.SkillList[3] is SheaS4 sheaS4) sheaS4.TriggerStacksUp(caster);
    }
    protected override void PositiveEffectRemove(Creature target)
    {
        if (this.isDark) target.AtkSpd -= 250;
        if (this.tranceStorage) target.isImmuneCC -= 1;
    }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void NegativeEffectRemove(Creature caster) { }
}
public class SheaS3 : Skill
{
    private double AtkBoost { get; set; }
    private bool tranceStorage { get; set; }
    public SheaS3(Creature owner) : base("Dance of Red Passion", "Damage battery", 0, 0, 3000, 28, 14)
    {
        this.owner = owner;
        this.AtkBoost = 106866;
        if (this.isLight) this.AtkBoost *= 1.4;
    }
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster)
    {
        SheaCoroutine.Instance.StartCoroutine(PositiveEffectApplyCoroutine(caster));
    }
    private IEnumerator PositiveEffectApplyCoroutine(Creature caster)
    {
        this.isChanneling = true;
        this.tranceStorage = caster.AbilityApplied.Contains(this.owner.SkillList[3]);
        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.isBuffImmune != 0) continue;
            if (hero.AbilityApplied.Contains(this))
            {
                this.PositiveEffectRemove(hero);
                hero.AbilityApplied.Remove(this);
            }
            hero.AtkFlat += this.tranceStorage ? this.AtkBoost * 2 : this.AtkBoost;
            hero.AbilityApplied.Add(this);
        }
        int nTicks = 0;

        this.Multiplier = this.tranceStorage ? 1.092 : 0.546;
        this.FlatValue = this.tranceStorage ? 48910 : 24455;
        while (this.isChanneling && nTicks < 8)
        {
            foreach (Creature enemy in Globals.listEnemies)
            {
                HpManager.ComputeDamage(caster, enemy, this);
            }
            nTicks += 1;
            if (nTicks < 8) yield return new WaitForSeconds(1000f / (float)caster.AtkSpd);
        }
        this.isChanneling = false;
        if (this.owner.SkillList[3] is SheaS4 sheaS4) sheaS4.TriggerStacksUp(caster);
    }
    protected override void PositiveEffectRemove(Creature target)
    {
        target.AtkFlat -= this.tranceStorage ? this.AtkBoost * 2 : this.AtkBoost;
    }
    protected override void NegativeEffectApply(Creature caster)
    {
        foreach (Creature enemy in Globals.listEnemies)
        {
            if (enemy.isDebuffImmune != 0) continue;
            if (this.isDark)
            {
                enemy.PWeakness += 0.15;
                enemy.MWeakness += 0.15;
                enemy.AbilityApplied.Add(this);
            }
        }
    }
    protected override void NegativeEffectRemove(Creature target)
    {
        if (this.isDark)
        {
            target.PWeakness -= 0.15;
            target.MWeakness -= 0.15;
        }
    }
}
public class SheaS4 : Skill
{
    private int blissfulperformancestacks;
    public int blissfulPerformanceStacks
    {
        get => blissfulperformancestacks;
        set => blissfulperformancestacks = Math.Clamp(value, 0, 3);
    }
    public event EventHandler StacksUp;
    public SheaS4(Creature owner) : base("Trance", "Whatever", 0, 0, 0, 0, 12)
    {
        this.isIrremovable = true;
        this.blissfulperformancestacks = 0;
        if (this.isLight)
        {
            this.Multiplier = 6.6;
            this.FlatValue = 297451;
            return;
        }
        else
        {
            this.Multiplier = 3.3;
            this.FlatValue = 178470;
        }
    }
    public void TriggerStacksUp(Creature caster)
    {
        this.blissfulPerformanceStacks++;
        StacksUp?.Invoke(this, EventArgs.Empty);
        if (this.blissfulPerformanceStacks == 3)
        {
            this.blissfulPerformanceStacks = 0;
            this.Use(caster);
        }
    }
    protected override void InstantEffectApply(Creature caster)
    {
        if (!caster.AbilityApplied.Contains(this)) caster.AbilityApplied.Add(this);
        if (!this.isDark)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                HpManager.SkillHeal(caster, hero, this);
            }
        }
    }
    protected override void PositiveEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature caster) { }
    protected override void NegativeEffectApply(Creature caster)
    {
        if (this.isDark)
        {
            foreach (Creature enemy in Globals.listEnemies)
            {
                enemy.PDefSkills -= 0.2;
                enemy.MDefSkills -= 0.2;
                HpManager.ComputeDamage(caster, enemy, this);
                enemy.AbilityApplied.Add(this);
            }
        }
    }
    protected override void NegativeEffectRemove(Creature target)
    {
        target.PDefSkills += 0.2;
        target.MDefSkills += 0.2;
    }
}
public class Schwanheite : Gear
{
    public Schwanheite() : base(3, 0)
    {
        this.Name = "Schwanheite";
        this.Type = GearType.Weapon;
        this.Atk = 147459;
        this.RuneListLength = 3;
        this.LimitHero = "Shea";
    }
    public override void GearEffectApply()
    {
        if (this.EquippingHero.SkillList[3] is SheaS4 sheaS4)
        {
            BattleEventManager._Instance.Register(sheaS4,
                                                  sheaS4 => sheaS4.StacksUp += this.SchwanheiteEffect,
                                                  sheaS4 => sheaS4.StacksUp -= this.SchwanheiteEffect);
        }
        this.EquippingHero.CCResist += 750;
    }
    public override void GearEffectUnapply()
    {
        if (this.EquippingHero.SkillList[3] is SheaS4 sheaS4) sheaS4.StacksUp -= this.SchwanheiteEffect;
        this.EquippingHero.CCResist -= 750;
    }
    private void SchwanheiteEffect(object sender, EventArgs e)
    {
        foreach (Skill skill in this.EquippingHero.SkillList)
        {
            skill.CurrentCooldown -= 7;
        }
        this.EquippingHero.Mp += 1000;
    }
    ~Schwanheite()
    {
        SheaGlobals.OnSheaEnd -= SchwanheiteEffect;
    }
    public override Gear CloneGear()
    {
        return new Schwanheite();
    }
}
public class WindyFlower : Gear
{
    public WindyFlower() : base(0, 2)
    {
        this.Name = "Windy Flower";
        this.Type = GearType.Treasure;
        this.Hp = 3990076;
        this.RuneListLength = 0;
        this.LimitHero = "Shea";
    }
    public override void GearEffectApply()
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            foreach (Skill skill in hero.SkillList)
            {
                skill.OnUse += WindyFlowerMpRegen;
            }
        }
    }
    public override void GearEffectUnapply()
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            foreach (Skill skill in hero.SkillList)
            {
                skill.OnUse -= WindyFlowerMpRegen;
            }
        }
    }
    private void WindyFlowerMpRegen(Creature caster, EventArgs e)
    {
        if (!caster.AbilityApplied.Contains(this.EquippingHero.SkillList[1]))
        {
            return;
        }
        caster.Mp += 400;
        //WriteLine($"This hero receives Shea's UT 2 effect : {caster.Name}");
    }
    ~WindyFlower()
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            foreach (Skill skill in hero.SkillList)
            {
                skill.OnUse -= WindyFlowerMpRegen;
            }
        }
    }
    public override Gear CloneGear()
    {
        return new WindyFlower();
    }
}
