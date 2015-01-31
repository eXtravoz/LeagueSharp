using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FarmHelper.Plugin;

using LeagueSharp;
using LeagueSharp.Common;

namespace FarmHelper
{
    class Program
    {
        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Annie":
                    new Annie();
                    Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
                    break;

                case "Gangplank":
                    new Gangplank();
                    Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
                    break;

                case "Veigar":
                    new Veigar();
                    Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
                    break;

                case "Nasus":
                    new Nasus();
                    Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
                    break;

                case "LeBlanc":
                    new LeBlanc();
                    Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
                    break;

                default:
                    Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='FF0000'>Not Supported!</font>");
                    break;

            }

            /*
            Player = ObjectManager.Player;
            if (Player.ChampionName != "Annie")
            {
                new Annie();
                Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
            }

            if (Player.ChampionName != "Gangplank")
            {
                new Gangplank();
                Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
            }

            if (Player.ChampionName != "Veigar")
            {
                new Veigar();
                Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
            }

            if (Player.ChampionName != "Nasus")
            {
                new Nasus();
                Game.PrintChat("<font color='0088FF'>FarmHelper: </font>" + ObjectManager.Player.ChampionName + " " + "<font color='0088FF'>Loaded!</font>");
            }
            */

        }
       
    }
}
