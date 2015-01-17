using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;
using SharpDX;

namespace Disrespect
{
    class Program
    {

        private const string laugh = "/laugh";
        static void Main(string[] args)
        {

            Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;

        }

        static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if (args.NetworkId != ObjectManager.Player.NetworkId)
            {
                return;
            }

            switch (args.EventId)
            {

                case GameEventId.OnKill:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;

                case GameEventId.OnChampionPentaKill:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;

                case GameEventId.OnChampionQuadraKill:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;
                
                case GameEventId.OnChampionTripleKill:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;

                case GameEventId.OnChampionKill:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;

                case GameEventId.OnChampionDoubleKill:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;
            }
        }
    }
}
