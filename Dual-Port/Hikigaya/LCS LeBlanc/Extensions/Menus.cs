using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace LCS_LeBlanc.Extensions
{
    internal static class Menus
    {
        public static Menu Config, comboMenu, harassMenu, laneclearMenu, jungleClear, miscMenu;

        public static void Initialize()
        {
            Config = MainMenu.AddMenu(":: LCS LeBlanc", ":: LCS LeBlanc");

            comboMenu = Config.AddSubMenu(":: Combo Settings", ":: Combo Settings");
            comboMenu.AddGroupLabel("Combo Settings : ");
            comboMenu.Add("combo.mode", new ComboBox("Default Combo Mode :", 0, "QRWE", "WRQE"));
            comboMenu.Add("combo.style", new ComboBox("Combo Style :", 0, "Selected Target", "Auto"));
            comboMenu.AddGroupLabel("Q Settings : ");
            comboMenu.Add("q.combo", new CheckBox("Use (Q)"));
            comboMenu.AddGroupLabel("W Settings : ");
            comboMenu.Add("w.combo", new CheckBox("Use (W)"));
            comboMenu.Add("w.combo.back", new CheckBox("Back Old (W) Location ?"));
            comboMenu.Add("w.hit.chance", new ComboBox("(W) Hit Chance", 1, Utilities.HitchanceNameArray));
            comboMenu.AddGroupLabel("E Settings : ");
            comboMenu.Add("e.combo", new CheckBox("Use (E)"));
            comboMenu.Add("e.hit.chance", new ComboBox("(E) Hit Chance", 1, Utilities.HitchanceNameArray));
            comboMenu.AddGroupLabel("R Settings : ");
            comboMenu.Add("r.combo", new CheckBox("Use (R)"));

            harassMenu = Config.AddSubMenu(":: Harass Settings", ":: Harass Settings");
            harassMenu.Add("q.harass", new CheckBox("Use (Q)"));
            harassMenu.Add("w.harass", new CheckBox("Use (W)"));
            harassMenu.Add("harass.mana", new Slider("Min. Mana Percentage", 50, 1, 99));

            laneclearMenu = Config.AddSubMenu(":: Wave Clear", ":: Wave Clear");
            laneclearMenu.Add("q.lasthit", new CheckBox("Use (Q) - [lasthit] - [only siege minions]"));
            laneclearMenu.Add("w.clear", new CheckBox("Use (W)"));
            laneclearMenu.Add("w.hit.x.minion", new Slider("Min. Minion", 4, 1, 5));
            laneclearMenu.Add("clear.mana", new Slider("LaneClear Min. Mana Percentage", 50, 1, 99));

            jungleClear = Config.AddSubMenu(":: Jungle Clear", ":: Jungle Clear");
            jungleClear.Add("q.jungle", new CheckBox("Use (Q)"));
            jungleClear.Add("w.jungle", new CheckBox("Use (W)"));
            jungleClear.Add("e.jungle", new CheckBox("Use (E)"));
            jungleClear.Add("jungle.mana", new Slider("Jungle Min. Mana Percentage", 50, 1, 99));

            miscMenu = Config.AddSubMenu(":: Miscellaneous", ":: Miscellaneous");
            miscMenu.Add("anti-gapcloser.e", new CheckBox("Anti-Gapcloser (E) ?"));
        }
    }
}
