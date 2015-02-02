using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;

using Color = System.Drawing.Color;

namespace JungleAIO
{
    class Program
    {
        static void Main(string[] args)
        {

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        static void Game_OnGameLoad(EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Nocturne":
                    new Champions.Nocturne();
                    Game.PrintChat("Loaded - [JungleAIO]");
                    Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                    break;

             // case "ChampionName":
                 // new Champions.ChampionName();
                 // Game.PrintChat("Loaded - [JungleAIO]");
                 // Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                 // break;
            }

            
        }
    }
}
