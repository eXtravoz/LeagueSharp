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
    class Nocturne : Program
    {

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
        public static bool PacketCast;

        // Items
        public static Items.Item Biscuit = new Items.Item(2010, 10);
        public static Items.Item HPpot = new Items.Item(2003, 10);
        public static Items.Item Flask = new Items.Item(2041, 10);

        public Nocturne()
        {
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

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            SmiteSlot = ObjectManager.Player.GetSpellSlot("SummonerSmite");

            Spells.Add(Q);
            Spells.Add(W);
            Spells.Add(E);
            Spells.Add(R);

            Config = new Menu("JungleAIO: Nocturne", "Nocturne", true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var tsMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(tsMenu);
            Config.AddSubMenu(tsMenu);

            Config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("QCombo", "Use Q in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("WCombo", "Use W in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ECombo", "Use E in Combo").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("autoIgnite", "Ignite if Killable").SetValue(true));

            Config.AddSubMenu(new Menu("Killsteal Settings", "KillSteal"));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("Qks", "KS with Q").SetValue(true));
            Config.SubMenu("KillSteal").AddItem(new MenuItem("Eks", "KS with E").SetValue(true));

            Config.AddSubMenu(new Menu("Harass ", "Harass"));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harassKey", "Harass Key").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Harass")
               .AddItem(
                   new MenuItem("QHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassMana", "Min. Mana Percent:").SetValue(new Slider(50)));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("qFarm", "LaneClear with Q").SetValue(true));

            Config.AddSubMenu(new Menu("JungleClear", "Jungle"));
            Config.SubMenu("Jungle").AddItem(new MenuItem("qJungle", "Clear with Q").SetValue(true));
            Config.SubMenu("Jungle").AddItem(new MenuItem("eJungle", "Clear with E").SetValue(true));

            Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("mDraw", "Disable all drawings").SetValue(false));
            Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("QDraw", "Draw Q Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing")
                .AddItem(
                    new MenuItem("EDraw", "Draw E Range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "Draw R on Minimap").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));

            Config.AddSubMenu(new Menu("Itens Settings", "Itens"));
            Config.SubMenu("Itens").AddItem(new MenuItem("useBlade", "Blade of the Ruined King").SetValue(true));
            Config.SubMenu("Itens").AddItem(new MenuItem("useYoumuu", "Youmuu's Ghostblade").SetValue(true));
            Config.SubMenu("Itens").AddItem(new MenuItem("useRanduin", "Randuin's Omen").SetValue(true));

            Config.AddSubMenu(new Menu("AutoPot", "AutoPot"));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AutoPot", "AutoPot enabled").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H", "Health Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M", "Mana Pot").SetValue(true));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_H_Per", "Health Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_M_Per", "Mana Pot %").SetValue(new Slider(35, 1)));
            Config.SubMenu("AutoPot").AddItem(new MenuItem("AP_Ign", "Auto pot when ignite").SetValue(true));

            Config.AddSubMenu(new Menu("Misc Settings", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("stopChannel", "Interrupt Spells").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("wgapcloser", "Auto W on Gapcloser").SetValue(true));
            Config.SubMenu("JClear").AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            Config.SubMenu("misc").AddItem(new MenuItem("usePackets", "Use Packets to Cast Spells").SetValue(false));

            Config.AddToMainMenu();

            var Packets = Config.Item("usePackets").GetValue<bool>();

            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Player.IsDead) return;

            if (R.Level == 0) return;
            if (Config.Item("DrawRM").GetValue<bool>() && R.Level > 0)
            {
                Utility.DrawCircle(Player.Position, GetRRange(), System.Drawing.Color.Aqua, 1, 30, true);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo(target);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Farm();
                    Harass(target);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm();
                    break;
            }

            AutoPot();
            KillSteal(target);
            Harass(target);
        }

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

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("wgapcloser").GetValue<bool>())
            {
                return;
            }

            W.Cast();
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("stopChannel").GetValue<bool>())
            {
                return;
            }

            if (unit.IsValidTarget(E.Range))
            {
                return;
            }

            E.CastOnUnit(unit, PacketCast);
        }

        private static float GetRRange()
        {
            return 1250 + (750 * R.Level);
        }

        private static void KillSteal(Obj_AI_Hero unit)
        {
            if (!Config.Item("KillSteal").GetValue<bool>())
                return;

            var target = HeroManager.Enemies;
            if (target == null)
            {
                return;
            }

            /*if (Q.IsReady())
            {
                foreach (var kstarget in from kstarget in target
                                         let actualHp =
                                             (HealthPrediction.GetHealthPrediction(kstarget,
                                                 (int)(Player.Distance(kstarget) * 1000 / 1500)) <= kstarget.MaxHealth * 0.15)
                                                 ? Player.GetSpellDamage(kstarget, SpellSlot.Q) * 2
                                                 : Player.GetSpellDamage(kstarget, SpellSlot.Q)
                                         where kstarget.IsValidTarget() && HealthPrediction.GetHealthPrediction(kstarget,
                                             (int)(Player.Distance(kstarget) * 1000 / 1500)) <= actualHp
                                         select kstarget)
                {
                    Q.CastOnUnit(kstarget, PacketCast);
                    return;
                }
            }
             */

            if (Config.Item("AutoIgnite").GetValue<bool>())
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

        private static void AutoRKillable()
        {
            var Target = TargetSelector.GetTarget(1250 + (750 * R.Level), TargetSelector.DamageType.Physical);
            if (R.IsReady())
            {
                if (Target != null)
                {
                    if (R.IsInRange(Target) && ObjectManager.Player.GetSpellDamage(Target, SpellSlot.R) >= Target.Health)
                    {
                        var Packets = Config.Item("usePackets").GetValue<bool>();
                        R.Cast();
                        R.CastOnUnit(Target, Packets);
                    }
                }
            }
        }        

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
            var ruinedKing = ItemData.Blade_of_the_Ruined_King.GetItem();
            var youmuuSword = ItemData.Youmuus_Ghostblade.GetItem();
            var randuinOmen = ItemData.Randuins_Omen.GetItem();
            if (target == null) return;

            if (Q.IsReady() && Config.Item("QCombo").GetValue<bool>())
            {
                if (Q.GetPrediction(target).Hitchance >= HitChance.High)
                    Q.Cast(target.Position, PacketCast);
            }

            if (W.IsReady() && Config.Item("WCombo").GetValue<bool>())
            {
                W.CastOnUnit(Player, PacketCast);
            }

            if (E.IsReady() && Config.Item("ECombo").GetValue<bool>())
            {
                E.Cast(target, PacketCast);
            }

            if (Config.Item("useBlade").GetValue<bool>())
            {
                if (ruinedKing.IsOwned() && ruinedKing.IsReady() && ruinedKing.IsInRange(target))
                {
                    ruinedKing.Cast(target);
                }
            }

            if (Config.Item("useYoumuu").GetValue<bool>())
            {
                if (youmuuSword.IsOwned() && youmuuSword.IsReady())
                {
                    youmuuSword.Cast();
                }
            }

            if (Config.Item("useRanduin").GetValue<bool>())
            {
                if (randuinOmen.IsOwned() && randuinOmen.IsReady() && randuinOmen.IsInRange(target))
                {
                    randuinOmen.Cast();
                }
            }
        }

        public static void Harass(Obj_AI_Base target)
        {

            if (!Config.Item("harassKey").GetValue<KeyBind>().Active) return;
            if (target == null) return;

            var mana = Player.MaxMana * (Config.Item("harassMana").GetValue<Slider>().Value / 100.0);
            if (!(Player.Mana > mana)) return;

            if (Config.Item("QHarass").GetValue<bool>())
            {
                if (Q.GetPrediction(target).Hitchance >= HitChance.High)
                    Q.Cast(target.Position, PacketCast);
            }
        }

        private static void Farm()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (minions.Count > 0)
            {
                var minion = minions[0];

                if (Config.Item("qFarm").GetValue<bool>() && Q.IsReady() &&
                    minion.IsValidTarget(Q.Range))
                {
                    Q.CastOnUnit(minion, false);
                }
            }
        }

        private static void JungleClear()
        {

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (Q.IsReady() && Config.Item("qJungle").GetValue<bool>())
                {
                    Q.Cast(mob.Position);
                }

                if (E.IsReady() && Config.Item("eJungle").GetValue<bool>())
                {
                    E.CastOnUnit(mob);
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

        public static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;
            if (Q.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (E.IsReady())
            {
                dmg += Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            if (SmiteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SmiteSlot) == SpellState.Ready)
            {
                dmg += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Smite);
            }

            return (float)dmg;
        }
    }
}
