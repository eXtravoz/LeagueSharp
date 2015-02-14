
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JungleAIO.Champions;

using LeagueSharp.Common;
using LeagueSharp;
using LeagueSharp.Common.Data;

using Color = System.Drawing.Color;

namespace JungleAIO.Champions
{
    class Warwick : Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q, W, E, R;

        public static SpellSlot IgniteSlot = Player.GetSpellSlot("summonerdot");
        public static SpellSlot SmiteSlot;
        public static SpellSlot FlashSlot = Player.GetSpellSlot("SummonerFlash");

        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        public Warwick()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {

            Q = new Spell(SpellSlot.Q, 400f);
            W = new Spell(SpellSlot.W, 1250f);
            // E = new Spell(SpellSlot.E, 1600f);
            R = new Spell(SpellSlot.R, 700f);

            Config = new Menu("Warwick", "Warwick", true);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("QCombo", "Use Q in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("WCombo", "Use W in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("RCombo", "Use R in Combo").SetValue(false));

            Config.AddSubMenu(new Menu("LaneClear", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));

            Config.AddSubMenu(new Menu("JungleClear", "JClear"));
            Config.SubMenu("JClear").AddItem(new MenuItem("UseQJClear", "Use Q").SetValue(true));
            Config.SubMenu("JClear").AddItem(new MenuItem("UseWJClear", "Use W").SetValue(true));

            Config.AddSubMenu(new Menu("Items Settings", "Items"));
            Config.SubMenu("Items").AddItem(new MenuItem("UseBlade", "Blade of.R King").SetValue(true));
            Config.SubMenu("Items").AddItem(new MenuItem("UseRanduin", "Randuin's Omen").SetValue(true));

            Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("stealActive", "Enable Killsteal").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseQKs", "Use Q").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("UseRKs", "Use R").SetValue(false));

            Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("SpellInterrupt", "Interrupt Spells").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));

            Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AutoPot", "AutoPot enabled").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings Settings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(true));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Config.Item("SpellInterrupt").GetValue<bool>())
            {
                if (unit.IsValidTarget(R.Range) && R.IsReady())
                    R.CastOnUnit(unit);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Player.IsDead)
                return;

            if (Config.Item("QRange").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.White);

            if (Config.Item("WRange").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.White);

            if (Config.Item("ERange").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);

            if (Config.Item("RRange").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
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

            if (Config.Item("stealActive").GetValue<bool>())
                Killsteal();
        }

        private static void Killsteal()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            var dmgQ = Player.GetSpellDamage(target, SpellSlot.Q);
            var dmgR = Player.GetSpellDamage(target, SpellSlot.R);

            if (target.Health < dmgQ && Q.IsReady() && target.IsValidTarget(R.Range))
                Q.CastOnUnit(target);

            if (target.Health < dmgR && R.IsReady() && target.IsValidTarget(R.Range))
                R.CastOnUnit(target);

        }

        private static void JungleClear()
        {

            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var minions = mobs[0];

                if (Config.Item("UseQJClear").GetValue<bool>() && Q.IsReady() &&
                    minions.IsValidTarget(Q.Range))
                {
                    Q.Cast(minions);
                }

                if (Config.Item("UseWJClear").GetValue<bool>() && W.IsReady())
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

                if (Config.Item("useQFarm").GetValue<bool>() && Q.IsReady() &&
                    minion.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(minion, false);
                }

                if (Config.Item("useWFarm").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }
            }
        }

        private static void Combo()
        {

            var Randuin = ItemData.Randuins_Omen.GetItem();
            var ruinedKing = ItemData.Blade_of_the_Ruined_King.GetItem();

            var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (t == null) return;

            if (Config.Item("RCombo").GetValue<bool>())
            {
                if (R.IsReady() && t.IsValidTarget(R.Range))
                {
                    R.CastOnUnit(t);
                }
            }

            if (Config.Item("useBlade").GetValue<bool>())
            {
                if (ruinedKing.IsOwned() && ruinedKing.IsReady() && ruinedKing.IsInRange(t))
                    ruinedKing.Cast(t);
            }

            if (Config.Item("useRanduin").GetValue<bool>())
            {
                if (Randuin.IsOwned() && Randuin.IsReady() && Randuin.IsInRange(t))
                    Randuin.Cast();
            }

            if (Config.Item("QCombo").GetValue<bool>())
            {
                if (Q.IsReady() && t.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(t);
                }
            }

            if (Config.Item("WCombo").GetValue<bool>())
            {
                if (W.IsReady() && t.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private void AutoPot()
        {
            if (Config.Item("AutoPot").GetValue<bool>())
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
                if (Player.Health / 100 <= Config.Item("AP_H_Per").GetValue<Slider>().Value &&
                    !Player.HasBuff("RegenerationPotion", true))
                {
                    Items.UseItem(2003);
                }
                //Mana Pots
                if (Player.Health / 100 <= Config.Item("AP_M_Per").GetValue<Slider>().Value &&
                    !Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    Items.UseItem(2004);
                }
            }
        }

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
                    }
                }
            }
            else
            {
                if (IgniteSlot.IsReady() && damage > targetHealth)
                {
                    ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, unit);
                }
            }
        }

        private static void AutoSmite()
        {

            // Thanks to FedNocturne from gFederal
            if (SmiteSlot == SpellSlot.Unknown)
            {
                if (!Config.Item("AutoSmite").GetValue<KeyBind>().Active)
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
    }
}

