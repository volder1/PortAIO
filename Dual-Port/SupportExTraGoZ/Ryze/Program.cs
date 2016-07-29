/**
 * 
 * Love Ya Lads!
 * 
 * 
 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Data;
using SharpDX;
using SebbyLib;
using Color = System.Drawing.Color;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;

namespace SurvivorRyze
{
    class Program
    {

        #region Declaration
        private static LeagueSharp.Common.Spell Q, W, E, R;
        private static SpellSlot IgniteSlot;
        private static Items.Item HealthPot;
        private static Items.Item BiscuitOfRej;
        private static Items.Item TearOfGod;
        private static Items.Item Seraph;
        private static Items.Item Manamune;
        private static Items.Item Archangel;
        private static Items.Item Flask;
        private static Items.Item Flask1;
        private static Items.Item Flask2;
        private static Items.Item HexProtobelt;
        private static Items.Item HexGunBlade;
        private static Items.Item HexGLP;
        private static Menu menu;
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        private const string ChampionName = "Ryze";
        private static int lvl1, lvl2, lvl3, lvl4;
        private static float RangeR;
        #endregion

        public static Menu ComboMenu, HarassMenu, LaneClearMenu, ItemsMenu, HitChanceMenu, UltimateMenu, MiscMenu, AutoLevelerMenu, DrawingMenu;

        public static void Game_OnGameLoad()
        {
            if (Player.ChampionName != ChampionName)
                return;

            #region Spells
            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 1000f);
            Q.SetSkillshot(0.7f, 55f, float.MaxValue, true, SkillshotType.SkillshotLine);
            W = new LeagueSharp.Common.Spell(SpellSlot.W, 610f);
            W.SetTargetted(0.103f, 550f);
            E = new LeagueSharp.Common.Spell(SpellSlot.E, 610f);
            E.SetTargetted(.5f, 550f);
            R = new LeagueSharp.Common.Spell(SpellSlot.R);
            R.SetSkillshot(2.5f, 450f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            #endregion

            #region Items/SummonerSpells
            IgniteSlot = Player.GetSpellSlot("summonerdot");
            HealthPot = new Items.Item(2003, 0);
            BiscuitOfRej = new Items.Item(2010, 0);
            TearOfGod = new Items.Item(3070, 0);
            Manamune = new Items.Item(3004, 0);
            Seraph = new Items.Item(3040, 0);
            Archangel = new Items.Item(3003, 0);
            Flask = new Items.Item(2031, 0);
            Flask1 = new Items.Item(2032, 0);
            Flask2 = new Items.Item(2033, 0);
            HexGunBlade = new Items.Item(3146, 600f);
            HexProtobelt = new Items.Item(3152, 300f);
            HexGLP = new Items.Item(3030, 300f);
            #endregion

            #region Menu
            menu = MainMenu.AddMenu("SurvivorRyze", "SurvivorRyze");

            ComboMenu = menu.AddSubMenu("Combo", "Combo");
            ComboMenu.Add("ComboMode", new ComboBox("Combo Mode:", 0, "Burst", "Survivor Mode (Shield)"));
            ComboMenu.Add("CUseQ", new CheckBox("Cast Q"));
            ComboMenu.Add("CUseW", new CheckBox("Cast W"));
            ComboMenu.Add("CUseE", new CheckBox("Cast E"));
            ComboMenu.Add("CBlockAA", new CheckBox("Block AA in Combo Mode"));
            ComboMenu.Add("Combo2TimesMana", new CheckBox("Champion needs to have mana for atleast 2 times (Q/W/E)?"));
            ComboMenu.Add("CUseR", new CheckBox("Ultimate (R) in Ultimate Menu"));
            ComboMenu.Add("CUseIgnite", new CheckBox("Use Ignite (Smart)"));

            HarassMenu = menu.AddSubMenu("Harass", "Harass");
            HarassMenu.Add("HarassQ", new CheckBox("Use Q"));
            HarassMenu.Add("HarassW", new CheckBox("Use W", false));
            HarassMenu.Add("HarassE", new CheckBox("Use E", false));
            HarassMenu.Add("HarassManaManager", new Slider("Mana Manager (%)", 30, 1, 100));

            LaneClearMenu = menu.AddSubMenu("Lane Clear", "LaneClear");
            LaneClearMenu.Add("UseQLC", new CheckBox("Use Q to LaneClear"));
            LaneClearMenu.Add("UseELC", new CheckBox("Use E to LaneClear"));
            LaneClearMenu.Add("LaneClearManaManager", new Slider("Mana Manager (%)", 30, 1, 100));

            ItemsMenu = menu.AddSubMenu("Items Menu", "ItemsMenu");
            ItemsMenu.AddGroupLabel("Tear Stacking : ");
            ItemsMenu.Add("StackTear", new CheckBox("Stack Tear in Fountain?"));
            ItemsMenu.Add("StackTearNF", new CheckBox("Stack Tear if Blue Buff?"));
            ItemsMenu.AddGroupLabel("Seraph's Embrace : ");
            ItemsMenu.Add("UseSeraph", new CheckBox("Use [Seraph's Embrace]?"));
            ItemsMenu.Add("UseSeraphIfEnemiesAreNearby", new CheckBox("Use [Seraph's Embrace] only if Enemies are nearby?"));
            ItemsMenu.Add("UseSeraphAtHP", new Slider("Activate [Seraph's Embrace] at HP %?", 15, 0, 100));
            ItemsMenu.AddGroupLabel("Potions : ");
            ItemsMenu.Add("UseHPPotion", new Slider("Use HP Potion/Biscuit/Flask at % Health", 15, 0, 100));
            ItemsMenu.Add("UseItemFlask", new CheckBox("Use Flasks If You don't have Potions?"));
            ItemsMenu.Add("UsePotionOnlyIfEnemiesAreInRange", new CheckBox("Use Potions/Flasks only if Enemies are nearby?"));
            ItemsMenu.AddGroupLabel("Gunblade : ");
            ItemsMenu.Add("UseHexGunBlade", new CheckBox("Use [Hextech Gunblade]?"));
            ItemsMenu.Add("HexGunBladeAtHP", new Slider("Use [Hextech Gunblade] at HP %?", 25, 0, 100));
            ItemsMenu.AddGroupLabel("Protobelt : ");
            ItemsMenu.Add("UseHexProtobelt", new CheckBox("Use [Hextech Protobelt-01]?"));
            ItemsMenu.Add("HexProtobeltAtHP", new Slider("Use [Hextech Protobelt-01] at HP %?", 25, 0, 100));
            ItemsMenu.AddGroupLabel("GLP-800 : ");
            ItemsMenu.Add("UseHexGLP", new CheckBox("Use [Hextech GLP-800]?"));
            ItemsMenu.Add("HexGLPAtHP", new Slider("Use [Hextech GLP-800] at HP %?", 25, 0, 100));

            HitChanceMenu = menu.AddSubMenu("HitChance Menu", "HitChance");
            HitChanceMenu.Add("HitChance", new ComboBox("Hit Chance", 0, "Medium", "High", "Very High"));

            UltimateMenu = menu.AddSubMenu("Ultimate Menu", "UltMenu");
            UltimateMenu.Add("UseR", new KeyBind("Use R Automatically (Beta)", false, KeyBind.BindTypes.HoldActive, 'G'));

            MiscMenu = menu.AddSubMenu("Misc Menu", "MiscMenu");
            MiscMenu.Add("KSQ", new CheckBox("Use Q to KS"));
            MiscMenu.Add("KSW", new CheckBox("Use W to KS"));
            MiscMenu.Add("KSE", new CheckBox("Use E to KS"));
            MiscMenu.Add("InterruptWithW", new CheckBox("Use W to Interrupt Channeling Spells"));
            MiscMenu.Add("WGapCloser", new CheckBox("Use W on Enemy GapCloser (Irelia's Q)"));
            MiscMenu.Add("ChaseWithR", new CheckBox("Use R to Chase (Being Added)"));
            MiscMenu.Add("EscapeWithR", new CheckBox("Use R to Escape (Ultimate Menu)"));

            AutoLevelerMenu = menu.AddSubMenu("AutoLeveler Menu", "AutoLevelerMenu");
            AutoLevelerMenu.Add("AutoLevelUp", new CheckBox("AutoLevel Up Spells?"));
            AutoLevelerMenu.Add("AutoLevelUp1", new ComboBox("First: ", 3, "Q", "W", "E", "R"));
            AutoLevelerMenu.Add("AutoLevelUp2", new ComboBox("Second: ", 0, "Q", "W", "E", "R"));
            AutoLevelerMenu.Add("AutoLevelUp3", new ComboBox("Third: ", 2, "Q", "W", "E", "R"));
            AutoLevelerMenu.Add("AutoLevelUp4", new ComboBox("Fourth: ", 1, "Q", "W", "E", "R"));
            AutoLevelerMenu.Add("AutoLvlStartFrom", new Slider("AutoLeveler Start from Level: ", 2, 1, 6));

            DrawingMenu = menu.AddSubMenu("Drawing", "Drawing");
            DrawingMenu.Add("DrawQ", new CheckBox("Draw Q Range"));
            DrawingMenu.Add("DrawWE", new CheckBox("Draw W/E Range"));
            DrawingMenu.Add("DrawR", new CheckBox("Draw R Range", false));
            #endregion

            #region Subscriptions
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            #endregion
            Chat.Print("<font color='#800040'>[SurvivorSeries] Ryze</font> <font color='#ff6600'>Loaded.</font>");
        }

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

        private static void Drawing_OnDraw(EventArgs args)
        {
            switch(R.Level)
            {
                case 1:
                    RangeR = 1500f;
                    break;
                case 2:
                    RangeR = 3000f;
                    break;
            }
            if (DrawingMenu["DrawQ"].Cast<CheckBox>().CurrentValue)
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
            if (DrawingMenu["DrawWE"].Cast<CheckBox>().CurrentValue)
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.AliceBlue);
            if (DrawingMenu["DrawR"].Cast<CheckBox>().CurrentValue)
                Render.Circle.DrawCircle(Player.Position, RangeR, Color.Orchid);
        }

        private static void AABlock()
        {
            Orbwalker.DisableAttacking = (ComboMenu["CBlockAA"].Cast<CheckBox>().CurrentValue);
            //SebbyLib.OktwCommon.blockAttack = Menu["CBlockAA"].Cast<CheckBox>().CurrentValue;
        }

        private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args)
        {
            if (!sender.IsMe || !AutoLevelerMenu["AutoLevelUp"].Cast<CheckBox>().CurrentValue || ObjectManager.Player.Level < AutoLevelerMenu["AutoLvlStartFrom"].Cast<Slider>().CurrentValue)
                return;
            if (lvl2 == lvl3 || lvl2 == lvl4 || lvl3 == lvl4)
                return;
            int delay = 700;
            LeagueSharp.Common.Utility.DelayAction.Add(delay, () => LevelUp(lvl1));
            LeagueSharp.Common.Utility.DelayAction.Add(delay + 50, () => LevelUp(lvl2));
            LeagueSharp.Common.Utility.DelayAction.Add(delay + 100, () => LevelUp(lvl3));
            LeagueSharp.Common.Utility.DelayAction.Add(delay + 150, () => LevelUp(lvl4));
        }

        private static void LevelUp(int indx)
        {
            if (ObjectManager.Player.Level < 4)
            {
                if (indx == 0 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (indx == 1 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (indx == 2 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
            else
            {
                if (indx == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (indx == 1)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (indx == 2)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (indx == 3)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }

        private static void PotionsCheck()
        {
            if (Player.HasBuffOfType(BuffType.Heal) || Player.HasBuff("ItemMiniRegenPotion") || Player.HasBuff("ItemCrystalFlaskJungle") || Player.HasBuff("ItemCrystalFlask") || Player.HasBuff("ItemDarkCrystalFlask"))
                return;

            if (Player.CountEnemiesInRange(1250) > 0)
            {
                if (!Player.InFountain() || !Player.IsRecalling())
                {
                    if (Player.HealthPercent < ItemsMenu["UseHPPotion"].Cast<Slider>().CurrentValue && (!Player.HasBuff("HealthPotion") || !Player.HasBuff("Health Potion") || !Player.HasBuff("ItemDarkCrystalFlask") || !Player.HasBuff("ItemCrystalFlask") || !Player.HasBuff("ItemCrystalFlaskJungle") || !Player.HasBuff("ItemMiniRegenPotion")))
                    {
                        HealthPot.Cast();
                    }
                    if (Player.HealthPercent < ItemsMenu["UseHPPotion"].Cast<Slider>().CurrentValue && (!Player.HasBuff("ItemCrystalFlaskJungle") || !Player.HasBuff("ItemDarkCrystalFlask") || !Player.HasBuff("ItemCrystalFlask") || !Player.HasBuff("HealthPotion") || !Player.HasBuff("Health Potion") || !Player.HasBuff("ItemMiniRegenPotion")))
                    {
                        BiscuitOfRej.Cast();
                    }
                    if (ItemsMenu["UseItemFlask"].Cast<CheckBox>().CurrentValue && !Player.HasBuff("ItemCrystalFlaskJungle") || !Player.HasBuff("ItemCrystalFlask") || !Player.HasBuff("ItemDarkCrystalFlask") || !Player.HasBuff("HealthPotion") || !Player.HasBuff("Health Potion") || !Player.HasBuff("ItemMiniRegenPotion"))
                    {
                        Flask.Cast();
                        Flask1.Cast();
                        Flask2.Cast();
                    }
                }
            }
        }

        private static void ItemsChecks()
        {
            // Check when you can use items (potions, ex) && Cast them (Probelt Usage please)
            if (Player.HealthPercent < ItemsMenu["UseSeraphAtHP"].Cast<Slider>().CurrentValue && ItemsMenu["UseSeraph"].Cast<CheckBox>().CurrentValue && !Player.InFountain() || !Player.IsRecalling())
            {
                if (Player.HealthPercent < ItemsMenu["UseSeraphAtHP"].Cast<Slider>().CurrentValue && ItemsMenu["UseSeraph"].Cast<CheckBox>().CurrentValue && Player.CountEnemiesInRange(1000) > 0 && ItemsMenu["UseSeraphIfEnemiesAreNearby"].Cast<CheckBox>().CurrentValue)
                {
                        Seraph.Cast();
                }
            }
            var target = TargetSelector.GetTarget(600f, DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if (target == null || !target.IsValidTarget(600f) || target.IsInvulnerable)
                return;

            if (ItemsMenu["UseHexGunBlade"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(600) && target.HealthPercent < ItemsMenu["HexGunBladeAtHP"].Cast<Slider>().CurrentValue)
            {
                Items.UseItem(3146, target);
            }
            if (ItemsMenu["UseHexProtobelt"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(300) && target.HealthPercent < ItemsMenu["HexProtobeltAtHP"].Cast<Slider>().CurrentValue)
            {
                Items.UseItem(3152, target.Position);
            }
            if (ItemsMenu["UseHexGLP"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(300) && target.HealthPercent < ItemsMenu["HexGLPAtHP"].Cast<Slider>().CurrentValue)
            {
                Items.UseItem(3030, target.Position);
            }
        }

        private static void StackItems()
        {
            if (Player.InFountain() || Player.HasBuff("CrestoftheAncientGolem") && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None)) && ItemsMenu["StackTearNF"].Cast<CheckBox>().CurrentValue) // Add if Player has Blue Buff
            {
                if (Items.HasItem(3004, Player) || Items.HasItem(3003, Player) || Items.HasItem(3070, Player) || Items.HasItem(3072, Player) || Items.HasItem(3073, Player) || Items.HasItem(3008, Player))
                {
                    Q.Cast(Player.ServerPosition);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
                return;

            if (ItemsMenu["StackTear"].Cast<CheckBox>().CurrentValue)
            {
                StackItems();
            }
            ItemsChecks();
            KSCheck();
            PotionsCheck();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                AABlock();
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None))
            {
                Orbwalker.DisableAttacking = false;
                Orbwalker.DisableMovement = false;
            }
            
            if (UltimateMenu["UseR"].Cast<KeyBind>().CurrentValue)
            {
                Orbwalker.DisableAttacking = true;
                Orbwalker.DisableMovement = true;
                REscape();
            }
            //AutoLeveler
            if (AutoLevelerMenu["AutoLevelUp"].Cast<CheckBox>().CurrentValue)
            {
                lvl1 = AutoLevelerMenu["AutoLevelUp1"].Cast<ComboBox>().CurrentValue;
                lvl2 = AutoLevelerMenu["AutoLevelUp2"].Cast<ComboBox>().CurrentValue;
                lvl3 = AutoLevelerMenu["AutoLevelUp3"].Cast<ComboBox>().CurrentValue;
                lvl4 = AutoLevelerMenu["AutoLevelUp4"].Cast<ComboBox>().CurrentValue;
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!MiscMenu["WGapCloser"].Cast<CheckBox>().CurrentValue || Player.Mana < W.Instance.SData.Mana + Q.Instance.SData.Mana)
                return;

            var t = gapcloser.Sender;

            if (gapcloser.End.Distance(Player.ServerPosition) < W.Range)
            {
                W.Cast(t);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient t, Interrupter2.InterruptableTargetEventArgs args)
        {
            var WCast = MiscMenu["InterruptWithW"].Cast<CheckBox>().CurrentValue;
            if (!WCast || !t.IsValidTarget(W.Range) || !W.IsReady()) return;
            W.Cast(t);
        }

        private static void KSCheck()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if (target == null || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            var ksQ = MiscMenu["KSQ"].Cast<CheckBox>().CurrentValue;
            var ksW = MiscMenu["KSW"].Cast<CheckBox>().CurrentValue;
            var ksE = MiscMenu["KSE"].Cast<CheckBox>().CurrentValue;

            #region SebbyPrediction
            //SebbyPrediction
            SebbyLib.Prediction.SkillshotType PredSkillShotType = SebbyLib.Prediction.SkillshotType.SkillshotLine;
            bool Aoe10 = false;

            var predictioninput = new SebbyLib.Prediction.PredictionInput
            {
                Aoe = Aoe10,
                Collision = Q.Collision,
                Speed = Q.Speed,
                Delay = Q.Delay,
                Range = Q.Range,
                From = Player.ServerPosition,
                Radius = Q.Width,
                Unit = target,
                Type = PredSkillShotType
            };
            //SebbyPrediction END
            #endregion
            // Input = 'var predictioninput'
            var predpos = SebbyLib.Prediction.Prediction.GetPrediction(predictioninput);

            // KS
            if (ksQ && SebbyLib.OktwCommon.GetKsDamage(target, Q) > target.Health && target.IsValidTarget(Q.Range))
            {
                if (target.CanMove && predpos.Hitchance >= SebbyLib.Prediction.HitChance.High)
                {
                    Q.Cast(predpos.CastPosition);
                }
                else if (!target.CanMove)
                {
                    Q.Cast(target.Position);
                }
            }
            if (ksW && SebbyLib.OktwCommon.GetKsDamage(target, W) > target.Health && target.IsValidTarget(W.Range))
            {
                W.CastOnUnit(target);
            }
            if (ksE && SebbyLib.OktwCommon.GetKsDamage(target, E) > target.Health && target.IsValidTarget(E.Range))
            {
                E.CastOnUnit(target);
            }
        }

        public static bool RyzeCharge0()
        {
            return Player.HasBuff("ryzeqiconnocharge");
        }

        public static bool RyzeCharge1()
        {
            return Player.HasBuff("ryzeqiconhalfcharge");
        }

        public static bool RyzeCharge2()
        {
            return Player.HasBuff("ryzeqiconfullcharge");
        }

        private static void SebbySpell(LeagueSharp.Common.Spell QR, Obj_AI_Base target)
        {
            SebbyLib.Prediction.SkillshotType CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotLine;
            bool aoe2 = false;

            if (QR.Type == SkillshotType.SkillshotCircle)
            {
                CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotCircle;
                aoe2 = true;
            }

            if (QR.Width > 80 && !QR.Collision)
                aoe2 = true;

            var predInput2 = new SebbyLib.Prediction.PredictionInput
            {
                Aoe = aoe2,
                Collision = QR.Collision,
                Speed = QR.Speed,
                Delay = QR.Delay,
                Range = QR.Range,
                From = Player.ServerPosition,
                Radius = QR.Width,
                Unit = target,
                Type = CoreType2
            };
            var poutput2 = SebbyLib.Prediction.Prediction.GetPrediction(predInput2);

            if (QR.Speed != float.MaxValue && OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                return;

            if (HitChanceMenu["HitChance"].Cast<ComboBox>().CurrentValue == 0)
            {
                if (poutput2.Hitchance >= SebbyLib.Prediction.HitChance.Medium)
                    QR.Cast(poutput2.CastPosition);
            }
            else if (HitChanceMenu["HitChance"].Cast<ComboBox>().CurrentValue == 1)
            {
                if (poutput2.Hitchance >= SebbyLib.Prediction.HitChance.High)
                    QR.Cast(poutput2.CastPosition);

            }
            else if (HitChanceMenu["HitChance"].Cast<ComboBox>().CurrentValue == 2)
            {
                if (poutput2.Hitchance >= SebbyLib.Prediction.HitChance.VeryHigh)
                    QR.Cast(poutput2.CastPosition);
            }
        }

        private static void Combo()
        {
            // Combo
            var CUseQ = ComboMenu["CUseQ"].Cast<CheckBox>().CurrentValue;
            var CUseW = ComboMenu["CUseW"].Cast<CheckBox>().CurrentValue;
            var CUseE = ComboMenu["CUseE"].Cast<CheckBox>().CurrentValue;
            // Checks
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if (target == null || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            switch (ComboMenu["ComboMode"].Cast<ComboBox>().CurrentValue)
            {
                case 0:
                    #region Burst Mode
                    if (ComboMenu["Combo2TimesMana"].Cast<CheckBox>().CurrentValue)
                    {
                        if (Player.Mana >= 2 * (Q.Instance.SData.Mana + W.Instance.SData.Mana + E.Instance.SData.Mana))
                        {
                            if (CUseQ && CUseW && CUseE && target.IsValidTarget(Q.Range))
                            {
                                if (target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                                else if (!target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                                if (target.IsValidTarget(W.Range) && W.IsReady())
                                {
                                    W.CastOnUnit(target);
                                }
                                if (target.IsValidTarget(E.Range) && E.IsReady())
                                {
                                    E.CastOnUnit(target);
                                }
                            }
                            if (CUseW && target.IsValidTarget(W.Range) && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (CUseQ && target.IsValidTarget(Q.Range))
                            {
                                if (target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                                else if (!target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                            }
                            if (CUseE && target.IsValidTarget(E.Range) && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }
                    }
                    else
                    {
                        if (Player.Mana >= Q.Instance.SData.Mana + W.Instance.SData.Mana + E.Instance.SData.Mana)
                        {
                            if (CUseW && target.IsValidTarget(W.Range) && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (CUseQ && target.IsValidTarget(Q.Range))
                            {
                                if (target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                                else if (!target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                            }
                            if (CUseE && target.IsValidTarget(E.Range) && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }
                        else
                        {
                            if (CUseW && target.IsValidTarget(W.Range) && W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            if (CUseQ && target.IsValidTarget(Q.Range))
                            {
                                if (target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                                else if (!target.CanMove)
                                {
                                    SebbySpell(Q, target);
                                }
                            }
                            if (CUseE && target.IsValidTarget(E.Range) && E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }
                    }
                    #endregion
                    break;

                case 1:
                    #region SurvivorMode
                    if (Q.Level >= 1 && W.Level >= 1 && E.Level >= 1)
                    {
                        if (!target.IsValidTarget(W.Range - 15f) && Q.IsReady())
                        {
                            if (target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                            else if (!target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                        }
                        // Try having Full Charge if either W or E spells are ready... :pokemon:
                        if (RyzeCharge1() && Q.IsReady() && (W.IsReady() || E.IsReady()))
                        {
                            if (E.IsReady())
                            {
                                E.Cast(target);
                            }
                            if (W.IsReady())
                            {
                                W.Cast(target);
                            }
                        }
                        // Rest in Piece XDDD
                        if (RyzeCharge1() && !E.IsReady() && !W.IsReady())
                        {
                            if (target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                            else if (!target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                        }

                        if (RyzeCharge0() && !E.IsReady() && !W.IsReady())
                        {
                            if (target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                            else if (!target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                        }

                        if (!RyzeCharge2())
                        {
                            E.Cast(target);
                            W.Cast(target);
                        }
                        else
                        {
                            if (target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                            else if (!target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                        }
                    }
                    else
                    {
                        if (target.IsValidTarget(Q.Range) && Q.IsReady())
                        {
                            if (target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                            else if (!target.CanMove)
                            {
                                SebbySpell(Q, target);
                            }
                        }

                        if (target.IsValidTarget(W.Range) && W.IsReady())
                        {
                            W.Cast(target);
                        }

                        if (target.IsValidTarget(E.Range) && E.IsReady())
                        {
                            E.Cast(target);
                        }
                    }
                    #endregion
                    break;
            }
        }

        private static void Harass()
        {
            // Harass
            var HarassUseQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var HarassUseW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var HarassUseE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            // Checks
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if (target == null || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            // Execute the Lad
            if (Player.ManaPercent > HarassMenu["HarassManaManager"].Cast<Slider>().CurrentValue)
            {
                if (HarassUseW && target.IsValidTarget(W.Range))
                {
                    W.CastOnUnit(target);
                }
                if (HarassUseQ && target.IsValidTarget(Q.Range))
                {
                    if (target.CanMove)
                    {
                        SebbySpell(Q, target);
                    }
                    else if (!target.CanMove)
                    {
                        SebbySpell(Q, target);
                    }
                }
                if (HarassUseE && target.IsValidTarget(W.Range))
                {
                    E.CastOnUnit(target);
                }
            }
        }

        private static void LastHit()
        {
            // To be Done
            if (Player.ManaPercent > LaneClearMenu["LaneClearManaManager"].Cast<Slider>().CurrentValue)
            {
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Enemy);
                if (Q.IsReady())
                {
                    if (allMinionsQ.Count > 0)
                    {
                        foreach (var minion in allMinionsQ)
                        {
                            if (!minion.IsValidTarget() || minion == null)
                                return;
                            if (minion.Health < Q.GetDamage(minion))
                                Q.Cast(minion.Position);
                            else if (minion.Health < Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) && minion.IsValidTarget(SebbyLib.Orbwalking.GetRealAutoAttackRange(minion)))
                            {
                                Q.Cast(minion.Position);
                                Orbwalker.ForcedTarget = (minion);
                            }
                        }
                    }
                }
            }
        }

        private static void LaneClear()
        {
            // LaneClear | Notes: Rework on early levels not using that much abilities since Spell Damage is lower, higher Lvl is fine
            if (LaneClearMenu["UseQLC"].Cast<CheckBox>().CurrentValue || LaneClearMenu["UseELC"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.ManaPercent> LaneClearMenu["LaneClearManaManager"].Cast<Slider>().CurrentValue)
                {
                    var ryzeebuffed = MinionManager.GetMinions(Player.Position, Q.Range).Find(x => x.HasBuff("RyzeE") && x.IsValidTarget(Q.Range));
                    var ryzenotebuffed = MinionManager.GetMinions(Player.Position, Q.Range).Find(x => !x.HasBuff("RyzeE") && x.IsValidTarget(Q.Range));
                    var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Enemy);
                    var allMinions = Cache.GetMinions(Player.ServerPosition, E.Range, MinionTeam.Enemy);
                    if (Q.IsReady() && !E.IsReady())
                    {
                        if (allMinionsQ.Count > 0)
                        {
                            foreach (var minion in allMinionsQ)
                            {
                                if (!minion.IsValidTarget() || minion == null)
                                    return;
                                if (minion.Health < Q.GetDamage(minion))
                                    Q.Cast(minion);
                                else if (minion.Health < Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) && minion.IsValidTarget(SebbyLib.Orbwalking.GetRealAutoAttackRange(minion)))
                                {
                                    Q.Cast(minion);
                                    Orbwalker.ForcedTarget = (minion);
                                }
                            }
                        }
                    }
                    if (!Q.IsReady() && E.IsReady())
                    {
                        if (ryzeebuffed != null)
                        {
                            if (ryzeebuffed.Health < E.GetDamage(ryzeebuffed) + Q.GetDamage(ryzeebuffed) && ryzeebuffed.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(ryzeebuffed);
                                if (Q.IsReady())
                                    Q.Cast(ryzeebuffed);

                                Orbwalker.ForcedTarget = (ryzeebuffed);
                            }
                        }
                        else if (ryzeebuffed == null)
                        {
                            foreach (var minion in allMinions)
                            {
                                if (minion.IsValidTarget(E.Range) && minion.Health < E.GetDamage(minion) + Q.GetDamage(minion))
                                {
                                    E.CastOnUnit(minion);
                                    if (Q.IsReady())
                                        Q.Cast(ryzeebuffed);
                                }
                            }
                        }
                    }
                    if (Q.IsReady() && E.IsReady())
                    {
                        if (ryzeebuffed != null)
                        {
                            if (ryzeebuffed.Health < Q.GetDamage(ryzeebuffed) + E.GetDamage(ryzeebuffed) + Q.GetDamage(ryzeebuffed) && ryzeebuffed.IsValidTarget(E.Range))
                            {
                                Q.Cast(ryzeebuffed);
                                if (ryzeebuffed.IsValidTarget(E.Range))
                                {
                                    E.CastOnUnit(ryzeebuffed);
                                }
                                if (!E.IsReady() && Q.IsReady())
                                    Q.Cast(ryzeebuffed);
                            }
                        }
                        else if (ryzeebuffed == null)
                        {
                            Q.Cast(ryzeebuffed);
                            if (ryzenotebuffed.IsValidTarget(E.Range))
                            {
                                Orbwalker.ForcedTarget = (ryzenotebuffed);
                                E.CastOnUnit(ryzenotebuffed);
                            }
                            if (!E.IsReady() && Q.IsReady())
                                Q.Cast(ryzenotebuffed);
                        }
                    }
                }
            }
        } // LaneClear End

        private static void REscape()
        {
            switch (R.Level)
            {
                case 1:
                    RangeR = 1500f;
                    break;
                case 2:
                    RangeR = 3000f;
                    break;
            }
            var NearByTurrets = ObjectManager.Get<Obj_AI_Turret>().Find(turret => turret.Distance(Player) < RangeR && turret.IsAlly);
            if (NearByTurrets != null)
            {
                R.Cast(NearByTurrets.Position);
            }
        }

        //RUsage

        private static float CalculateDamage(Obj_AI_Base enemy)
        {
            float damage = 0;
            if (Q.IsReady() || Player.Mana <= Q.Instance.SData.Mana + Q.Instance.SData.Mana)
                damage += Q.GetDamage(enemy) + Q.GetDamage(enemy);
            else if (Q.IsReady() || Player.Mana <= Q.Instance.SData.Mana)
                damage += Q.GetDamage(enemy);

            if (W.IsReady() || Player.Mana <= W.Instance.SData.Mana + W.Instance.SData.Mana)
                damage += W.GetDamage(enemy) + W.GetDamage(enemy);
            else if (W.IsReady() || Player.Mana <= W.Instance.SData.Mana)
                damage += W.GetDamage(enemy);

            if (E.IsReady() || Player.Mana <= E.Instance.SData.Mana + E.Instance.SData.Mana)
                damage += E.GetDamage(enemy) + E.GetDamage(enemy);
            else if (E.IsReady() || Player.Mana <= E.Instance.SData.Mana)
                damage += E.GetDamage(enemy);

            if (ComboMenu["CUseIgnite"].Cast<CheckBox>().CurrentValue)
            {
                damage += (float)Player.GetSummonerSpellDamage(enemy, LeagueSharp.Common.Damage.SummonerSpell.Ignite);
            }

            return damage;
        }
    }
}