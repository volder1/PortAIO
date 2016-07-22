using LeagueSharp.SDK.Core.Utils;
using SharpDX;
using System;
using System.Linq;
using PrideStalker_Rengar.Main;
using LeagueSharp.SDK;
using EloBuddy;
using EloBuddy.SDK;

 namespace PrideStalker_Rengar.Handlers
{
    class BeforeAA : Core
    {
        
        public static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) return;

            if (MenuConfig.ComboMode != 1) return;

            if (!MenuConfig.TripleQAAReset) return;

            Spells.Q.Cast();
        }
    }
}
