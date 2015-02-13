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
            if (ObjectManager.Player.BaseSkinName == "Hecarim")
            {
                new Champions.Hecarim();
                Game.PrintChat("<font color='#881df2'>[JungleAIO] - </font> Hecarim Plugin!");
            }
            
            if (ObjectManager.Player.BaseSkinName == "Nautilus")
            {
                new Champions.Nautilus();
                Game.PrintChat("<font color='#881df2'>[JungleAIO] - </font> Nautilus Plugin!");
            }

            if (ObjectManager.Player.BaseSkinName == "Nocturne")
            {
                new Champions.Nocturne();
                Game.PrintChat("<font color='#881df2'>[JungleAIO] - </font> Nocturne Plugin!");
            }

            if (ObjectManager.Player.BaseSkinName == "Pantheon")
            {
                new Champions.Pantheon();
                Game.PrintChat("<font color='#881df2'>[JungleAIO] - </font> Pantheon Plugin!");
            }

            if (ObjectManager.Player.BaseSkinName == "Warwick")
            {
                new Champions.Warwick();
                Game.PrintChat("<font color='#881df2'>[JungleAIO] - </font> Warwick Plugin!");
            }

            if (ObjectManager.Player.BaseSkinName.Contains("Zhao"))
            {
                new Champions.XinZhao();
                Game.PrintChat("<font color='#881df2'>[JungleAIO] - </font> Xin Zhao Plugin!");
            }            
        }
    }
}
