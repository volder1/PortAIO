using System;
using System.Linq;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Utils;
using EloBuddy.SDK;
using EloBuddy;

namespace ExorAIO.Champions.Ryze
{
    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        /// <summary>
        ///     Called when the game updates itself.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void Combo(EventArgs args)
        {
            if (!Targets.Target.IsValidTarget() ||
                Invulnerable.Check(Targets.Target, DamageType.Magical))
            {
                return;
            }

            if (Bools.HasSheenBuff() &&
                Targets.Target.IsValidTarget(Vars.AARange))
            {
                return;
            }

            /// <summary>
            ///     Dynamic Combo Logic.
            /// </summary>
            switch (Vars.RyzeStacks)
            {
                case 0:
                case 1:
                    /// <summary>
                    ///     The Q Combo Logic.
                    /// </summary>
                    if (Vars.RyzeStacks != 1 ||
                        (GameObjects.Player.HealthPercent >
                            Vars.getSliderItem(Vars.QMenu, "shield") ||
                        Vars.getSliderItem(Vars.QMenu, "shield") == 0))
                    {
                        if (Vars.Q.IsReady() &&
                            Environment.TickCount - Vars.LastTick > 250 &&
                            Targets.Target.IsValidTarget(Vars.Q.Range - 100f) &&
                            Vars.getCheckBoxItem(Vars.QMenu, "combo"))
                        {
                            Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
                        }
                    }

                    /// <summary>
                    ///     The W Combo Logic.
                    /// </summary>
                    if (Vars.W.IsReady() &&
                        Targets.Target.IsValidTarget(Vars.W.Range) &&
                        Vars.getCheckBoxItem(Vars.WMenu, "combo"))
                    {
                        Vars.W.CastOnUnit(Targets.Target);
                    }

                    /// <summary>
                    ///     The E Combo Logic.
                    /// </summary>
                    if (Vars.E.IsReady() &&
                        Targets.Target.IsValidTarget(Vars.E.Range) &&
                        Vars.getCheckBoxItem(Vars.EMenu, "combo"))
                    {
                        Vars.E.CastOnUnit(Targets.Target);
                        Vars.LastTick = Environment.TickCount;
                        return;
                    }
                    break;

                default:
                    /// <summary>
                    ///     The Q Combo Logic.
                    /// </summary>
                    if (Vars.Q.IsReady() &&
                        Targets.Target.IsValidTarget(Vars.Q.Range - 100f) &&
                        Vars.getCheckBoxItem(Vars.QMenu, "combo"))
                    {
                        Vars.Q.Cast(Vars.Q.GetPrediction(Targets.Target).UnitPosition);
                    }
                    break;
            }
        }
    }
}