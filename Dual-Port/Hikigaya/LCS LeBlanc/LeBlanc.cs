using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LCS_LeBlanc.Extensions;
using LCS_LeBlanc.Modes;
using LCS_LeBlanc.Modes.Combo;
using LeagueSharp.Common;
using LeagueSharp;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace LCS_LeBlanc
{
    public class LeBlanc
    {
        public LeBlanc()
        {
            OnLoad();
        }

        private static void OnLoad()
        {
            Spells.Initialize();
            Menus.Initialize();

            Game.OnUpdate += OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += OnGapcloser;
        }
        private static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsEnemy && gapcloser.Sender.IsValidTarget(Spells.E.Range) &&
                (gapcloser.Sender.LastCastedSpellTarget().IsMe || ObjectManager.Player.Distance(gapcloser.End) < 100) && Spells.E.IsReady()
                && Utilities.Enabled("anti-gapcloser.e", Menus.miscMenu))
            {
                Spells.E.Cast(gapcloser.Sender.Position);
            }
        }

        private static void ComboSelector()
        {
            switch (Menus.comboMenu["combo.mode"].Cast<ComboBox>().CurrentValue)
            {
                case 0:
                    QRWE.Init();
                    break;
                case 1:
                    WRQE.WRQECombo();
                    break;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
           
            Utilities.UpdateUltimateVariable();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                ComboSelector();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Mixed.Init();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Clear.WaveInit();
                Clear.JungleInit();
            }
        }

    }
}
