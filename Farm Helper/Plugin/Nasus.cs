using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

namespace FarmHelper.Plugin
{
    class Nasus
    {
        public static Menu _config;
        private readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 0f)},
        };

        public Nasus()
        {
            _spells[SpellSlot.Q].SetTargetted(0.5f, 0f);
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            var fm = _config = new Menu("Nasus FHelper", "FMNasus", true);
            fm.SubMenu("FMNasus").AddItem(new MenuItem("fmEnable", "Enabled").SetValue(true));
            fm.SubMenu("FMNasus").AddItem(new MenuItem("fmEnableQ", "Use Q").SetValue(true));

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (_config.Item("fmEnable").GetValue<bool>())
                FMQMinion();

        }

        private void FMQMinion()
        {
            foreach (
                    var minion in
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(minion => minion.Team != ObjectManager.Player.Team)
                    )
            {
                if (minion.Health < _spells[SpellSlot.Q].GetDamage(minion) && _spells[SpellSlot.Q].IsReady() && minion.IsValidTarget(_spells[SpellSlot.Q].Range))
                {
                    if (_config.Item("fmEnableQ").GetValue<bool>())
                    {
                        _spells[SpellSlot.Q].Cast(minion);
                    }
                }
            }

        }
    }
}
