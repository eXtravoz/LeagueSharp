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
    class XinZhao
    {

        public static Obj_AI_Hero Player;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        public static SpellSlot IgniteSlot;

        private readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 0)},
	        {SpellSlot.W, new Spell(SpellSlot.W, 0)},
	        {SpellSlot.E, new Spell(SpellSlot.E, 650)},
	        {SpellSlot.R, new Spell(SpellSlot.R, 500)}
        };

        public XinZhao()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {                      

            Config = new Menu("Xin Zhao", "XinZhao", true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var SimpleTs = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(SimpleTs);
            Config.AddSubMenu(SimpleTs);

            var c = Config.AddSubMenu(new Menu("Combo", "Combo"));
            c.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("igniteCombo", "Use Ignite if Killable").SetValue(true));
            c.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var f = Config.AddSubMenu(new Menu("Farm", "Farm"));
            f.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            f.SubMenu("LaneClear").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(true));
            f.SubMenu("LaneClear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            f.SubMenu("LaneClear").AddItem(new MenuItem("FarmActive", "Farm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var l = Config.AddSubMenu(new Menu("JungleClear", "JClear"));
            l.SubMenu("JungleClear").AddItem(new MenuItem("UseQJC", "Use Q").SetValue(true));
            l.SubMenu("JungleClear").AddItem(new MenuItem("UseWJC", "Use W").SetValue(true));
            l.SubMenu("JungleClear").AddItem(new MenuItem("UseEJC", "Use E").SetValue(true));
            l.SubMenu("JungleClear").AddItem(new MenuItem("JCActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            var i = Config.AddSubMenu(new Menu("Items", "Items"));
            l.SubMenu("Items").AddItem(new MenuItem("UseHydra", "Use Hydra").SetValue(true));
            l.SubMenu("Items").AddItem(new MenuItem("UseTiamat", "Use Tiamat").SetValue(true));

            var k = Config.AddSubMenu(new Menu("KillSteal", "KillSteal"));
            k.SubMenu("KillSteal").AddItem(new MenuItem("stealActive", "Enable Killsteal").SetValue(true));
            k.SubMenu("KillSteal").AddItem(new MenuItem("UseEKs", "Use E").SetValue(true));
            k.SubMenu("KillSteal").AddItem(new MenuItem("UseRKs", "Use R").SetValue(true));

            var m = Config.AddSubMenu(new Menu("Misc", "Misc"));
            m.SubMenu("Misc").AddItem(new MenuItem("SpellInterrupt", "Interrupt Spells").SetValue(true));
            m.SubMenu("Misc").AddItem(new MenuItem("GapCloser", "Anti-GapCloser").SetValue(true));
            // m.SubMenu("Misc").AddItem(new MenuItem("UsePackets", "Packets").SetValue(true));

            var d = Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            d.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(true));
            d.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(true));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Game.PrintChat("<font color='0088FF'>eXtravoz :</font> Xin Zhao loaded!");

        }

        private void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("GapCloser").GetValue<bool>())
            {
                if (_spells[SpellSlot.R].IsReady())
                {
                    _spells[SpellSlot.R].Cast(gapcloser.Sender);
                }
            }
        }


        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {

            if (Config.Item("SpellInterrupt").GetValue<bool>())
            {
                if (Player.Distance(unit) < _spells[SpellSlot.R].Range && _spells[SpellSlot.R].IsReady())
                {
                    _spells[SpellSlot.R].Cast();
                }
            }
        }



        private void Drawing_OnDraw(EventArgs args)
        {

            var drawE = Config.Item("ERange").GetValue<bool>();
            var drawR = Config.Item("RRange").GetValue<bool>();

            if (drawE)
                Render.Circle.DrawCircle(Player.Position, _spells[SpellSlot.E].Range, Color.White);

            if (drawR)
                Render.Circle.DrawCircle(Player.Position, _spells[SpellSlot.R].Range, Color.White);

        }

        private void Game_OnGameUpdate(EventArgs args)
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

        private void JungleClear()
        {

            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            var mobs = MinionManager.GetMinions(Player.ServerPosition, _spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var minions = mobs[0];

                if (Config.Item("UseQJC").GetValue<bool>() && _spells[SpellSlot.Q].IsReady() &&
                    minions.IsValidTarget(_spells[SpellSlot.Q].Range))
                {
                    _spells[SpellSlot.Q].Cast();
                }

                if (Config.Item("UseWJC").GetValue<bool>() && _spells[SpellSlot.W].IsReady())
                {
                    _spells[SpellSlot.W].Cast();
                }

                if (Config.Item("UseEJC").GetValue<bool>() && _spells[SpellSlot.E].IsReady() && minions.IsValidTarget(_spells[SpellSlot.E].Range))
                {
                    _spells[SpellSlot.E].Cast(minions);
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

        private void LaneClear()
        {

            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            var minion = MinionManager.GetMinions(Player.ServerPosition, _spells[SpellSlot.E].Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minion.Count > 0)
            {
                var minions = minion[0];

                if (Config.Item("UseQFarm").GetValue<bool>() && _spells[SpellSlot.Q].IsReady() &&
                    minions.IsValidTarget(_spells[SpellSlot.Q].Range))
                {
                    _spells[SpellSlot.Q].Cast();
                }

                if (Config.Item("UseWFarm").GetValue<bool>() && _spells[SpellSlot.W].IsReady())
                {
                    _spells[SpellSlot.W].Cast();
                }

                if (Config.Item("UseEFarm").GetValue<bool>() && _spells[SpellSlot.E].IsReady() && minions.IsValidTarget(_spells[SpellSlot.E].Range))
                {
                    _spells[SpellSlot.E].Cast(minions);
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

        private void Combo()
        {

            var Target = TargetSelector.GetTarget(_spells[SpellSlot.E].Range, TargetSelector.DamageType.Physical);
            var Tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            var Hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();

            if (Target != null)
            {
                return;
            }

            if (Config.Item("UseECombo").GetValue<bool>())
            {
                if (_spells[SpellSlot.E].IsReady() && Target.IsValidTarget(_spells[SpellSlot.E].Range))
                {
                    _spells[SpellSlot.E].Cast(Target);
                }
            }

            if (Config.Item("UseQCombo").GetValue<bool>())
            {
                if (_spells[SpellSlot.Q].IsReady() && Target.IsValidTarget(_spells[SpellSlot.Q].Range))
                {
                    _spells[SpellSlot.Q].Cast();
                }
            }

            if (Config.Item("UseW").GetValue<bool>())
            {
                if (_spells[SpellSlot.W].IsReady() && Target.IsValidTarget(_spells[SpellSlot.W].Range))
                {
                    _spells[SpellSlot.W].Cast();
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

        private void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(_spells[SpellSlot.R].Range)))
            {
                if (Config.Item("UseRKs").GetValue<bool>())
                {
                    if (_spells[SpellSlot.R].IsReady() &&
                        hero.Distance(ObjectManager.Player) <= _spells[SpellSlot.R].Range &&
                        Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                    {
                        _spells[SpellSlot.R].Cast();
                    }
                }

                if (Config.Item("UseEKs").GetValue<bool>())
                {
                    if (_spells[SpellSlot.E].IsReady() &&
                        hero.Distance(ObjectManager.Player) <= _spells[SpellSlot.E].Range &&
                        Player.GetSpellDamage(hero, SpellSlot.E) >= hero.Health)
                    {
                        _spells[SpellSlot.E].Cast();
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