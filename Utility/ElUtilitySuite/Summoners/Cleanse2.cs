namespace ElUtilitySuite.Summoners
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using Color = SharpDX.Color;
    using ItemData = LeagueSharp.Common.Data.ItemData;
    using EloBuddy;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Menu;

    public class Cleanse2 : IPlugin
    {
        #region Properties

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static AIHeroClient Player => ObjectManager.Player;

        /// <summary>
        ///     Gets the buff indexes handled.
        /// </summary>
        /// <value>
        ///     The buff indexes handled.
        /// </value>
        private Dictionary<int, List<int>> BuffIndexesHandled { get; } = new Dictionary<int, List<int>>();

        /// <summary>
        ///     Gets or sets the buffs to cleanse.
        /// </summary>
        /// <value>
        ///     The buffs to cleanse.
        /// </value>
        private IEnumerable<BuffType> BuffsToCleanse { get; set; }

        /// <summary>
        ///     Gets or sets the items.
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        private List<CleanseItem> Items { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private Menu Menu { get; set; }

        /// <summary>
        ///     Gets or sets the random.
        /// </summary>
        /// <value>
        ///     The random.
        /// </value>
        private Random Random { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets or sets the spells.
        /// </summary>
        /// <value>
        ///     The spells.
        /// </value>
        public static List<CleanseIgnore> Spells { get; set; }

        /// <summary>
        ///     Long hair dont care
        /// </summary>
        public static readonly List<string> TrueStandard = new List<string> { "Stun", "Charm", "Flee", "Fear", "Taunt", "Polymorph" };

        /// <summary>
        ///     Credits to Exory
        /// </summary>
        public static readonly List<string> InvalidSnareCasters = new List<string> { "Leona", "Zyra", "Lissandra" };
        public static readonly List<string> InvalidStunCasters = new List<string> { "Amumu", "LeeSin", "Alistar", "Hecarim", "Blitzcrank" };

        /// <summary>
        ///     Initializes the <see cref="Cleanse" /> class.
        /// </summary>
        static Cleanse2()
        {

            Spells = new List<CleanseIgnore>
                         {
                            new CleanseIgnore { Champion = "Ashe", Spellname = "frostarrow" },
                            new CleanseIgnore { Champion = "Ashe", Spellname = "ashepassiveslow" },
                            new CleanseIgnore { Champion = "Vi", Spellname = "vir" },
                            new CleanseIgnore { Champion = "Yasuo", Spellname = "yasuorknockupcombo" },
                            new CleanseIgnore { Champion = "Yasuo", Spellname = "yasuorknockupcombotar" },
                            new CleanseIgnore { Champion = "Zyra", Spellname = "zyrabramblezoneknockup" },
                            new CleanseIgnore { Champion = "Velkoz", Spellname = "velkozresearchstack" },
                            new CleanseIgnore { Champion = "Darius", Spellname = "dariusaxebrabcone" },
                            new CleanseIgnore { Champion = "Fizz", Spellname = "fizzmoveback" },
                            new CleanseIgnore { Champion = "Blitzcrank", Spellname = "rocketgrab2" },
                            new CleanseIgnore { Champion = "Alistar", Spellname = "pulverize" },
                            new CleanseIgnore { Champion = "Azir", Spellname = "azirqslow" },
                            new CleanseIgnore { Champion = "Rammus", Spellname = "powerballslow" },
                            new CleanseIgnore { Champion = "Rammus", Spellname = "powerballstun" },
                            new CleanseIgnore { Champion = "MonkeyKing", Spellname = "monkeykingspinknockup" },
                            new CleanseIgnore { Champion = "Alistar", Spellname = "headbutttarget" },
                            new CleanseIgnore { Champion = "Hecarim", Spellname = "hecarimrampstuncheck" },
                            new CleanseIgnore { Champion = "Hecarim", Spellname = "hecarimrampattackknockback" },
                            new CleanseIgnore { Spellname = "frozenheartaura" },
                            new CleanseIgnore { Spellname = "frozenheartauracosmetic" },
                            new CleanseIgnore { Spellname = "itemsunfirecapeaura" },
                            new CleanseIgnore { Spellname = "blessingofthelizardelderslow" },
                            new CleanseIgnore { Spellname = "dragonburning" },
                            new CleanseIgnore { Spellname = "chilled" }
                         };
        }

        /// <summary>
        ///     Represents a spell that cleanse can be used on.
        /// </summary>
        public class CleanseIgnore
        {
            #region Public Properties

            /// <summary>
            ///     Gets or sets the champion.
            /// </summary>
            /// <value>
            ///     The champion.
            /// </value>
            public string Champion { get; set; }

            /// <summary>
            ///     Gets or sets the spellname.
            /// </summary>
            /// <value>
            ///     The spellname.
            /// </value>
            public string Spellname { get; set; }

            #endregion
        }


        /// <summary>
        ///     Creates the menu.
        /// </summary>
        /// <param name="rootMenu">The root menu.</param>
        public void CreateMenu(Menu rootMenu)
        {
            this.CreateItems();
            this.BuffsToCleanse = this.Items.SelectMany(x => x.WorksOn).Distinct();

            this.Menu = rootMenu.AddSubMenu("Cleanse RELOADED", "BuffTypeStyleCleanser");
            this.Menu.Add("MinDuration", new Slider("Minimum Duration (MS)", 500, 0, 25000));
            this.Menu.Add("CleanseEnabled.Health", new CheckBox("Cleanse on health", false));
            this.Menu.Add("Cleanse.HealthPercent", new Slider("Cleanse when HP <=", 75, 0, 100));
            this.Menu.Add("CleanseEnabled", new CheckBox("Enabled"));

            humanizerDelay = this.Menu.AddSubMenu("Humanizer Delay", "CleanseHumanizer");
            {
                humanizerDelay.Add("MinHumanizerDelay", new Slider("Min Humanizer Delay (MS)", 100, 0, 500));
                humanizerDelay.Add("MaxHumanizerDelay", new Slider("Max Humanizer Delay (MS)", 150, 0, 500));
                humanizerDelay.Add("HumanizerEnabled", new CheckBox("Enabled", false));
            }

            buffTypeMenu = this.Menu.AddSubMenu("Buff Types", "BuffTypeSettings");
            foreach (var buffType in this.BuffsToCleanse.Select(x => x.ToString()))
            {
                buffTypeMenu.Add($"3Cleanse{buffType}", new CheckBox(buffType, TrueStandard.Contains($"{buffType}")));
            }
        }

        public static Menu humanizerDelay, buffTypeMenu;

        /// <summary>
        ///     Loads this instance.
        /// </summary>
        public void Load()
        {
            this.Random = new Random(Environment.TickCount);
            HeroManager.Allies.ForEach(x => this.BuffIndexesHandled.Add(x.NetworkId, new List<int>()));

            Game.OnUpdate += this.OnUpdate;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the items.
        /// </summary>
        private void CreateItems()
        {
            this.Items = new List<CleanseItem>
                             {
                                 new CleanseItem
                                     {
                                         Slot =
                                             () =>
                                             Player.GetSpellSlot("summonerboost") == SpellSlot.Unknown
                                                 ? SpellSlot.Unknown
                                                 : Player.GetSpellSlot("summonerboost"),
                                         WorksOn =
                                             new[]
                                                 {
                                                     BuffType.Blind, BuffType.Charm, BuffType.Flee, BuffType.Slow,
                                                     BuffType.Polymorph, BuffType.Silence, BuffType.Snare, BuffType.Stun,
                                                     BuffType.Taunt, BuffType.Damage
                                                 },
                                         Priority = 2
                                     },
                                 new CleanseItem
                                     {
                                         Slot = () =>
                                             {
                                                 var slots = ItemData.Quicksilver_Sash.GetItem().Slots;
                                                 return slots.Count == 0 ? SpellSlot.Unknown : slots[0];
                                             },
                                         WorksOn =
                                             new[]
                                                 {
                                                     BuffType.Blind, BuffType.Charm, BuffType.Flee,
                                                     BuffType.Slow, BuffType.Polymorph, BuffType.Silence,
                                                     BuffType.Snare, BuffType.Stun, BuffType.Taunt,
                                                     BuffType.Damage
                                                 },
                                         Priority = 0
                                     },
                                 new CleanseItem
                                     {
                                         Slot = () =>
                                             {
                                                 var slots = ItemData.Dervish_Blade.GetItem().Slots;
                                                 return slots.Count == 0 ? SpellSlot.Unknown : slots[0];
                                             },
                                         WorksOn =
                                             new[]
                                                 {
                                                     BuffType.Blind, BuffType.Charm, BuffType.Flee,
                                                     BuffType.Slow, BuffType.Polymorph, BuffType.Silence,
                                                     BuffType.Snare, BuffType.Stun, BuffType.Taunt,
                                                     BuffType.Damage
                                                 },
                                         Priority = 0
                                     },
                                 new CleanseItem
                                     {
                                         Slot = () =>
                                             {
                                                 var slots = ItemData.Mercurial_Scimitar.GetItem().Slots;
                                                 return slots.Count == 0 ? SpellSlot.Unknown : slots[0];
                                             },
                                         WorksOn =
                                             new[]
                                                 {
                                                     BuffType.Blind, BuffType.Charm, BuffType.Flee,
                                                     BuffType.Slow, BuffType.Polymorph, BuffType.Silence,
                                                     BuffType.Snare, BuffType.Stun, BuffType.Taunt,
                                                     BuffType.Damage
                                                 },
                                         Priority = 0
                                     },
                                 new CleanseItem
                                     {
                                         Slot = () =>
                                             {
                                                 var slots = ItemData.Mikaels_Crucible.GetItem().Slots;
                                                 return slots.Count == 0 ? SpellSlot.Unknown : slots[0];
                                             },
                                         WorksOn =
                                             new[]
                                                 {
                                                     BuffType.Stun, BuffType.Snare, BuffType.Taunt,
                                                     BuffType.Silence, BuffType.Slow,
                                                     BuffType.Fear
                                                 },
                                         WorksOnAllies = true, Priority = 1
                                     }
                             };
        }

        public static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        public static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        public static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }

        public static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }

        /// <summary>
        ///     Gets the best cleanse item.
        /// </summary>
        /// <param name="ally">The ally.</param>
        /// <param name="buff">The buff.</param>
        /// <returns></returns>
        private Spell GetBestCleanseItem(GameObject ally, BuffInstance buff)
        {
            foreach (var item in Items.OrderBy(x => x.Priority))
            {
                if (!item.WorksOn.Any(x => buff.Type.HasFlag(x)))
                {
                    continue;
                }

                if (!(ally.IsMe || item.WorksOnAllies))
                {
                    continue;
                }

                if (!item.Spell.IsReady() || !item.Spell.IsInRange(ally) || item.Spell.Slot == SpellSlot.Unknown)
                {
                    continue;
                }

                return item.Spell;
            }

            return null;
        }


        private void OnUpdate(EventArgs args)
        {
            if (!getCheckBoxItem(this.Menu, "CleanseEnabled"))
            {
                return;
            }

            foreach (var ally in HeroManager.Allies)
            {
                foreach (
                    var buff in
                        ally.Buffs.Where(
                            x =>
                            this.BuffsToCleanse.Contains(x.Type) && x.Caster.Type == GameObjectType.AIHeroClient && x.Caster.IsEnemy))
                {
                    if (!getCheckBoxItem(buffTypeMenu, $"3Cleanse{buff.Type}")
                        || getSliderItem(this.Menu, "MinDuration") / 1000f
                        > buff.EndTime - buff.StartTime || this.BuffIndexesHandled[ally.NetworkId].Contains(buff.Index) || Spells.Any(b => buff.Name.Equals(b.Spellname, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    if (buff.Type == BuffType.Snare && InvalidSnareCasters.Contains(((AIHeroClient)buff.Caster).ChampionName, StringComparer.InvariantCultureIgnoreCase) || buff.Type == BuffType.Stun && InvalidStunCasters.Contains(((AIHeroClient)buff.Caster).ChampionName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    var cleanseItem = this.GetBestCleanseItem(ally, buff);
                    if (cleanseItem == null)
                    {
                        continue;
                    }

                    Console.WriteLine($"Casted bufftype: {buff.Type} by {buff.Caster.Name} - {buff.Name}");

                    this.BuffIndexesHandled[ally.NetworkId].Add(buff.Index);

                    if (getCheckBoxItem(humanizerDelay, "HumanizerEnabled"))
                    {
                        LeagueSharp.Common.Utility.DelayAction.Add(
                            (int)
                            Math.Min(
                                this.Random.Next(
                                    getSliderItem(humanizerDelay, "MinHumanizerDelay"),
                                    getSliderItem(humanizerDelay, "MaxHumanizerDelay")),
                                (buff.StartTime - buff.EndTime) * 1000),
                            () =>
                            {
                                if (getCheckBoxItem(this.Menu, "CleanseEnabled.Health"))
                                {
                                    if (getSliderItem(this.Menu, "Cleanse.HealthPercent") <= ObjectManager.Player.HealthPercent)
                                    {
                                        cleanseItem.Cast(ally);
                                        this.BuffIndexesHandled[ally.NetworkId].Remove(buff.Index);
                                    }
                                }
                                else
                                {
                                    cleanseItem.Cast(ally);
                                    this.BuffIndexesHandled[ally.NetworkId].Remove(buff.Index);
                                }
                            });
                    }
                    else
                    {
                        if (getCheckBoxItem(this.Menu, "CleanseEnabled.Health"))
                        {
                            if (getSliderItem(this.Menu, "Cleanse.HealthPercent") <= ObjectManager.Player.HealthPercent)
                            {
                                cleanseItem.Cast(ally);
                                this.BuffIndexesHandled[ally.NetworkId].Remove(buff.Index);
                            }
                        }
                        else
                        {
                            cleanseItem.Cast(ally);
                            this.BuffIndexesHandled[ally.NetworkId].Remove(buff.Index);
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     An item/spell that can be used to cleanse a spell.
    /// </summary>
    public class CleanseItem
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CleanseItem" /> class.
        /// </summary>
        public CleanseItem()
        {
            this.Range = float.MaxValue;
            this.WorksOnAllies = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the priority.
        /// </summary>
        /// <value>
        ///     The priority.
        /// </value>
        public int Priority { get; set; }

        /// <summary>
        ///     Gets or sets the range.
        /// </summary>
        /// <value>
        ///     The range.
        /// </value>
        public float Range { get; set; }

        /// <summary>
        ///     Gets or sets the slot delegate.
        /// </summary>
        /// <value>
        ///     The slot delegate.
        /// </value>
        public Func<SpellSlot> Slot { get; set; }

        /// <summary>
        ///     Gets or sets the spell.
        /// </summary>
        /// <value>
        ///     The spell.
        /// </value>
        public Spell Spell => new Spell(this.Slot(), this.Range);

        /// <summary>
        ///     Gets or sets what the spell works on.
        /// </summary>
        /// <value>
        ///     The buff types the spell works on.
        /// </value>
        public BuffType[] WorksOn { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the spell works on allies.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the spell works on allies; otherwise, <c>false</c>.
        /// </value>
        public bool WorksOnAllies { get; set; }

        #endregion
    }
}