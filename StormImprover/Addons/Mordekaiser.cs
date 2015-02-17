#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;

using LeagueSharp.Common.Data;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

#endregion 

namespace StormImprover.Addons
{
    class Mordekaiser
    {
        public static Spell Q, W, E, R;

        public static List<Spell> SpellList = new List<Spell>();
        public static SpellSlot IgniteSlot;
        public static Menu _Menu;
        public static Orbwalking.Orbwalker Orbwalker;

        public static bool ultImprover = false;

        public static Items.Item Potion = new Items.Item(2003, 0);
        public static Items.Item ManaPotion = new Items.Item(2004, 0);

        public Mordekaiser()
        {
            Game_OnGameLoad();            
        }

        private static void Game_OnGameLoad()
        {
            Q = new Spell(SpellSlot.Q, 0f);

            W = new Spell(SpellSlot.W, 750f);

            E = new Spell(SpellSlot.E, 700f);
            E.SetSkillshot(0.25f, 90, 2000, false, SkillshotType.SkillshotCone);

            R = new Spell(SpellSlot.R, 850);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");

            _Menu = new Menu("Mordekaiser", "Mordekaiser", true);
            _Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _Menu.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(_Menu.SubMenu("Orbwalking"));

            _Menu.AddSubMenu(new Menu("Combo Settings", "Combo"));
            _Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            _Menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            _Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            _Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "If killable use R/Ign").SetValue(false));
            _Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(_Menu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            _Menu.AddSubMenu(new Menu("Potion Manager", "PotionManager"));
            _Menu.SubMenu("PotionManager").AddItem(new MenuItem("AutoPot", "Enable Auto-Pot").SetValue(true));

            _Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            _Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "Use Q").SetValue(true));
            _Menu.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "Use E").SetValue(true));
            _Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "Laneclear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            _Menu.AddSubMenu(new Menu("Items Settings", "Item"));
            _Menu.SubMenu("Item").AddItem(new MenuItem("uBilgewaterCutlass", "Use Bilgewater").SetValue(true));
            _Menu.SubMenu("Item").AddItem(new MenuItem("uHexTech", "Use Hextech").SetValue(true));

            _Menu.AddSubMenu(new Menu("Misc Settings", "Misc"));
            _Menu.SubMenu("Misc").AddItem(new MenuItem("WGapCloser", "Auto W on Gapcloser").SetValue(true));
            _Menu.SubMenu("Misc").AddItem(new MenuItem("eHarass", "Auto Harass w/ E").SetValue<KeyBind>(new KeyBind('T', KeyBindType.Toggle)));
            _Menu.SubMenu("Misc").AddItem(new MenuItem("Disrespecter", "Disrespect Mode").SetValue(false));
            _Menu.SubMenu("Misc").AddItem(new MenuItem("usePackets", "Packets").SetValue(false));

            _Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            _Menu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _Menu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _Menu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _Menu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            _Menu.AddToMainMenu();
            
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.AfterAttack += AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;

            // Damage Calculator
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;
        }

        private static void Combo()
        {

            if (ObjectManager.Player.Health >= 10)
                return; 

            var target = TargetSelector.GetTarget(1100, TargetSelector.DamageType.Magical);
            var BilgewaterCutlass = ItemData.Bilgewater_Cutlass.GetItem();
            var HexTech = ItemData.Hextech_Revolver.GetItem();

            Orbwalker.SetAttack(true);
            Orbwalker.SetMovement(true);

            if (_Menu.Item("uBilgewaterCutlass").GetValue<bool>() && target != null)
            {
                if (BilgewaterCutlass.IsOwned() && BilgewaterCutlass.IsReady() && BilgewaterCutlass.IsInRange(target))
                    BilgewaterCutlass.Cast(target);
            }

            if (_Menu.Item("uHexTech").GetValue<bool>() && target != null)
            {
                if (HexTech.IsOwned() && HexTech.IsReady() && BilgewaterCutlass.IsInRange(target))
                    HexTech.Cast(target);
            }

            if (_Menu.Item("UseW").GetValue<bool>() && target != null)
            {
                if (target.Distance(ObjectManager.Player.Position) <= 250 && W.IsReady())
                    W.Cast(ObjectManager.Player);
            }

            if (_Menu.Item("UseR").GetValue<bool>() && R.IsReady() && target.IsValidTarget(R.Range) && target != null)
            {
                if (target != null)
                {
                    if (target.Health + 30 > (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.2))
                    {
                        R.Cast(target);
                        ultImprover = true;

                        Utility.DelayAction.Add(40000, () => ultImprover = false);

                        var damage = IgniteSlot == SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready ? 0 : ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                        var targetHealth = target.Health;
                        var hasPots = Items.HasItem(ItemData.Health_Potion.Id) || Items.HasItem(ItemData.Crystalline_Flask.Id);
                        if (hasPots || target.HasBuff("RegenerationPotion", true))
                        {
                            if (damage * 0.5 > targetHealth)
                            {
                                if (IgniteSlot.IsReady())
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
                                }
                            }
                        }
                        else
                        {
                            if (IgniteSlot.IsReady() && damage > targetHealth)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
                            }
                        }
                    }
                }                                                     
            }   

            if (_Menu.Item("UseE").GetValue<bool>() && E.IsReady() && target.IsValidTarget(E.Range) && target != null)
            {
                E.CastIfHitchanceEquals(target, HitChance.High, _Menu.Item("usePackets").GetValue<bool>());
            }
        }

        static void LaneClear()
        {           
            var minionRanged = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range,
                MinionTypes.Ranged,
                MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (minionRanged.Count > 2)
            {
                var minions = minionRanged[2];
                if (_Menu.Item("UseELaneClear").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions);
                }
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {            
            if (_Menu.Item("LaneClearActive").GetValue<bool>())
            {
                var minionMelee = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range,
                MinionTypes.Melee,
                MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

                if (minionMelee.Count > 0)
                {
                    var minions = minionMelee[0];
                    if (_Menu.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range))
                    {
                        Q.Cast();
                    }
                }
            }

            if (_Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                if (_Menu.Item("UseQ").GetValue<bool>() && Q.IsReady())
                {
                    Q.Cast();
                }
            }
        }

        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_Menu.Item("WGapCloser").GetValue<bool>() && W.IsReady() && gapcloser.Sender.IsValidTarget(250f))
                W.Cast(ObjectManager.Player);
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (_Menu.Item("ComboActive").GetValue<KeyBind>().Active)
                Combo();
            if (_Menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                LaneClear();
            if (_Menu.Item("AutoPot").GetValue<bool>())
                AutoPot();
            if (_Menu.Item("eHarass").GetValue<KeyBind>().Active)
                eHrass();
        }

        private static void eHrass()
        {
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (E.IsReady() && eTarget.IsValidTarget(E.Range))
                E.Cast(eTarget);
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

        private static void AutoPot()
        {

            if (_Menu.Item("AutoPot").GetValue<bool>() && !ObjectManager.Player.InFountain() && !ObjectManager.Player.HasBuff("Recall"))
            {
                if (Potion.IsReady() && !ObjectManager.Player.HasBuff("RegenerationPotion", true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(700) > 0 && ObjectManager.Player.Health + 200 < ObjectManager.Player.MaxHealth)
                        Potion.Cast();
                    else if (ObjectManager.Player.Health < ObjectManager.Player.MaxHealth * 0.6)
                        Potion.Cast();
                }
                if (ManaPotion.IsReady() && !ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true))
                {
                    if (ObjectManager.Player.CountEnemiesInRange(1200) > 0 && ObjectManager.Player.Mana + 200 < ObjectManager.Player.MaxMana)
                        ManaPotion.Cast();
                }
            }
        }

        private static float ComboDamage(Obj_AI_Base target)
        {
            var dmg = 0d;

            if (Q.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);
            }

            if (E.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
            }

            if (R.IsReady())
            {
                dmg += ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
            }

            if (IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                dmg += ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }

            var BilgewaterCutlass = ItemData.Bilgewater_Cutlass.GetItem();
            var HexTech = ItemData.Hextech_Revolver.GetItem();

            if (HexTech.IsOwned(ObjectManager.Player) && HexTech.IsReady() && HexTech.IsInRange(target))
                dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Hexgun);

            if (BilgewaterCutlass.IsOwned(ObjectManager.Player) && BilgewaterCutlass.IsReady() && BilgewaterCutlass.IsInRange(target))
                dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Bilgewater);

            return (float)dmg;
        }

        private static void OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = _Menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if (args.NetworkId != ObjectManager.Player.NetworkId)
                return;

            if (_Menu.Item("Disrespecter").GetValue<bool>())
            {
                switch (args.EventId)
                {
                    case GameEventId.OnKill:
                        Game.Say("/laugh");
                        break;
                }
            }
        }
    }
}
