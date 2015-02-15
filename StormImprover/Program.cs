using LeagueSharp;
using LeagueSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StormImprover
{
    class Program
    {
        public static string CharacterManager;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;          
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            CharacterManager = ObjectManager.Player.BaseSkinName;           

            if (ObjectManager.Player.BaseSkinName != "Mordekaiser")
            {
                new Addons.Mordekaiser();
                Game.PrintChat("<font color='#2F4F4F'>StormImprover [Addon] - </font>Mordekaiser");
            }
        }       
    }
}
