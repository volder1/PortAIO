using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LCS_LeBlanc.Extensions;
using LeagueSharp;
using LeagueSharp.Common;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace LCS_LeBlanc.Modes.Combo
{
    internal static class WRQE
    {
        public static void WRQECombo()
        {
            if (ObjectManager.Player.HasBuff("LeblancSlide") && Utilities.Enabled("w.combo.back", Menus.comboMenu))
            {
                Spells.W.Cast();
            }

            if (Spells.W.IsReady() && Utilities.Enabled("w.combo", Menus.comboMenu))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Spells.W.Range)))
                {
                    var hit = Spells.W.GetPrediction(enemy);
                    if (hit.HitChance >= Utilities.HikiChance("w.hit.chance"))
                    {
                        Spells.W.Cast(enemy);
                    }
                }
            }

            if (!Spells.W.IsReady() && Spells.R.IsReady() && Utilities.Enabled("r.combo", Menus.comboMenu) && Utilities.UltimateKey() == "W")
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Spells.R.Range)))
                {
                    var hit = Spells.R.GetPrediction(enemy);
                    if (hit.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                    {
                        Spells.R.Cast(enemy);
                    }
                }
            }

            if (!Spells.W.IsReady() && !Spells.R.IsReady() && Spells.Q.IsReady() && Utilities.Enabled("q.combo", Menus.comboMenu))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Spells.Q.Range)))
                {
                    Spells.Q.CastOnUnit(enemy);
                }
            }

            if (!Spells.W.IsReady() && !Spells.R.IsReady() && !Spells.Q.IsReady() && 
                Spells.E.IsReady() && Utilities.Enabled("e.combo", Menus.comboMenu))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Spells.E.Range)))
                {
                    var hit = Spells.E.GetPrediction(enemy);
                    if (hit.HitChance >= Utilities.HikiChance("e.hit.chance"))
                    {
                        Spells.E.Cast(enemy);
                    }
                }
            }
        }

        public static void WRQESelected(AIHeroClient enemy)
        {
            if (ObjectManager.Player.HasBuff("LeblancSlide") && Utilities.Enabled("w.combo.back", Menus.comboMenu))
            {
                Spells.W.Cast();
            }

            if (Spells.W.IsReady() && Utilities.Enabled("w.combo", Menus.comboMenu) && enemy.IsValidTarget(Spells.W.Range))
            {
                var hit = Spells.W.GetPrediction(enemy);
                if (hit.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                {
                    Spells.W.Cast(enemy);
                }
            }

            if (!Spells.W.IsReady() && Spells.R.IsReady() && Utilities.Enabled("r.combo", Menus.comboMenu) && Utilities.UltimateKey() == "W"
                && enemy.IsValidTarget(Spells.R.Range))
            {
                var hit = Spells.R.GetPrediction(enemy);
                if (hit.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                {
                    Spells.R.Cast(enemy);
                }
            }

            if (!Spells.W.IsReady() && !Spells.R.IsReady() && Spells.Q.IsReady() && Utilities.Enabled("q.combo", Menus.comboMenu)
                && enemy.IsValidTarget(Spells.Q.Range))
            {
                Spells.Q.CastOnUnit(enemy);
            }

            if (!Spells.W.IsReady() && !Spells.R.IsReady() && !Spells.Q.IsReady() &&
                Spells.E.IsReady() && Utilities.Enabled("e.combo", Menus.comboMenu) && enemy.IsValidTarget(Spells.E.Range))
            {
                var hit = Spells.E.GetPrediction(enemy);
                if (hit.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                {
                    Spells.E.Cast(enemy);
                }
            }
        }

        public static void Init()
        {
            switch (Menus.comboMenu["combo.style"].Cast<ComboBox>().CurrentValue)
            {
                case 0:
                    WRQESelected(TargetSelector.SelectedTarget);
                    break;
                case 1:
                    WRQECombo();
                    break;
            }
        }
    }
}
