using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;
using LeagueSharp.Common.Damage;
using LeagueSharp.Network;
using SharpDX;

namespace Disrespect
{
    internal class Program
    {

        private const string laugh = "/laugh";
        public static SpellSlot IgniteSlot;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {

            Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

        }

        // BarackObama's kappa

        private static void UseIgnite(Obj_AI_Hero unit)
        {
            var damage = IgniteSlot == SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready ? 0 : ObjectManager.Player.GetSummonerSpellDamage(unit, Damage.SummonerSpell.Ignite);
            var targetHealth = unit.Health;
            var hasPots = Items.HasItem(ItemData.Health_Potion.Id) || Items.HasItem(ItemData.Crystalline_Flask.Id);
            if (hasPots || unit.HasBuff("RegenerationPotion", true))
            {
                if (damage * 0.5 > targetHealth)
                {
                    if (IgniteSlot.IsReady())
                    {
                        ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, unit);
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                    }
                }
            }
            else
            {
                if (IgniteSlot.IsReady() && damage > targetHealth)
                {
                    ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, unit);
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                }
            }
        }


        private static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if (args.NetworkId != ObjectManager.Player.NetworkId)
            {
                return;
            }

            switch (args.EventId)
            {

                case GameEventId.OnSuperMonsterKill:
                    break;

                case GameEventId.OnDeathAssist:
                    Game.Say(laugh);
                    Game.PrintChat("Laughing!");
                    break;

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
