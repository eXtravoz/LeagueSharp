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
        private const string taunt = "/taunt";
        private const string joke = "/joke";

        private static Menu _menu;
        
        public static SpellSlot IgniteSlot;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        private static void Main(string[] args)
        {

            Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

            Game.PrintChat("<font color=\"#008B45\">Disrespect - by eXtravoz</font>");
            Game.PrintChat("Loaded sucefull!");

        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            _menu = new Menu("Disrespect", "Disrespect", true);
            _menu.AddItem(new MenuItem("active", "Enabled").SetValue(true));
            _menu.AddToMainMenu();

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
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                    }
                }
            }
            else
            {
                if (IgniteSlot.IsReady() && damage > targetHealth)
                {
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

            if (_menu.Item("active").GetValue<bool>())
            {
                switch (args.EventId)
                {

                    case GameEventId.OnKillDragonSteal:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                        break;

                    case GameEventId.OnTurretKill:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                        break;

                        case GameEventId.OnTurretDamage:
                        Game.Say(taunt);
                        Game.PrintChat("Laughing!");
                        break;

                    case GameEventId.OnDamageGiven:
                        Game.Say(taunt);
                        Game.PrintChat("Taunting!");
                        break;

                    case GameEventId.OnDamageTaken:
                        Game.Say(taunt);
                        Game.PrintChat("Taunting!");
                        break;

                    case GameEventId.OnMinionKill:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                        break;

                    case GameEventId.OnFirstBlood:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                        break;

                    case GameEventId.OnSpellLevelup1:
                        Game.Say(joke);
                        Game.PrintChat("Joking!");
                        break;

                    case GameEventId.OnSpellLevelup2:
                        Game.Say(joke);
                        Game.PrintChat("Joking!");
                        break;

                    case GameEventId.OnSpellLevelup3:
                        Game.Say(joke);
                        Game.PrintChat("Joking!");
                        break;

                    case GameEventId.OnSpellLevelup4:
                        Game.Say(joke);
                        Game.PrintChat("Joking!");
                        break;

                    case GameEventId.OnNexusCrystalStart:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                        break;

                    case GameEventId.OnKillDragon:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
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

                    case GameEventId.OnPlaceWard:
                        Game.Say(laugh);
                        Game.PrintChat("Laughing!");
                        break;

                    case GameEventId.OnKillWard:
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
}
