using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using EloBuddy;

namespace LCS_LeBlanc
{
    class Program
    {
        public static void OnLoad()
        {
            if (ObjectManager.Player.ChampionName != "Leblanc")
            {
                return;
            }
            else
            {
                new LeBlanc();
            }
        }
    }
}
