using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Nautilus
{
    internal class Program
    {

        private const string championScript = "Nautilus";

        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;

        // Thanks to Jouza
        private static SpellSlot SmiteSlot;
        private static Items.Item biscuit = new Items.Item(2010, 10);
        private static Items.Item HPpot = new Items.Item(2003, 10);
        private static Items.Item Flask = new Items.Item(2041, 10);

        private static Menu _menu;
        private static Obj_AI_Hero Target;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Obj_AI_Hero _player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += LoadGame;
            Drawing.OnDraw += SharpDraw;
            Game.OnGameUpdate += Updating;
            Interrupter.OnPossibleToInterrupt += Interrupt;
        }

        private static void LoadGame(EventArgs args)
        {
            if (_player.ChampionName != championScript)
                return;

            _q = new Spell(SpellSlot.Q, 950f);
            _q.SetSkillshot(0.25f, 90f, 2000f, true, SkillshotType.SkillshotLine);

            _w = new Spell(SpellSlot.W, 0f);
            _e = new Spell(SpellSlot.E, 600f);
            _r = new Spell(SpellSlot.R, 825f);

            _menu = new Menu("Nautilus", "Nautilus", true);
            var thisOrb = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(thisOrb);
            _menu.AddSubMenu(thisOrb);

            var SimpleTs = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(SimpleTs);
            _menu.AddSubMenu(SimpleTs);

            var comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("comboQ", "Use Q")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboW", "Use W")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboE", "Use E")).SetValue(true);
            comboMenu.AddItem(new MenuItem("comboR", "Use R")).SetValue(false);

            _menu.AddSubMenu(comboMenu);

            // Jouza, thanks :)
            var potionManager = new Menu("potionManager", "Potion Manager");
            potionManager.AddItem(new MenuItem("potOn", "Enable Auto-Pot").SetValue(true));
            potionManager.AddItem(new MenuItem("hPot", "Health Potion").SetValue(true));
            potionManager.AddItem(new MenuItem("mPot", "Mana Potion").SetValue(true));
            potionManager.AddItem(new MenuItem("hPotPer", "Health Pot %").SetValue(new Slider(35, 1)));
            potionManager.AddItem(new MenuItem("mPotPer", "Mana Pot %").SetValue(new Slider(35, 1)));
            potionManager.AddItem(new MenuItem("ignitePot", "Auto-Pot if ignited").SetValue(true));


            var jungleMenu = new Menu("Jungle", "Jungle");
            jungleMenu.AddItem(new MenuItem("jungleW", "Use W")).SetValue(true);
            jungleMenu.AddItem(new MenuItem("jungleE", "Use E")).SetValue(true);
            _menu.AddSubMenu(jungleMenu);

            var drawingMenu = new Menu("Drawing", "Drawing");
            drawingMenu.AddItem(new MenuItem("qDraw", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("eDraw", "Draw E").SetValue(false));
            drawingMenu.AddItem(new MenuItem("rDraw", "Draw R").SetValue(true));
            _menu.AddSubMenu(drawingMenu);

            var stealMenu = new Menu("KillSteal", "KillSteal");
            stealMenu.AddItem(new MenuItem("stealOn", "Enable KillSteal")).SetValue(false);
            stealMenu.AddItem(new MenuItem("stealQ", "Q KillSteal")).SetValue(false);
            stealMenu.AddItem(new MenuItem("stealE", "E KillSteal")).SetValue(false);
            stealMenu.AddItem(new MenuItem("stealR", "R KillSteal")).SetValue(false);
            stealMenu.AddItem(new MenuItem("stealSmite", "Smite KillSteal")).SetValue(false);
            _menu.AddSubMenu(stealMenu);

            var miscMenu = new Menu("Misc", "Misc");
            miscMenu.AddItem(new MenuItem("EnemiesE", "X enemies to E")).SetValue(new Slider(1, 0, 5));
            miscMenu.AddItem(new MenuItem("Packets", "Packet Cast")).SetValue(true);
            miscMenu.AddItem(new MenuItem("Interrupter", "Interrupt skills")).SetValue(true);
            miscMenu.AddItem(new MenuItem("Gapcloser", "Use W on Gapcloser")).SetValue(true);
            miscMenu.AddItem(new MenuItem("EGapcloser", "Use E on Gapcloser")).SetValue(true);
            _menu.AddSubMenu(miscMenu);
            _menu.AddToMainMenu();

            if (SmiteSlot != SpellSlot.Unknown)
            {
                comboMenu.AddItem(new MenuItem("comboSmite", "Use Smite")).SetValue(true);
            }

            Game.PrintChat("<font color=\"#00BFFF\">Nautilus -</font> <font color=\"#FFFFFF\">Loaded</font>");

        }

        private static void Updating(EventArgs args)
        {

            var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
            var jungleKey = _menu.Item("jungleKey").GetValue<KeyBind>().Active;

            if (_menu.Item("potOn").GetValue<bool>())
            {
                AutoPot();
            }

            if (_menu.Item("stealOn").GetValue<bool>())
            {
                Steal(target);
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Jungle();
                    break;        
                default:
                    break;
            }
        }

        private static void SharpDraw(EventArgs args)
        {
            var drawQ = _menu.Item("qDraw").GetValue<bool>();
            var drawE = _menu.Item("eDraw").GetValue<bool>();
            var drawR = _menu.Item("rDraw").GetValue<bool>();

            var position = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(position, _q.Range, _q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(position, _e.Range, _e.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(position, _r.Range, _r.IsReady() ? Color.Aqua : Color.Red);
            }
        }

        private static void Interrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {

            if (_menu.Item("Interrupter").GetValue<bool>())
            {
                if ((unit.Distance(unit.Position) <= _q.Range) && _q.IsReady())
                    _q.CastOnUnit(unit, _menu.Item("Packets").GetValue<bool>());
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_menu.Item("Gapcloser").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(_w.Range))
                {
                    _w.CastOnUnit(_player, _menu.Item("Packets").GetValue<bool>());
                }
            }

            if (!_menu.Item("EGapcloser").GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(_e.Range))
                {
                    _e.CastOnUnit(_player, _menu.Item("Packets").GetValue<bool>());
                }
            }

        }

        private static void Steal(Obj_AI_Hero target)
        {

            var dmgSmite = _player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite);
            var dmgQ = _player.GetSpellDamage(target, SpellSlot.Q);
            var dmgE = _player.GetSpellDamage(target, SpellSlot.E);
            var dmgR = _player.GetSpellDamage(target, SpellSlot.R);

            if (target.IsValidTarget())
            {
                if (_menu.Item("stealSmite").GetValue<bool>() &&
                    _player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    if (dmgSmite > target.Health && _player.Distance(target) < 600)
                    {
                        _player.Spellbook.CastSpell(SmiteSlot, target);
                    }
                }

                if (_menu.Item("stealQ").GetValue<bool>())
                {
                    if (_q.IsReady())
                    {
                        if (dmgQ > target.Health && _player.Distance(target) < 950)
                        {
                            _q.Cast(target, _menu.Item("Packets").GetValue<bool>());
                        }
                    }
                }

                if (_menu.Item("stealE").GetValue<bool>())
                {
                    if (dmgE > target.Health && _player.Distance(target) < 600)
                    {
                        if (_e.IsReady())
                        {
                            _e.Cast(target, _menu.Item("Packets").GetValue<bool>());
                        }
                    }
                }
            }
        }

        // Jouza's logic WOOOO 
        private static void AutoPot()
        {
            if (_menu.Item("ignitePot").GetValue<bool>())
                if (_player.HasBuff("summonerdot") || _player.HasBuff("MordekaiserChildrenOfTheGrave"))
                {
                    if (!_player.InFountain())

                        if (Items.HasItem(biscuit.Id) && Items.CanUseItem(biscuit.Id) &&
                            !_player.HasBuff("ItemMiniRegenPotion"))
                        {
                            biscuit.Cast(_player);
                        }
                        else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) &&
                                 !_player.HasBuff("RegenerationPotion") && !_player.HasBuff("Health Potion"))
                        {
                            HPpot.Cast(_player);
                        }
                        else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) &&
                                 !_player.HasBuff("ItemCrystalFlask"))
                        {
                            Flask.Cast(_player);
                        }
                }

            if (ObjectManager.Player.HasBuff("Recall") || _player.InFountain() && _player.InShop())
            {
                return;
            }

            if (_menu.Item("hPot").GetValue<bool>())
            {
                if (_player.Health / 100 <= _menu.Item("hPotPer").GetValue<Slider>().Value &&
                    !_player.HasBuff("RegenerationPotion", true))
                {
                    Items.UseItem(2003);
                }
            }

            if (_menu.Item("mPot").GetValue<bool>())
            {
                if (_player.Health / 100 <= _menu.Item("mPotPer").GetValue<Slider>().Value &&
                    !_player.HasBuff("FlaskOfCrystalWater", true))
                {
                    Items.UseItem(2004);
                }
            }
        }

        private static void Combo()
        {

            if (TargetSelector.GetSelectedTarget() != null)
                return;


            if (Target.IsValidTarget() && _r.IsReady())
            {
                if (_menu.Item("comboR").GetValue<bool>())
                {
                    _r.Cast(Target, _menu.Item("Packets").GetValue<bool>());
                }
            }

            if (Target.IsValidTarget() && _q.IsReady())
            {
                var prediction = _q.GetPrediction(Target);

                if (_menu.Item("comboQ").GetValue<bool>())
                {
                    if (prediction.Hitchance >= HitChance.High)
                    {
                        _q.Cast(Target, _menu.Item("Packets").GetValue<bool>());
                    }
                }
            }

            if (Target.IsValidTarget() && _w.IsReady())
            {
                if (_menu.Item("comboW").GetValue<bool>())
                {
                    _w.Cast(_player, _menu.Item("Packets").GetValue<bool>());
                }
            }

            if (Target.IsValidTarget())
            {
                if (!_e.IsReady() || _menu.Item("EnemiesE").GetValue<Slider>().Value == 0) return;
                if (_menu.Item("EnemiesE").GetValue<Slider>().Value == 1)
                {
                    var target = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.Magical);
                    if (!_menu.Item("ult" + target.SkinName).GetValue<bool>()) _e.Cast(target, _menu.Item("packets").GetValue<bool>());
                }
                else
                {
                    foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsEnemy && !i.IsDead && _player.Distance(i) < _e.Range && _player.Distance(i) > 825f && countChampsAtrange(i, 350f) >= _menu.Item("EnemisE").GetValue<Slider>().Value).OrderByDescending(l => countChampsAtrange(l, 350f)))
                    {
                        _e.Cast(enemy, _menu.Item("Packets").GetValue<bool>());
                        return;
                    }
                }
            }
        }

        private static void Jungle()
        {
            // Thanks Jouza again
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _w.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var packets = _menu.Item("Packets").GetValue<bool>();
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];
            if (mob == null)
            {
                return;
            }
            if (_menu.Item("jungleE").GetValue<bool>() && _e.IsReady())
            {
                _e.CastOnUnit(mob, packets);
            }
            if (_menu.Item("wJungle").GetValue<bool>() && _w.IsReady())
            {
                _w.CastOnUnit(mob, packets);
            }
        }

        private static int countChampsAtrange(Obj_AI_Hero l, float p)
        {
            int num = 0;
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(i => !i.IsDead && i.IsEnemy && i.Distance(l) < p))
            {
                num++;
            }

            return num;
        }
    }
}
