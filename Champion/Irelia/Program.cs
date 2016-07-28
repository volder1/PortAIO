using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using Spell = LeagueSharp.Common.Spell;
using SharpDX;

namespace Challenger_Series
{
    public static class Irelia
    {
        public static Spell Q, W, E, R;

        private static int UseQComboStringList;

        private static bool UseWComboBool;

        private static int UseEComboStringList;

        private static bool UseEKSBool;

        private static int QGapcloseModeStringList;

        private static int MinDistForQGapcloser;

        public static Menu config;

        public static int LastAutoAttackTick;

        public static int LastQCastTick;

        public static int LastECastTick;

        public static int LastSpellCastTick;

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
        private static int BladesSpellCount
        {
            get
            {
                    return
                        ObjectManager.Player.Buffs.Where(buff => buff.Name.ToLower() == "ireliatranscendentbladesspell")
                            .Select(buff => buff.Count)
                            .FirstOrDefault();
            }
        }
        public static void OnLoad()
        {
            Q = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 425);
            R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(100, 50, 1600, false, SkillshotType.SkillshotLine);

            InitMenu();
            Game.OnUpdate += OnUpdate;
            Orbwalker.OnPreAttack += OnOrbwalkerAction;
            Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;
        }
        public static float LaneMinManaPercent
        {
            get
            {
                if (getCheckBoxItem(config, "Farm.MinMana.Enable"))
                {
                    return HeroManager.Enemies.Find(e => e.LSIsValidTarget(2000) && !e.IsZombie) == null
                        ? getSliderItem(config, "Lane.MinMana.Alone")
                        : getSliderItem(config, "Lane.MinMana.Enemy");
                }

                return 0f;
            }
        }
        private static void OnOrbwalkerAction(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (args.Target is AIHeroClient && UseWComboBool)
            {
                W.Cast();
            }
        }

