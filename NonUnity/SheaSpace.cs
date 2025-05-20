using BaseLogic;

namespace HeroSpace
{
    /*class SheaCoroutine : MonoBehaviour
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
    }*/
    class Shea : Creature, IHeroClone
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
    class SheaAA : AutoAttack
    {
        public SheaAA(Creature owner) : base(1, 1, 0, owner) { }
        public override void AutoAtkEffect() { }
    }
    class SheaS1 : Skill
    {
        private bool tranceStorage { get; set; }
        public SheaS1(Creature owner) : base("Green Nature's Melody", "Hp battery", 1.2, 54143, 2000, 28, 0, owner) { }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            if (this.isLight)
            {
                foreach (Creature hero in Globals.listHeroes) hero.Hp += hero.MaxHp * 0.15;
            }
            this.tranceStorage = caster.AppliedBuffs.ContainsKey(this.owner.SkillList[3]);
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.isBuffImmune != 0) continue;
                if (hero.AppliedBuffs.ContainsKey(this))
                {
                    this.PositiveEffectRemove(hero);
                    hero.AppliedBuffs.Remove(this);
                }
                if (this.isDark) hero.NonHeroAmp *= 1.2;
                
                hero.PTough += this.tranceStorage ? 700 : 350;
                hero.MTough += this.tranceStorage ? 700 : 350;
                hero.AppliedBuffs.Add(this, 0);
            }
            this.isChanneling = true;
            int nTicks = 0;
            
            this.Multiplier = this.tranceStorage ? 2.4 : 1.2;
            this.FlatValue = this.tranceStorage ? 108286 : 54143;
            
            while (this.isChanneling && nTicks < 8)
            {
                foreach (Creature hero in Globals.listHeroes) HpManager.SkillHeal(caster, hero, this);
                nTicks += 1;
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
        protected override void NegativeEffectRemove(Creature caster) { }
    }
    class SheaS2 : Skill
    {
        private bool tranceStorage { get; set; }
        public SheaS2(Creature owner) : base("Blue Wind's Song", "Mana battery", 1, 45118, 3000, 28, 0, owner) { }
        public override void LightEffectApply() { }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            if (this.isLight)
            {
                foreach (Creature hero in Globals.listHeroes) hero.Mp += 500;
            }
            this.tranceStorage = caster.AppliedBuffs.ContainsKey(this.owner.SkillList[3]);
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.isBuffImmune != 0) continue;
                if (hero.AppliedBuffs.ContainsKey(this))
                {
                    this.PositiveEffectRemove(hero);
                    hero.AppliedBuffs.Remove(this);
                }
                if (this.isDark)  hero.AtkSpd += 250;
                if (this.tranceStorage) hero.isImmuneCC += 1;
                hero.AppliedBuffs.Add(this, 0);
            }
            this.isChanneling = true;
            int nTicks = 0;

            double mpValue = this.tranceStorage ? 160 : 120;
            this.Multiplier = this.tranceStorage ? 1.5 : 1;
            this.FlatValue = this.tranceStorage ? 67677 : 45118;

            while (this.isChanneling && nTicks < 8)
            {
                foreach (Creature hero in Globals.listHeroes)
                {
                    hero.Mp += mpValue;
                    if (hero.Mp == hero.MaxMp) HpManager.SkillHeal(caster, hero, this);
                }
                nTicks += 1;
            }
            this.isChanneling = false;
            if (this.owner.SkillList[3] is SheaS4 sheaS4) sheaS4.TriggerStacksUp(caster);
        }
        protected override void PositiveEffectRemove(Creature target)
        {
            if (this.isDark) target.AtkSpd -= 250;
            if (this.tranceStorage) target.isImmuneCC -= 1;
        }
        protected override void NegativeEffectRemove(Creature caster) { }
    }
    class SheaS3 : Skill
    {
        private double AtkBoost = 106866;
        private bool tranceStorage { get; set; }
        public SheaS3(Creature owner) : base("Dance of Red Passion", "Damage battery", 0, 0, 3000, 28, 0, owner) { }
        public override void LightEffectApply()
        {
            this.AtkBoost *= 1.4;
        }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            foreach (Creature enemy in Globals.listEnemies)
            {
                if (enemy.AppliedDebuffs.ContainsKey(this) || enemy.isDebuffImmune != 0) continue;
                if (this.isDark)
                {
                    enemy.PWeakness += 0.15;
                    enemy.MWeakness += 0.15;
                    enemy.AppliedDebuffs.Add(this, 0);
                }
            }
            this.tranceStorage = caster.AppliedBuffs.ContainsKey(this.owner.SkillList[3]);
            foreach (Creature hero in Globals.listHeroes)
            {
                if (hero.isBuffImmune != 0) continue;
                if (hero.AppliedBuffs.ContainsKey(this))
                {
                    this.PositiveEffectRemove(hero);
                    hero.AppliedBuffs.Remove(this);
                }

                hero.AtkFlat += this.tranceStorage ? this.AtkBoost * 2 : this.AtkBoost;
                hero.AppliedBuffs.Add(this, 0);
            }
            this.isChanneling = true;
            int nTicks = 0;

            this.Multiplier = this.tranceStorage ? 1.092 : 0.546;
            this.FlatValue = this.tranceStorage ? 48910 : 24455;

            while (this.isChanneling && nTicks < 8)
            {
                foreach (Creature enemy in Globals.listEnemies) HpManager.ComputeDamage(caster, enemy, this);
                nTicks += 1;
            }
            this.isChanneling = false;
            if (this.owner.SkillList[3] is SheaS4 sheaS4) sheaS4.TriggerStacksUp(caster);
        }
        protected override void PositiveEffectRemove(Creature target)
        {
            target.AtkFlat -= this.tranceStorage ? this.AtkBoost * 2 : this.AtkBoost;
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
    class SheaS4 : Skill
    {
        private int blissfulperformancestacks;
        public int blissfulPerformanceStacks
        {
            get => blissfulperformancestacks;
            set => blissfulperformancestacks = Math.Clamp(value, 0, 3);
        }
        public event EventHandler StacksUp;
        public SheaS4(Creature owner) : base("Trance", "Whatever", 3.3, 178470, 0, 0, 12, owner)
        {
            this.isIrremovable = true;
            this.blissfulperformancestacks = 0;
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
        public override void LightEffectApply()
        {
            this.Multiplier = 6.6;
            this.FlatValue = 297451;
        }
        public override void DarkEffectApply() { }
        protected override void ApplyEffect(Creature caster)
        {
            if (!caster.AppliedBuffs.ContainsKey(this)) caster.AppliedBuffs.Add(this, 0);
            if (!this.isDark)
            {
                foreach (Creature hero in Globals.listHeroes)
                {
                    HpManager.SkillHeal(caster, hero, this);
                }
            }
            if (this.isDark)
            {
                foreach (Creature enemy in Globals.listEnemies)
                {
                    if (enemy.AppliedDebuffs.ContainsKey(this) || enemy.isDebuffImmune != 0) continue;
                    enemy.PDefSkills -= 0.2;
                    enemy.MDefSkills -= 0.2;
                    HpManager.ComputeDamage(caster, enemy, this);
                    enemy.AppliedDebuffs.Add(this, 0);
                }
            }
        }
        protected override void PositiveEffectRemove(Creature caster) { }
        protected override void NegativeEffectRemove(Creature target)
        {
            target.PDefSkills += 0.2;
            target.MDefSkills += 0.2;
        }
    }
    class Schwanheite : Gear
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
        public override Gear CloneGear()
        {
            return new Schwanheite();
        }
    }
    class WindyFlower : Gear
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
            BattleEventManager._Instance.Register(BattleEvent._Instance,
                                                  instance => instance.OnUseSkill += this.WindyFlowerMpRegen,
                                                  instance => instance.OnUseSkill -= this.WindyFlowerMpRegen);
        }
        public override void GearEffectUnapply()
        {
            BattleEvent._Instance.OnUseSkill -= this.WindyFlowerMpRegen;
        }
        private void WindyFlowerMpRegen(object sender, EventArgs e)
        {
            var eventArgs = (SkillCasterEventArgs)e;
            if (!eventArgs.caster.AppliedBuffs.ContainsKey(this.EquippingHero.SkillList[1])) return;
            eventArgs.caster.Mp += 400;
            WriteLine($"This hero receives Shea's UT 2 effect : {eventArgs.caster.Name}");
        }
        public override Gear CloneGear()
        {
            return new WindyFlower();
        }
    }
}
