using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = System.Drawing.Color;

using LeagueSharp.Common;
using LeagueSharp;
using LeagueSharp.Common.Data;

namespace JungleAIO.Champions
{
    class Nocturne
    {

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        private static SpellSlot IgniteSlot;
        private static SpellSlot SmiteSlot;

        private readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
	        {SpellSlot.Q, new Spell(SpellSlot.Q, 1125)},
	        {SpellSlot.W, new Spell(SpellSlot.W, 0)},
	        {SpellSlot.E, new Spell(SpellSlot.E, 425)},
	        {SpellSlot.R, new Spell(SpellSlot.R, 2000)}
        };

        public Nocturne()
        {
            _spells[SpellSlot.Q].SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            _spells[SpellSlot.E].SetTargetted(0.5f, 1700f);
            _spells[SpellSlot.R].SetTargetted(0.75f, 2500f);

            CustomEvents.Game.OnGameLoad += JungleAIO_Load;           
        }

        private void JungleAIO_Load(EventArgs args)
        {
            Config = new Menu("JungleAIO: Nocturne", "Nocturne", true);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            var SimpleTs = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(SimpleTs);
            Config.AddSubMenu(SimpleTs);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            SmiteSlot = ObjectManager.Player.GetSpellSlot("SummonerSmite");

            Config.AddSubMenu(new Menu("JungleAIO: Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoIgnite", "Auto Ignite").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleAIO: Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleAIO: JClear", "JClear"));
            Config.SubMenu("JClear").AddItem(new MenuItem("UseQJC", "Use Q").SetValue(true));
            Config.SubMenu("JClear").AddItem(new MenuItem("UseEJC", "Use E").SetValue(true));
            Config.SubMenu("JClear").AddItem(new MenuItem("AutoSmite", "Auto Smite").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            Config.SubMenu("JClear").AddItem(new MenuItem("JCActive", "JungleClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleAIO: LClear", "LaneClear"));
            Config.SubMenu("LClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            Config.SubMenu("LClear").AddItem(new MenuItem("LClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleAIO: Itens", "Itens"));
            Config.SubMenu("Itens").AddItem(new MenuItem("useBlade", "Blade of the Ruined King").SetValue(true));
            Config.SubMenu("Itens").AddItem(new MenuItem("useYoumuu", "Youmuu's Ghostblade").SetValue(true));
            Config.SubMenu("Itens").AddItem(new MenuItem("useRanduin", "Randuin's Omen").SetValue(true));
            
            Config.AddSubMenu(new Menu("JungleAIO: Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("WGapCloser", "Auto-W on Gapcloser").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoRKillable", "Auto R if target killable").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Use Packets").SetValue(true));

            Config.AddSubMenu(new Menu("Drawing", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Draw Q").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            Config.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "Draw R on Minimap").SetValue(new Circle(false, Color.FromArgb(150, Color.DodgerBlue))));

            Config.AddToMainMenu();

            Game.OnGameUpdate += JungleAIO_OnGameUpdate;
            Drawing.OnDraw += JungleAIO_OnDraw;
            Drawing.OnEndScene += JungleAIO_OnEndScene;
            Interrupter.OnPossibleToInterrupt += JungleAIO_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += JungleAIO_OnOnEnemyGapcloser;
                    
        }

        private void JungleAIO_OnOnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Config.Item("WGapCloser").GetValue<bool>())
            {
                _spells[SpellSlot.W].Cast();
            }
        }

        private void JungleAIO_OnPossibleToInterrupt(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>())
            {   // if (_spells[SpellSlot.E].IsInRange(unit)
                if (_spells[SpellSlot.E].IsInRange(unit) && _spells[SpellSlot.E].IsReady() && unit.IsEnemy)
                {
                    var Packets = Config.Item("usePackets").GetValue<bool>();                    
                    _spells[SpellSlot.E].CastOnUnit(unit, Packets);
                }
            }
        }

        private void JungleAIO_OnEndScene(EventArgs args)
        {
            if (_spells[SpellSlot.R].Level > 0)
            {
                var rDraw = Config.Item("DrawR").GetValue<Circle>();
                if (rDraw.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, 1250 + (750 * _spells[SpellSlot.R].Level), rDraw.Color, 2, 30, true);
                }
            }
        }

        private void JungleAIO_OnDraw(EventArgs args)
        {
            if (Config.Item("DrawQ").GetValue<bool>())
            {
                if(_spells[SpellSlot.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spells[SpellSlot.Q].Range, Color.White);
                }
            }

            if (Config.Item("DrawE").GetValue<bool>())
            {
                if (_spells[SpellSlot.E].Level > 0)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _spells[SpellSlot.E].Range, Color.White);
                }
            }
        }

        private void JungleAIO_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                Harass();

            if (Config.Item("LClearActive").GetValue<KeyBind>().Active)
                LaneClear();

