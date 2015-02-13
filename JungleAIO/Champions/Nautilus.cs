

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp.Common;
using LeagueSharp;
using LeagueSharp.Common.Data;
using JungleAIO.Champions;

#endregion

using Color = System.Drawing.Color;

namespace JungleAIO.Champions
{
    class Nautilus : Program
    { 
        
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        public static SpellSlot SmiteSlot;
        public static Menu _Menu;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static bool PacketCast;

        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        public Nautilus()
        {
            Q.SetSkillshot(0.25f, 90f, 2000f, true, SkillshotType.SkillshotLine);
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {

            Q = new Spell(SpellSlot.Q, 1125);
            Q.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 425);
            E.SetTargetted(0.5f, 1700f);

            R = new Spell(SpellSlot.R, 2000);
            R.SetTargetted(0.75f, 2500f);

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            SmiteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

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

            var jungleMenu = new Menu("JungleClear", "JClear");
            {
                jungleMenu.AddItem(new MenuItem("UseWJC", "Use Q").SetValue(true));
                jungleMenu.AddItem(new MenuItem("UseEJC", "Use W").SetValue(true));
                jungleMenu.AddItem(new MenuItem("AutoSmite", "AutoSmite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));                
            }
            _Menu.AddSubMenu(jungleMenu);

            var farmMenu = new Menu("Farm Settings", "LaneClear");
            {
                farmMenu.SubMenu("LaneClear").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
                farmMenu.SubMenu("LaneClear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            }
            _Menu.AddSubMenu(farmMenu);

            var miscMenu = new Menu("Misc Settings", "Misc");
            {
                miscMenu.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
                miscMenu.SubMenu("Misc").AddItem(new MenuItem("WGapCloser", "Auto W on GapCloser").SetValue(true));
                miscMenu.SubMenu("Misc").AddItem(new MenuItem("usePackets", "usePackets").SetValue(false));
            }
            _Menu.AddSubMenu(miscMenu);

            var drawMenu = new Menu("Draw Settings", "Drawing");
            {
                miscMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(true));
                miscMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "Draw E Range").SetValue(true));
                miscMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "Draw R Range").SetValue(true));
            }
            _Menu.AddSubMenu(drawMenu);

            Game.OnGameUpdate += JungleAIO_OnGameUpdate;
            Drawing.OnDraw += JungleAIO_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += JungleAIO_OnOnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
        }

        
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (_Menu.Item("InterruptSpells").GetValue<bool>())
            {
                if (Q.IsInRange(unit) && Q.IsReady() && unit.IsEnemy)
                {
                    var Packets = _Menu.Item("usePackets").GetValue<bool>();
                    Q.CastOnUnit(unit, Packets);
                }
            }
        }
         

        private static void JungleAIO_OnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_Menu.Item("WGapCloser").GetValue<bool>())
            {
                W.Cast();
            }
        }

        
        private static void JungleAIO_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (_Menu.Item("InterruptSpells").GetValue<bool>())
            {
                if (Q.IsInRange(unit) && Q.IsReady() && unit.IsEnemy)
                {
                    var Packets = _Menu.Item("usePackets").GetValue<bool>();
                    Q.CastOnUnit(unit, Packets);
                }
            }
        }
        

        private static void JungleAIO_OnDraw(EventArgs args)
        {
            if (_Menu.Item("DrawQ").GetValue<bool>())
            {
                if (Q.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
                }
            }

            if (_Menu.Item("DrawE").GetValue<bool>())
            {
                if (E.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
                }
            }

            if (_Menu.Item("DrawR").GetValue<bool>())
            {
                if (R.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
                }
            }
        }

        private static void JungleAIO_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    JungleClear();
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LaneClear();
                    break;
                default:
                    break;

                if (_Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
                        AutoSmite();
            }
        }

        private static void AutoSmite()
        {

            // Thanks to FedNocturne from gFederal
            if (SmiteSlot == SpellSlot.Unknown)
            {
                if (!_Menu.Item("AutoSmite").GetValue<KeyBind>().Active)
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
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (W.IsReady() && _Menu.Item("UseWJC").GetValue<bool>())
                {
                    W.Cast();
                }

                if (E.IsReady() && _Menu.Item("UseEJC").GetValue<bool>())
                {
                    E.Cast();
                }
            }
        }

        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (minions.Count > 0)
            {
                var minion = minions[0];

                if (_Menu.Item("UseWFarm").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }

                if (_Menu.Item("UseEFarm").GetValue<bool>() && E.IsReady())
                {
                    E.Cast();
                }
            }
        }

        private static void Combo()
        {
            var Packets = _Menu.Item("usePackets").GetValue<bool>();
            var randuinOmen = ItemData.Randuins_Omen.GetItem();
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Target != null)
            {
                if (_Menu.Item("UseQCombo").GetValue<bool>())
                {
                    if (Q.IsReady() && Q.IsInRange(Target) && Target.IsValidTarget(Q.Range))
                    {

                        if (Q.GetPrediction(Target).Hitchance >= HitChance.High)
                            Q.Cast(Target.Position, Packets);
                    }
                }

                if (_Menu.Item("UseWCombo").GetValue<bool>())
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                    }
                }

                if (_Menu.Item("UseECombo").GetValue<bool>())
                {
                    if (E.IsReady() && E.IsInRange(Target) && Target.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }
                }

                if (randuinOmen.IsOwned() && randuinOmen.IsReady() && randuinOmen.IsInRange(Target))
                {
                    randuinOmen.Cast();
                }
            }
        }
    }
}