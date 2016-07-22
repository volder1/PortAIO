using EloBuddy;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using System;
using System.Linq;
using PrideStalker_Rengar.Main;
using EloBuddy.SDK;

namespace PrideStalker_Rengar.Handlers
{
    class AfterAA : Core
    {

        public static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                if (Player.Mana == 5 && MenuConfig.Passive)
                {
                    return;
                }

                if (MenuConfig.ComboMode != 2)
                {
                    if (Spells.Q.IsReady() && Player.HealthPercent >= 35 && Player.Mana == 5)
                    {
                        Spells.Q.Cast();
                    }
                    var mob = ObjectManager.Get<Obj_AI_Minion>().Where(m => !m.IsDead && !m.IsZombie && m.Team == GameObjectTeam.Neutral && m.IsValidTarget(Spells.W.Range)).ToList();

                    foreach (var m in mob.Where(m => Player.Mana < 5 && m.Health > Player.GetAutoAttackDamage(m)))
                    {
                        Spells.Q.Cast();
                    }
                }

                if (MenuConfig.ComboMode != 2) return;

                if (Player.Mana < 5)
                {
                    Spells.Q.Cast();
                }
            }
        }
    }
}
