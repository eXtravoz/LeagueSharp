﻿#region

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
    class Pantheon : Program
    {
        public const string CharName = "Pantheon";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> Spells = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        public static SpellSlot SmiteSlot;
        public static Menu Config;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        //Packet casting
        public static bool PacketCast;
        //E Fix
        public static bool UsingE;
        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        public Pantheon()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);
            //R = new Spell(SpellSlot.R, 800);

            IgniteSlot = Player.GetSpellSlot("summonerdot");
            SetSmiteSlot();

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            Config = new Menu(CharName, CharName, true);

            //Orbwalker Menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Target Selector Menu
            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo Settings", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("useQ", "Use Q in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E in Combo").SetValue(true));
            //Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("smartW", "Smart W").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("smartSmite", "Smart Smite").SetValue(true));


            //Killsteal
            Config.AddSubMenu(new Menu("Killsteal Settings", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Auto KS enabled").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksQ", "KS with Q").SetValue(true));
            //Config.SubMenu("KillSteal").AddItem(new MenuItem("ksW", "KS with W").SetValue(true));
            //Config.SubMenu("KillSteal").AddItem(new MenuItem("ksE", "KS with E").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksI", "KS with Ignite").SetValue(true));
            //Config.SubMenu("KillSteal").AddItem(new MenuItem("ksS", "KS with Smite").SetValue(true));

            //Harass Menu
            Config.AddSubMenu(new Menu("Harass Settings", "harass"));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("harassKey", "Harass Key").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("hMode", "Harass Mode:").SetValue(new StringList(new[] { "Q only", "W+E", "W+Q" })));
            Config.SubMenu("harass")
                .AddItem(
                    new MenuItem("autoQ", "Auto Q").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("harass").AddItem(new MenuItem("autoQsmart", "Smart Auto Q").SetValue(true));
            Config.SubMenu("harass").AddItem(new MenuItem("harassMana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Farm Menu
            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Q").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("eFarm", "Farm with E").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle").AddItem(new MenuItem("qJungle", "Clear with Q").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("wJungle", "Clear with W").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Clear with E").SetValue(true));

            //Drawing Menu
            Config.AddSubMenu(new Menu("Draw Settings", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("mDraw", "Disable all drawings").SetValue(false));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("Target", "Highlight Target").SetValue(
                        new Circle(true, Color.FromArgb(255, 255, 255, 0))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("QDraw", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("WDraw", "Draw W Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("EDraw", "Draw E Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            /*Config.SubMenu("drawing")
                .AddItem(
                    new MenuItem("RDraw", "Draw R Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));*/

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("gapcloser", "Interrupt Gapclosers").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));
            Config.SubMenu("misc")
                .AddItem(
                    new MenuItem("autolvlup", "Auto Level Spells").SetValue(
                        new StringList(new[] { "Q>E>W", "Q>W>E", "E>Q>W" })));

            //AutoPots menu
            Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AutoPot", "AutoPot enabled").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            if (SmiteSlot != SpellSlot.Unknown)
            {
                Config.SubMenu("combo").AddItem(new MenuItem("useSmite", "Use Smite").SetValue(true));
            }

            //Make menu visible
            Config.AddToMainMenu();

            PacketCast = Config.Item("usePackets").GetValue<bool>();

            //Damage Drawer
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            //Necessary Stuff
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            //Announce that the assembly has been loaded

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            Orbwalker.SetAttack(!EFix());
            Orbwalker.SetMovement(!EFix());

            //Main features with Orbwalker
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
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

            AutoPot();
            KillSteal();
            Harass(target);
        }

        //Drawing
        private static void Drawing_OnDraw(EventArgs args)
        {
            //Main drawing switch
            if (Config.Item("mDraw").GetValue<bool>())
            {
                return;
            }

            //Spells drawing
            foreach (var spell in Spells.Where(spell => Config.Item(spell.Slot + "Draw").GetValue<Circle>().Active))
            {
                Render.Circle.DrawCircle(
                    ObjectManager.Player.Position, spell.Range, spell.IsReady() ? Color.Green : Color.Red);
            }

            //Target Drawing
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Config.Item("Target").GetValue<Circle>().Active && target != null)
            {
                Render.Circle.DrawCircle(target.Position, 50, Config.Item("Target").GetValue<Circle>().Color);
            }
        }

        //Anti Gapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("gapcloser").GetValue<bool>())
            {
                return;
            }

            if (gapcloser.Sender.IsValidTarget(Q.Range))
            {
                W.CastOnUnit(gapcloser.Sender, PacketCast);
            }
        }

        // Interrupter
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            // if (!(Player.Distance(unit) <= W.Range) || !W.IsReady())
            {
                return;
            }

            W.CastOnUnit(unit, PacketCast);
        }

        //Killsteal
        private static void KillSteal()
        {
            if (!Config.Item("KillSteal").GetValue<bool>())
            {
                return;
            }

            var target = HeroManager.Enemies;
            if (target == null)
            {
                return;
            }

            if (Q.IsReady())
            {
                foreach (var kstarget in from kstarget in target
                                         let actualHp =
                                            (HealthPrediction.GetHealthPrediction(kstarget, (int)(Player.Distance(kstarget) * 1000 / 1500)) <=
                                              kstarget.MaxHealth * 0.15)
                                                 ? Player.GetSpellDamage(kstarget, SpellSlot.Q) * 2
                                                 : Player.GetSpellDamage(kstarget, SpellSlot.Q)
                                         where
                                             kstarget.IsValidTarget() &&
                                             HealthPrediction.GetHealthPrediction(kstarget, (int)(Player.Distance(kstarget) * 1000 / 1500)) <=
                                             actualHp
                                         select kstarget)
                {
                    Q.CastOnUnit(kstarget, PacketCast);
                    return;
                }
            }


            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                foreach (var kstarget in from kstarget in target
                                         where
                                         kstarget.IsValidTarget() &&
                                         kstarget.Health <=
                                         ObjectManager.Player.GetSummonerSpellDamage(kstarget, Damage.SummonerSpell.Ignite) &&
                                        ObjectManager.Player.Distance(kstarget) < 600
                                         select kstarget)
                {
                    ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, kstarget);
                }
            }
        }

        //Auto pot
        private static void AutoPot()
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

        //Combo
        public static void Combo(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (EFix())
            {
                return;
            }

            if (Config.Item("useSmite").GetValue<bool>())
            {
                if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    if (Config.Item("smartSmite").GetValue<bool>())
                    {
                        if (Q.IsReady() && W.IsReady() && E.IsReady())
                        {
                            Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                    Player.Spellbook.CastSpell(SmiteSlot, target);
                }
            }

            if (Q.IsReady() && Config.Item("useQ").GetValue<bool>())
            {
                Q.CastOnUnit(target, PacketCast);
            }

            if (W.IsReady() && Config.Item("useW").GetValue<bool>())
            {
                W.CastOnUnit(target, PacketCast);
            }

            if (Config.Item("comboItems").GetValue<bool>())
            {
                UseItems(target);
            }

            if (E.IsReady() && !W.IsReady() && Config.Item("useE").GetValue<bool>())
            {
                E.Cast(target, PacketCast);
            }
        }

        //Harass
        public static void Harass(Obj_AI_Base target)
        {
            if (!Config.Item("harassKey").GetValue<KeyBind>().Active)
            {
                return;
            }
            if (target == null)
            {
                return;
            }

            if (EFix())
            {
                return;
            }

            var mana = Player.MaxMana * (Config.Item("harassMana").GetValue<Slider>().Value / 100.0);
            if (!(Player.Mana > mana))
            {
                return;
            }

            var menuItem = Config.Item("hMode").GetValue<StringList>().SelectedIndex;
            switch (menuItem)
            {
                case 0:
                    if (Q.IsReady())
                    {
                        Q.CastOnUnit(target, PacketCast);
                    }
                    break;
                case 1:
                    if (W.IsReady())
                    {
                        W.CastOnUnit(target, PacketCast);
                    }

                    if (!W.IsReady() && E.IsReady())
                    {
                        E.Cast(target, PacketCast);
                    }
                    break;
                case 2:
                    if (W.IsReady())
                    {
                        W.CastOnUnit(target, PacketCast);
                    }

                    if (!W.IsReady() && Q.IsReady())
                    {
                        Q.Cast(target, PacketCast);
                    }
                    break;
            }
        }

        //Auto Q
        public static void AutoQ(Obj_AI_Base target)
        {
            if (!Config.Item("autoQ").GetValue<KeyBind>().Active)
            {
                return;
            }
            if (target == null)
            {
                return;
            }

            var mana = Player.MaxMana * (Config.Item("harassMana").GetValue<Slider>().Value / 100.0);
            if (!(Player.Mana > mana))
            {
                return;
            }

            if (Config.Item("autoQsmart").GetValue<bool>()
                ? !Player.UnderTurret(true)
                : Player.UnderTurret(true) && Player.Distance(target) <= Q.Range && Q.IsReady())
            {
                Q.CastOnUnit(target, PacketCast);
            }

        }

        //Farm
        public static void Farm()
        {
            if (EFix())
            {
                return;
            }

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var mana = Player.MaxMana * (Config.Item("farmMana").GetValue<Slider>().Value / 100.0);
            if (!(Player.Mana > mana))
            {
                return;
            }


            if (Config.Item("qFarm").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in from minion in minions
                                       let actualHp =
                                           (HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1500)) <=
                                            minion.MaxHealth * 0.15)
                                               ? Player.GetSpellDamage(minion, SpellSlot.Q) * 2
                                               : Player.GetSpellDamage(minion, SpellSlot.Q)
                                       where
                                           minion.IsValidTarget() &&
                                           HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion) * 1000 / 1500)) <=
                                           actualHp
                                       select minion)
                {
                    Q.CastOnUnit(minion, PacketCast);
                    return;
                }
            }

            if (Config.Item("eFarm").GetValue<bool>() && E.IsReady())
            {
                foreach (var minion in
                    minions.Where(
                        minion =>
                            minion != null && minion.IsValidTarget(E.Range) &&
                            HealthPrediction.GetHealthPrediction(minion, (int)(Player.Distance(minion))) <
                            Player.GetSpellDamage(minion, SpellSlot.E)))
                {
                    E.CastOnUnit(minion, PacketCast);
                    return;
                }
            }
        }


        //Jungleclear
        public static void JungleClear()
        {
            if (EFix())
            {
                return;
            }

            var mobs = MinionManager.GetMinions(
                Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
            {
                return;
            }

            var mob = mobs[0];
            if (mob == null)
            {
                return;
            }

            if (Config.Item("qJungle").GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnUnit(mob, PacketCast);
            }

            if (Config.Item("wJungle").GetValue<bool>() && W.IsReady() && W.IsInRange(mob))
            {
                W.CastOnUnit(mob, PacketCast);
            }

            if (Config.Item("eJungle").GetValue<bool>() && E.IsReady())
            {
                E.Cast(mob, PacketCast);
            }
        }

        //Combo Damage calculating
        public static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;
            if (Q.IsReady())
            {
                dmg += (target.Health <= target.MaxHealth * 0.15)
                    ? (Player.GetSpellDamage(target, SpellSlot.Q) * 2)
                    : Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite);
            }

            return (float)dmg;
        }

        //Items using 
        public static void UseItems(Obj_AI_Base target)
        {
            if (target == null)
            {
                return;
            }

            if (EFix())
            {
                return;
            }

            Int16[] targetedItems = { 3188, 3153, 3144, 3128, 3146, 3184 };
            Int16[] nonTargetedItems = { 3180, 3131, 3074, 3077, 3142 };

            foreach (var itemId in targetedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
            {
                Items.UseItem(itemId, target);
            }

            foreach (var itemId in nonTargetedItems.Where(itemId => Items.HasItem(itemId) && Items.CanUseItem(itemId)))
            {
                Items.UseItem(itemId);
            }
        }

        //Get smite type
        public static string SmiteType()
        {
            int[] redSmite = { 3715, 3718, 3717, 3716, 3714 };
            int[] blueSmite = { 3706, 3710, 3709, 3708, 3707 };

            return blueSmite.Any(itemId => Items.HasItem(itemId))
                ? "s5_summonersmiteplayerganker"
                : (redSmite.Any(itemId => Items.HasItem(itemId)) ? "s5_summonersmiteduel" : "summonersmite");
        }

        //Setting Smite slot
        public static void SetSmiteSlot()
        {
            foreach (var spell in
                ObjectManager.Player.Spellbook.Spells.Where(
                    spell => String.Equals(spell.Name, SmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                SmiteSlot = spell.Slot;
                break;
            }
        }

        //E Fix
        public static bool EFix()
        {
            if (Player.HasBuff("pantheonesound"))
            {
                UsingE = true;
            }

            return UsingE || Player.IsChannelingImportantSpell();
        }
    }
}