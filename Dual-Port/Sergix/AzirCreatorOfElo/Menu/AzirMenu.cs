using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;

namespace Azir_Creator_of_Elo
{
    class AzirMenu : Menu
    {
        public static EloBuddy.SDK.Menu.Menu _drawSettingsMenu, _miscMenu,_jumpMenu,_comboMenu, _harashMenu, _laneClearMenu, _JungleClearMenu, GapCloserMenu, interruptMenu;
      
        public AzirMenu(String name,AzirMain  azir) : base(name)
        {
            LoadMenu(azir);
        }

        public override void LoadMenu(AzirMain azir)
        {
            base.LoadMenu(azir);
            LoadLaneClearMenu();
            LoadHarashMenu();
            LoadComboMenu();
            LoadJungleClearMenu();
            LoadDrawings();
            LoadJumps();
            LoadMiscInterrupt(azir);
            LoadMiscMenu(azir);
        }

        public void LoadDrawings()
        {
            _drawSettingsMenu = GetMenu.AddSubMenu("Drawings", "Draw Settings");
            {
                _drawSettingsMenu.Add("dsl", new CheckBox("Draw Soldier Line"));
                _drawSettingsMenu.Add("dcr", new CheckBox("Draw Control range"));
                _drawSettingsMenu.Add("dfr", new CheckBox("Draw Flee range"));
            }
        }
        public void LoadComboMenu()
        {
            _comboMenu = GetMenu.AddSubMenu("Combo", "Combo Menu");
            {
                _comboMenu.Add("SoldiersToQ", new Slider("Soldiers to Q", 1, 1, 3));
                _comboMenu.Add("CQ", new CheckBox("Use Q"));
                _comboMenu.Add("CW", new CheckBox("Use W"));
                _comboMenu.Add("CR", new CheckBox("Use R killeable"));
            }
        }
        public void LoadLaneClearMenu()
        {
            _laneClearMenu = GetMenu.AddSubMenu("Laneclear", "Laneclear Menu");
            {
                _laneClearMenu.Add("LQ", new CheckBox("Use Q"));
                _laneClearMenu.Add("LW", new CheckBox("Use W"));
                _laneClearMenu.Add("LWM", new Slider("Minions at W range to cast", 3, 1, 6));
                _laneClearMenu.Add("LQM", new Slider("Soldiers to Q ", 1, 1, 3));

            }
        }
        public void LoadJungleClearMenu()
        {
            _JungleClearMenu = GetMenu.AddSubMenu("JungleClear", "JungleClear  Menu");
            {
                _JungleClearMenu.Add("JW", new CheckBox("Use W"));
                _JungleClearMenu.Add("JQ", new CheckBox("Use Q"));
            }
        }

        public void LoadMiscInterrupt(AzirMain azir)
        {
          
        }
        public void LoadMiscMenu(AzirMain azir)
        {
            List<String> spellsin = new List<string>();
            foreach (AIHeroClient hero in HeroManager.Enemies)
            {
                for (int i = 0; i < 4; i++)
                {
                    //  hero.GetSpell(Trans(i)).Name;
                    foreach (String s in azir.Interrupt)
                    {
                        if (s == hero.GetSpell(azir.Trans(i)).Name)
                        {
                            spellsin.Add("[" + hero.ChampionName + "]" + s);
                        }
                    }
                }
            }
            azir.InterruptSpell = spellsin;
            int num = 0;
            var interruptMenu = GetMenu.AddSubMenu("Spell Interrupt", "R Interrupt spells");
            {
                interruptMenu.Add("UseRInterrupt", new CheckBox("Use R Interrupt"));
                foreach (String s in spellsin)
                {
                    interruptMenu.Add("S" + num, new CheckBox(s));
                    num++;
                }
            }
            azir.InterruptNum = num;
            List<String> spellgap = new List<string>();
            foreach (AIHeroClient hero in HeroManager.Enemies)
            {
                for (int i = 0; i < 4; i++)
                {
                    foreach (String s in azir.Gapcloser)
                    {
                        if (s == hero.GetSpell(azir.Trans(i)).Name)
                        {
                            spellgap.Add("[" + hero.ChampionName + "]" + s);
                        }
                    }
                }
            }
            int numg = 0;
            azir.InterruptSpell = spellgap;
            GapCloserMenu = GetMenu.AddSubMenu("Spell Gapcloser", "R to Gapcloser");
            {
                GapCloserMenu.Add("UseRGapcloser", new CheckBox("Use R Gapcloser"));
                foreach (String s in spellgap)
                {
                    GapCloserMenu.Add("G" + numg, new CheckBox(s));
                    numg++;
                }
            }
            numg = azir.GapcloserNum;
            _miscMenu = GetMenu.AddSubMenu("Misc", "Harash Menu");
            {
                _miscMenu.Add("FMJ", new CheckBox("Max Range Jump Only"));
                _miscMenu.Add("ARUT", new CheckBox("auto R under the Turret"));
            }
        }

        public void LoadHarashMenu()
        {
            _harashMenu = GetMenu.AddSubMenu("Harass", "Harass Menu");
            {
                _harashMenu.Add("hSoldiersToQ", new Slider("Soldiers to Q", 1, 1, 3));
                _harashMenu.Add("HQ", new CheckBox("Use Q"));
                _harashMenu.Add("HW", new CheckBox("Use W"));
                _harashMenu.Add("HW2", new CheckBox("Save on 1 w for flee"));
            }
        }
        public void LoadJumps()
        {
            _jumpMenu = GetMenu.AddSubMenu("Keys Menu", "Key Menu");
            {
              _jumpMenu.Add("fleekey", new KeyBind("Jump key", false, KeyBind.BindTypes.HoldActive, 'Z'));
              _jumpMenu.Add("inseckey", new KeyBind("Insec key", false, KeyBind.BindTypes.HoldActive, 'T'));
           
            }
        }
    }
    }
