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
                    Game.PrintChat("<font color='0088FF'> Loaded - [JungleAIO] </font>");
                    Game.PrintChat("<font color='0088FF'>[JungleAIO]</font> - " + ObjectManager.Player.ChampionName);
                    break;
                    
                case "Pantheon":
                    new Pantheon();
                    Game.PrintChat("<font color='0088FF'> Loaded - [JungleAIO] </font>");
                    Game.PrintChat("<font color='0088FF'>[JungleAIO]</font> - " + ObjectManager.Player.ChampionName);
                    break;

                case "Nautilus":                   
                    new Nautilus();
                    Game.PrintChat("<font color='0088FF'> Loaded - [JungleAIO] </font>");
                    Game.PrintChat("<font color='0088FF'>[JungleAIO]</font> - " + ObjectManager.Player.ChampionName);
                    break;

                case "XinZhao":
                    new XinZhao();
                    Game.PrintChat("<font color='0088FF'> Loaded - [JungleAIO] </font>");
                    Game.PrintChat("<font color='0088FF'>[JungleAIO]</font> - " + ObjectManager.Player.ChampionName);
                    break;
                    
                    default:
                    Game.PrintChat("<font color='0088FF'>[JungleAIO]</font> - Champion Not Supported!");
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
