using LeagueSharp.SDK;
using PrideStalker_Rengar.Main;
using System.Linq;
using LeagueSharp.SDK.Enumerations;

namespace PrideStalker_Rengar.Handlers
{
    class KillSteal : Core
    {

        public static void Killsteal()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.LSIsValidTarget(800) && !x.IsDead && !x.IsZombie).Where(target => target.LSIsValidTarget()))
            {
                if (Spells.E.IsReady() && target.Health < Spells.E.GetDamage(target))
                {
                    Spells.E.CastIfHitchanceEquals(target, HitChance.High);
                }
                if (Spells.W.IsReady() && target.Health < Spells.W.GetDamage(target))
                {
                    Spells.W.Cast(target);
                }
            }

            if (!MenuConfig.KillStealSummoner) return;

            foreach (var target in GameObjects.EnemyHeroes.Where(t => t.LSIsValidTarget(600f)).Where(target => target.Health < Dmg.IgniteDmg && Spells.Ignite.IsReady()))
            {
                GameObjects.Player.Spellbook.CastSpell(Spells.Ignite, target);
            }
        }
    }
}