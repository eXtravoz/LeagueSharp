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
    class MasterYi
    {
        public const string CharName = "Master Yi";
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
        //W Fix
        public static bool UsingW;
        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        public MasterYi()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Ignite = new Spell(Player.GetSpellSlot("summonerdot"), 600);
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
            Config.SubMenu("combo").AddItem(new MenuItem("useW", "Use W to cancel AA").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useE", "Use E in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("useR", "Use R in Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "Use Items with Combo").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("smartW", "Smart Q").SetValue(true));
            Config.SubMenu("combo").AddItem(new MenuItem("smartSmite", "Smart Smite").SetValue(true));

            //Q Menu
            Config.AddSubMenu(new Menu("Q Settings", "qconfig"));
            Config.SubMenu("qconfig").AddItem(new MenuItem("smartQturret", "Don't Q under enemy turret").SetValue(true));
            Config.SubMenu("qconfig").AddItem(new MenuItem("smartQsave", "Save Q for dodging").SetValue(true));
            Config.SubMenu("qconfig").AddItem(new MenuItem("smartQloseaggro", "Lose turret aggro with Q").SetValue(true));
            Config.SubMenu("qconfig").AddItem(new MenuItem("smartQrunaway", "Use Q if enemy is running away").SetValue(true));

            //Killsteal
            Config.AddSubMenu(new Menu("Killsteal Settings", "killsteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("KillSteal", "Auto KS enabled").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksQ", "KS with Q").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksI", "KS with Ignite").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("ksS", "KS with Smite").SetValue(true));

            //Farm Menu
            Config.AddSubMenu(new Menu("Farming Settings", "farm"));
            Config.SubMenu("farm").AddItem(new MenuItem("qFarm", "Farm with Q").SetValue(true));
            Config.SubMenu("farm").AddItem(new MenuItem("farmMana", "Min. Mana Percent:").SetValue(new Slider(50)));

            //Jungle Clear Menu
            Config.AddSubMenu(new Menu("Jungle Clear Settings", "jungle"));
            Config.SubMenu("jungle").AddItem(new MenuItem("qJungle", "Clear with Q").SetValue(true));
            Config.SubMenu("jungle").AddItem(new MenuItem("eJungle", "Clear with E").SetValue(true));

            //Flee Menu
            Config.AddSubMenu(new Menu("Flee settings", "flee"));
            Config.SubMenu("flee").AddItem(new MenuItem("fleeKey", "Flee key").KeyBind())
            Config.SubMenu("flee").AddItem(new MenuItem("fleeQ", "Flee with Q").SetValue(true));
            Config.SubMenu("flee").AddItem(new MenuItem("fleeR", "Flee with R").SetValue(true));

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

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packet Casting").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            Config.SubMenu("misc")
                .AddItem(
                    new MenuItem("autolvlup", "Auto Level Spells").SetValue(
                        new StringList(new[] { "Q>E>W", "Q>W>E", "E>Q>W" })));

            //Targetted items
            MenuTargetedItems = new Menu("Targeted Items", "menuTargetItems");
            combo.AddSubMenu(MenuTargetedItems);
            MenuTargetedItems.AddItem(new MenuItem("item3153", "Blade of the Ruined King").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "Randuin's Omen").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "Bilgewater Cutlass").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3146", "Hextech Gunblade").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "Entropy ").SetValue(true));

            //Non-targetted items
            MenuNonTargetedItems = new Menu("AOE Items", "menuNonTargetedItems");
            combo.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "Odyn's Veil").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "Sword of the Divine").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "Ravenous Hydra").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "Youmuu's Ghostblade").SetValue(true));

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
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Select default target
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            Orbwalker.SetAttack(!WFix());
            Orbwalker.SetMovement(!WFix());

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

            if (Q.IsReady() && target.Health <= Q.GetDamage(target) && )
            {
               Q.Cast(target)
            }


            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && target.Health <= GetSpellDamage(target, IgniteSlot))
            {
                CastSpell(IgniteSlot, target);
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

            if (WFix())
            {
                return;
            }

            if (Config.Item("useSmite").GetValue<bool>())
            {
                if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
                {
                    if (Config.Item("smartSmite").GetValue<bool>())
                    {
                        if (Q.IsReady() && R.IsReady() && E.IsReady())
                        {
                            Player.Spellbook.CastSpell(SmiteSlot, target);
                        }
                    }
                    Player.Spellbook.CastSpell(SmiteSlot, target);
                }
            }

            if (Q.IsReady() && Config.Item("useQ").GetValue<bool>())
            {
                if (Config.Item("smartQ").GetValue<bool>())
                {
                    SmartQ();
                }
                Q.CastOnUnit(target, PacketCast);
            }

            if (E.IsReady() && Config.Item("useE").GetValue<bool>())
            {
                E.Cast(PacketCast);
            }
            
            if (Config.Item("useW").GetValue<bool>())
            {
                WCancel();
            }

            if (Config.Item("comboItems").GetValue<bool>())
            {
                UseItems(target);
            }
        }


        //Smart Q
        public static void SmartQ()
        {
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
            if (WFix())
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
            if (WFix())
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

            if (WFix())
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
        
        private static void AutoSmite()
        {

            // Thanks to FedNocturne from gFederal
            if (SmiteSlot == SpellSlot.Unknown)
            {
                if (Config.Item("AutoSmite").GetValue<KeyBind>().Active)
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

        //E Fix
        public static bool WFix()
        {
            if (Player.HasBuff("pantheonesound"))
            {
                UsingW = true;
            }

            return UsingW || Player.IsChannelingImportantSpell();
        }
    }
}
