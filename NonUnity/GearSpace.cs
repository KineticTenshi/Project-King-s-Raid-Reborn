using System;
using System.Collections.ObjectModel;
using System.Transactions;
using BaseLogic;

namespace GearSpace
{
    [Flags]
    enum HeroClassLimit
    {
        Knight = 1 << 0,
        Warrior = 1 << 1,
        Assassin = 1 << 2,
        Archer = 1 << 3,
        Mechanic = 1 << 4,
        Wizard = 1 << 5,
        Priest = 1 << 6,
        EarthClass = Knight | Warrior,
        SkyClass = Archer | Assassin | Mechanic,
        FlowClass = Priest | Wizard
    }
    enum GearType
    {
        Weapon,
        Armor,
        SubArmor,
        Earrings,
        Bracelet,
        Necklace,
        Ring,
        Orb,
        Treasure
    }
    enum GearLineType
    {
        MaxHp,
        Atk,
        Def,
        PDef,
        MDef,
        Crit,
        CritDmg,
        Penetration,
        Acc,
        Block,
        PBlock,
        MBlock,
        Dodge,
        PDodge,
        MDodge,
        Lifesteal,
        AtkSpd,
        CCAcc,
        CCResist,
        MpRecovAtk,
        MpRecovSecPercent,
        CritResist,
        PCritResist,
        MCritResist
    }
    [Flags]
    enum GearTier
    {
        T6 = 1 << 0,
        T7 = 1 << 1,
        T8 = 1 << 2,
        T10 = 1 << 3,
        TM_a = 1 << 4,
        TM_b = 1 << 5,
        TM = TM_a | TM_b,
        NonTM = T6 | T7 | T8,
    }
    abstract class Gear //add set effect and enchants
    {
        //misc stats
        public string Name { get; set; }
        public GearType Type { get; set; }
        private int awakeninglvl;
        public int AwakeningLvl
        {
            get => awakeninglvl;
            set => Math.Clamp(value, 0, 5);
        }
        public GearTier gearTier { get; set; }
        //main stats
        public int LimitLvl { get; set; }
        public HeroClassLimit LimitClass { get; set; }
        public string LimitHero { get; set; }
        public double Hp { get; set; }
        public double Atk { get; set; }
        public double PDef { get; set; }
        public double MDef { get; set; }
        public List<Rune> RuneList { get; set; }
        public List<GearLine> LineList { get; set; }
        public int RuneListLength { get; set; }
        public int LineListLength { get; set; }
        public bool isLocked;
        //other stats
        public double FailureBonus { get; set; }
        public bool isEquippedGear { get; set; }
        public Creature EquippingHero { get; set; }

