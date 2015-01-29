using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using System.Collections.Generic;

namespace Nautilus
{
    internal class Program
    {

        private const string championName = "Nautilus";

        private static Orbwalking.Orbwalker Orbwalker;

        private static readonly List<Spell> SpellList = new List<Spell>();

        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;

        private static Menu _config;

        private static SpellSlot SmiteSlot;

        private static Items.Item biscuit = new Items.Item(2010, 10);

        private static Items.Item HPpot = new Items.Item(2003, 10);

        private static Items.Item Flask = new Items.Item(2041, 10);

        private static bool Packets
        {
            get { return _config.Item("Packets").GetValue<bool>(); }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += LoadGame;
        }

        private static void LoadGame(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != championName)
            {
                return;
            }

            SmiteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

            _q = new Spell(SpellSlot.Q, 950f);
            _q.SetSkillshot(0.25f, 90f, 2000f, true, SkillshotType.SkillshotLine);

            _w = new Spell(SpellSlot.W, 0f);
            _e = new Spell(SpellSlot.E, 600f);
            _r = new Spell(SpellSlot.R, 825f);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _config = new Menu(championName, championName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
            _config.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));

            _config.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            _config.SubMenu("Killsteal").AddItem(new MenuItem("ksOn", "Enable KS").SetValue(true));
            _config.SubMenu("Killsteal").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            _config.SubMenu("Killsteal").AddItem(new MenuItem("UseE", "Use E").SetValue(true));

            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("Packets", "Use Packets").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("Interrupts", "Interrupt Q").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("Interruptr", "Interrupt R").SetValue(false));
            _config.SubMenu("Misc").AddItem(new MenuItem("Gapcloses", "Anti-Gapcloser").SetValue(true));

            _config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            _config.SubMenu("AutoPot").AddItem(new MenuItem("apOn", "Enable Auto-Pot").SetValue(true));
            _config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            _config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            _config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            _config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            _config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            _config.AddSubMenu(new Menu("qMenu", "qMenu"));
            _config.SubMenu("qMenu").AddItem(new MenuItem("AutoQIm", "Auto Q Immobile").SetValue(true));
            _config.SubMenu("qMenu").AddItem(new MenuItem("AutoQDash", "Auto Q Dashing").SetValue(true));

            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("qDraw", "Draw Q").SetValue(true));
            _config.SubMenu("Drawings").AddItem(new MenuItem("eDraw", "Draw E").SetValue(true));

            _config.AddToMainMenu();

            Drawing.OnDraw += SharpDraw;
            Game.OnGameUpdate += Updating;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += InterrupterOnOnPossibleToInterrupt;
            Utility.HpBarDamageIndicator.DamageToUnit = DamageToUnit;
            Utility.HpBarDamageIndicator.Enabled = true;

        }

        private static void InterrupterOnOnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (spell.DangerLevel != InterruptableDangerLevel.High)
            {
                return;
            }

            var interrupt = _config.Item("interrupts").GetValue<bool>();

            if (_q.IsReady()& _q.IsInRange(unit, _q.Range) && interrupt)
            {
                _q.Cast(unit, Packets);
            }

            var interruptr = _config.Item("interrupt").GetValue<bool>();

            if (_r.IsReady() && _q.IsInRange(unit, _q.Range) && interruptr)
            {
                _r.Cast(unit, Packets);
            }

            /* if (_w.IsReady())
            {
                _
            }
            */

        }

        private static void SharpDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {

                if (_config.Item("qDraw").GetValue<bool>())
                {
                    if (_q.Level > 0)
                    {
                        Render.Circle.DrawCircle(
                            ObjectManager.Player.Position, _q.Range, _q.IsReady() ? Color.Aqua : Color.Red);
                    }
                }

                if (_config.Item("eDraw").GetValue<bool>())
                {
                    if (_e.Level > 0)
                    {
                        Render.Circle.DrawCircle(
                            ObjectManager.Player.Position, _e.Range, _e.IsReady() ? Color.Aqua : Color.Red);
                    }
                }
            }
        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsValidTarget() && _config.Item("Gapcloses").GetValue<bool>() && _w.IsReady())
            { 
                _w.Cast(ObjectManager.Player, Packets);
            }
        }

        // Thanks to ChewyMoons
        private static float DamageToUnit(Obj_AI_Hero hero)
        {
            double dmg = 0;


            if (_q.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.Q);
            }

            if (_w.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.W);
            }

            if (_e.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.E);
            }

            if (_r.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(hero, SpellSlot.R);
            }

            if (SmiteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
            }

            return (float)dmg;
        }

        private static void Updating(EventArgs args)
        {
            Immobile();
            Dashing();
            KSz();
            AutoPot();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Ancora();
                    Escudo();
                    Profundeza();
                    JogarPraCima();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                default:
                    break;
            }
        }

        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, _w.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (minions.Count <= 0)
            {
                return;
            }

            var minion = minions[0];

            if (minion == null)
            {
                return;
            }
            if (_config.Item("UseWFarm").GetValue<bool>() && _w.IsReady())
            {
                _w.CastOnUnit(minion, Packets);
            }
            if (_config.Item("UseEFarm").GetValue<bool>() && _e.IsReady())
            {
                _e.CastOnUnit(minion, Packets);
            }

        }

        /*
        private static void Should()
        {
            if (_config.Item("AutoQCC").GetValue<bool>())
            {
                if(_q.GetPrediction())
            }
        }
        */
        var targets = TargetSelector.GetTarget(_r.Range, TargetSelector.DamageType.Magical);
        
        private static void JogarPraCima()
        {
            if (target == null)
            {
                if (_r.IsReady() && ObjectManager.Player.Distance(targets) <= _r.Range && _config.Item("UseRCombo").GetValue<bool>())
                {
                    _r.Cast(targets, Packets);
                }
            }
            
        }
        
        private static void Ancora()
        {
            if (target == null)
            {
                if (_q.IsReady() && ObjectManager.Player.Distance(targets) <= _q.Range && _config.Item("UseQCombo").GetValue<bool>())
                {
                    var qPred = _q.GetPrediction(targets);
                    if (qPred.Hitchance >= HitChance.High)
                    {
                        _q.Cast(qPred.CastPosition);
                    }
                }
            }
        }
        
        private static void Escudo()
        {
            if (_w.IsReady() && _config.Item("UseWCombo").GetValue<bool>())
            {
                 _w.Cast(ObjectManager.Player, Packets);
            }
        }
        
        private static void Profundeza()
        {
            if (_e.IsReady() && ObjectManager.Player.Distance(targets) <= _e.Range && _config.Item("UseECombo").GetValue<bool>())
            {
                _e.Cast(targets, Packets);
            }
        }

        private static void Immobile()
        {
            if (_config.Item("AutoQIm").GetValue<bool>())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(_q.Range))
                {
                    _q.CastIfHitchanceEquals(target, HitChance.Immobile, Packets);
                }
            }
        }

        private static void Dashing()
        {
            if (_config.Item("AutoQDash").GetValue<bool>())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (target.IsValidTarget(_q.Range))
                {
                    _q.CastIfHitchanceEquals(target, HitChance.Dashing, Packets);
                }
            }
        }

        private static void KSz()
        {
            if (_config.Item("ksOn").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical);
                if (target == null)
                    return;

                var prediction = _q.GetPrediction(target);
                if (_q.IsReady())
                {
                    if (_config.Item("UseQ").GetValue<bool>())
                    {

                        if (target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q))
                        {
                            if (prediction.Hitchance >= HitChance.High &&
                                prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 1)
                            {
                                _q.Cast(prediction.CastPosition, Packets);
                            }
                        }
                    }
                }

                if (_e.IsReady())
                {
                    if (_config.Item("UseE").GetValue<bool>())
                    {
                        if (target.Health < ObjectManager.Player.GetSpellDamage(target, SpellSlot.E))
                        {
                           _e.Cast(target, Packets);
                        }
                    }
                }
            }
        }

        private static void AutoPot()
        {
            if (_config.Item("apOn").GetValue<bool>())
            {
                if (_config.SubMenu("AutoPot").Item("AP_Ign").GetValue<bool>())
                    if (ObjectManager.Player.HasBuff("summonerdot") ||
                        ObjectManager.Player.HasBuff("MordekaiserChildrenOfTheGrave"))
                    {
                        if (!ObjectManager.Player.InFountain())

                            if (Items.HasItem(biscuit.Id) && Items.CanUseItem(biscuit.Id) &&
                                !ObjectManager.Player.HasBuff("ItemMiniRegenPotion"))
                            {
                                biscuit.Cast(ObjectManager.Player);
                            }
                            else if (Items.HasItem(HPpot.Id) && Items.CanUseItem(HPpot.Id) &&
                                     !ObjectManager.Player.HasBuff("RegenerationPotion") &&
                                     !ObjectManager.Player.HasBuff("Health Potion"))
                            {
                                HPpot.Cast(ObjectManager.Player);
                            }
                            else if (Items.HasItem(Flask.Id) && Items.CanUseItem(Flask.Id) &&
                                     !ObjectManager.Player.HasBuff("ItemCrystalFlask"))
                            {
                                Flask.Cast(ObjectManager.Player);
                            }
                    }


                if (ObjectManager.Player.HasBuff("Recall") ||
                    ObjectManager.Player.InFountain() && ObjectManager.Player.InShop())
                {
                    return;
                }

                //Health Pots
                if (ObjectManager.Player.Health / 100 <= _config.Item("AP_H_Per").GetValue<Slider>().Value &&
                    !ObjectManager.Player.HasBuff("RegenerationPotion", true))
                {
                    Items.UseItem(2003);
                }
                //Mana Pots
                if (ObjectManager.Player.Health / 100 <= _config.Item("A_M_Per").GetValue<Slider>().Value &&
                    !ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    Items.UseItem(2004);
                }
            }
        }
    }
}
