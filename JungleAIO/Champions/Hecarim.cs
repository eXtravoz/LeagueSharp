using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;
using LeagueSharp.Common.Data;

using SharpDX;
using Color = System.Drawing.Color;

namespace JungleAIO.Champions
{
    class Hecarim : Program
    {

        public static Obj_AI_Hero Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu _Menu;
        public static Spell Q, W, E, R;

        public static SpellSlot IgniteSlot = Player.GetSpellSlot("summonerdot");
        public static SpellSlot FlashSlot = Player.GetSpellSlot("SummonerFlash");
        public static SpellSlot SmiteSlot = Player.GetSpellSlot("SummonerSmite");

        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        public Hecarim() 
        {
            Game_OnGameLoad();
        }       

        private static void Game_OnGameLoad() 
        {

            float self = 0f;

            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, self);
            R = new Spell(SpellSlot.R, 1350);

            R.SetSkillshot(0.5f, 200f, 1200f, false, SkillshotType.SkillshotLine);             

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            _Menu = new Menu("Hecarim", "HecarimMenu", true);
            var orbwalkerMenu = new Menu("Orbwalker", "OrbwalkerMenu");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            _Menu.AddSubMenu(orbwalkerMenu);
            TargetSelector.AddToMenu(_Menu);

            var comboMenu = new Menu("Combo Settings", "Combo");
            {
                comboMenu.AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
                comboMenu.AddItem(new MenuItem("UseRCombo", "Use R ( Start Combo )").SetValue(true));
            }
            _Menu.AddSubMenu(comboMenu);

            var farmMenu = new Menu("Farm Settings", "Farm");
            {
                farmMenu.AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
                farmMenu.AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
            }
            _Menu.AddSubMenu(farmMenu);

            var jungleMenu = new Menu("Jungle Settings", "JCLear");
            {
                jungleMenu.AddItem(new MenuItem("UseQJC", "Use Q").SetValue(true));
                jungleMenu.AddItem(new MenuItem("UseQJC", "Use W").SetValue(true));
                jungleMenu.AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            }
            _Menu.AddSubMenu(jungleMenu);

            var potsMenu = new Menu("Potion Manager", "AutoPot");
            {
                potsMenu.AddItem(new MenuItem("AutoPot", "Enable Auto-Pot").SetValue(true));
                potsMenu.AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
                potsMenu.AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
                potsMenu.AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
                potsMenu.AddItem(new MenuItem("AP_M_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
                potsMenu.AddItem(new MenuItem("AP_Ign", "Auto pot when ignited").SetValue(true));
            }
            _Menu.AddSubMenu(potsMenu);

            var miscMenu = new Menu("Misc Settings", "Misc");
            {
                miscMenu.AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
                miscMenu.AddItem(new MenuItem("InterruptSpellsR", "Interrupt with R").SetValue(true));
                miscMenu.AddItem(new MenuItem("usePackets", "Use Packets").SetValue(false));
            }
            _Menu.AddSubMenu(miscMenu);

            var drawMenu = new Menu("Drawing Settings", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
                drawMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            }
            _Menu.AddSubMenu(miscMenu);
        }       

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (_Menu.Item("InterruptSpells").GetValue<bool>())
                return;         
            var packetCast = _Menu.Item("usePackets").GetValue<bool>();
            if (unit.IsValidTarget(E.Range)) {
                E.CastOnUnit(Player, packetCast);
            }
            if (_Menu.Item("InterruptSpellsR").GetValue<bool>())
                if (unit.IsValidTarget(R.Range))
                    R.CastOnUnit(unit, packetCast);
            
            
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            var qSprite = _Menu.Item("DrawQ").GetValue<bool>();
            var wSprite = _Menu.Item("DrawW").GetValue<bool>();
            var rSprite = _Menu.Item("DrawR").GetValue<bool>();
            var pos = Player.Position;

            if (!Player.IsDead)
                return;

            if (qSprite)
                Render.Circle.DrawCircle(pos, Q.Range, Q.IsReady() ? Color.White : Color.Red);

            if (wSprite) 
                Render.Circle.DrawCircle(pos, W.Range, W.IsReady() ? Color.White : Color.Red);

            if (rSprite)
                Render.Circle.DrawCircle(pos, R.Range, R.IsReady() ? Color.White : Color.Red);

            
        }  

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var tx = TargetSelector.GetTarget(1350f, TargetSelector.DamageType.Magical);