        public Gear(int RuneListLength, int LineListLength)
        {
            this.RuneListLength = RuneListLength;
            this.LineListLength = LineListLength;
            this.RuneList = new List<Rune>(this.RuneListLength);
            this.LineList = new List<GearLine>(this.LineListLength);
            this.EquippingHero = null;
        }
        public void GearEquip(Creature creature)
        {
            if (this.isEquippedGear || creature.Gears.ContainsKey(this.Type)) return;
            if (this.Type == GearType.Weapon) creature.Weapon = this;

            else if (this.Type == GearType.Treasure && !creature.TreasureList.Any(gear => gear.Name == this.Name))
            {
                creature.TreasureList.Add(this);
            }
            else
            {
                creature.Gears[this.Type] = this;
            }
            this.isEquippedGear = true;
            this.EquippingHero = creature;

            creature.MaxHpBase += this.Hp;
            creature.AtkBase += this.Atk;
            creature.PDefBase += this.PDef;
            creature.MDefBase += this.MDef;
            creature.Hp = creature.MaxHp;

            foreach (Rune rune in this.RuneList) rune.RuneEffectApply();
            foreach (GearLine line in this.LineList) line.LineEffect(true);

            GearEffectApply();
        }
        public void GearUnequip(Creature creature)
        {
            if (!this.isEquippedGear || !creature.Gears.ContainsKey(this.Type))return;
            if (this.Type == GearType.Weapon) creature.Weapon = null;
            else if (this.Type == GearType.Treasure)
            {
                creature.TreasureList.Remove(this);
            }
            else
            {
                creature.Gears.Remove(this.Type);
            }
            this.isEquippedGear = false;

            creature.MaxHpBase -= this.Hp;
            creature.AtkBase -= this.Atk;
            creature.PDefBase -= this.PDef;
            creature.MDefBase -= this.MDef;

            foreach (Rune rune in this.RuneList) rune.RuneEffectUnapply();
            foreach (GearLine Line in this.LineList) Line.LineEffect(false);
            this.EquippingHero = null;

            GearEffectUnapply();
        }
        public abstract void GearEffectApply();
        public abstract void GearEffectUnapply();
        public abstract Gear CloneGear();
    }
    class GearAwakener
    {
        private static List<Gear> ListGearToSacrifice = new List<Gear>();
        private static Random rand = new Random();
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
                WriteLine("Weapon at max awakening or no gear to sacrifice !");
                return;
            }
            double probability = ComputeAwakeningChance(gearToAwaken);
            if (rand.Next(0, 100) <= probability + gearToAwaken.FailureBonus)
            {
                WriteLine("Success !");
                gearToAwaken.AwakeningLvl += 1;
                gearToAwaken.FailureBonus = 0;
            }
            else
            {
                WriteLine("Failure !");
                gearToAwaken.FailureBonus += probability / 2;
            }
            ListGearToSacrifice.Clear(); //enlever toutes les futures références pour que le GC supprimme l'objet
        }
    }
    abstract class Rune
    {
        //misc stats
        public string Name { get; set; }
        public int Rarity { get; set; }
        //logic stats
        public GearType EquippableGear { get; set; }
        public Gear EquippingGear { get; set; }
        public bool isEquippedRune { get; set; }
        public void RuneEquip(Gear gear)
        {
            if (gear.Type != this.EquippableGear || gear.RuneList.Count() == gear.RuneListLength || this.isEquippedRune)
            {
                return;
            }

            gear.RuneList.Add(this);
            this.EquippingGear = gear;
            this.isEquippedRune = true;

            if (gear.isEquippedGear)
            {
                RuneEffectApply();
            }
        }

        public void RuneUnequip(Gear gear)
        {
            if (!this.isEquippedRune)
            {
                gear.RuneList.Remove(this);
                this.EquippingGear = null;
                this.isEquippedRune = false;
            }

            if (gear.isEquippedGear)
            {
                RuneEffectUnapply();
            }
        }
        public abstract void RuneEffectApply();
        public abstract void RuneEffectUnapply();
        public abstract Rune CloneRune();
    }
    class GearLine
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
            WriteLine("Line Equipped !");
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
                WriteLine("Line Unequipped !");
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
        private Random rand = new Random();
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
            int enumLength = Enum.GetValues<GearLineType>().Length;
            GearLineType newline;
            double newvalue;

            if ((Line.EquippingGear.gearTier & GearTier.NonTM) != 0)
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

            WriteLine($"New Line Type : {newline} with value : {newvalue}" +
                       $"\nAccept new line ? Yes = 1, No = 0");

            bool validInput = false;
            do
            {
                if (int.TryParse(ReadLine(), out int i))
                {
                    switch (i)
                    {
                        case 0:
                            WriteLine("\nNew line rejected");
                            validInput = true;
                            break;
                        case 1:
                            WriteLine("\nNew line accepted");
                            Line.LineEffect(false);
                            Line.LineType = newline;
                            Line.LineValue = newvalue;
                            Line.LineEffect(true);
                            validInput = true;
                            break;
                        default:
                            WriteLine("\nPlease enter a valid command (0 or 1)");
                            break;
                    }
                }
                else
                {
                    WriteLine("\nInvalid input, please enter 0 or 1");
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
    interface IWeapon
    {
        public SoulWeapon SoulWeapon { get; set; }
    }
    class SoulWeapon
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxCharges { get; set; }
        public int CurrentCharges { get; set; }
        public int MaxChargingGauge { get; set; }
        public int CurrentChargingGauge { get; set; }
        public double Cooldown { get; set; }
        public Skill Skill { get; set; }
        public int SoulLvl { get; set; }
        public double SoulSplit { get; set; }
        public double SoulHp { get; set; }
        public double SoulAtk { get; set; }
        public int AdvancementLvl { get; set; }
        public Gear EquippingWeapon { get; set; }
        
        public void SoulWeaponInitialisation()
        {

        }
        public void SoulWeaponAwakening(IWeapon weapon)
        {
            this.AdvancementLvl = 0;
        }
        public void OnBattleStartChargeSW()
        {
            this.CurrentCharges = MaxCharges;
        }
    }
    class ArmorTest : Gear
    {
        public ArmorTest() : base(1, 4)
        {
            this.Type = GearType.Armor;
            this.PDef = 32000;
            this.gearTier = GearTier.T8;
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
}
