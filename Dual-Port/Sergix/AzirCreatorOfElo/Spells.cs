
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azir_Free_elo_Machine.Math;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK;

namespace Azir_Creator_of_Elo
{
    class Spells
    {
       public LeagueSharp.Common.Spell _q, _w, _e, _r;

        public LeagueSharp.Common.Spell Q
        {
            get { return _q; }
        }

        public LeagueSharp.Common.Spell W
        {
            get { return _w; }
        }

        public LeagueSharp.Common.Spell E
        {
            get { return _e; }
        }

        public LeagueSharp.Common.Spell R
        {
            get { return _r; }
        }

        public Spells()
        {
            _q = new LeagueSharp.Common.Spell(SpellSlot.Q, 825);


            _w = new LeagueSharp.Common.Spell(SpellSlot.W, 450);
            _e = new LeagueSharp.Common.Spell(SpellSlot.E, 1250);
            _r = new LeagueSharp.Common.Spell(SpellSlot.R, 450);

            _q.SetSkillshot(0, 70, 1600, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0, 100, 1700, false, SkillshotType.SkillshotLine);
            _r.SetSkillshot(0.5f, 0, 1400, false, SkillshotType.SkillshotLine);
            //  ignite = ObjectManager.Player.GetSpellSlot("SummonerDot");
        }



      

    }


     internal class StaticSpells
     {
         private static Points _pointer;
        public static void CastQ(AzirMain azir, AIHeroClient target, bool useQ)
        {
           var pointsAttack=new Points[120];
            var points = Azir_Free_elo_Machine.Math.Geometry.PointsAroundTheTarget(target.ServerPosition, 640, 80);
            var i = 0;
           
            foreach (var point in points)
            {
               
                    if (point.Distance(azir.Hero.ServerPosition) <= azir.Spells.Q.Range)
                    {
                        _pointer.hits = Azir_Free_elo_Machine.Math.Geometry.Nattacks(azir, point, target);
                        _pointer.point = point;
                        pointsAttack[i] = _pointer;


                    }
                    i++;
        

            }
            if (pointsAttack.MaxOrDefault(x => x.hits).hits > 0)
            {
                azir.Spells.Q.Cast(pointsAttack.MaxOrDefault(x => x.hits).point);
            }
        }
  
     
    }

}
  