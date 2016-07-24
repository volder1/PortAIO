using System;
using ExorAIO.Utilities;
using LeagueSharp;
using LeagueSharp.SDK;
using System.Linq;
using EloBuddy;

 namespace ExorAIO.Champions.Warwick
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
        public static void Automatic(EventArgs args)
        {
            if (GameObjects.Player.LSIsRecalling())
            {
                return;
            }

            /// <summary>
            ///     The Automatic Q Logic.
            /// </summary>
            if (Vars.Q.IsReady() &&
                Targets.Minions.Any() &&
                !GameObjects.EnemyHeroes.Any(t => t.LSIsValidTarget(Vars.R.Range)) &&
                Vars.getCheckBoxItem(Vars.QMenu, "logical"))
            {
                if (GameObjects.Player.MaxHealth <
                        GameObjects.Player.Health +
                        (float)GameObjects.Player.LSGetSpellDamage(Targets.Minions.FirstOrDefault(), SpellSlot.Q) * 0.8)
                {
                    Vars.Q.CastOnUnit(Targets.Minions.FirstOrDefault());
                }
            }

            /// <summary>
            ///     The Automatic W Logic.
            /// </summary>
            if (Vars.W.IsReady() &&
                GameObjects.Player.CountAllyHeroesInRange(Vars.W.Range) > 1 &&
                Vars.getCheckBoxItem(Vars.WMenu, "logical"))
            {
                Vars.W.Cast();
            }

            /// <summary>
            ///     The Automatic E Logic.
            /// </summary>
            if (Vars.E.IsReady() &&
                GameObjects.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1 &&
                Vars.getCheckBoxItem(Vars.EMenu, "logical"))
            {
                Vars.E.Cast();
            }
        }
    }
}