        public static void OnUpdate(EventArgs args)
        {
            UseQComboStringList = getBoxItem(config, "useqcombo");

            UseWComboBool = getCheckBoxItem(config, "usewcombo");

            UseEComboStringList = getBoxItem(config, "useecombo");

            UseEKSBool = getCheckBoxItem(config, "useeks");

            QGapcloseModeStringList = getBoxItem(config, "qgc");

            MinDistForQGapcloser = getSliderItem(config, "mindistqgapcloser");
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                //if (Modes.ModeSettings.MenuSettingE.Item("Settings.E.Auto").GetValue<StringList>().SelectedIndex == 1)
                {
                    var t = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                    if (t.LSIsValidTarget() && t.CanStun())
                    {
                        E.Cast(t);
                    }
                }
            }
            var target = TargetSelector.GetTarget(1000, DamageType.Physical);
            if (target != null)
            {
                if (R.IsReady() && getCheckBoxItem(config, "usercombo") && target.LSIsValidTarget(R.Range) && BladesSpellCount >= 0 && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (!target.LSIsValidTarget(Q.Range + Orbwalking.GetRealAutoAttackRange(null)) && target.Health < R.GetDamage(target) * 4)
                    {
                        var rPredictionOutput = R.GetPrediction(target);
                        Vector3 castPosition = rPredictionOutput.CastPosition.LSExtend(ObjectManager.Player.Position, -(ObjectManager.Player.LSDistance(target.ServerPosition) >= 450 ? 80 : 120));
                        if (rPredictionOutput.HitChance >= (ObjectManager.Player.LSDistance(target.ServerPosition) >= R.Range / 2 ? EloBuddy.SDK.Enumerations.HitChance.High : EloBuddy.SDK.Enumerations.HitChance.High) && ObjectManager.Player.LSDistance(castPosition) < R.Range)
                        {
                            R.Cast(castPosition);
                        }
                    }

                    if (GetComboDamage(target) > target.Health && target.LSIsValidTarget(Q.Range) && Q.IsReady())
                    {
                        R.Cast(target, false, true);
                    }
                    if (R.IsReady() && BladesSpellCount > 0 && BladesSpellCount <= 3 && target.LSIsValidTarget(R.Range))
                    {
                        var enemy = HeroManager.Enemies.Find(e => e.Health < R.GetDamage(e) * BladesSpellCount && e.LSIsValidTarget(R.Range));
                        if (enemy == null)
                        {
                            foreach (var e in HeroManager.Enemies.Where(e => e.LSIsValidTarget(R.Range)))
                            {
                                R.Cast(e);
                            }
                        }
                        else
                        {
                            R.Cast(enemy);
                        }
                    }
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Q.IsReady())
                    {
                        var killableEnemy =
                            ObjectManager.Get<AIHeroClient>()
                                .FirstOrDefault(
                                    hero =>
                                        hero.IsEnemy && hero.LSIsValidTarget() && hero.Health <= Q.GetDamage(hero) + ObjectManager.Player.GetAutoAttackDamage(target,true) &&
                                        hero.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) < 650);
                        if (killableEnemy != null && killableEnemy.LSIsValidTarget())
                        {
                            Q.Cast(killableEnemy);
                            LastQCastTick = Environment.TickCount;
                        }

                        var qMode = UseQComboStringList;
                        if (qMode == 0)
                        {
                            var distBetweenMeAndTarget =
                                ObjectManager.Player.ServerPosition.LSDistance(target.ServerPosition);
                            if (distBetweenMeAndTarget > MinDistForQGapcloser)
                            {
                                if (distBetweenMeAndTarget < 650)
                                {
                                    Q.Cast(target);
                                    LastQCastTick = Environment.TickCount;
                                }
                                else
                                {
                                    var minionGapclosingMode = QGapcloseModeStringList;
                                    if (minionGapclosingMode == 0)
                                    {
                                        var gapclosingMinion =
                                            ObjectManager.Get<Obj_AI_Minion>()
                                                .Where(
                                                    m =>
                                                        m.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) <
                                                        650 &&
                                                        m.IsEnemy &&
                                                        m.ServerPosition.LSDistance(target.ServerPosition) <
                                                        distBetweenMeAndTarget && m.LSIsValidTarget() &&
                                                        m.Health < Q.GetDamage(m))
                                                .OrderBy(m => m.Position.LSDistance(target.ServerPosition))
                                                .FirstOrDefault();
                                        if (gapclosingMinion != null)
                                        {
                                            Q.Cast(gapclosingMinion);
                                        }
                                    }
                                    else
                                    {
                                        var firstGapclosingMinion =
                                            ObjectManager.Get<Obj_AI_Minion>()
                                                .Where(
                                                    m =>
                                                        m.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) <
                                                        650 && m.IsEnemy &&
                                                        m.ServerPosition.LSDistance(target.ServerPosition) <
                                                        distBetweenMeAndTarget &&
                                                        m.LSIsValidTarget() && m.Health < Q.GetDamage(m))
                                                .OrderByDescending(m => m.Position.LSDistance(target.ServerPosition))
                                                .FirstOrDefault();
                                        if (firstGapclosingMinion != null)
                                        {
                                            Q.Cast(firstGapclosingMinion);
                                        }
                                    }
                                }
                            }
                        }
                        if (qMode == 1)
                        {
                            var distBetweenMeAndTarget =
                                ObjectManager.Player.ServerPosition.LSDistance(target.ServerPosition);
                            if (distBetweenMeAndTarget < 650)
                            {
                                Q.Cast(target);
                                LastQCastTick = Environment.TickCount;
                            }
                            else
                            {
                                var firstGapclosingMinion =
                                    ObjectManager.Get<Obj_AI_Minion>()
                                        .Where(
                                            m =>
                                                m.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) <
                                                650 && m.IsEnemy &&
                                                m.ServerPosition.LSDistance(target.ServerPosition) <
                                                distBetweenMeAndTarget &&
                                                m.LSIsValidTarget() && m.Health < Q.GetDamage(m))
                                        .OrderByDescending(m => m.Position.LSDistance(target.ServerPosition))
                                        .FirstOrDefault();
                                if (firstGapclosingMinion != null)
                                {
                                    Q.Cast(firstGapclosingMinion);
                                }
                            }
                        }
                    }
                    if (E.IsReady())
                    {
                        var killableEnemy =
                            ObjectManager.Get<AIHeroClient>()
                                .FirstOrDefault(
                                    hero =>
                                        hero.IsEnemy && !hero.IsDead && hero.Health < E.GetDamage(hero) &&
                                        hero.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) < 425 &&
                                        hero.ServerPosition.LSDistance(ObjectManager.Player.ServerPosition) >
                                        ObjectManager.Player.GetAutoAttackRange());
                        if (!Q.IsReady() && UseEKSBool && killableEnemy.LSIsValidTarget(425))
                        {
                            E.Cast(killableEnemy);
                            LastECastTick = Environment.TickCount;
                        }

                        var eMode = UseEComboStringList;
                        var targetE = TargetSelector.GetTarget(425, DamageType.Physical);
                        if (eMode == 0)
                        {
                            if (ObjectManager.Player.HealthPercent <= targetE.HealthPercent && targetE.LSIsValidTarget(425))
                            {
                                E.Cast(targetE);
                                LastECastTick = Environment.TickCount;
                            }
                            if (targetE.HealthPercent < ObjectManager.Player.HealthPercent &&
                                targetE.MoveSpeed > ObjectManager.Player.MoveSpeed - 5 &&
                                ObjectManager.Player.ServerPosition.LSDistance(targetE.ServerPosition) > 300 && targetE.LSIsValidTarget(425))
                            {
                                E.Cast(targetE);
                                LastECastTick = Environment.TickCount;
                            }
                        }
                        if (eMode == 1 && targetE.LSIsValidTarget(425))
                        {
                            E.Cast(targetE);
                            LastECastTick = Environment.TickCount;
                        }
                    }
                }
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                ExecuteQuickLaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                ExecuteLaneClear();
            }
        }
        public static float GetComboDamage(Obj_AI_Base t)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
            {
                fComboDamage += ObjectManager.Player.LSGetSpellDamage(t, SpellSlot.Q);
            }


            //if (ObjectManager.Player.Health >= 20 && ObjectManager.Player.Health <= 50)
            //{
            //    fComboDamage += ObjectManager.Player.TotalAttackDamage*3;
            //}

            //if (ObjectManager.Player.Health > 50)
            //{
            //    fComboDamage += ObjectManager.Player.TotalAttackDamage * 7;
            //}

            if (E.IsReady())
            {
                fComboDamage += ObjectManager.Player.LSGetSpellDamage(t, SpellSlot.E);
            }

            if (R.IsReady())
            {
                fComboDamage += ObjectManager.Player.LSGetSpellDamage(t, SpellSlot.R) * 4;
            }

            if (t.LSIsValidTarget(Q.Range + E.Range) && Q.IsReady() && R.IsReady())
            {
                fComboDamage += ObjectManager.Player.TotalAttackDamage * 2;
            }

            fComboDamage += ObjectManager.Player.TotalAttackDamage * 2;

            return (float)fComboDamage;
        }
        static bool CanStun(this Obj_AI_Base t)
        {
            float targetHealth =Q.IsReady() && !t.LSIsValidTarget(E.Range)
                ? t.Health + Q.GetDamage(t)
                : t.Health;
            return targetHealth / t.MaxHealth * 100 >= ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100;

            //return t.HealthPercent > ObjectManager.Player.HealthPercent;
        }

        public static void Game_OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe)
            {
                return;
            }
            if (spell.SData.Name.Contains("summoner"))
            {
                return;
            }
            //Game.PrintChat(spell.SData.Name);
            if (spell.SData.Name.ToLower().Contains("attack"))
            {
                LastAutoAttackTick = Environment.TickCount;

            }

            //if (!spell.SData.Name.ToLower().Contains("attack"))
            //{
            //    LastSpellCastTick = Environment.TickCount;
            //    Orbwalking.ResetAutoAttackTimer();
            //}


            switch (spell.Slot)
            {
                case SpellSlot.Q:
                    {
                        LastQCastTick = Environment.TickCount;
                        LastSpellCastTick = Environment.TickCount;
                        Orbwalker.ResetAutoAttack();
                        break;
                    }

                case SpellSlot.E:
                    {
                        LastECastTick = Environment.TickCount;
                        LastSpellCastTick = Environment.TickCount;
                        Orbwalker.ResetAutoAttack();
                        break;
                    }

                case SpellSlot.R:
                    {
                        LastQCastTick = Environment.TickCount;
                        LastSpellCastTick = Environment.TickCount;
                        Orbwalker.ResetAutoAttack();
                        break;
                    }
            }
        }
        private static void ExecuteLaneClear()
        {
            if (!getCheckBoxItem(config, "Farm.Enable"))
            {
                return;
            }

            if (ObjectManager.Player.ManaPercent < LaneMinManaPercent)
            {
                return;
            }

            if (Q.IsReady())
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range).Where(m => !m.UnderTurret(true));
                foreach (
                        var minion in
                            minions.Where(
                                m =>
                                    HealthPrediction.GetHealthPrediction(m,
                                        (int)(ObjectManager.Player.AttackCastDelay * 1000), Game.Ping / 2 - 100) < 0)
                                .Where(m => m.Health < Q.GetDamage(m) && Q.CanCast(m)))
                {
                    CastQObjects(minion);
                }
            }
        }
        private static void ExecuteQuickLaneClear()
        {
            if (!getCheckBoxItem(config, "Farm.Enable"))
            {
                return;
            }
            if (ObjectManager.Player.ManaPercent < LaneMinManaPercent)
            {
                return;
            }

            if (Q.IsReady())
            {
                var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

                foreach (
                    var minion in
                        MinionManager.GetMinions(Q.Range)
                            .Where(m => m.Health < Q.GetDamage(m) && Q.CanCast(m)))
                {
                    CastQObjects(minion);
                }
            }
        }

        static void CastQObjects(Obj_AI_Base t)
        {
            if (!Q.CanCast(t))
            {
                return;
            }

            if (Environment.TickCount - LastQCastTick >= 250)
            {
                Q.CastOnUnit(t);
            }
        }

        private static void InitMenu()
        {
            config = MainMenu.AddMenu("Irelia", "Irelia");
            config.Add("useqcombo", new ComboBox("Q Combo MODE : ", 0, "CHALLENGER", "BRONZE", "NEVER"));
            config.Add("useecombo", new ComboBox("Use E Combo", 0, "CHALLENGER", "BRONZE", "NEVER"));
            config.AddSeparator();
            config.Add("usewcombo", new CheckBox("Use W Combo"));
            config.Add("usercombo", new CheckBox("Use R Combo"));
            config.Add("useeks", new CheckBox("Use E KS if Q on CD"));
            config.AddSeparator();
            config.Add("qgc", new ComboBox("Q Gapcloser Mode : ", 0, "ONLY-CLOSEST-TO-TARGET", "ALL-KILLABLE-MINIONS"));
            config.Add("mindistqgapcloser", new Slider("Min Distance for Q Gapclose", 350, 325, 625));
            config.AddSeparator();
            config.Add("Farm.Enable", new CheckBox(":: Lane / Jungle Clear Active!"));
            config.Add("Farm.MinMana.Enable", new CheckBox("Min. Mana Control Active!"));
            config.Add("Lane.MinMana.Alone", new Slider("Min. Mana: I'm Alone %", 30, 0, 100));
            config.Add("Lane.MinMana.Enemy", new Slider("Min. Mana: I'm NOT Alone (Enemy Close) %", 60));
        }
    }
}
