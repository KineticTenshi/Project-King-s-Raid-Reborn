using System.Collections.Generic;
using System;
using UnityEngine;

public class Clause : Creature, IHeroClone //dummy
{
    public Clause() : base()
    {
        this.Name = "Clause";
        this.Class = "Hero";
        this.AtkType = AtkType.Physical;
        this.AtkSubType = "Melee";
        this.Position = "Front";
        this.BaseKnightStatsApply();
        this.Hp = this.MaxHp;

        this.SkillList.AddRange(new List<Skill> { new ClauseS1(), new ClauseS2(), new ClauseS3(), new ClauseS4(this), new ExianSkill() });
        this.AutoAttack = new ClauseAA(this);
    }
    public Creature CloneHero()
    {
        return new Clause();
    }
}
public class ClauseAA : AutoAttack
{
    public ClauseAA(Creature owner) : base()
    {
        this.owner = owner;
        this.AutoAtkNumberPerCycle = 1;
        this.Multiplier = 1;
    }
    public override void AutoAtkEffect() { }
}
public class ClauseS1 : Skill
{
    public ClauseS1() : base("Cut Ground", "Stun and Amp", 1.98, 61266, 2000, 9, 10)
    {
        if (this.isLight)
        {
            this.Cooldown -= 2;
        }
    }
    protected override void InstantEffectApply(Creature caster)
    {
        HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
    }
    protected override void PositiveEffectApply(Creature caster) { }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature caster) { }
    protected override void NegativeEffectRemove(Creature caster) { }
}
public class ClauseS2 : Skill
{
    public ClauseS2() : base("Shield Strike", "Def Shred", 2.8100, 112448, 1000, 8, 20) { }
    protected override void InstantEffectApply(Creature caster)
    {
        HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
        HpManager.OnBlock += OnBlockCooldownReduction;
    }
    protected override void PositiveEffectApply(Creature caster) { }
    protected override void NegativeEffectApply(Creature caster)
    {
        foreach (Creature enemy in Globals.listEnemies)
        {
            if (enemy.AbilityApplied.Contains(this) || enemy.isDebuffImmune != 0)
            {
                continue;
            }
            else
            {
                if (this.isLight)
                {
                    enemy.PWeakness += 0.25;
                    enemy.MWeakness += 0.25;
                }
                enemy.PDefSkills -= 0.3;
                enemy.MDefSkills -= 0.3;
                enemy.AtkSpd -= 500;
                enemy.AbilityApplied.Add(this);
            }
        }
    }
    protected override void PositiveEffectRemove(Creature caster) { }
    protected override void NegativeEffectRemove(Creature target)
    {
        if (this.isLight)
        {
            target.PWeakness -= 0.25;
            target.MWeakness -= 0.25;
        }
        target.PDefSkills += 0.3;
        target.MDefSkills += 0.3;
        target.AtkSpd += 500;
    }
    private void OnBlockCooldownReduction(Creature creature, EventArgs e)
    {
        creature.SkillList[1].CurrentCooldown -= 0.5;
        Console.WriteLine($"Block event sur {creature}");
    }
}
public class ClauseS3 : Skill
{
    public ClauseS3() : base("Guardian Shield", "Block buff", 0, 18044, 2000, 12, 20)
    {
        if (this.isLight)
        {
            this.Multiplier *= 1.4;
        }
    }
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster)
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.AbilityApplied.Contains(this) || hero.isBuffImmune != 0)
            {
                continue;
            }
            else
            {
                hero.PDefFlat += this.Multiplier;
                hero.PCritResist += 300;
                hero.PTough += 250;
                if (this.isDark)
                {
                    hero.PTough += 250;
                    hero.PBlock += 250;
                }
            }
        }
    }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature target)
    {
        target.PDefFlat -= this.Multiplier;
        target.PCritResist -= 300;
        target.PTough -= 250;
        if (this.isDark)
        {
            target.PTough -= 250;
            target.PBlock -= 250;
        }
    }
    protected override void NegativeEffectRemove(Creature caster) { }
}
public class ClauseS4 : Skill
{
    public ClauseS4(Creature owner) : base("Vow of Knight", "Def stat stick", 0.3670, 11391, 0, 0, 8)
    {
        this.owner = owner;
        HpManager.OnBlock += OnBlockDamage;
        HpManager.OnDamageReceived += OnAllyReceiveDamage;
    }
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster) { }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature target)
    {
        target.PDefSkills -= 0.2;
        target.PCritResist -= 200;
    }
    protected override void NegativeEffectRemove(Creature target)
    {
        target.AtkSkills += 0.2;
        if (this.isLight)
        {
            target.PWeakness -= 0.2;
            target.MWeakness -= 0.2;
        }
        if (this.isDark)
        {
            target.AtkSkills += 0.1;
        }
    }
    private void OnBlockDamage(Creature creature, EventArgs e)
    {
        HpManager.ComputeDamage(creature, creature.EnemyTarget, this);
        creature.Mp += 300;
        if (creature.EnemyTarget.AbilityApplied.Contains(this) || creature.EnemyTarget.isDebuffImmune != 0)
        {
            return;
        }
        else
        {
            creature.EnemyTarget.AtkSkills -= 0.2;
            if (this.isLight)
            {
                creature.EnemyTarget.PWeakness += 0.2;
                creature.EnemyTarget.MWeakness += 0.2;
            }
            if (this.isDark)
            {
                creature.EnemyTarget.AtkSkills -= 0.1;
            }
            creature.AbilityApplied.Add(this);
        }
    }
    private void OnAllyReceiveDamage(Creature creature, EventArgs e)
    {
        if (creature is Clause)
        {
            return;
        }
        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.AbilityApplied.Contains(this) || hero.isBuffImmune != 0)
            {
                continue;
            }
            hero.PDefSkills += 0.2;
            hero.PCritResist += 200;
            hero.AbilityApplied.Add(this);
        }
    }
    ~ClauseS4()
    {
        HpManager.OnBlock -= OnBlockDamage;
        HpManager.OnDamageReceived -= OnAllyReceiveDamage;
    }
}
public class Exian : Gear
{
    private int exianstacks;
    public int ExianStacks
    {
        get => exianstacks;
        set => exianstacks = Math.Clamp(value, 0, 3);
    }
    public Exian() : base(3, 0)
    {
        this.Name = "Exian";
        this.Type = GearType.Weapon;
        this.Atk = 112763;
        this.RuneListLength = 3;
        this.LimitHero = "Clause";
    }
    public override void GearEffectApply()
    {
        foreach (Skill skill in this.EquippingHero.SkillList)
        {
            skill.OnUse += StacksUp;
        }
    }
    public override void GearEffectUnapply()
    {
        foreach (Skill skill in this.EquippingHero.SkillList)
        {
            skill.OnUse -= StacksUp;
        }
    }
    private void StacksUp(Creature caster, EventArgs e)
    {
        this.ExianStacks++;
        if (this.ExianStacks == 3)
        {
            this.ExianStacks = 0;
            caster.SkillList[4].Use(this.EquippingHero);
            caster.AbilityApplied.Add(caster.SkillList[4]);
            Console.WriteLine("Exian skill Up");
        }
    }
    ~Exian()
    {
        foreach (Skill skill in this.EquippingHero.SkillList)
        {
            skill.OnUse -= StacksUp;
        }
    }
    public override Gear CloneGear()
    {
        return new Exian();
    }
}
public class ExianSkill : Skill
{
    private Creature HeroMaxAtk { get; set; }
    public ExianSkill() : base("Exian Skill", "Atk and PDef Up", 0, 0, 0, 0, 15) { }
    protected override void InstantEffectApply(Creature caster) { }
    protected override void PositiveEffectApply(Creature caster)
    {
        this.HeroMaxAtk = ListParser.ParserAtk(Globals.listHeroes, true, 1)[0];
        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.AbilityApplied.Contains(this) || hero.isBuffImmune != 0)
            {
                continue;
            }
            hero.PDefSkills += 0.25;
            hero.AtkSkills += 0.25;
            if (hero == this.HeroMaxAtk)
            {
                hero.PDefSkills += 0.25;
                hero.AtkSkills += 0.25;
            }
            hero.AbilityApplied.Add(this);
        }
    }
    protected override void NegativeEffectApply(Creature caster) { }
    protected override void PositiveEffectRemove(Creature target)
    {
        target.PDefSkills -= 0.25;
        target.AtkSkills -= 0.25;
        if (target == this.HeroMaxAtk)
        {
            target.PDefSkills -= 0.25;
            target.AtkSkills -= 0.25;
        }
    }
    protected override void NegativeEffectRemove(Creature caster) { }
}
