using System;
using System.Linq;
using BaseLogic;

namespace HeroSpace
{
    class Kasel : Creature, IHeroClone //Hero Test
    {
        public Kasel() : base()
        {
            this.Name = "Kasel";
            this.Class = "Hero";
            this.AtkType = AtkType.Physical;
            this.AtkSubType = "Melee";
            this.Position = "Front";
            this.BaseWarriorStatsApply();
            this.Hp = this.MaxHp;

            this.SkillList.AddRange(new List<Skill> { new KaselS1(this), new KaselS2(this), new KaselS3(this), new KaselS4(this) });
            this.AutoAttack = new KaselAA(this);


            if (this.isLight)
            {
                this.AtkSkills += 0.15;
                this.PDefSkills += 0.15;
                this.MDefSkills += 0.15;
                this.MaxHpSkills += 0.15;
                this.PDodge += 100;
            } 
        }
        public Creature CloneHero()
        {
            return new Kasel();
        }
    }
    class KaselAA : AutoAttack
    {
        Random rand = new Random();
        public KaselAA(Creature owner) : base(5, 0.7, 0, owner) { }
        public override void AutoAtkEffect()
        { 
            if (this.owner.Gears[GearType.Weapon] is Aea)
            {
                if (rand.Next(0, 100) >= 65)
                {
                    HpManager.ComputeDamage(this.owner, this.owner.EnemyTarget, this);
                }
            }
        }
    }
    class KaselS1 : Skill
    {
        public KaselS1(Creature owner) : base("Judgement Blade", "Attack twice and deal PDmg", 3.366, 112.55, 2000, 10, 0, owner) { }
        public override void LightEffectApply()
        {
            this.Multiplier = 3.366 * 1.4;
            this.FlatValue = 112.55 * 1.4;
        }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            /*if (caster.AbilityApplied.Contains(this.owner.SkillList[2]))
            {
                //wider attack range à ajouter plus tard
            }*/

            if(this.isDark)
            {
                caster.isIgnoreDef += 1;
                HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
                HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
                caster.isIgnoreDef -= 1;
            }
            else
            {
                HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
                HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
            }
        }
        protected override void PositiveEffectRemove(Creature caster) { }
        protected override void NegativeEffectRemove(Creature caster) { }
    }
    class KaselS2 : Skill
    {
        public KaselS2(Creature owner) : base("Valiant Dash", "Attacks three times and increases target P.Weakness", 4.407, 147.315, 2000, 0, 10, owner) { }
        public override void LightEffectApply()
        {
            this.MpCost -= 1000;
        }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            if (caster.EnemyTarget.AppliedDebuffs.ContainsKey(this) || caster.EnemyTarget.isDebuffImmune != 0) return;

            if (this.isDark) caster.EnemyTarget.PDefSkills -= 0.30;
            caster.EnemyTarget.PWeakness += 0.25;
            caster.EnemyTarget.AppliedDebuffs.Add(this, 0);

            if (caster.AppliedBuffs.ContainsKey(this.owner.SkillList[2]))
            {
                caster.Crit += 500;
                for (int i = 0; i < 3; i++) HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
                caster.Crit -= 500;
            }
            else
            {
                for (int i = 0; i < 3; i++) HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
            }
        }
        protected override void PositiveEffectRemove(Creature caster) { }
        protected override void NegativeEffectRemove(Creature target)
        {
            if (this.isDark) target.PDefSkills += 0.30;
            target.PWeakness -= 0.25;
        }
    }
    class KaselS3 : Skill
    {
        private bool isDarkApplied;
        private double AtkFlatS3 { get; set; }
        public KaselS3(Creature owner) : base("Proxy of God", "Big Stat Stick for Kasel lmao", 1.26, 42.152, 3000, 0, 20, owner)
        {
            this.isIrremovable = true;
            this.AtkFlatS3 = 32000;
        }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() 
        {
            this.MpCost += 1000;
        }
        protected override void ApplyEffect(Creature caster)
        {
            if (caster.AppliedBuffs.ContainsKey(this) || caster.isBuffImmune != 0) return;
            if (this.isDark) caster.NonHeroAmp *= 2;
            if (this.isLight)
            {
                caster.PTough += 200;
                caster.MTough += 200;
            }

            caster.AtkFlat += this.AtkFlatS3;
            caster.AtkSpd += 250;
            caster.PDodge += 250;
            caster.MDodge += 250;
            caster.AutoAttack.Multiplier *= 2.25;
            caster.isImmuneCC += 1;
            caster.AppliedBuffs.Add(this, 0);
            HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
        }
        protected override void PositiveEffectRemove(Creature target)
        {
            if (this.isDark) target.NonHeroAmp /= 2;
            if (this.isLight)
            {
                target.PTough -= 200;
                target.MTough -= 200;
            }

            target.AtkFlat -= this.AtkFlatS3;
            target.AtkSpd -= 250;
            target.PDodge -= 250;
            target.MDodge -= 250;

            target.AutoAttack.Multiplier /= 2.25;
            target.isImmuneCC -= 1;
        }
        protected override void NegativeEffectRemove(Creature caster) { }
    }
    class KaselS4 : Skill
    {
        public double DefFlatS4 = 32760;
        public KaselS4(Creature owner) : base("Goddess Strike", "Another massive Stat Stick for Kasel", 0, 0, 0, 0, 0, owner) //owner du skill en argument du constructeur : permet de référencer l'instance qui possède le skill et non la classe
        {
            this.isIrremovable = true;
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnBattleStart += OnStartActivatePassive,
                                                  instance => instance.OnBattleStart -= OnStartActivatePassive);
        }
        public override void LightEffectApply()
        {
            this.owner.PDodge += 150;
            this.owner.MDodge += 150;
        }
        public override void DarkEffectApply()
        {
            this.owner.NonHeroResist *= (1 - 0.1);
        }
        private void OnStartActivatePassive(object sender, EventArgs e)
        {
            this.Use(this.owner);
        }
        protected override void ApplyEffect(Creature caster) 
        {
            if (caster.AppliedBuffs.ContainsKey(this) || caster.isBuffImmune != 0) return;
            if (this.owner.TreasureList[0] is LuasTear) this.DefFlatS4 *= 1.5;
            this.owner.PDefFlat += this.DefFlatS4;
            this.owner.MDefFlat += this.DefFlatS4;
            this.owner.PDodge += 200;
            this.owner.MDodge += 200;
            this.owner.AppliedBuffs.Add(this, 0);
        }
        protected override void PositiveEffectRemove(Creature caster) { }
        protected override void NegativeEffectRemove(Creature caster) { }
    }
    class Aea : Gear
    {
        public Aea() : base(3, 0)
        {
            this.Name = "Aea";
            this.Type = GearType.Weapon;
            this.Atk = 127798;
            this.RuneListLength = 3;
            this.LimitHero = "Kasel";
        }
        public override void GearEffectApply() { }
        public override void GearEffectUnapply() { }
        public override Gear CloneGear()
        {
            return new Aea();
        }
    }
    class LuasTear : Gear
    {
        public LuasTear() : base(0, 2)
        {
            this.Name = "Lua's Tear";
            this.Type = GearType.Treasure;
            this.Hp = 3990076;
            this.RuneListLength = 0;
            this.LimitHero = "Kasel";
        }
        public override void GearEffectApply()
        {
            this.EquippingHero.BossAmp *= 1.3;
        }
        public override void GearEffectUnapply()
        {
            this.EquippingHero.BossAmp /= 1.3;
        }
        public override Gear CloneGear()
        {
            return new LuasTear();
        }
    }
}
