using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.GraphicsBuffer;

class ValanceCoroutine : MonoBehaviour
{
    private static ValanceCoroutine _instance;

    public static ValanceCoroutine Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ValanceCoroutineRunner");
                _instance = go.AddComponent<ValanceCoroutine>();
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
class Valance : Creature, IHeroClone
{
    public Valance() : base()
    {
        this.Name = "Valance";
        this.Class = "Hero";
        this.AtkType = AtkType.Physical;
        this.AtkSubType = "Ranged";
        this.Position = "Middle";
        this.BaseMechanicStatsApply();
        this.Hp = this.MaxHp;

        this.SkillList.AddRange(new List<Skill> { new ValanceS1(this), new ValanceS2(this), new ValanceS3(this), new ValanceS4(this) });
        this.AutoAttack = new ValanceAA(this);

        if (this.isLight)
        {
            this.AtkSkills += 0.15;
            this.PDefSkills += 0.15;
            this.MDefSkills += 0.15;
            this.MaxHpSkills += 0.15;
            this.CCResist += 100;
        }
    }
    public Creature CloneHero()
    {
        return new Valance();
    }
}
class ValanceAA : AutoAttack
{
    public ValanceAA(Creature owner) : base() { }
    public override void AutoAtkEffect() { }
}
class ValanceS1 : Skill
{
    private List<Creature> listAllyHighestAtk;
    private double atkBoost = 149610;
    private double cdReduction = 0.2;
    private int intTargetApply = 1;
    private double nonHeroBoost = 1;
    public ValanceS1(Creature owner) : base("Prototype - VA", "Hp battery", 1.162, 46304, 3000, 15, 15)
    {

    }
    /*public override void LightEffectApply()
    {
        this.intTargetApply = 2;
    }
    public override void DarkEffectApply() { }*/
    protected override void InstantEffectApply(Creature caster) { }
    protected override async void PositiveEffectApply(Creature caster)
    {
        /*if (this.owner.TreasureList[0] is FormulaInputDevice)
        {
            this.nonHeroBoost = 1.25;
            this.cdReduction = 0.3;
        }*/
        this.listAllyHighestAtk = ListParser.ParserAtk(Globals.listHeroes, true, this.intTargetApply);
        foreach (Creature target in this.listAllyHighestAtk)
        {
            if (target.AbilityApplied.Contains(this) || target.isBuffImmune != 0) continue;
            else
            {
                Cleanse(target);
                target.isDebuffImmune += 1;
                target.AtkFlat += atkBoost;
                target.Crit += 350;
                target.NonHeroAmp *= this.nonHeroBoost;
                if (this.isDark) target.CritDmg += 50;
            }
        }
        int nTicks = 0;
        while (nTicks < 15)
        {
            foreach (Creature target in this.listAllyHighestAtk)
            {
                HpManager.SkillHealNonCrit(caster, target, this);
                if (!target.AbilityApplied.Contains(this) || target.isBuffImmune != 0) continue;
                else foreach (Skill skill in target.SkillList) skill.CurrentCooldown -= this.cdReduction;
            }
            nTicks += 1;
            await Task.Delay(1000);
        }
    }
    protected override void PositiveEffectRemove(Creature target)
    {
        target.isDebuffImmune -= 1;
        target.AtkFlat -= atkBoost;
        target.Crit -= 350;
        if (this.isDark) target.CritDmg -= 50;
    }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void NegativeEffectRemove(Creature caster) { }
}
class ValanceS2 : Skill
{
    public float healRadius = 10f;
    public int healTicks = 5;
    public float tickInterval = 1f;

    public bool isSaved;
    public Transform savedPosition;

