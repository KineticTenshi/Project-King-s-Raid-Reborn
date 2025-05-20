using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using BaseLogic;

namespace HeroSpace
{
    class Clause : Creature, IHeroClone //dummy
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

            this.SkillList.AddRange(new List<Skill> { new ClauseS1(this), new ClauseS2(this), new ClauseS3(this), new ClauseS4(this)});
        }
        public Creature CloneHero()
        {
            return new Clause();
        }
    }
    class ClauseS1 : Skill
    {
        public ClauseS1(Creature owner) : base("Cut Ground", "Stun and Amp", 1.98, 61266, 0, 0, 10, owner) { } //2000, 9
        protected override void ApplyEffect(Creature caster)
        {
            HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
            //if target receives damage : target.EnemyTarget = this.owner if this.isDark
        }
        public override void LightEffectApply()
        {
            this.Cooldown -= 2;
        }
        public override void DarkEffectApply() 
        { 
            this.MpCost += 1000;
        }
        protected override void PositiveEffectRemove(Creature caster) { }
        protected override void NegativeEffectRemove(Creature caster) { }
    }
    class ClauseS2 : Skill
    {
        public ClauseS2(Creature owner) : base("Shield Strike", "Def Shred", 2.8100, 112448, 1000, 8, 20, owner) 
        {
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnBlock += OnBlockCooldownReduction,
                                                  instance => instance.OnBlock -= OnBlockCooldownReduction);
        }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            foreach (Creature enemy in Globals.listEnemies)
            {
                if (enemy.AppliedDebuffs.ContainsKey(this) || enemy.isDebuffImmune != 0) continue;
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
                    enemy.AppliedDebuffs.Add(this, 0);
                }
            }
            HpManager.ComputeDamage(caster, caster.EnemyTarget, this);
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
        private void OnBlockCooldownReduction(object sender, EventArgs e)
        {
            var eventArgs = (AttackerReceiverEventArgs)e;
            Console.WriteLine($"Block event sur {eventArgs.receiver}");
            if (eventArgs.receiver is Clause) this.CurrentCooldown -= 0.5;
        }
    }
    class ClauseS3 : Skill
    {
        public ClauseS3(Creature owner) : base("Guardian Shield", "Block buff", 0, 18044, 2000, 12, 20, owner) { }
        public override void LightEffectApply()
        {
            this.FlatValue *= 1.4;
        }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.AppliedBuffs.ContainsKey(this) || hero.isBuffImmune != 0) continue;
                else
                {
                    hero.PDefFlat += this.FlatValue;
                    hero.PCritResist += 300;
                    hero.PTough += 250;
                    if (this.isDark)
                    {
                        hero.PTough += 250;
                        hero.PBlock += 250;
                    }
                    hero.AppliedBuffs.Add(this, 0);
                }
            }
        }
        protected override void PositiveEffectRemove(Creature target)
        {
            target.PDefFlat -= this.FlatValue;
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
    class ClauseS4 : Skill
    {
        public ClauseS4(Creature owner) : base("Vow of Knight", "Def stat stick", 0.3670, 11391, 0, 0, 8, owner)
        {
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnBlock += OnBlockDamage,
                                                  instance => instance.OnBlock -= OnBlockDamage);
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnBlock += OnAllyReceiveDamage,
                                                  instance => instance.OnBlock -= OnAllyReceiveDamage);
        }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster) { }
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
        private void OnBlockDamage(object sender, EventArgs e)
        {
            var eventArgs = (AttackerReceiverEventArgs)e;
            if (!(eventArgs.receiver is Clause)) return;
            var target = eventArgs.attacker;

            HpManager.ComputeDamage(this.owner, this.owner.EnemyTarget, this);
            this.owner.Mp += 300;
            if (target.AppliedDebuffs.ContainsKey(this) || target.isDebuffImmune != 0) return;
            else
            {
                target.AtkSkills -= 0.2;
                if (this.isLight)
                {
                    this.owner.EnemyTarget.PWeakness += 0.2;
                    this.owner.EnemyTarget.MWeakness += 0.2;
                }
                if (this.isDark)
                {
                    this.owner.EnemyTarget.AtkSkills -= 0.1;
                }
                target.AppliedDebuffs.Add(this, 0);
            }
        }
        private void OnAllyReceiveDamage(object sender, EventArgs e)
        {
            var eventArgs = (AttackerReceiverEventArgs)e;
            if (eventArgs.receiver is Clause || Globals.listEnemies.Contains(eventArgs.receiver)) return;
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.AppliedBuffs.ContainsKey(this) || hero.isBuffImmune != 0) continue;
                hero.PDefSkills += 0.2;
                hero.PCritResist += 200;
                hero.AppliedBuffs.Add(this, 0);
            }
        }
    }
    class Exian : Gear, IWeapon
    {
        private int exianstacks;
        private Skill exianSkill;
        public int ExianStacks
        {
            get => exianstacks;
            set => exianstacks = Math.Clamp(value, 0, 3);
        }
        public SoulWeapon SoulWeapon { get; set; }
        public Exian() : base(3, 0)
        {
            this.Name = "Exian";
            this.Type = GearType.Weapon;
            this.Atk = 112763;
            this.RuneListLength = 3;
            this.LimitHero = "Clause";
            this.exianSkill = new ExianSkill(this.EquippingHero);
            this.SoulWeapon = new ExianSoulWeapon();
            this.SoulWeapon.EquippingWeapon = this;
            WriteLine($"Equipping Hero : {this.EquippingHero}");
        }
        public override void GearEffectApply()
        {
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnUseSkill += this.StacksUp,
                                                  instance => instance.OnUseSkill -= this.StacksUp);
            exianSkill.owner = this.EquippingHero;
            if (SoulWeapon != null) this.SoulWeapon.Skill.owner = this.EquippingHero;
        }
        public override void GearEffectUnapply()
        {
            BattleEvent._Instance.OnUseSkill -= this.StacksUp;
            exianSkill.owner = null;
        }
        private void StacksUp(object sender, EventArgs e)
        {
            var eventArgs = (SkillCasterEventArgs)e;
            if (eventArgs.caster is Clause && (eventArgs.skill is ClauseS1 || eventArgs.skill is ClauseS2 || eventArgs.skill is ClauseS3)) this.ExianStacks++;
            if (this.ExianStacks == 3)
            {
                this.ExianStacks = 0;
                exianSkill.Use(this.EquippingHero);
                Console.WriteLine("Exian skill Up");
            }
        }
        public override Gear CloneGear()
        {
            return new Exian();
        }
    }
    class ExianSkill : Skill
    {
        private Creature HeroMaxAtk { get; set; }
        public ExianSkill(Creature owner) : base("Exian Skill", "Atk and PDef Up", 0, 0, 0, 0, 15, owner) { }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            this.HeroMaxAtk = ListParser.ParserAtk(Globals.listHeroes, true, 1)[0];
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.AppliedBuffs.ContainsKey(this) || hero.isBuffImmune != 0) continue;

                hero.PDefSkills += 0.25;
                hero.AtkSkills += 0.25;
                if (hero == this.HeroMaxAtk)
                {
                    hero.PDefSkills += 0.25;
                    hero.AtkSkills += 0.25;
                }
                hero.AppliedBuffs.Add(this, 0);
            }
        }
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
    class ExianSoulWeapon : SoulWeapon
    {
        public ExianSoulWeapon()
        {
            MaxCharges = 5;
            MaxChargingGauge = 5;
            Cooldown = 22;
            AdvancementLvl = -1;
            Skill = new ExianSoulWeaponSkill(this, null);
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnUseSkill += this.StacksUp,
                                                  instance => instance.OnUseSkill -= this.StacksUp);
        }
        private void StacksUp(object sender, EventArgs e)
        {
            WriteLine($"\nEvent notifictaion received");
            if (this.CurrentCharges == 0) return;
            var eventArgs = (SkillCasterEventArgs)e;
            if (eventArgs.caster is Clause && (eventArgs.skill is ClauseS1 || eventArgs.skill is ClauseS2 || eventArgs.skill is ClauseS3)) this.CurrentChargingGauge++;
            if (this.CurrentChargingGauge == MaxChargingGauge)
            {
                this.Skill.isLocked = false;
                WriteLine($"\nClause SW is useable !");
            }
            WriteLine($"\nClause SW stacks up : {this.CurrentChargingGauge}");
        }
    }
    class ExianSoulWeaponSkill : Skill
    {
        private Creature HeroMaxAtk { get; set; }
        private double DefToAtkBuff { get; set; }
        private SoulWeapon SoulWeapon { get; set; }
        public ExianSoulWeaponSkill(SoulWeapon soulweapon, Creature owner) : base("Exian SW Skill", "Atk and PDef Up", 0, 0, 0, 0, 12, owner) 
        { 
            this.SoulWeapon = soulweapon;
            this.isIrremovable = true;
            this.isLocked = true;
        }
        public override void DarkEffectApply() { }
        public override void LightEffectApply() { }

        protected override void ApplyEffect(Creature caster)
        {
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.AppliedBuffs.ContainsKey(this) || hero.isBuffImmune != 0) continue;

                hero.PDefSkills += 0.7;
                hero.AppliedBuffs.Add(this, 0);
            }

            if (this.SoulWeapon.AdvancementLvl >= 1)
            {
                this.HeroMaxAtk = ListParser.ParserAtk(Globals.listHeroes, true, 1)[0];
                this.HeroMaxAtk.PTough += 800;

                owner = this.SoulWeapon.EquippingWeapon.EquippingHero;
                DefToAtkBuff = owner.PDefBase * (1 + owner.PDefLines) * (1 + owner.PDefSkills) + owner.PDefFlat;
                this.HeroMaxAtk.AtkFlat += DefToAtkBuff;
            }
            
            if (SoulWeapon.AdvancementLvl >= 2)
            {
                this.HeroMaxAtk = ListParser.ParserAtk(Globals.listHeroes, true, 1)[0];
                this.HeroMaxAtk.PAmp += 0.15;
                this.HeroMaxAtk.MAmp += 0.15;
            }
            this.isLocked = true;
            this.SoulWeapon.CurrentChargingGauge = 0;
            this.SoulWeapon.CurrentCharges -= 1;
        }

        protected override void NegativeEffectRemove(Creature target)
        {

        }

        protected override void PositiveEffectRemove(Creature target)
        {
            target.PDefSkills -= 0.7;
            if (target == HeroMaxAtk)
            {
                target.PTough -= 800;
                target.AtkFlat -= DefToAtkBuff;
            }
        }
    }
}
namespace BossSpace
{
    class Boss : Creature //dummy
    {
        public Boss() : base()
        {
            this.Name = "Goblin";
            this.Class = "Monster";
            this.SubClass = SubClassHeroMonster.Goblin;
            this.AtkType = AtkType.Physical;
            this.AtkSubType = "Melee";
            this.Position = "Front";
            this.MaxHpBase = 1500000;
            this.Hp = this.MaxHp;
            this.AtkBase = 0;
            this.Mp = 0;
            this.PDefBase = 500000;
            this.PBlock = 500;
            this.PTough = 450;
            this.PDodge = 300;

            this.SkillList.Add(new CleanseTest(this));
        }
    }
    class CleanseTest : Skill
    {
        public CleanseTest(Creature owner) : base("Cleanse Test", "Remove Negative Effects", 0, 0, 0, 0, 0, owner)
        {
            this.owner = owner;
        }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            Cleanse(this.owner);

            foreach (Creature hero in Globals.listHeroes)
            {
                Dispel(hero);
            }
        }
        protected override void PositiveEffectRemove(Creature caster) { }
        protected override void NegativeEffectRemove(Creature caster) { }
    }
}
