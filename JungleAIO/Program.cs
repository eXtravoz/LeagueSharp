using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JungleAIO.Champions;

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
                    new Nocturne();
                    Game.PrintChat("Loaded - [JungleAIO]");
                    Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                    break;
                    
                case "Pantheon":
                    new Pantheon();
                    Game.PrintChat("Loaded - [JungleAIO]");
                    Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                    break;

                case "Nautilus":
                    new Nautilus();
                    Game.PrintChat("Loaded - [JungleAIO]");
                    Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                    break;
                    
                    default:
                    Game.PrintChat("[JungleAIO] - Champion Not Supported!");
                    break;

                /* case "Nautilus":
                    new Nautilus();
                    Game.PrintChat("Loaded - [JungleAIO]");
                    Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                    break;
                    */

             // case "ChampionName":
                 // new Champions.ChampionName();
                 // Game.PrintChat("Loaded - [JungleAIO]");
                 // Game.PrintChat("[JungleAIO] - " + ObjectManager.Player.ChampionName);
                 // break;
            }

            
        }
    }
}