    public ValanceS2(Creature owner) : base("Absolute Protection Field - LA", "Mana battery", 1, 45118, 5000, 20, 5)
    {
        this.isIrremovable = true;
        this.isDark = true;
    }
    /*public override void LightEffectApply() { }
    public override void DarkEffectApply() { }*/
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster)
    {
        ValanceCoroutine.Instance.StartCoroutine(ValanceS2HealCoroutine(caster));
        ValanceCoroutine.Instance.StartCoroutine(ValanceS2BuffCoroutine(caster));
    }
    private IEnumerator ValanceS2HealCoroutine(Creature caster)
    {
        int nTicks = 0;
        while (nTicks < healTicks)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                if (!isSaved) { savedPosition = caster.gameObject.transform; isSaved = true; }
                if (Vector3.Distance(hero.gameObject.transform.position, savedPosition.position) <= healRadius)
                {
                    HpManager.SkillHealNonCrit(caster, hero, this);
                    hero.Mp += 300;
                }
            }
            nTicks++;
            if (nTicks < 5) yield return new WaitForSeconds(tickInterval);
        }
    }
    private IEnumerator ValanceS2BuffCoroutine(Creature caster)
    {
        while (this.CurrentDuration > 0)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                if (Vector3.Distance(hero.gameObject.transform.position, savedPosition.position) <= healRadius && !hero.AbilityApplied.Contains(this))
                {
                    hero.isImmuneCC += 1;
                    hero.PDefSkills += 0.15;
                    hero.MDefSkills += 0.15;
                    if (this.isLight) hero.AtkSpd += 300;
                    if (this.isDark)
                    {
                        hero.PTough += 250;
                        hero.MTough += 250;
                    }
                    hero.AbilityApplied.Add(this);
                }
                if (Vector3.Distance(hero.gameObject.transform.position, savedPosition.position) > healRadius && hero.AbilityApplied.Contains(this))
                {
                    PositiveEffectRemove(hero);
                    hero.AbilityApplied.Remove(this);
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
    protected override void PositiveEffectRemove(Creature target)
    {
        target.isImmuneCC -= 1;
        target.PDefSkills -= 0.15;
        target.MDefSkills -= 0.15;
        if (this.isLight) target.AtkSpd -= 300;
        if (this.isDark)
        {
            target.PTough -= 250;
            target.MTough -= 250;
        }
    }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void NegativeEffectRemove(Creature caster) { }
}
class ValanceS3 : Skill
{
    public ValanceS3(Creature owner) : base("Ultimate Laser Cannon - NCE", "Damage battery", 2.73, 138920, 4000, 20, 15) { }
    /*public override void LightEffectApply()
    {
        this.AtkBoost *= 1.4;
    }
    public override void DarkEffectApply() 
    {
        this.MpCost = 3000;
    }*/
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature target) { }
    protected override void NegativeEffectApply(Creature caster)
    {
        this.Multiplier = 2.73;
        this.FlatValue = 138920;
        int target = Globals.rand.Next(0, Globals.listEnemies.Count);
        HpManager.ComputeDamage(caster, Globals.listEnemies[target], this);

        this.Multiplier = 1.365;
        this.FlatValue = 14244;
        for (int i = 0; i < 9; i++)
        {
            target = Globals.rand.Next(0, Globals.listEnemies.Count);
            Creature targetCreature = Globals.listEnemies[target];
            double dmg = HpManager.ComputeDamage(caster, targetCreature, this);
            if (this.isLight && dmg != 0) Dispel(targetCreature);
            if (targetCreature.isDebuffImmune > 0) continue;

            if (targetCreature.StackableSkills.TryGetValue(this, out int currentStacks) && currentStacks < 7)
            {
                targetCreature.HealRate -= 0.05;
                targetCreature.PWeakness += 0.07;
                targetCreature.MWeakness += 0.07;
                targetCreature.StackableSkills[this] = Math.Min(7, currentStacks + 1);
            }
            else if (!targetCreature.StackableSkills.ContainsKey(this))
            {
                targetCreature.HealRate -= 0.05;
                targetCreature.PWeakness += 0.07;
                targetCreature.MWeakness += 0.07;
                targetCreature.StackableSkills.Add(this, 1);
            }
        }
    }
    protected override void NegativeEffectRemove(Creature target)
    {
        target.HealRate += target.StackableSkills[this] * 0.05;
        target.PWeakness -= target.StackableSkills[this] * 0.07;
        target.MWeakness -= target.StackableSkills[this] * 0.07;
        Debug.Log($"Stacks : {target.StackableSkills[this]}");
        target.StackableSkills.Remove(this);
    }
}
class ValanceS4 : Skill
{
    public ValanceS4(Creature owner) : base("Trance", "Whatever", 3.3, 178470, 0, 0, 12)
    {

    }
   
    /*public override void LightEffectApply()
    {
        this.Multiplier = 6.6;
        this.FlatValue = 297451;
    }
    public override void DarkEffectApply() { }*/
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature caster) { }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void NegativeEffectRemove(Creature target) { }
}
class FormulaInputDevice : Gear
{
    public FormulaInputDevice() : base(0, 2)
    {
        this.Name = "Formula Input Device";
        this.Type = GearType.Treasure;
        this.Hp = 3990076;
        this.RuneListLength = 0;
        this.LimitHero = "Shea";
    }
    public override void GearEffectApply() { }
    public override void GearEffectUnapply() { }
    public override Gear CloneGear()
    {
        return new FormulaInputDevice();
    }
}

