

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
    class Nautilus
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

        public Nautilus()
        {
            Q.SetSkillshot(0.25f, 90f, 2000f, true, SkillshotType.SkillshotLine);
            Game_OnGameLoad();
        }

        private void Game_OnGameLoad()
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

            Config = new Menu("JungleAIO: Nautilus", "Nautilus", true);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var SimpleTs = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(SimpleTs);
            Config.AddSubMenu(SimpleTs);

            SmiteSlot = ObjectManager.Player.GetSpellSlot("SummonerSmite");

            Config.AddSubMenu(new Menu("Combo Settings", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind("K".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleClear", "JClear"));
            Config.SubMenu("JClear").AddItem(new MenuItem("UseWJC", "Use W").SetValue(true));
            Config.SubMenu("JClear").AddItem(new MenuItem("UseEJC", "Use E").SetValue(true));
            Config.SubMenu("JClear").AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            Config.SubMenu("JClear").AddItem(new MenuItem("JCActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LClear").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
            Config.SubMenu("LClear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            Config.SubMenu("LClear").AddItem(new MenuItem("LClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Misc Settings", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("WGapCloser", "Auto-W on Gapcloser").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Use Packets").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings Settings", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));

            Config.AddToMainMenu();

            Game.OnGameUpdate += JungleAIO_OnGameUpdate;
            Drawing.OnDraw += JungleAIO_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += JungleAIO_OnOnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
        }

        
        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Config.Item("InterruptSpells").GetValue<bool>())
            {
                if (Q.IsInRange(unit) && Q.IsReady() && unit.IsEnemy)
                {
                    var Packets = Config.Item("usePackets").GetValue<bool>();
                    Q.CastOnUnit(unit, Packets);
                }
            }
        }
         

        private void JungleAIO_OnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("WGapCloser").GetValue<bool>())
            {
                W.Cast();
            }
        }

        
        private void JungleAIO_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (Config.Item("InterruptSpells").GetValue<bool>())
            {
                if (Q.IsInRange(unit) && Q.IsReady() && unit.IsEnemy)
                {
                    var Packets = Config.Item("usePackets").GetValue<bool>();
                    Q.CastOnUnit(unit, Packets);
                }
            }
        }
        

        private void JungleAIO_OnDraw(EventArgs args)
        {
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                if (Q.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.White);
                }
            }

            if (Config.Item("DrawE").GetValue<bool>())
            {
                if (E.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
                }
            }

            if (Config.Item("DrawR").GetValue<bool>())
            {
                if (R.Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, Color.White);
                }
            }
        }

        private void JungleAIO_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();

            if (Config.Item("LClearActive").GetValue<KeyBind>().Active)
                LaneClear();

            if (Config.Item("JClearActive").GetValue<KeyBind>().Active)
                JungleClear();

            if (Config.Item("AutoSmite").GetValue<KeyBind>().Active)
                AutoSmite();
        }

        private void AutoSmite()
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

        private void JungleClear()
        {
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (W.IsReady() && Config.Item("UseWJC").GetValue<bool>())
                {
                    W.Cast();
                }

                if (E.IsReady() && Config.Item("UseEJC").GetValue<bool>())
                {
                    E.Cast();
                }
            }
        }

        private void LaneClear()
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
            if (minions.Count > 0)
            {
                var minion = minions[0];

                if (Config.Item("UseWFarm").GetValue<bool>() && W.IsReady())
                {
                    W.Cast();
                }

                if (Config.Item("UseEFarm").GetValue<bool>() && E.IsReady())
                {
                    E.Cast();
                }
            }
        }

        private void Combo()
        {
            var Packets = Config.Item("usePackets").GetValue<bool>();
            var randuinOmen = ItemData.Randuins_Omen.GetItem();
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Target != null)
            {
                if (Config.Item("UseQCombo").GetValue<bool>())
                {
                    if (Q.IsReady() && Q.IsInRange(Target) && Target.IsValidTarget(Q.Range))
                    {

                        if (Q.GetPrediction(Target).Hitchance >= HitChance.High)
                            Q.Cast(Target.Position, Packets);
                    }
                }

                if (Config.Item("UseWCombo").GetValue<bool>())
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                    }
                }

                if (Config.Item("UseECombo").GetValue<bool>())
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
