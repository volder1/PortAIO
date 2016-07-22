using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using UnderratedAIO.Helpers;
using Environment = UnderratedAIO.Helpers.Environment;
using EloBuddy.SDK.Menu;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace UnderratedAIO.Champions
{
    internal class Gangplank
    {
        public static Menu config;
        public static LeagueSharp.Common.Spell Q, W, E, R;
        public static readonly AIHeroClient player = ObjectManager.Player;
        public static bool justQ, justE, chain, blockedE, movingToBarrel;
        public Vector3 ePos;
        public const int BarrelExplosionRange = 325;
        public const int BarrelConnectionRange = 670;
        public List<Barrel> savedBarrels = new List<Barrel>();
        public List<CastedBarrel> castedBarrels = new List<CastedBarrel>();
        public double[] Rwave = new double[] { 50, 70, 90 };
        public double[] EDamage = new double[] { 60, 90, 120, 150, 180 };
        public Obj_AI_Minion NeedToBeDestroyed;

        public Gangplank()
        {
            InitGangPlank();
            InitMenu();
            //Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Gangplank</font>");
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Game_ProcessSpell;
            GameObject.OnCreate += GameObjectOnOnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.E && getCheckBoxItem(menuM, "barrelCorrection") &&
                Game.CursorPos.LSDistance(args.StartPosition) < 50)
            {
                var barrel =
                    GetBarrels()
                        .Where(
                            b =>
                                b.LSDistance(Game.CursorPos) > BarrelConnectionRange &&
                                b.LSDistance(Game.CursorPos) < BarrelConnectionRange * 2 &&
                                b.Position.LSExtend(Game.CursorPos, BarrelConnectionRange).LSDistance(args.StartPosition) <
                                BarrelExplosionRange)
                        .OrderBy(b => b.LSDistance(Game.CursorPos))
                        .FirstOrDefault();
                if (barrel != null && !blockedE)
                {
                    args.Process = false;
                    blockedE = true;
                    LeagueSharp.Common.Utility.DelayAction.Add(
                        5, () =>
                        {
                            E.Cast(barrel.Position.LSExtend(Game.CursorPos, BarrelConnectionRange));
                            blockedE = false;
                        });
                }
            }
        }

        private static void CleanserManager()
        {
            // List of disable buffs
            if (W.IsReady() && ((Player.HasBuffOfType(BuffType.Charm)) || (Player.HasBuffOfType(BuffType.Flee)) || (Player.HasBuffOfType(BuffType.Polymorph)) || (Player.HasBuffOfType(BuffType.Snare)) || (Player.HasBuffOfType(BuffType.Stun)) || (Player.HasBuffOfType(BuffType.Taunt)) || (Player.HasBuff("summonerexhaust")) || (Player.HasBuffOfType(BuffType.Suppression))))
            {
                LeagueSharp.Common.Utility.DelayAction.Add(100, () => { W.Cast(); });
            }
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            for (int i = 0; i < savedBarrels.Count; i++)
            {
                if (savedBarrels[i].barrel != null &&
                    (savedBarrels[i].barrel.NetworkId == sender.NetworkId || savedBarrels[i].barrel.IsDead))
                {
                    savedBarrels.RemoveAt(i);
                    return;
                }
            }
        }

        private void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                savedBarrels.Add(new Barrel(sender as Obj_AI_Minion, System.Environment.TickCount));
            }
        }

        private IEnumerable<Obj_AI_Minion> GetBarrels()
        {
            return savedBarrels.Where(b => b.barrel != null).Select(b => b.barrel);
        }

        private bool KillableBarrel(Obj_AI_Base targetB,
            bool melee = false,
            float delay = 0,
            AIHeroClient sender = null,
            float missileTravelTime = -1)
        {
            if (targetB.Health < 2)
            {
                return true;
            }
            if (sender == null)
            {
                sender = player;
            }
            if (missileTravelTime == -1)
            {
                missileTravelTime = GetQTime(targetB);
            }
            var barrel = savedBarrels.FirstOrDefault(b => b.barrel.NetworkId == targetB.NetworkId);
            if (barrel != null)
            {
                var time = (targetB.Health * getEActivationDelay() * 1000) + delay;
                if ((System.Environment.TickCount - barrel.time +
                     (melee ? (sender.AttackCastDelay) : missileTravelTime) * 1000) > time)
                {
                    return true;
                }
            }
            return false;
        }

        private float GetQTime(Obj_AI_Base targetB)
        {
            return player.LSDistance(targetB) / 2800f + Q.Delay;
        }

        private void InitGangPlank()
        {
            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 590f); //2600f
            Q.SetTargetted(0.25f, 2200f);
            W = new LeagueSharp.Common.Spell(SpellSlot.W);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, 950);
            E.SetSkillshot(0.8f, 50, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R = new LeagueSharp.Common.Spell(SpellSlot.R);
            R.SetSkillshot(1f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.DisableAttacking = false;
            Orbwalker.DisableMovement = false;

            movingToBarrel = false;
            var barrels =
                GetBarrels()
                    .Where(
                        o =>
                            o.IsValid && !o.IsDead && o.LSDistance(player) < 3000 && o.BaseSkinName == "GangplankBarrel" &&
                            o.GetBuff("gangplankebarrellife").Caster.IsMe)
                    .ToList();
            var QMana = Q.ManaCost < player.Mana;
            var shouldAAbarrel = (!Q.IsReady() ||
                                  getBoxItem(menuM, "comboPrior") == 1 ||
                                  (Q.IsReady() && !QMana) || !getCheckBoxItem(menuC, "useq"));
            
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo(barrels, shouldAAbarrel, QMana);
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass(barrels, shouldAAbarrel, QMana);
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                if (getCheckBoxItem(menuH, "useqLHH") && !justE)
                {
                    Lasthit();
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Clear();
            }

            if (menuC["AutoW"].Cast<CheckBox>().CurrentValue)
            {
                CleanserManager();
            }

            if (getCheckBoxItem(menuM, "AutoR") && R.IsReady())
            {
                foreach (var enemy in
                    HeroManager.Enemies.Where(
                        e =>
                            ((e.UnderTurret(true) &&
                              e.MaxHealth / 100 * getSliderItem(menuM, "Rhealt") * 0.75f >
                              e.Health - Program.IncDamages.GetEnemyData(e.NetworkId).DamageTaken) ||
                             (!e.UnderTurret(true) &&
                              e.MaxHealth / 100 * getSliderItem(menuM, "Rhealt") >
                              e.Health - Program.IncDamages.GetEnemyData(e.NetworkId).DamageTaken)) &&
                            e.HealthPercent > getSliderItem(menuM, "RhealtMin") &&
                            e.IsValidTarget() && e.LSDistance(player) > 1500))
                {
                    var pred = Program.IncDamages.GetEnemyData(enemy.NetworkId);
                    if (pred != null && pred.DamageTaken < enemy.Health)
                    {
                        var ally =
                            HeroManager.Allies.OrderBy(a => a.Health).FirstOrDefault(a => enemy.LSDistance(a) < 1000);
                        if (ally != null)
                        {
                            var pos = LeagueSharp.Common.Prediction.GetPrediction(enemy, 0.75f);
                            if (pos.CastPosition.LSDistance(enemy.Position) < 450 && pos.Hitchance >= HitChance.VeryHigh)
                            {
                                if (enemy.IsMoving)
                                {
                                    R.Cast(enemy.Position.LSExtend(pos.CastPosition, 450));
                                }
                                else
                                {
                                    R.Cast(enemy.ServerPosition);
                                }
                            }
                        }
                    }
                }
            }
            if (getKeyBindItem(menuC, "EQtoCursor") && E.IsReady() && Q.IsReady())
            {
                Orbwalker.DisableMovement = true;
                var barrel =
                    GetBarrels()
                        .Where(
                            o =>
                                o.IsValid && !o.IsDead && o.LSDistance(player) < Q.Range &&
                                o.BaseSkinName == "GangplankBarrel" && o.GetBuff("gangplankebarrellife").Caster.IsMe &&
                                KillableBarrel(o, false, -260))
                        .OrderBy(o => o.LSDistance(Game.CursorPos))
                        .FirstOrDefault();
                if (barrel != null)
                {
                    var cp = Game.CursorPos;
                    var cursorPos = barrel.LSDistance(cp) > BarrelConnectionRange
                        ? barrel.Position.LSExtend(cp, BarrelConnectionRange)
                        : cp;
                    var points =
                        CombatHelper.PointsAroundTheTarget(player.Position, E.Range - 200, 15, 6)
                            .Where(p => p.LSDistance(player.Position) < E.Range);
                    var cursorPos2 = cursorPos.LSDistance(cp) > BarrelConnectionRange
                        ? cursorPos.LSExtend(cp, BarrelConnectionRange)
                        : cp;
                    var middle = GetMiddleBarrel(barrel, points, cursorPos);
                    var threeBarrel = cursorPos.LSDistance(cp) > BarrelExplosionRange && E.Instance.Ammo >= 2 &&
                                      Game.CursorPos.LSDistance(player.Position) < E.Range && middle.IsValid();
                    var firsDelay = threeBarrel ? 500 : 265;
                    if (cursorPos.IsValid() && cursorPos.LSDistance(player.Position) < E.Range)
                    {
                        E.Cast(threeBarrel ? middle : cursorPos);
                        LeagueSharp.Common.Utility.DelayAction.Add(firsDelay, () => Q.CastOnUnit(barrel));
                        if (threeBarrel)
                        {
                            if (player.IsMoving)
                            {
                                Player.IssueOrder(GameObjectOrder.Stop, player.Position);
                            }
                            LeagueSharp.Common.Utility.DelayAction.Add(801, () => E.Cast(middle.LSExtend(cp, BarrelConnectionRange)));
                        }
                        else
                        {
                            if (Orbwalker.CanMove)
                            {
                                Orbwalker.DisableMovement = false;
                                Orbwalker.MoveTo(Game.CursorPos);
                            }
                        }
                    }
                }
                else
                {
                    if (Orbwalker.CanMove)
                    {
                        Orbwalker.DisableMovement = false;
                        Orbwalker.MoveTo(Game.CursorPos);
                    }
                }
            }
            else if (getKeyBindItem(menuC, "EQtoCursor"))
            {
                if (Orbwalker.CanMove)
                {
                    Orbwalker.DisableMovement = false;
                    Orbwalker.MoveTo(Game.CursorPos);
                }
            }
            if (getKeyBindItem(menuC, "QbarrelCursor") && Q.IsReady())
            {
                var meleeRangeBarrel =
                    GetBarrels()
                        .OrderBy(o => o.LSDistance(Game.CursorPos))
                        .FirstOrDefault(
                            o =>
                                o.Health > 1 && o.LSDistance(player) < Orbwalking.GetRealAutoAttackRange(o) &&
                                !KillableBarrel(o, true, 265));
                if (meleeRangeBarrel != null && Orbwalker.CanAutoAttack)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, meleeRangeBarrel);
                    return;
                }
                var barrel =
                    GetBarrels()
                        .Where(
                            o =>
                                o.IsValid && !o.IsDead && o.LSDistance(player) < Q.Range &&
                                o.BaseSkinName == "GangplankBarrel" && o.GetBuff("gangplankebarrellife").Caster.IsMe &&
                                KillableBarrel(o))
                        .OrderBy(o => o.LSDistance(Game.CursorPos))
                        .FirstOrDefault();
                if (barrel != null)
                {
                    Q.CastOnUnit(barrel);
                }
            }
            if (NeedToBeDestroyed != null && NeedToBeDestroyed.IsValidTarget() && NeedToBeDestroyed.IsValidTarget() &&
                Orbwalker.CanAutoAttack && NeedToBeDestroyed.IsInAttackRange())
            {
                Console.WriteLine("NeedToBeDestroyed");
                Player.IssueOrder(GameObjectOrder.AttackUnit, NeedToBeDestroyed);
            }
            if (getCheckBoxItem(menuM, "AutoQBarrel") && !movingToBarrel)
            {
                var target = TargetSelector.GetTarget( 1650, DamageType.Physical);
                if (target != null && !target.IsInvulnerable && BlowUpBarrel(barrels, shouldAAbarrel, false, target))
                {
                    if (!chain)
                    {
                        chain = true;
                        LeagueSharp.Common.Utility.DelayAction.Add(450, () => chain = false);
                    }
                }
            }
            for (int i = 0; i < castedBarrels.Count; i++)
            {
                if (castedBarrels[i].shouldDie())
                {
                    castedBarrels.RemoveAt(i);
                    break;
                }
            }
        }

        private Vector3 GetMiddleBarrel(Obj_AI_Minion barrel, IEnumerable<Vector3> points, Vector3 cursorPos)
        {
            var middle =
                points.Where(
                    p =>
                        !p.IsWall() && p.LSDistance(barrel.Position) < BarrelConnectionRange &&
                        p.LSDistance(barrel.Position) > BarrelExplosionRange &&
                        p.LSDistance(cursorPos) < BarrelConnectionRange && p.LSDistance(cursorPos) > BarrelExplosionRange &&
                        p.LSDistance(barrel.Position) + p.LSDistance(cursorPos) > BarrelExplosionRange * 2 - 100)
                    .OrderByDescending(p => p.CountEnemiesInRange(BarrelExplosionRange))
                    .ThenByDescending(p => p.LSDistance(barrel.Position))
                    .FirstOrDefault();
            return middle;
        }

        private void Lasthit()
        {
            if (Q.IsReady())
            {
                var mini =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(m => m.Health < Q.GetDamage(m) && m.BaseSkinName != "GangplankBarrel")
                        .OrderByDescending(m => m.MaxHealth)
                        .ThenByDescending(m => m.LSDistance(player))
                        .FirstOrDefault();

                if (mini != null && !justE)
                {
                    Q.CastOnUnit(mini);
                }
            }
        }


        private void Harass(List<Obj_AI_Minion> barrels, bool shouldAAbarrel, bool qMana)
        {
            float perc = getSliderItem(menuH, "minmanaH") / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }
            AIHeroClient target = TargetSelector.GetTarget(
                Q.Range + BarrelExplosionRange, DamageType.Physical);

            if (getCheckBoxItem(menuH, "useqLHH"))
            {
                var mini =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(m => m.Health < Q.GetDamage(m) && m.BaseSkinName != "GangplankBarrel")
                        .OrderByDescending(m => m.MaxHealth)
                        .ThenByDescending(m => m.LSDistance(player))
                        .FirstOrDefault();

                if (mini != null)
                {
                    Q.CastOnUnit(mini);
                    return;
                }
            }

            if (target == null || Environment.Minion.KillableMinion(player.AttackRange + 50))
            {
                return;
            }
            var dontQ = false;
            //Blow up barrels
            if (getCheckBoxItem(menuH, "useqH") &&
                BlowUpBarrel(barrels, shouldAAbarrel, getCheckBoxItem(menuC, "movetoBarrel"), target))
            {
                if (!chain)
                {
                    chain = true;
                    LeagueSharp.Common.Utility.DelayAction.Add(450, () => chain = false);
                }
                return;
            }

            //Cast E to chain
            if (E.IsReady() && E.Instance.Ammo > 0 && !justQ && !chain && getCheckBoxItem(menuH, "useeH") &&
                getSliderItem(menuH, "eStacksH") < E.Instance.Ammo)
            {
                if (barrels.Any())
                {
                    var bestEMelee = GetEPos(barrels, target, true);
                    var bestEQ = GetEPos(barrels, target, false);
                    if (bestEMelee.IsValid() && shouldAAbarrel)
                    {
                        dontQ = true;
                        E.Cast(bestEMelee);
                    }
                    else if (bestEQ.IsValid() && getCheckBoxItem(menuH, "useqH") && Q.IsReady())
                    {
                        dontQ = true;
                        E.Cast(bestEQ);
                    }
                }
            }
            if (getCheckBoxItem(menuH, "useqH") && Q.IsReady() && !dontQ)
            {
                Q.CastOnUnit(target);
            }
        }

        private void Clear()
        {
            float perc = getSliderItem(menuLC, "minmana") / 100f;
            if (player.Mana < player.MaxMana * perc)
            {
                return;
            }
            if (getCheckBoxItem(menuLC, "useqLC"))
            {
                var barrel =
                    GetBarrels()
                        .FirstOrDefault(
                            o =>
                                o.IsValid && !o.IsDead && o.LSDistance(player) < Q.Range &&
                                o.BaseSkinName == "GangplankBarrel" && o.GetBuff("gangplankebarrellife").Caster.IsMe &&
                                Environment.Minion.countMinionsInrange(o.Position, BarrelExplosionRange) >= 1);
                if (barrel != null)
                {
                    var minis = MinionManager.GetMinions(
                        barrel.Position, BarrelExplosionRange, MinionTypes.All, MinionTeam.NotAlly);
                    var Killable =
                        minis.Where(e => Q.GetDamage(e) >= e.Health && e.Health > 3);
                    if (Q.IsReady() && KillableBarrel(barrel) &&
                        Killable.Any(t => HealthPrediction.LaneClearHealthPrediction(t, 1000) <= 0))
                    {
                        Q.CastOnUnit(barrel);
                    }


                    if (getCheckBoxItem(menuLC, "ePrep"))
                    {
                        if (Q.IsReady() && minis.Count == Killable.Count() && KillableBarrel(barrel))
                        {
                            Q.CastOnUnit(barrel);
                        }
                        else
                        {
                            foreach (var m in
                                minis.Where(
                                    e => Q.GetDamage(e) <= e.Health && e.Health > 3)
                                    .OrderBy(t => t.LSDistance(player))
                                    .ThenByDescending(t => t.Health))
                            {
                                Orbwalker.ForcedTarget = (m);
                                return;
                            }
                        }
                    }
                    else if (Q.IsReady() && KillableBarrel(barrel) &&
                             minis.Count >= getSliderItem(menuLC, "eMinHit"))
                    {
                        Q.CastOnUnit(barrel);
                    }

                    return;
                }
            }
            if (getCheckBoxItem(menuLC, "useqLC") && !justE)
            {
                Lasthit();
            }
            if (getCheckBoxItem(menuLC, "useeLC") && E.IsReady() &&
                getSliderItem(menuLC, "eStacksLC") < E.Instance.Ammo)
            {
                MinionManager.FarmLocation bestPositionE =
                    E.GetCircularFarmLocation(
                        MinionManager.GetMinions(
                            ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly),
                        BarrelExplosionRange);

                if (bestPositionE.MinionsHit >= getSliderItem(menuLC, "eMinHit") &&
                    bestPositionE.Position.LSDistance(ePos) > 400)
                {
                    E.Cast(bestPositionE.Position);
                }
            }
        }

        private void Combo(List<Obj_AI_Minion> barrels, bool shouldAAbarrel, bool Qmana)
        {
            var target = TargetSelector.GetTarget(1650, DamageType.Physical);
            if (target == null)
            {
                return;
            }
            var ignitedmg = (float)player.GetSummonerSpellDamage(target, LeagueSharp.Common.Damage.SummonerSpell.Ignite);
            bool hasIgnite = player.Spellbook.CanUseSpell(player.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            if (getCheckBoxItem(menuC, "useIgnite") && ignitedmg > target.Health && hasIgnite &&
                !CombatHelper.CheckCriticalBuffs(target) && !Q.IsReady() && !justQ)
            {
                player.Spellbook.CastSpell(player.GetSpellSlot("SummonerDot"), target);
            }
            var data = Program.IncDamages.GetAllyData(player.NetworkId);
            if (!data.AnyCC &&
                (getSliderItem(menuC, "usew") / 100f) * player.MaxHealth >
                player.Health - data.DamageTaken && player.CountEnemiesInRange(500) > 0)
            {
                W.Cast();
            }
            if (R.IsReady() && getCheckBoxItem(menuC, "user"))
            {
                var Rtarget =
                    HeroManager.Enemies.FirstOrDefault(e => e.HealthPercent < 50 && e.CountAlliesInRange(660) > 0);
                if (Rtarget != null)
                {
                    R.CastIfWillHit(Rtarget, getSliderItem(menuC, "Rmin"));
                }
            }
            var dontQ = false;

            //Blow up barrels
            if (BlowUpBarrel(barrels, shouldAAbarrel, getCheckBoxItem(menuC, "movetoBarrel"), target))
            {
                if (!chain)
                {
                    chain = true;
                    LeagueSharp.Common.Utility.DelayAction.Add(450, () => chain = false);
                }
                return;
            }

            //Cast E to chain
            if (E.IsReady() && E.Instance.Ammo > 0 && !justQ && !chain &&
                getCheckBoxItem(menuC, "detoneateTarget"))
            {
                if (barrels.Any())
                {
                    var bestEMelee = GetEPos(barrels, target, true);
                    var bestEQ = GetEPos(barrels, target, false);
                    if (bestEMelee.IsValid() && shouldAAbarrel)
                    {
                        dontQ = true;
                        E.Cast(bestEMelee);
                    }
                    else if (bestEQ.IsValid() && getCheckBoxItem(menuC, "useq") && Q.IsReady())
                    {
                        dontQ = true;
                        E.Cast(bestEQ);
                    }
                }
            }


            if (getCheckBoxItem(menuC, "useeAlways") && E.IsReady() && player.LSDistance(target) < E.Range &&
                !justE && target.Health > Q.GetDamage(target) + player.GetAutoAttackDamage(target) &&
                Orbwalker.CanMove && getSliderItem(menuC, "eStacksC") < E.Instance.Ammo)
            {
                CastE(target, barrels);
            }
            var Qbarrelsb =
                GetBarrels()
                    .FirstOrDefault(
                        o =>
                            o.LSDistance(player) < Q.Range &&
                            o.LSDistance(target) < BarrelConnectionRange + BarrelExplosionRange);
            if (Qbarrelsb != null && E.Instance.Ammo > 0 && Q.IsReady() && target.Health > Q.GetDamage(target))
            {
                dontQ = true;
            }
            if (getCheckBoxItem(menuC, "useq") && Q.CanCast(target) && Orbwalker.CanMove && !justE &&
                (!getCheckBoxItem(menuC, "useqBlock") || !dontQ))
            {
                CastQonHero(target, barrels);
            }
        }

        private bool BlowUpBarrel(List<Obj_AI_Minion> barrels,
            bool shouldAAbarrel,
            bool movetoBarrel,
            AIHeroClient target)
        {
            if (barrels.Any())
            {
                var moveDist = movetoBarrel ? getSliderItem(menuC, "movetoBarrelDist") : 0;
                var bestBarrelMelee = GetBestBarrel(barrels, true, target, moveDist);
                var bestBarrelQ = GetBestBarrel(barrels, false, target);
                if (bestBarrelMelee != null && shouldAAbarrel &&
                    HeroManager.Enemies.FirstOrDefault(
                        e => e.LSDistance(bestBarrelMelee) < player.LSDistance(bestBarrelMelee)) == null)
                {
                    if (Orbwalking.GetRealAutoAttackRange(bestBarrelMelee) < player.LSDistance(bestBarrelMelee))
                    {
                        movingToBarrel = true;
                        Orbwalker.DisableAttacking = true;
                        Orbwalker.MoveTo(bestBarrelMelee.Position);
                    }
                    else
                    {
                        if (KillableBarrel(bestBarrelMelee, true) && Orbwalker.CanAutoAttack)
                        {
                            Orbwalker.ForcedTarget = (bestBarrelMelee);
                        }
                    }
                    return true;
                }
                if (bestBarrelQ != null && getCheckBoxItem(menuC, "useq"))
                {
                    Q.CastOnUnit(bestBarrelQ);
                    return true;
                }
            }
            return false;
        }

        private bool EnemiesInBarrelRange(Vector3 barrel, float delay)
        {
            if (
                HeroManager.Enemies.Count(
                    enemy => enemy.IsValidTarget() && enemy.LSDistance(barrel) < BarrelExplosionRange) > 0)
            {
                return true;
            }
            return false;
        }

        private Obj_AI_Minion GetBestBarrel(List<Obj_AI_Minion> barrels,
            bool isMelee,
            AIHeroClient target,
            float moveDist = 0f)
        {
            var meleeBarrels =
                barrels.Where(
                    b =>
                        player.LSDistance(b) <
                        (isMelee
                            ? Orbwalking.GetRealAutoAttackRange(b) +
                              (CombatHelper.IsFacing(player, b.Position) ? moveDist : 0f)
                            : Q.Range) && KillableBarrel(b, isMelee, 265));
            var secondaryBarrels = barrels.Select(b => b.Position).Concat(castedBarrels.Select(c => c.pos));
            var meleeDelay = isMelee ? 0.25f : 0;
            if (moveDist > 0f)
            {
                meleeDelay -= (moveDist / player.MoveSpeed);
            }
            foreach (var melee in meleeBarrels)
            {
                var secondBarrels =
                    secondaryBarrels.Where(
                        b =>
                            !meleeBarrels.Any(n => b.LSDistance(n.Position) < 10) &&
                            melee.LSDistance(b) < BarrelConnectionRange);
                foreach (var second in secondBarrels)
                {
                    var thirdBarrels =
                        secondaryBarrels.Where(
                            b =>
                                !secondBarrels.Any(n => b.LSDistance(n) < 10) &&
                                !meleeBarrels.Any(n => b.LSDistance(n.Position) < 10) &&
                                second.LSDistance(b) < BarrelConnectionRange);
                    foreach (var third in thirdBarrels)
                    {
                        if (EnemiesInBarrelRange(third, 1.25f - meleeDelay))
                        {
                            return melee;
                        }
                    }
                    if (EnemiesInBarrelRange(second, 1f - meleeDelay))
                    {
                        return melee;
                    }
                }
                if (EnemiesInBarrelRange(melee.Position, 0.75f - meleeDelay))
                {
                    return melee;
                }
            }
            return null;
        }

        private Vector3 GetE(Vector3 barrel, AIHeroClient target, float delay, List<Vector3> barrels)
        {
            var enemies =
                HeroManager.Enemies.Where(
                    e =>
                        e.IsValidTarget(1650) && e.LSDistance(barrel) > BarrelExplosionRange &&
                        !barrels.Any(b => b.LSDistance(e.Position) < BarrelExplosionRange));
            var targetPred = LeagueSharp.Common.Prediction.GetPrediction(target, delay);
            var pos = Vector3.Zero;
            pos =
                GetBarrelPoints(barrel)
                    .Where(
                        p =>
                            !p.IsWall() && p.LSDistance(barrel) < BarrelConnectionRange &&
                            p.LSDistance(player.Position) < E.Range &&
                            barrels.Count(b => b.LSDistance(p) < BarrelExplosionRange) == 0 &&
                            targetPred.CastPosition.LSDistance(p) < BarrelExplosionRange &&
                            target.LSDistance(p) < BarrelExplosionRange)
                    .OrderByDescending(p => enemies.Count(e => e.LSDistance(p) < BarrelExplosionRange))
                    .ThenBy(p => p.LSDistance(targetPred.CastPosition))
                    .FirstOrDefault();
            return pos;
        }

        private Vector3 GetMiddleE(Vector3 barrel, AIHeroClient target, float delay, List<Vector3> barrels)
        {
            if (E.Instance.Ammo < 2)
            {
                return Vector3.Zero;
            }
            var enemies =
                HeroManager.Enemies.Where(
                    e =>
                        e.IsValidTarget(1650) && e.LSDistance(barrel) > BarrelExplosionRange &&
                        !barrels.Any(b => b.LSDistance(e.Position) < BarrelExplosionRange));
            var targetPred = LeagueSharp.Common.Prediction.GetPrediction(target, delay);
            var pos = Vector3.Zero;
            pos =
                GetBarrelPoints(barrel)
                    .Where(
                        p =>
                            p.LSDistance(barrel) < BarrelConnectionRange && p.LSDistance(player.Position) < E.Range &&
                            barrels.Count(b => b.LSDistance(p) < BarrelExplosionRange) == 0 &&
                            targetPred.CastPosition.LSDistance(p) < (BarrelExplosionRange - 25) * 2)
                    .OrderByDescending(p => enemies.Count(e => e.LSDistance(p) < BarrelExplosionRange))
                    .ThenBy(p => p.LSDistance(targetPred.CastPosition))
                    .FirstOrDefault();
            return pos;
        }

        private Vector3 GetEPos(List<Obj_AI_Minion> barrels, AIHeroClient target, bool isMelee)
        {
            var barrelPositions = barrels.Select(b => b.Position).Concat(castedBarrels.Select(c => c.pos)).ToList();
            var moveDist = getCheckBoxItem(menuC, "movetoBarrel")
                ? getSliderItem(menuC, "movetoBarrelDist")
                : 0;
            var barrelsInCloseRange =
                barrels.Where(
                    b =>
                        player.LSDistance(b) < target.LSDistance(b) &&
                        player.LSDistance(b) <
                        (isMelee
                            ? Orbwalking.GetRealAutoAttackRange(b) +
                              (CombatHelper.IsFacing(player, b.Position) ? moveDist : 0f)
                            : Q.Range) && KillableBarrel(b, isMelee, -265))
                    .Select(b => b.Position)
                    .Concat(castedBarrels.Select(c => c.pos));
            var meleeDelay = isMelee ? 0.25f : 0;

            foreach (var melee in barrelsInCloseRange)
            {
                var secondPos = GetE(melee, target, 1.265f - meleeDelay, barrelPositions);
                var middle = GetMiddleE(melee, target, 1.465f - meleeDelay, barrelPositions);
                if (secondPos.IsValid())
                {
                    return secondPos;
                }
                var secondBarrels = barrelPositions.Where(b => melee.LSDistance(b) < BarrelConnectionRange).ToList();
                foreach (var secondBarrel in secondBarrels)
                {
                    var thirdE = GetE(secondBarrel, target, 1.265f - meleeDelay, barrelPositions);
                    if (thirdE.IsValid())
                    {
                        return thirdE;
                    }
                }
                if (middle.IsValid())
                {
                    return middle;
                }
            }
            return Vector3.Zero;
        }


        private void CastQonHero(AIHeroClient target, List<Obj_AI_Minion> barrels)
        {
            if (barrels.FirstOrDefault(b => target.LSDistance(b.Position) < BarrelExplosionRange) != null &&
                target.Health > Q.GetDamage(target))
            {
                return;
            }
            Q.CastOnUnit(target);
        }

        private void CastE(AIHeroClient target, List<Obj_AI_Minion> barrels)
        {
            if (barrels.Count(b => b.CountEnemiesInRange(BarrelConnectionRange) > 0) < 1)
            {
                if (getCheckBoxItem(menuC, "useeAlways"))
                {
                    CastEtarget(target);
                }
                return;
            }
            var enemies =
                HeroManager.Enemies.Where(e => e.IsValidTarget() && e.LSDistance(player) < E.Range)
                    .Select(e => LeagueSharp.Common.Prediction.GetPrediction(e, 0.35f));
            List<Vector3> points = new List<Vector3>();
            foreach (var barrel in
                barrels.Where(b => b.LSDistance(player) < Q.Range && KillableBarrel(b)))
            {
                if (barrel != null)
                {
                    var newP = GetBarrelPoints(barrel.Position).Where(p => !p.IsWall());
                    if (newP.Any())
                    {
                        points.AddRange(newP.Where(p => p.LSDistance(player.Position) < E.Range));
                    }
                }
            }
            var bestPoint =
                points.Where(b => enemies.Count(e => e.UnitPosition.LSDistance(b) < BarrelExplosionRange) > 0)
                    .OrderByDescending(b => enemies.Count(e => e.UnitPosition.LSDistance(b) < BarrelExplosionRange))
                    .FirstOrDefault();
            if (bestPoint.IsValid() &&
                !savedBarrels.Any(b => b.barrel.Position.LSDistance(bestPoint) < BarrelConnectionRange))
            {
                E.Cast(bestPoint);
            }
        }

        private void CastEtarget(AIHeroClient target)
        {
            var ePred = LeagueSharp.Common.Prediction.GetPrediction(target, 1);
            var pos = target.Position.LSExtend(ePred.CastPosition, BarrelExplosionRange);
            if (pos.LSDistance(ePos) > 400 && !justE)
            {
                E.Cast(pos);
            }
        }

        private void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(getCheckBoxItem(menuD, "drawqq"), Q.Range, Color.FromArgb(180, 100, 146, 166));
            DrawHelper.DrawCircle(getCheckBoxItem(menuD, "drawee"), E.Range, Color.FromArgb(180, 100, 146, 166));

            var drawecr = getCheckBoxItem(menuD, "draweecr");
            if (drawecr)
            {
                foreach (var barrel in GetBarrels().Where(b => b.LSDistance(player) < E.Range + BarrelConnectionRange))
                {
                    Render.Circle.DrawCircle(barrel.Position, BarrelConnectionRange, Color.FromArgb(180, 167, 141, 56), 7);
                }
            }
            if (getCheckBoxItem(menuD, "drawMTB"))
            {
                Render.Circle.DrawCircle(
                    player.Position,
                    Math.Max(
                        getSliderItem(menuC, "movetoBarrelDist") + 200 - player.BoundingRadius -
                        60, 250), Color.DarkSlateGray, 5);
            }
            
            if (getCheckBoxItem(menuD, "drawW"))
            {
                if (W.IsReady() && player.HealthPercent < 100)
                {
                    float Heal = new int[] { 50, 75, 100, 125, 150 }[W.Level - 1] +
                                 (player.MaxHealth - player.Health) * 0.15f + player.FlatMagicDamageMod * 0.9f;
                    float mod = Math.Max(100f, player.Health + Heal) / player.MaxHealth;
                    float xPos = (float)((double)player.HPBarPosition.X + 36 + 103.0 * mod);
                    Drawing.DrawLine(
                        xPos, player.HPBarPosition.Y + 8, xPos, (float)((double)player.HPBarPosition.Y + 17), 2f,
                        Color.Coral);
                }
            }
            var tokens = player.GetBuff("gangplankbilgewatertoken");
            if (player.InFountain() && getCheckBoxItem(menuD, "drawQpass") && tokens != null &&
                tokens.Count > 500)
            {
                var second = DateTime.Now.Second.ToString();
                var time = int.Parse(second[second.Length - 1].ToString());
                var color = Color.DeepSkyBlue;
                if (time >= 3 && time < 6)
                {
                    color = Color.GreenYellow;
                }
                if (time >= 6 && time < 8)
                {
                    color = Color.Yellow;
                }
                if (time >= 8)
                {
                    color = Color.Orange;
                }
                Drawing.DrawText(
                    Drawing.WorldToScreen(Game.CursorPos).X - 150, Drawing.WorldToScreen(Game.CursorPos).Y - 50, color,
                    "Spend your Silver Serpents, landlubber!");
            }
            if (getBoxItem(menuD, "drawKillableSL") != 0 && R.IsReady())
            {
                var text = new List<string>();
                foreach (var enemy in HeroManager.Enemies.Where(e => e.IsValidTarget()))
                {
                    if (getRDamage(enemy) > enemy.Health)
                    {
                        text.Add(enemy.ChampionName + "(" + Math.Ceiling(enemy.Health / Rwave[R.Level - 1]) + " wave)");
                    }
                }
                if (text.Count > 0)
                {
                    var result = string.Join(", ", text);
                    switch (getBoxItem(menuD, "drawKillableSL"))
                    {
                        case 2:
                            drawText(2, result);
                            break;
                        case 1:
                            drawText(1, result);
                            break;
                        default:
                            return;
                    }
                }
            }

            try
            {
                if (Q.IsReady() && getCheckBoxItem(menuD, "drawEQ"))
                {
                    var points =
                        CombatHelper.PointsAroundTheTarget(player.Position, E.Range - 200, 15, 6)
                            .Where(p => p.LSDistance(player.Position) < E.Range);


                    var barrel =
                        GetBarrels()
                            .Where(
                                o =>
                                    o.IsValid && !o.IsDead && o.LSDistance(player) < Q.Range &&
                                    o.BaseSkinName == "GangplankBarrel" && o.GetBuff("gangplankebarrellife").Caster.IsMe &&
                                    KillableBarrel(o))
                            .OrderBy(o => o.LSDistance(Game.CursorPos))
                            .FirstOrDefault();
                    if (barrel != null)
                    {
                        var cp = Game.CursorPos;
                        var cursorPos = barrel.LSDistance(cp) > BarrelConnectionRange
                            ? barrel.Position.LSExtend(cp, BarrelConnectionRange)
                            : cp;
                        var cursorPos2 = cursorPos.LSDistance(cp) > BarrelConnectionRange
                            ? cursorPos.LSExtend(cp, BarrelConnectionRange)
                            : cp;
                        var middle = GetMiddleBarrel(barrel, points, cursorPos);
                        var threeBarrel = cursorPos.LSDistance(cp) > BarrelExplosionRange && E.Instance.Ammo >= 2 &&
                                          cursorPos2.LSDistance(player.Position) < E.Range && middle.IsValid();
                        if (threeBarrel)
                        {
                            Render.Circle.DrawCircle(
                                middle.LSExtend(cp, BarrelConnectionRange), BarrelExplosionRange, Color.DarkOrange, 6);
                            Render.Circle.DrawCircle(middle, BarrelExplosionRange, Color.DarkOrange, 6);
                            Drawing.DrawLine(
                                Drawing.WorldToScreen(barrel.Position),
                                Drawing.WorldToScreen(middle.LSExtend(barrel.Position, BarrelExplosionRange)), 2,
                                Color.DarkOrange);
                        }
                        else if (E.Instance.Ammo >= 1)
                        {
                            Drawing.DrawLine(
                                Drawing.WorldToScreen(barrel.Position),
                                Drawing.WorldToScreen(cursorPos.LSExtend(barrel.Position, BarrelExplosionRange)), 2,
                                Color.DarkOrange);
                            Render.Circle.DrawCircle(cursorPos, BarrelExplosionRange, Color.DarkOrange, 6);
                        }
                    }
                }
            }
            catch (Exception) { }
            if (getCheckBoxItem(menuD, "drawWcd"))
            {
                foreach (var barrelData in savedBarrels)
                {
                    float time =
                        Math.Min(
                            System.Environment.TickCount - barrelData.time -
                            barrelData.barrel.Health * getEActivationDelay() * 1000f, 0) / 1000f;
                    if (time < 0)
                    {
                        Drawing.DrawText(
                            barrelData.barrel.HPBarPosition.X - -20, barrelData.barrel.HPBarPosition.Y - 20,
                            Color.DarkOrange, string.Format("{0:0.00}", time).Replace("-", ""));
                    }
                }
            }

            if (getCheckBoxItem(menuD, "drawEmini"))
            {
                try
                {
                    var barrels =
                        GetBarrels()
                            .Where(
                                o =>
                                    o.IsValid && !o.IsDead && o.LSDistance(player) < E.Range &&
                                    o.BaseSkinName == "GangplankBarrel" && o.GetBuff("gangplankebarrellife").Caster.IsMe);
                    foreach (var b in barrels)
                    {
                        var minis = MinionManager.GetMinions(
                            b.Position, BarrelExplosionRange, MinionTypes.All, MinionTeam.NotAlly);
                        foreach (var m in
                            minis.Where(e => Q.GetDamage(e) >= e.Health && e.Health > 3))
                        {
                            Render.Circle.DrawCircle(m.Position, 57, Color.Yellow, 7);
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        public void drawText(int mode, string result)
        {
            const string baseText = "Killable with R: ";
            if (mode == 1)
            {
                Drawing.DrawText(
                    Drawing.Width / 2 - (baseText + result).Length * 5, Drawing.Height * 0.75f, Color.Red,
                    baseText + result);
            }
            else
            {
                Drawing.DrawText(
                    player.HPBarPosition.X - (baseText + result).Length * 5 + 110, player.HPBarPosition.Y + 250,
                    Color.Red, baseText + result);
            }
        }

        private float getRDamage(AIHeroClient enemy)
        {
            return
                (float)
                    LeagueSharp.Common.Damage.CalcDamage(
                        player, enemy, DamageType.Magical,
                        (Rwave[R.Level - 1] + 0.1 * player.FlatMagicDamageMod) * waveLength());
        }

        public int waveLength()
        {
            if (player.HasBuff("GangplankRUpgrade1"))
            {
                return 18;
            }
            else
            {
                return 12;
            }
        }

        private static float ComboDamage(AIHeroClient hero)
        {
            double damage = 0;
            if (Q.IsReady())
            {
                damage += LeagueSharp.Common.Damage.LSGetSpellDamage(player, hero, SpellSlot.Q);
            }
            //damage += ItemHandler.GetItemsDamage(hero);
            var ignitedmg = player.GetSummonerSpellDamage(hero, LeagueSharp.Common.Damage.SummonerSpell.Ignite);
            if (player.Spellbook.CanUseSpell(player.GetSpellSlot("summonerdot")) == SpellState.Ready &&
                hero.Health < damage + ignitedmg)
            {
                damage += ignitedmg;
            }
            return (float)damage;
        }

        private void Game_ProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "GangplankQWrapper")
                {
                    if (!justQ)
                    {
                        justQ = true;
                        LeagueSharp.Common.Utility.DelayAction.Add(200, () => justQ = false);
                    }
                }
                if (args.SData.Name == "GangplankE")
                {
                    ePos = args.End;
                    if (!justE)
                    {
                        justE = true;
                        LeagueSharp.Common.Utility.DelayAction.Add(500, () => justE = false);
                        castedBarrels.Add(new CastedBarrel(ePos, System.Environment.TickCount));
                    }
                }
            }
            if (sender.IsEnemy && args.Target != null && sender is AIHeroClient && sender.LSDistance(player) < E.Range)
            {
                var targetBarrels =
                    savedBarrels.Where(
                        b =>
                            b.barrel.NetworkId == args.Target.NetworkId &&
                            KillableBarrel(
                                b.barrel, sender.IsMelee, 0, (AIHeroClient)sender,
                                sender.LSDistance(b.barrel) / args.SData.MissileSpeed));
                foreach (var barrelData in targetBarrels)
                {
                    if (Orbwalker.CanAutoAttack && NeedToBeDestroyed.IsInAttackRange())
                    {
                        NeedToBeDestroyed = barrelData.barrel;
                        LeagueSharp.Common.Utility.DelayAction.Add(230, () => NeedToBeDestroyed = null);
                    }
                    savedBarrels.Remove(barrelData);
                    return;
                }
            }
        }

        private IEnumerable<Vector3> GetBarrelPoints(Vector3 point)
        {
            return
                CombatHelper.PointsAroundTheTarget(point, BarrelConnectionRange, 15f)
                    .Where(p => !p.IsWall() && p.LSDistance(point) > BarrelExplosionRange);
        }

        private float getEActivationDelay()
        {
            if (player.Level >= 13)
            {
                return 0.475f;
            }
            if (player.Level >= 7)
            {
                return 0.975f;
            }
            return 1.975f;
        }

        private void InitMenu()
        {
            config = config.AddSubMenu("Gangplank ", "Gangplank");

            // Draw settings
            menuD = config.AddSubMenu("Drawings ", "dsettings");
            menuD.Add("drawqq", new CheckBox("Draw Q range", false));//.SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.Add("drawW", new CheckBox("Draw W", true));
            menuD.Add("drawee", new CheckBox("Draw E range", false));//.SetValue(new Circle(false, Color.FromArgb(180, 100, 146, 166)));
            menuD.Add("draweecr", new CheckBox("Draw Connection ranges", false));//.SetValue(new Circle(false, Color.FromArgb(180, 167, 141, 56)));
            menuD.Add("drawWcd", new CheckBox("Draw E countdown", true));
            menuD.Add("drawEmini", new CheckBox("Draw killable minions around E", true));
            menuD.Add("drawcombo", new CheckBox("Draw combo damage", true));
            menuD.Add("drawEQ", new CheckBox("Draw EQ to cursor", false));
            menuD.Add("drawMTB", new CheckBox("Draw Move to barrel range", false));
            menuD.Add("drawKillableSL", new ComboBox("Show killable targets with R", 1, "OFF", "Above HUD", "Under GP"));
            menuD.Add("drawQpass", new CheckBox("Draw notification about Silver serpents", true));
            
            // Combo Settings
            menuC = config.AddSubMenu("Combo ", "csettings");
            menuC.Add("EQtoCursor", new KeyBind("EQ to cursor", false, KeyBind.BindTypes.HoldActive, 'T'));
            menuC.Add("QbarrelCursor", new KeyBind("Q barrel at cursor", false, KeyBind.BindTypes.HoldActive, 'H'));
            menuC.AddSeparator();
            menuC.Add("useq", new CheckBox("Use Q", true));
            menuC.Add("useqBlock", new CheckBox("Q : Block Q to save for EQ", false));
            menuC.Add("detoneateTarget", new CheckBox("Q : Blow up target with E", true));
            menuC.Add("usew", new Slider("Use W under health", 20, 0, 100));
            menuC.Add("AutoW", new CheckBox("Use W with QSS options", true));
            menuC.Add("useeAlways", new CheckBox("Use E always", true));
            menuC.Add("eStacksC", new Slider("E : Keep stacks", 0, 0, 5));
            menuC.Add("movetoBarrel", new CheckBox("Move to barrel to AA", false));
            menuC.Add("movetoBarrelDist", new Slider("Move to Barrel Max distance", 300, 0, 450));
            menuC.Add("user", new CheckBox("Use R", true));
            menuC.Add("Rmin", new Slider("R : R min", 2, 1, 5));
            menuC.Add("useIgnite", new CheckBox("Use Ignite", true));

            // Harass Settings
            menuH = config.AddSubMenu("Harass ", "Hsettings");
            menuH.Add("useqH", new CheckBox("Use Q harass", true));
            menuH.Add("useqLHH", new CheckBox("Use Q lasthit", true));
            menuH.Add("useeH", new CheckBox("Use E", true));
            menuH.Add("eStacksH", new Slider("E : Keep stacks", 0, 0, 5));
            menuH.Add("minmanaH", new Slider("Keep X% mana", 1, 1, 100));

            // LaneClear Settings
            menuLC = config.AddSubMenu("LaneClear ", "Lcsettings");
            menuLC.Add("useqLC", new CheckBox("Use Q", true));
            menuLC.Add("useeLC", new CheckBox("Use E", true));
            menuLC.Add("eMinHit", new Slider("E : Min hit", 3, 1, 6));
            menuLC.Add("eStacksLC", new Slider("E : Keep stacks", 0, 0, 5));
            menuLC.Add("ePrep", new CheckBox("E : Prepare minions", true));
            menuLC.Add("minmana", new Slider("Keep X% mana", 1, 1, 100));

            // Misc Settings
            menuM = config.AddSubMenu("Misc ", "Msettings");
            menuM.Add("AutoR", new CheckBox("Cast R to get assists", false));
            menuM.Add("Rhealt", new Slider("R : Enemy health %", 35, 0, 100));
            menuM.Add("RhealtMin", new Slider("R : Enemy min health %", 10, 0, 100));
            menuM.Add("AutoQBarrel", new CheckBox("AutoQ barrel near enemies", false));
            menuM.Add("comboPrior", new ComboBox("Combo priority", 0, "E-Q", "E-AA"));
            menuM.Add("barrelCorrection", new CheckBox("Barrel placement correction", true));
        }

        public static Menu menuM, menuLC, menuH, menuC, menuD;

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
    }

    internal class Barrel
    {
        public Obj_AI_Minion barrel;
        public float time;

        public Barrel(Obj_AI_Minion objAiBase, int tickCount)
        {
            barrel = objAiBase;
            time = tickCount;
        }
    }

    internal class CastedBarrel
    {
        public float time;
        public Vector3 pos;

        public CastedBarrel(Vector3 position, int tickCount)
        {
            pos = position;
            time = tickCount;
        }

        public bool shouldDie()
        {
            return System.Environment.TickCount - time > 260;
        }
    }
}