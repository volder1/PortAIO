using System;
using System.Linq;
using EloBuddy;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Menu;

 namespace SephCassiopeia
{
    class CassioUtils
    {
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

        public static EloBuddy.SDK.Enumerations.HitChance RChance()
        {
            switch (getBoxItem(CassiopeiaMenu.hc, "Hitchance.R"))
            {
                case 0:
                    return EloBuddy.SDK.Enumerations.HitChance.Low;
                case 1:
                    return EloBuddy.SDK.Enumerations.HitChance.Medium;
                case 2:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                case 3:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                default:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
            }
        }
        public static EloBuddy.SDK.Enumerations.HitChance WChance()
        {
            switch (getBoxItem(CassiopeiaMenu.hc, "Hitchance.W"))
            {
                case 0:
                    return EloBuddy.SDK.Enumerations.HitChance.Low;
                case 1:
                    return EloBuddy.SDK.Enumerations.HitChance.Medium;
                case 2:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                case 3:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                default:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
            }
        }
        public static EloBuddy.SDK.Enumerations.HitChance QChance()
        {
            switch (getBoxItem(CassiopeiaMenu.hc, "Hitchance.Q"))
            {
                case 0:
                    return EloBuddy.SDK.Enumerations.HitChance.Low;
                case 1:
                    return EloBuddy.SDK.Enumerations.HitChance.Medium;
                case 2:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                case 3:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                default:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
            }
        }

        private static AIHeroClient Player = Cassiopeia.Player;

        public static bool isHealthy()
        {
            return Player.HealthPercent > 25;
        }

        public static bool PointUnderEnemyTurret(Vector3 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy && Vector3.Distance(t.Position, Point) < 900f);
            return EnemyTurrets.Any();
        }

        public static bool PointUnderAllyTurret(Vector3 Point)
        {
            var AllyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsAlly && Vector3.Distance(t.Position, Point) < 900f);
            return AllyTurrets.Any();
        }


    }
}
