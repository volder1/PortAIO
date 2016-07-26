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
    internal static class QRWE
    {
        public static void QRWECombo()
        {
            if (ObjectManager.Player.HasBuff("LeblancSlide") && Utilities.Enabled("w.combo.back", Menus.comboMenu))
            {
                Spells.W.Cast();
            }

            if (Spells.Q.IsReady() && Utilities.Enabled("q.combo", Menus.comboMenu) )
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Spells.Q.Range)))
                {
                    Spells.Q.CastOnUnit(enemy);
                }
            }
            else if (Spells.R.IsReady() && Utilities.Enabled("r.combo", Menus.comboMenu) && !Spells.Q.IsReady() && 
                Utilities.UltimateKey() == "Q")
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Spells.R.Range)))
                {
                    Spells.R.CastOnUnit(enemy);
                }
            }
            else if (!Spells.R.IsReady() && !Spells.Q.IsReady() && Spells.W.IsReady() && Utilities.Enabled("w.combo", Menus.comboMenu))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x=> x.IsValidTarget(Spells.W.Range)))
                {
                    var hit = Spells.W.GetPrediction(enemy);
                    if (hit.HitChance >= Utilities.HikiChance("w.hit.chance"))
                    {
                        Spells.W.Cast(hit.CastPosition);
                    }
                }
            }
            else if (!Spells.R.IsReady() && !Spells.Q.IsReady() && !Spells.W.IsReady() && Spells.E.IsReady() && Utilities.Enabled("e.combo", Menus.comboMenu))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(Spells.E.Range)))
                {
                    var hit = Spells.E.GetPrediction(enemy);
                    if (hit.HitChance >= Utilities.HikiChance("e.hit.chance"))
                    {
                        Spells.E.Cast(hit.CastPosition);
                    }
                }
            }
        }

        public static void QRWESelected(AIHeroClient enemy)
        {
            if (ObjectManager.Player.HasBuff("LeblancSlide") && Utilities.Enabled("w.combo.back", Menus.comboMenu))
            {
                Spells.W.Cast();
            }

            if (Spells.Q.IsReady() && Utilities.Enabled("q.combo", Menus.comboMenu) && enemy.IsValidTarget(Spells.Q.Range))
            {
                Spells.Q.CastOnUnit(enemy);
            }
            else if (Spells.R.IsReady() && Utilities.Enabled("r.combo", Menus.comboMenu) && !Spells.Q.IsReady() &&
                Utilities.UltimateKey() == "Q" && enemy.IsValidTarget(Spells.R.Range))
            {
                Spells.R.CastOnUnit(enemy);
            }
            else if (!Spells.R.IsReady() && !Spells.Q.IsReady() && Spells.W.IsReady() && Utilities.Enabled("w.combo", Menus.comboMenu)
                && enemy.IsValidTarget(Spells.W.Range))
            {
                var hit = Spells.W.GetPrediction(enemy);
                if (hit.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                {
                    Spells.W.Cast(hit.CastPosition);
                }
                
            }
            else if (!Spells.R.IsReady() && !Spells.Q.IsReady() && !Spells.W.IsReady() && Spells.E.IsReady() && 
                Utilities.Enabled("e.combo", Menus.comboMenu) && enemy.IsValidTarget(Spells.E.Range))
            {
                var hit = Spells.E.GetPrediction(enemy);
                if (hit.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                {
                    Spells.E.Cast(hit.CastPosition);
                }
            }
        }

        public static void Init()
        {
            switch (Menus.comboMenu["combo.style"].Cast<ComboBox>().CurrentValue)
            {
                case 0:
                    QRWESelected(TargetSelector.SelectedTarget);
                    break;
                case 1:
                    QRWECombo();
                    break;
            }
        }
    }
}
