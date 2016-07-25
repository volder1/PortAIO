namespace EatMyCass
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;
    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Menu;

    /// <summary>
    /// EAT MY ASS
    /// </summary>
    internal class EatMyCass
    {
        #region Properties

        /// <summary>
        ///     The stun buff types
        /// </summary>
        private static IEnumerable<BuffType> StunBuffTypes { get; } = new List<BuffType>
                                                                          {
                                                                              BuffType.Knockup, BuffType.Snare, 
                                                                              BuffType.Stun, BuffType.Suppression
                                                                          };

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private LeagueSharp.Common.Spell E { get; set; }

        /// <summary>
        ///     Gets the minimum w range.
        /// </summary>
        /// <value>
        ///     The minimum w range.
        /// </value>
        private int MinimumWRange => 500;

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private AIHeroClient Player => ObjectManager.Player;

        /// <summary>
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        private LeagueSharp.Common.Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the E
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        private LeagueSharp.Common.Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the spells.
        /// </summary>
        /// <value>
        ///     The spells.
        /// </value>
        private List<LeagueSharp.Common.Spell> Spells { get; set; }

        /// <summary>
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        private LeagueSharp.Common.Spell W { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the target.
        /// </summary>
        /// <returns></returns>
        public AIHeroClient GetTarget()
        {
            return getCheckBoxItem(miscMenu, "CustomTargeting")
                       ? HeroManager.Enemies.Where(x => x.LSIsValidTarget(this.Q.Range) && x.IsPoisoned())
                             .OrderByDescending(
                                 x =>
                                 this.Player.CalcDamage(x, DamageType.Magical, 100) / (1 + x.Health)
                                 * TargetSelector.GetPriority(x))
                             .FirstOrDefault()
                         ?? TargetSelector.GetTarget(this.Q.Range, DamageType.Magical)
                       : TargetSelector.GetTarget(this.Q.Range, DamageType.Magical);
        }

        /// <summary>
        ///     Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void OnLoad()
        {
            if (this.Player.ChampionName != "Cassiopeia")
            {
                return;
            }

            Load();

            this.Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 850);
            this.W = new LeagueSharp.Common.Spell(SpellSlot.W, 900);
            this.E = new LeagueSharp.Common.Spell(SpellSlot.E, 700);
            this.R = new LeagueSharp.Common.Spell(SpellSlot.R, 825);

            this.Q.SetSkillshot(0.7f, 75, float.MaxValue, false, SkillshotType.SkillshotCircle);
            this.W.SetSkillshot(0.75f, 160, 1000, false, SkillshotType.SkillshotCircle);
            this.E.SetTargetted(0.125f, 1000);
            this.R.SetSkillshot(0.5f, (float)(80 * Math.PI / 180), 3200, false, SkillshotType.SkillshotCone);

            this.Spells = new List<LeagueSharp.Common.Spell> { this.Q, this.W, this.E, this.R };

            Game.OnUpdate += this.OnUpdate;
            Obj_AI_Base.OnBuffGain += this.OnBuffAdd;
            AntiGapcloser.OnEnemyGapcloser += this.OnEnemyGapcloser;
            Orbwalker.OnUnkillableMinion += this.OnNonKillableMinion;
            Orbwalker.OnPreAttack += this.OnBeforeAttack;
            Interrupter2.OnInterruptableTarget += this.OnInterruptableTarget;
            Drawing.OnDraw += this.OnDraw;

            Chat.Print("<font color='#00FFFF'>Eat My Cass:</font> <font color='#FFFFFF'>Loaded!</font>");
        }

        public Menu Menu { get; set; }

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

        public static Menu comboMenu, blackListMenu, harassMenu, waveClearMenu, ksMenu, drawingSettings, miscMenu;

        public void Load()
        {
            this.Menu = MainMenu.AddMenu("Eat My Cass", "EatMyCass-CM");

            blackListMenu = this.Menu.AddSubMenu("Blacklisted Ult Champions", "BlackPeopleLOL");
            HeroManager.Enemies.ForEach(x => blackListMenu.Add($"Blacklist{x.ChampionName}", new CheckBox(x.ChampionName, false)));

            comboMenu = this.Menu.AddSubMenu("Combo Settings", "ComboSettings");
            comboMenu.Add("UseQCombo", new CheckBox("Use Q"));
            comboMenu.Add("UseWCombo", new CheckBox("Use W"));
            comboMenu.Add("UseECombo", new CheckBox("Use E"));
            comboMenu.Add("UseEPoisonCombo", new CheckBox("Use E Only if Poisoned", false));
            comboMenu.Add("UseRCombo", new CheckBox("Use R"));
            comboMenu.Add("UseREnemyFacing", new Slider("Use R on X Enemies Facing", 3, 1, HeroManager.Enemies.Count));
            comboMenu.Add("UseRComboKillable", new CheckBox("Use R If Killable With Combo"));
            comboMenu.Add("UseRAboveEnemyHp", new Slider("Use R if Target Health Above", 75));

            harassMenu = this.Menu.AddSubMenu("Harass Settings", "HarassSettings");
            harassMenu.Add("UseQHarass", new CheckBox("Use Q"));
            harassMenu.Add("UseWHarass", new CheckBox("Use W"));
            harassMenu.Add("UseEHarass", new CheckBox("Use E"));
            harassMenu.Add("UseEPoisonHarass", new CheckBox("Use E Only if Poisoned"));
            harassMenu.Add("UseEFarmHarass", new CheckBox("Last hit with E"));
            harassMenu.Add("HarassMana", new Slider("Harass Mana", 50));

            waveClearMenu = this.Menu.AddSubMenu("WaveClear Settings", "WaveClearSettings");
            waveClearMenu.Add("UseQWaveClear", new CheckBox("Use Q"));
            waveClearMenu.Add("UseEWaveClear", new CheckBox("Use E"));
            waveClearMenu.Add("WaveClearChamps", new CheckBox("Wave Clear only if no Champs"));
            waveClearMenu.Add("WaveClearHarass", new CheckBox("Harass in Wave Clear"));
            waveClearMenu.Add("WaveClearMana", new Slider("WaveClear Mana", 65));

            ksMenu = this.Menu.AddSubMenu("KillSteal Settings", "KillStealSettings");
            ksMenu.Add("UseQKS", new CheckBox("Use Q"));
            ksMenu.Add("UseWKS", new CheckBox("Use W"));
            ksMenu.Add("UseEKS", new CheckBox("Use E"));
            ksMenu.Add("UseRKS", new CheckBox("Use R", false));

            drawingSettings = this.Menu.AddSubMenu("Drawing Settings", "DrawingSettings");
            drawingSettings.Add("DrawQ", new CheckBox("Draw Q"));
            drawingSettings.Add("DrawW", new CheckBox("Draw W", false));
            drawingSettings.Add("DrawE", new CheckBox("Draw E", false));
            drawingSettings.Add("DrawR", new CheckBox("Draw R", false));

            miscMenu = this.Menu.AddSubMenu("Miscellaneous Settings", "MiscSettings");
            miscMenu.Add("AutoAttackCombo", new CheckBox("Auto Attack in Combo", false));
            miscMenu.Add("AutoAttackHarass", new CheckBox("Auto Attack in Harass", false));
            miscMenu.Add("CustomTargeting", new CheckBox("Advanced Targeting"));
            miscMenu.Add("AutoWCC", new CheckBox("Auto W on CC'd Targets"));
            miscMenu.Add("AntiGapcloser", new CheckBox("Enable Anti-Gapcloser"));
            miscMenu.Add("Interrupter", new CheckBox("Interrupt with R"));
        }

        /// <summary>
        /// Raises the <see cref="E:BeforeAttack" /> event.
        /// </summary>
        /// <param name="args">The <see cref="Orbwalking.BeforeAttackEventArgs"/> instance containing the event data.</param>
        private void OnBeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            var aaCombo = getCheckBoxItem(miscMenu, "AutoAttackCombo");
            var aaHarass = getCheckBoxItem(miscMenu, "AutoAttackHarass");

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && args.Target.Type == GameObjectType.AIHeroClient)
            {
                args.Process = aaCombo;
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && args.Target.Type == GameObjectType.AIHeroClient)
            {
                args.Process = aaHarass;
            }   
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Executes the combo.
        /// </summary>
        private void ExecuteCombo()
        {
            var useQ = getCheckBoxItem(comboMenu, "UseQCombo");
            var useW = getCheckBoxItem(comboMenu, "UseWCombo");
            var useE = getCheckBoxItem(comboMenu, "UseECombo");
            var useR = getCheckBoxItem(comboMenu, "UseRCombo");
            var useREnemyFacing = getSliderItem(comboMenu, "UseREnemyFacing");
            var useRComboKillable = getCheckBoxItem(comboMenu, "UseRComboKillable");
            var useRAboveEnemyHp = getSliderItem(comboMenu, "UseRAboveEnemyHp");
            var useEPoisonCombo = getCheckBoxItem(comboMenu, "UseEPoisonCombo");

            var target = this.GetTarget();

            if (target == null)
            {
                return;
            }

            if (useR && this.R.IsReady()
                && ((HeroManager.Enemies.Count(x => x.LSIsValidTarget(this.R.Range) && x.IsFacing(this.Player))
                     >= useREnemyFacing)
                    || (useRComboKillable
                        && this.Player.GetComboDamage(target, this.Spells.Select(x => x.Slot)) > target.Health
                        && this.Player.GetComboDamage(
                            target, 
                            this.Spells.Where(x => x.Slot != SpellSlot.R).Select(x => x.Slot)) < target.Health)
                    || (target.HealthPercent > useRAboveEnemyHp)) && target.IsFacing(this.Player)
                && !getCheckBoxItem(blackListMenu, $"Blacklist{target.ChampionName}"))
            {
                this.R.Cast(target, aoe: true);
            }

            if (useW && this.W.IsReady() && target.Distance(this.Player) >= this.MinimumWRange)
            {
                this.W.Cast(target);
            }

            if (useQ && this.Q.IsReady())
            {
                this.Q.Cast(target);
            }

            if (!useE || !this.E.IsReady() || !useEPoisonCombo || target.IsPoisoned())
            {
                this.E.CastOnUnit(target);
            }
        }

        /// <summary>
        ///     Executes the harass.
        /// </summary>
        private void ExecuteHarass()
        {
            var useQ = getCheckBoxItem(harassMenu, "UseQHarass");
            var useW = getCheckBoxItem(harassMenu, "UseWHarass");
            var useE = getCheckBoxItem(harassMenu, "UseEHarass");
            var useEPoisonHarass = getCheckBoxItem(harassMenu, "UseEPoisonHarass");
            var useEFarm = getCheckBoxItem(harassMenu, "UseEFarmHarass");
            var harassMana = getSliderItem(harassMenu, "HarassMana");

            var target = this.GetTarget();
            var minionE = MinionManager.GetMinions(this.E.Range)
                .FirstOrDefault(x => this.E.GetDamage(x) > x.Health + 10);

            if (target == null)
            {
                if (useEFarm && this.E.IsReady() && minionE != null
                    && this.Player.GetAutoAttackDamage(minionE) < this.E.GetDamage(minionE))
                {
                    this.E.CastOnUnit(minionE);
                }

                return;
            }

            if (this.Player.ManaPercent < harassMana)
            {
                return;
            }

            if (useQ && this.Q.IsReady())
            {
                this.Q.Cast(target);
            }

            if (useW && this.W.IsReady() && target.Distance(this.Player) >= this.MinimumWRange)
            {
                this.W.Cast(target);
            }

            if (!useE || !this.E.IsReady() || !useEPoisonHarass || target.IsPoisoned())
            {
                this.E.CastOnUnit(target);
            }
            else if (this.E.IsReady() && minionE != null)
            {
                this.E.CastOnUnit(minionE);
            }
        }

        /// <summary>
        ///     Executes the wave clear.
        /// </summary>
        private void ExecuteWaveClear()
        {
            var useQ = getCheckBoxItem(waveClearMenu, "UseQWaveClear");
            var useE = getCheckBoxItem(waveClearMenu, "UseEWaveClear");
            var farmOnlyNoChamps = getCheckBoxItem(waveClearMenu, "WaveClearChamps");
            var waveClearMana = getSliderItem(waveClearMenu, "WaveClearMana");

            if (farmOnlyNoChamps && HeroManager.Enemies.Any(x => x.LSIsValidTarget(this.Q.Range * 1.5f)))
            {
                return;
            }

            if (useQ && this.Q.IsReady() && (waveClearMana < this.Player.ManaPercent))
            {
                var minions = MinionManager.GetMinions(this.Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                var farmLoc = this.Q.GetCircularFarmLocation(minions);

                if (farmLoc.MinionsHit >= (minions.Any(x => x.Team == GameObjectTeam.Neutral) ? 1 : 2))
                {
                    this.Q.Cast(farmLoc.Position);
                }
            }

            if (useE && this.E.IsReady())
            {
                var minionE =
                    MinionManager.GetMinions(this.E.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .FirstOrDefault(x => this.E.GetDamage(x) > x.Health);

                if (minionE != null && this.E.GetDamage(minionE) > this.Player.GetAutoAttackDamage(minionE))
                {
                    this.E.CastOnUnit(minionE);
                }
            }

            if (getCheckBoxItem(waveClearMenu, "WaveClearHarass"))
            {
                this.ExecuteHarass();
            }
        }

        /// <summary>
        ///     Called when a buff is added.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Obj_AI_BaseBuffAddEventArgs" /> instance containing the event data.</param>
        private void OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (args.Buff.Caster.IsEnemy || !this.W.IsInRange(sender) || !getCheckBoxItem(miscMenu, "AutoWCC")
                || StunBuffTypes.All(x => args.Buff.Type != x))
            {
                return;
            }

            this.W.Cast(sender);
        }

        /// <summary>
        ///     Raises the <see cref="E:Draw" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnDraw(EventArgs args)
        {
            var drawQ = getCheckBoxItem(drawingSettings, "DrawQ");
            var drawW = getCheckBoxItem(drawingSettings, "DrawW");
            var drawE = getCheckBoxItem(drawingSettings, "DrawE");
            var drawR = getCheckBoxItem(drawingSettings, "DrawR");

            if (drawQ && this.Q.Level > 0)
            {
                Render.Circle.DrawCircle(this.Player.Position, this.Q.Range, this.Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW && this.W.Level > 0)
            {
                Render.Circle.DrawCircle(this.Player.Position, this.W.Range, this.W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE && this.E.Level > 0)
            {
                Render.Circle.DrawCircle(this.Player.Position, this.E.Range, this.E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR && this.R.Level > 0)
            {
                Render.Circle.DrawCircle(this.Player.Position, this.R.Range, this.R.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!getCheckBoxItem(miscMenu, "AntiGapcloser"))
            {
                return;
            }

            if (this.Player.HealthPercent <= 30 && this.R.IsReady() && gapcloser.Sender.IsFacing(this.Player))
            {
                this.R.Cast(gapcloser.Sender);
            }
            else if (gapcloser.End.Distance(this.Player.ServerPosition) >= this.MinimumWRange)
            {
                this.W.Cast(gapcloser.Sender);
            }
        }

        /// <summary>
        ///     Called when there is an interruptible target.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Interrupter2.InterruptableTargetEventArgs" /> instance containing the event data.</param>
        private void OnInterruptableTarget(AIHeroClient sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!getCheckBoxItem(miscMenu, "Interrupter") || !sender.IsFacing(this.Player) || !sender.LSIsValidTarget(this.R.Range))
            {
                return;
            }

            this.R.Cast(sender);
        }

        /// <summary>
        ///     Called when the orbwalker will miss a minion.
        /// </summary>
        /// <param name="minion">The minion.</param>
        private void OnNonKillableMinion(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            var minionObj = target as Obj_AI_Base;

            if (minionObj == null)
            {
                return;
            }

            if ((!(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) || !getCheckBoxItem(waveClearMenu, "UseEWaveClear"))
                && (!(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || !getCheckBoxItem(harassMenu, "UseEFarmHarass"))
                    || !(this.E.GetDamage(minionObj) > target.Health)))
            {
                return;
            }

            this.E.CastOnUnit(minionObj);
        }

        /// <summary>
        ///     Raises the <see cref="E:Update" /> event.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OnUpdate(EventArgs args)
        {
            this.KillSteal();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                this.ExecuteCombo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                this.ExecuteHarass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                this.ExecuteWaveClear();
            }

           
        }

        /// <summary>
        /// Secures kills.
        /// </summary>
        private void KillSteal()
        {
            foreach (var enemy in HeroManager.Enemies.Where(x => x.LSIsValidTarget(Q.Range)))
            {
                var spell = this.Spells.Where(x => x.IsReady() && getCheckBoxItem(ksMenu, $"Use{x.Slot}KS"))
                        .OrderByDescending(x => x.GetDamage(enemy))
                        .FirstOrDefault();

                if (spell?.GetDamage(enemy) > enemy.Health)
                {
                    spell.Cast(enemy);
                }
            }
        }

        #endregion
    }
}