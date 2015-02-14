using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

using JungleAIO.Champions;

using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

using Color = System.Drawing.Color;

namespace JungleAIO.Champions
{
    class XinZhao : Program
    {
        public static Obj_AI_Hero Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;
        public static Spell Q, W, E, R;

        public static SpellSlot IgniteSlot = Player.GetSpellSlot("summonerdot");
        public static SpellSlot FlashSlot = Player.GetSpellSlot("SummonerFlash");

        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);


        public XinZhao()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {

            Q = new Spell(SpellSlot.Q, 0f);
            W = new Spell(SpellSlot.W, 0f);
            E = new Spell(SpellSlot.E, 625f);
            R = new Spell(SpellSlot.R, 500f);

            Config = new Menu("Xin Zhao", "XinZhao", true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var SimpleTs = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(SimpleTs);
            Config.AddSubMenu(SimpleTs);

            var c = Config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            c.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("igniteCombo", "Use Ignite if Killable").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var f = Config.AddSubMenu(new Menu("LaneClear", "Farm"));
            f.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            f.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
            f.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            f.SubMenu("Farm").AddItem(new MenuItem("FarmActive", "Farm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var l = Config.AddSubMenu(new Menu("JungleClear", "JClear"));
            l.SubMenu("JClear").AddItem(new MenuItem("UseQJC", "Use Q").SetValue(true));
            l.SubMenu("JClear").AddItem(new MenuItem("UseWJC", "Use W").SetValue(true));
            l.SubMenu("JClear").AddItem(new MenuItem("UseEJC", "Use E").SetValue(true));
            l.SubMenu("JClear").AddItem(new MenuItem("JCActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var i = Config.AddSubMenu(new Menu("Items Settings", "Items"));
            l.SubMenu("Items").AddItem(new MenuItem("UseHydra", "Use Hydra").SetValue(true));
            l.SubMenu("Items").AddItem(new MenuItem("UseTiamat", "Use Tiamat").SetValue(true));

            var k = Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            k.SubMenu("KillSteal").AddItem(new MenuItem("stealActive", "Enable Killsteal").SetValue(true));
            k.SubMenu("KillSteal").AddItem(new MenuItem("UseEKs", "Use E").SetValue(true));
            k.SubMenu("KillSteal").AddItem(new MenuItem("UseRKs", "Use R").SetValue(true));

            var m = Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            m.SubMenu("Misc").AddItem(new MenuItem("SpellInterrupt", "Interrupt Spells").SetValue(true));
            m.SubMenu("Misc").AddItem(new MenuItem("GapCloser", "Anti-GapCloser").SetValue(true));
            // m.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "Packets").SetValue(true));

            var p = Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            p.SubMenu("AutoPot").AddItem(new MenuItem("AutoPot", "AutoPot enabled").SetValue(true));
            p.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            p.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            p.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            p.SubMenu("AutoPot").AddItem(new MenuItem("AP_M_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            p.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            var d = Config.AddSubMenu(new Menu("Drawings Settings", "Drawings"));
            d.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(true));
            d.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(true));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;

        }

        private static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("GapCloser").GetValue<bool>())
            {
                if (R.IsReady())
                {
                    R.Cast(gapcloser.Sender);
                }
            }
        }        

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {

            if (Config.Item("SpellInterrupt").GetValue<bool>())
            {
                if (unit.IsValidTarget(R.Range) && R.IsReady())
                {
                    R.Cast();
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


        private static void Drawing_OnDraw(EventArgs args)
        {

            var drawE = Config.Item("ERange").GetValue<bool>();
            var drawR = Config.Item("RRange").GetValue<bool>();

            if (drawE)
                Render.Circle.DrawCircle(Player.Position, E.Range, Color.White);

            if (drawR)
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.White);

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var igniteT = TargetSelector.GetTarget(600f, TargetSelector.DamageType.Physical);
            if (Config.Item("igniteCombo").GetValue<bool>())
                UseIgnite(igniteT);
            if (Config.Item("stealActive").GetValue<bool>())
                KillSteal();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (Config.Item("JCActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (Config.Item("JCActive").GetValue<KeyBind>().Active)
                JungleClear();

        }

        private static void JungleClear()
        {

            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            var mobs = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var minions = mobs[0];

                if (Config.Item("UseQJC").GetValue<bool>() && Q.IsReady() &&
                    minions.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }

                if (Config.Item("UseWJC").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }

                if (Config.Item("UseEJC").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions);
                }

                if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(minions))
                {
                    Tiamat.Cast();
                }

                if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(minions))
                {
                    Hydra.Cast();
                }
            }

        }

        private static void LaneClear()
        {

            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            var minion = MinionManager.GetMinions(Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minion.Count > 0)
            {
                var minions = minion[0];

                if (Config.Item("UseQFarm").GetValue<bool>() && Q.IsReady() &&
                    minions.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }

                if (Config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }

                if (Config.Item("UseEFarm").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions);
                }

                if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(minions))
                {
                    Tiamat.Cast();
                }

                if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(minions))
                {
                    Hydra.Cast();
                }
            }
        }

        private static void Combo()
        {

            var Target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();

            if (Target != null)
            {
                return;
            }

            if (Config.Item("UseECombo").GetValue<bool>())
            {
                if (E.IsReady() && Target.IsValidTarget(E.Range))
                {
                    E.Cast(Target);
                }
            }

            if (Config.Item("UseQCombo").GetValue<bool>())
            {
                if (Q.IsReady() && Target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
            }

            if (Config.Item("UseW").GetValue<bool>())
            {
                if (W.IsReady() && Target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }

            if (Config.Item("UseHydra").GetValue<bool>())
            {
                if (Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(Target))
                {
                    Hydra.Cast();
                }
            }

            if (Config.Item("UseTiamat").GetValue<bool>())
            {
                if (Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(Target))
                {
                    Tiamat.Cast();
                }
            }
        }


        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)))
            {
                if (Config.Item("UseRKs").GetValue<bool>())
                {
                    if (R.IsReady() &&
                        hero.Distance(ObjectManager.Player) <= R.Range &&
                        Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                    {
                        R.Cast();
                    }
                }

                if (Config.Item("UseEKs").GetValue<bool>())
                {
                    if (E.IsReady() &&
                        hero.Distance(ObjectManager.Player) <= E.Range &&
                        Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                    {
                        E.Cast();
                    }
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
    }
}
