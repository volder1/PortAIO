using Azir_Free_elo_Machine;
using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;

namespace Azir_Creator_of_Elo
{
    internal class AzirModes : Modes
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
            RAllyTower(azir);

            if (!AzirMenu._jumpMenu["fleekey"].Cast<KeyBind>().CurrentValue) return;
            azir.Orbwalk(Game.CursorPos);
            Jump(azir);
        }

        private void RAllyTower(AzirMain azir)
        {

            var useR = AzirMenu._miscMenu["ARUT"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(1100, DamageType.Magical);
            if(useR)
            if (azir.Hero.Distance(target) < 220)
            {
           
                           var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(it =>it.IsAlly&& it.IsValidTarget(1000));
                if (tower != null)
                {
                    azir.Spells.R.Cast(tower.Position);
                }
            }
        }

        public void Jump(AzirMain azir)
        {
            _jump.updateLogic(Game.CursorPos);
        }

        public override void Harash(AzirMain azir)
        {
            var wCount = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Ammo;
            var useQ = AzirMenu._harashMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = AzirMenu._harashMenu["HW"].Cast<CheckBox>().CurrentValue;
            var savew = AzirMenu._harashMenu["HW2"].Cast<CheckBox>().CurrentValue;
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
                var checksQ = azir.SoldierManager.ChecksToCastQ(azir, target);
                if (checksQ)
                    StaticSpells.CastQ(azir, target, useQ);

            }
        }

        public override void Laneclear(AzirMain azir)
        {
            var useQ = AzirMenu._laneClearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var useW = AzirMenu._laneClearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var minToW = AzirMenu._laneClearMenu["LWM"].Cast<Slider>().CurrentValue;
            base.Laneclear(azir);
     
            // wpart
           var  minionW =
   MinionManager.GetMinions(
                           azir.Hero.Position,
                            azir.Spells.W.Range,
                            MinionTypes.All,
                            MinionTeam.NotAlly,
                            MinionOrderTypes.MaxHealth);
            if (minionW != null&&useW)
            {
                var wFarmLocation = azir.Spells.W.GetCircularFarmLocation(minionW,
                    315);
                if (wFarmLocation.MinionsHit >= minToW)
                {
                    var closestSoldier = azir.SoldierManager.getClosestSolider(wFarmLocation.Position.To3D());
                    if (closestSoldier == null)
                    {
                        azir.Spells.W.Cast(wFarmLocation.Position);
                    }
                    else if (wFarmLocation.Position.Distance(closestSoldier) >= 300)
                    {
                        azir.Spells.W.Cast(wFarmLocation.Position);
                    }
           
                }
            }
           var minionQ =
MinionManager.GetMinions(
                      azir.Hero.Position,
                       azir.Spells.Q.Range,
                       MinionTypes.All,
                       MinionTeam.NotAlly,
                       MinionOrderTypes.MaxHealth);
            if (minionQ == null || !useQ || !azir.SoldierManager.CheckQCastAtLaneClear(minionQ, azir)) return;
            {
                var wFarmLocation = azir.Spells.Q.GetCircularFarmLocation(minionW,
                    315);
                foreach (var objAiBase in   minionQ)
                {
                    var minion = (Obj_AI_Minion) objAiBase;
                    var closestSoldier = azir.SoldierManager.getClosestSolider(minion.ServerPosition);
                    if(closestSoldier!=null)
                        if (minion.Distance(closestSoldier) > 315)
                        {
                            azir.Spells.Q.Cast(minion.Position);
                            break;
                        }
                }
            }
        }
        
        public override void Jungleclear(AzirMain azir)
        {
            var useW = AzirMenu._JungleClearMenu["JQ"].Cast<CheckBox>().CurrentValue;
            var useQ = AzirMenu._JungleClearMenu["JW"].Cast<CheckBox>().CurrentValue;
            base.Jungleclear(azir);
            var minionW =
MinionManager.GetMinions(
                   azir.Hero.Position,
                    azir.Spells.W.Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth);
            if (minionW != null && useW)
            {
                var wFarmLocation = azir.Spells.W.GetCircularFarmLocation(minionW,
                    315);
                if (wFarmLocation.MinionsHit>=1)
                {
                    azir.Spells.W.Cast(wFarmLocation.Position);
                }
            }
            var minionQ =
MinionManager.GetMinions(
                      azir.Hero.Position,
                       azir.Spells.Q.Range,
                       MinionTypes.All,
                       MinionTeam.Neutral,
                       MinionOrderTypes.MaxHealth);
            if (minionQ == null || !useQ || !azir.SoldierManager.CheckQCastAtLaneClear(minionQ, azir)) return;
            {
                var wFarmLocation = azir.Spells.Q.GetCircularFarmLocation(minionW,
                    315);
                foreach (var objAiBase in minionQ)
                {
                    var minion = (Obj_AI_Minion) objAiBase;
                    var closestSoldier = azir.SoldierManager.getClosestSolider(minion.ServerPosition);
                    if (closestSoldier == null) continue;
                    if (!(minion.Distance(closestSoldier) > 315)) continue;
                    azir.Spells.Q.Cast(minion.Position);
                    break;
                }
            }
        }
        

        public override void Combo(AzirMain azir)
        {

            var useQ = AzirMenu._comboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var useW = AzirMenu._comboMenu["CW"].Cast<CheckBox>().CurrentValue;
            base.Combo(azir);
            var target = TargetSelector.GetTarget(900, DamageType.Magical);
            if (target == null) return;

            if (target.Distance(azir.Hero.ServerPosition) < 450)
            {
                if (target.IsRunningOfYou())
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
            var checksQ = azir.SoldierManager.ChecksToCastQ(azir, target);
            if (checksQ)
            {
                StaticSpells.CastQ(azir, target, useQ);
            }




            else if (azir.Spells.R.IsKillable(target))
            {
                if (!AzirMenu._comboMenu["CR"].Cast<CheckBox>().CurrentValue) return;
                if (!(target.Health < azir.Spells.R.GetDamage(target))) return;
                var pred = azir.Spells.R.GetPrediction(target);
                if (pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                {

                    azir.Spells.R.Cast(pred.CastPosition);
                }
                //      azir.Spells.R.Cast(target);
            }
        }


    }
}
