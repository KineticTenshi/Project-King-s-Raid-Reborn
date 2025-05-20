using System;
using UnityEngine;

public abstract class Skill : IAttackSource //implémenter channeling 
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

    public event UseDelegate OnUse; // Événement avec expéditeur

    public Skill(string Name, string Description, double Multiplier, double FlatValue, int MpCost, double Cooldown, double Duration)
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

    public void Use(Creature caster)
    {
        /*if (this.isChanneling)
        {
            this.isChanneling = false; //un peu dangereux. En cas d'interruption par CC => isChanneling resterait true et le skill devient chiant à lancer car il faut taper deux fois, puis le skill continue de tourner
        }*/
        //if (!isReadySkill(caster)) return;
        //else
        {
            //DeductSkillCost(caster);

            PositiveEffectApply(caster);
            NegativeEffectApply(caster);
            InstantEffectApply(caster);
            OnUse?.Invoke(caster, EventArgs.Empty);
        }
    }
    public void Unuse()
    {
        foreach (Creature hero in Globals.listHeroes)
        {
            if (hero.AbilityApplied.Contains(this))
            {
                PositiveEffectRemove(hero);
                NegativeEffectRemove(hero);
                hero.AbilityApplied.Remove(this);
            }
            if (hero.StackableSkills.ContainsKey(this))
            {
                PositiveEffectRemove(hero);
                NegativeEffectRemove(hero);
                hero.StackableSkills.Remove(this);
            }
        }
        foreach (Creature enemy in Globals.listEnemies)
        {
            if (enemy.AbilityApplied.Contains(this))
            {
                PositiveEffectRemove(enemy);
                NegativeEffectRemove(enemy);
                enemy.AbilityApplied.Remove(this);
            }
            if (enemy.StackableSkills.ContainsKey(this))
            {
                PositiveEffectRemove(enemy);
                NegativeEffectRemove(enemy);
                enemy.StackableSkills.Remove(this);
            }
        }
    }
    public void Cleanse(Creature target)
    {
        target.AbilityApplied.RemoveAll(skill =>  //RemoveAll parcourt chaque élément à supprimer de la liste
        {
            if (!skill.isIrremovable) //check si le skill parcouru est isIrremovable, si oui
            {
                skill.NegativeEffectRemove(target); //nettoyage des effets du skill à supprimer
                return true; // Supprime l'élément de la liste
            }
            return false; // Garde l'élément
        });
    }
    public void Dispel(Creature target)
    {
        target.AbilityApplied.RemoveAll(skill =>  //RemoveAll parcourt chaque élément à supprimer de la liste
        {
            if (!skill.isIrremovable) //check si le skill parcouru est isIrremovable, si oui
            {
                skill.PositiveEffectRemove(target); //nettoyage des effets du skill à supprimer
                return true; // Supprime l'élément de la liste
            }
            return false; // Garde l'élément
        });
    }
    public void DispelShield(Creature target, Skill skill)
    {
        if (target.AbilityApplied.Contains(skill))
        {
            skill.PositiveEffectRemove(target);
            target.AbilityApplied.Remove(skill);
        }
    }
    public bool isReadySkill(Creature caster)
    {
        if (this.isLocked) return false;
        if (caster.Mp < this.MpCost || this.CurrentCooldown != 0) return false;
        if ((caster.isSilenced || caster.isStunned || caster.isFrozen) && !this.isCleanse) return false;
        return true;
    }
    public void DeductSkillCost(Creature caster)
    {
        caster.Mp -= this.MpCost;
        this.CurrentCooldown = this.Cooldown;
    }
    public void ActivateDuration(Creature caster)
    {
        this.CurrentDuration = this.Duration;
    }
    //public abstract void LightEffectApply();
    //public abstract void DarkEffectApply();
    protected abstract void InstantEffectApply(Creature caster);
    protected abstract void PositiveEffectApply(Creature caster);
    protected abstract void PositiveEffectRemove(Creature caster);
    protected abstract void NegativeEffectApply(Creature caster);
    protected abstract void NegativeEffectRemove(Creature caster);
}

public abstract class AutoAttack : IAttackSource
{
    public string Name { get; set; }
    public int AutoAtkNumberPerCycle { get; set; }
    public double Multiplier { get; set; }
    public double FlatValue { get; set; }
    public double Delay { get; set; }
    public Creature owner { get; set; }
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