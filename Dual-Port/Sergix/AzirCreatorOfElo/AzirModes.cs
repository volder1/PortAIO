using Azir_Free_elo_Machine;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azir_Creator_of_Elo
{
    class AzirModes : Modes
    {
        public readonly JumpLogic _jump;
        private Insec _insec;


        public AzirModes(AzirMain azir)
        {
            _jump = new JumpLogic(azir);
            _insec = new Insec(azir);
        }

        public override void Update(AzirMain azir)
        {

            base.Update(azir);



            if (Menu._jumpMenu["fleekey"].Cast<KeyBind>().CurrentValue || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                azir.Orbwalk(Game.CursorPos);
                Jump(azir);
            }


        }

        public void Jump(AzirMain azir)
        {
            _jump.updateLogic(Game.CursorPos);
        }

        public override void Harash(AzirMain azir)
        {
            var wCount = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Ammo;
            var useQ = Menu._harashMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = Menu._harashMenu["HW"].Cast<CheckBox>().CurrentValue;
            var savew = Menu._harashMenu["HW2"].Cast<CheckBox>().CurrentValue;
            base.Harash(azir);
            var target = TargetSelector.GetTarget(900, DamageType.Magical);
            if (target != null)
            {
                if (target.Distance(azir.Hero.ServerPosition) < 450)
                {
                    var pred = azir.Spells.W.GetPrediction(target);
                    if (pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                    {
                        if (savew && (wCount == 1))
                        {

                        }
                        else
                        {
                            if (useW)
                                if (azir.Spells.W.IsReady())
                                    azir.Spells.W.Cast(pred.CastPosition);
                        }
                    }
                }
                else
                {
                    if (!savew || (wCount != 1))
                    {
                        if (useW)
                            azir.Spells.W.Cast(azir.Hero.Position.Extend(target.ServerPosition, 450));
                    }

                }
                var checksQ = azir.soldierManager.ChecksToCastQ(azir, target);
                if (checksQ)
                    StaticSpells.CastQ(azir, target, useQ);

            }
        }

        public override void Laneclear(AzirMain azir)
        {
            var useQ = Menu._laneClearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var useW = Menu._laneClearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var minToW = Menu._laneClearMenu["LWM"].Cast<Slider>().CurrentValue;
            base.Laneclear(azir);

            // wpart
            List<Obj_AI_Base> minionW =
   MinionManager.GetMinions(
                           azir.Hero.Position,
                            azir.Spells.W.Range,
                            MinionTypes.All,
                            MinionTeam.NotAlly,
                            MinionOrderTypes.MaxHealth);
            if (minionW != null && useW)
            {
                MinionManager.FarmLocation wFarmLocation = azir.Spells.W.GetCircularFarmLocation(minionW,
                    315);
                if (wFarmLocation.MinionsHit >= minToW)
                {
                    azir.Spells.W.Cast(wFarmLocation.Position);
                }
            }
            List<Obj_AI_Base> minionQ =
MinionManager.GetMinions(
                      azir.Hero.Position,
                       azir.Spells.Q.Range,
                       MinionTypes.All,
                       MinionTeam.NotAlly,
                       MinionOrderTypes.MaxHealth);
            if (minionQ != null && useQ && azir.soldierManager.CheckQCastAtLaneClear(minionQ, azir))
            {

                MinionManager.FarmLocation wFarmLocation = azir.Spells.Q.GetCircularFarmLocation(minionW,
                    315);
                if (wFarmLocation.MinionsHit >= minToW)
                {
                    azir.Spells.Q.Cast(wFarmLocation.Position);
                }
            }
        }

        public override void Jungleclear(AzirMain azir)
        {
            var useW = Menu._JungleClearMenu["JW"].Cast<CheckBox>().CurrentValue;
            base.Jungleclear(azir);
            var minion =
                MinionManager.GetMinions(azir.Spells.Q.Range, MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (minion == null) return;


            if (azir.Spells.W.IsInRange(minion))
            {
                var pred = azir.Spells.W.GetPrediction(minion);
                if (pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                {
                    if (useW)
                        azir.Spells.W.Cast(pred.CastPosition);
                }


            }
        }

        public override void Combo(AzirMain azir)
        {

            var useQ = Menu._comboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var useW = Menu._comboMenu["CW"].Cast<CheckBox>().CurrentValue;
            base.Combo(azir);
            var target = TargetSelector.GetTarget(900, DamageType.Magical);
            if (target == null) return;

            if (target.Distance(azir.Hero.ServerPosition) < 450)
            {
                if (target.isRunningOfYou())
                {
                    var pos = LeagueSharp.Common.Prediction.GetPrediction(target, 0.5f).UnitPosition;
                    azir.Spells.W.Cast(pos);
                }
                else
                {
                    var pred = azir.Spells.W.GetPrediction(target);
                    if (pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                    {
                        if (useW)
                            azir.Spells.W.Cast(pred.CastPosition);
                    }
                }
            }
            else
            {
                if (azir.Spells.Q.Level > 0 && azir.Spells.Q.IsReady())
                    if (useW)
                        if (target.Distance(HeroManager.Player) <= 750)
                            azir.Spells.W.Cast(azir.Hero.Position.Extend(target.ServerPosition, 450));
            }
            //Qc casting
            var checksQ = azir.soldierManager.ChecksToCastQ(azir, target);
            if (checksQ)
            {
                StaticSpells.CastQ(azir, target, useQ);
            }




            else if (azir.Spells.R.IsKillable(target))
            {
                if (Menu._comboMenu["CR"].Cast<CheckBox>().CurrentValue)
                {
                    if (target.Health < azir.Spells.R.GetDamage(target))
                    {
                        var pred = azir.Spells.R.GetPrediction(target);
                        if (pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                        {

                            azir.Spells.R.Cast(pred.CastPosition);
                        }
                    }
                    //      azir.Spells.R.Cast(target);

                }
            }
        }


    }
}