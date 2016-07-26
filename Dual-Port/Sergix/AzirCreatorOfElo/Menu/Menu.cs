using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azir_Free_elo_Machine;

namespace Azir_Creator_of_Elo
{
    class Menu
    {
        public EloBuddy.SDK.Menu.Menu GetMenu { get; private set; }
        private string _menuName;

        public Menu(string menuName)
        {
            this._menuName = menuName;
        }

        public virtual void LoadMenu(AzirMain azir)
        {
            GetMenu = EloBuddy.SDK.Menu.MainMenu.AddMenu(_menuName, _menuName);
        }
    }
}