            if (Config.Item("JClearActive").GetValue<KeyBind>().Active)
                JungleClear();

            if (Config.Item("AutoSmite").GetValue<KeyBind>().Active)
                AutoSmite();
            if (Config.Item("AutoRKillable").GetValue<bool>())
                AutoRKillable();
           
        }

        private void AutoRKillable()
        {
            var Target = TargetSelector.GetTarget(1250 + (750 * _spells[SpellSlot.R].Level), TargetSelector.DamageType.Physical);
            if (_spells[SpellSlot.R].IsReady())
            {
                if (Target != null)
                {
                    if (_spells[SpellSlot.R].IsInRange(Target) && ObjectManager.Player.GetSpellDamage(Target, SpellSlot.E) >= Target.Health)
                    {
                        var Packets = Config.Item("usePackets").GetValue<bool>();
                        _spells[SpellSlot.R].Cast();
                        _spells[SpellSlot.R].CastOnUnit(Target, Packets);
                    }
                }
            }
        }

        private void AutoIgnite(Obj_AI_Hero unit)
        {
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

        private void AutoSmite()
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

        private void JungleClear()
        {

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spells[SpellSlot.Q].Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (_spells[SpellSlot.Q].IsReady() && Config.Item("UseQJC").GetValue<bool>())
                {
                    _spells[SpellSlot.Q].Cast(mob.Position);
                }
                
                if (_spells[SpellSlot.E].IsReady() && Config.Item("UseEJC").GetValue<bool>())
                {
                    _spells[SpellSlot.E].CastOnUnit(mob);
                }
            }

        }

        private void LaneClear()
        {

            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spells[SpellSlot.Q].Range, MinionTypes.All, MinionTeam.NotAlly);            
            if (minions.Count > 0)
            {
                var minion = minions[0];

                if (Config.Item("UseQFarm").GetValue<bool>() && _spells[SpellSlot.Q].IsReady() &&
                    minion.IsValidTarget(_spells[SpellSlot.Q].Range))
                {
                    _spells[SpellSlot.Q].CastOnUnit(minion, false);
                }               
            }
            
        }

        private void Harass()
        {
            var Packets = Config.Item("usePackets").GetValue<bool>();
            var Target = TargetSelector.GetTarget(_spells[SpellSlot.Q].Range, TargetSelector.DamageType.Physical);

            if (Target != null)
            {
                if (Config.Item("UseQCombo").GetValue<bool>())
                {
                    if (_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.Q].IsInRange(Target) && Target.IsValidTarget(_spells[SpellSlot.Q].Range))
                    {

                        if (_spells[SpellSlot.Q].GetPrediction(Target).Hitchance >= HitChance.High)
                            _spells[SpellSlot.Q].Cast(Target.Position, Packets);
                    }
                }
            }
        }

        private void Combo()
        {
            var Packets = Config.Item("usePackets").GetValue<bool>();
            var ruinedKing = ItemData.Blade_of_the_Ruined_King.GetItem();
            var youmuuSword = ItemData.Youmuus_Ghostblade.GetItem();
            var randuinOmen = ItemData.Randuins_Omen.GetItem();
            var Target = TargetSelector.GetTarget(_spells[SpellSlot.Q].Range , TargetSelector.DamageType.Physical);
            if (Target != null)
            {
                if (Config.Item("UseQCombo").GetValue<bool>())
                {
                    if (_spells[SpellSlot.Q].IsReady() && _spells[SpellSlot.Q].IsInRange(Target) && Target.IsValidTarget(_spells[SpellSlot.Q].Range))
                    {              
                
                        if (_spells[SpellSlot.Q].GetPrediction(Target).Hitchance >= HitChance.High)
                            _spells[SpellSlot.Q].Cast(Target.Position, Packets);             
                    }
                }

                if (Config.Item("UseWCombo").GetValue<bool>())
                {
                    if (_spells[SpellSlot.W].IsReady())
                    {
                        _spells[SpellSlot.W].CastOnUnit(ObjectManager.Player);
                    }
                }

                if (Config.Item("UseECombo").GetValue<bool>())
                {
                    if (_spells[SpellSlot.E].IsReady() && _spells[SpellSlot.E].IsInRange(Target) && Target.IsValidTarget(_spells[SpellSlot.E].Range))
                    {
                        _spells[SpellSlot.E].CastOnUnit(Target, Packets);
                    }
                }

                if (Config.Item("useBlade").GetValue<bool>())
                {
                    if (ruinedKing.IsOwned() && ruinedKing.IsReady() && ruinedKing.IsInRange(Target))
                    {
                        ruinedKing.Cast(Target);
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
                    if (randuinOmen.IsOwned() && randuinOmen.IsReady() && randuinOmen.IsInRange(Target))
                    {
                        randuinOmen.Cast();
                    }
                }
            }
        }
    }    
}