            switch (Orbwalker.ActiveMode)
            {
                
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(tx);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Farm();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }
            
            AutoSmite();
            AutoPot();
            
        }

        private static void AutoPot()
        {
            if (_Menu.Item("AutoPot").GetValue<bool>())
            {
                if (Player.HasBuff("summonerdot") || Player.HasBuff("MordekaiserChildrenOfTheGrave"))
                {
                    if (!Player.InFountain())
                    {
                        if (Items.HasItem(Biscuit.Id) && Items.CanUseItem(Biscuit.Id) &&
                            !Player.HasBuff("ItemMiniRegenPotion"))
                        {
                            Biscuit.Cast(Player);
                        }
                        else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) &&
                                 !Player.HasBuff("RegenerationPotion") && !Player.HasBuff("Health Potion"))
                        {
                            HPpot.Cast(Player);
                        }
                        else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) &&
                                 !Player.HasBuff("ItemCrystalFlask"))
                        {
                            Flask.Cast(Player);
                        }
                    }
                }

                if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop())
                {
                    return;
                }

                //Health Pots
                if (Player.Health / 100 <= _Menu.Item("AP_H_Per").GetValue<Slider>().Value &&
                    !Player.HasBuff("RegenerationPotion", true))
                {
                    Items.UseItem(2003);
                }
                //Mana Pots
                if (Player.Health / 100 <= _Menu.Item("AP_M_Per").GetValue<Slider>().Value &&
                    !Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    Items.UseItem(2004);
                }
            }
        }

        private static void AutoSmite()
        {           
            // Thanks to FedNocturne from gFederal
            if (SmiteSlot == SpellSlot.Unknown)
            {
                if (_Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
                {
                    string[] monsterNames = { "LizardElder", "AncientGolem", "Worm", "Dragon" };
                    var firstOrDefault = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(
                        spell => spell.Name.Contains("mite"));
                    if (firstOrDefault == null) return;

                    var vMonsters = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, firstOrDefault.SData.CastRange[0], MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                    foreach (var vMonster in vMonsters.Where(vMonster => vMonster != null
                                                                      && !vMonster.IsDead
                                                                      && !ObjectManager.Player.IsDead
                                                                      && !ObjectManager.Player.IsStunned
                                                                      && SmiteSlot != SpellSlot.Unknown
                                                                      && ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                                                                      .Where(vMonster => (vMonster.Health < ObjectManager.Player.GetSummonerSpellDamage(vMonster, Damage.SummonerSpell.Smite)) && (monsterNames.Any(name => vMonster.BaseSkinName.StartsWith(name)))))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SmiteSlot, vMonster);
                    }
                }
            }
        }                    

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (Q.IsReady() && _Menu.Item("UseQJC").GetValue<bool>())
                {
                    Q.Cast();
                }

                if (W.IsReady() && _Menu.Item("UseWJC").GetValue<bool>())
                {
                    W.Cast();
                }
            }
        }

        private static void Farm()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (minions.Count > 0)
            {
                var minion = minions[0];

                if (_Menu.Item("UseQFarm").GetValue<bool>() && Q.IsReady() &&
                    minion.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }

                if (_Menu.Item("UseWFarm").GetValue<bool>() && W.IsReady() &&
                    minion.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private static void Combo(Obj_AI_Hero tx)
        {            

            bool qUsage = _Menu.Item("UseQCombo").GetValue<bool>();
            bool wUsage = _Menu.Item("UseWCombo").GetValue<bool>();
            bool eUsage = _Menu.Item("UseECombo").GetValue<bool>();
            bool rUsage = _Menu.Item("UseRCombo").GetValue<bool>();
            
                if (eUsage && tx != null)
                {
                    if (E.IsReady() && tx.IsValidTarget(300f))
                        E.Cast();
                }

                if (rUsage && tx != null)
                {
                    if (R.IsReady() && tx.IsValidTarget(R.Range))
                        R.Cast(tx);
                }

                if (qUsage && tx != null)
                {
                    if (Q.IsReady() && tx.IsValidTarget(Q.Range))
                        Q.Cast();
                }

                if (wUsage && tx != null)
                {
                    if (W.IsReady() && tx.IsValidTarget(W.Range))
                        W.Cast();
                }           
        }  
     }
 }
