using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

namespace FarmHelper.Plugin
{
    class Annie
    {
        public static Menu _config;
        private readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 625f)},
        };

        public Annie()
        {
            _spells[SpellSlot.Q].SetTargetted(0.25f, 1400);
            Game_OnGameLoad;
        }

        private void Game_OnGameLoad()
        {
            var fm = _config = new Menu("Annie FHelper", "FMAnnie", true);
            fm.SubMenu("FMAnnie").AddItem(new MenuItem("fmEnable", "Enabled").SetValue(true));
            fm.SubMenu("FMAnnie").AddItem(new MenuItem("fmEnableQ", "Use Q").SetValue(true));

            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {

            if (_config.Item("fmEnable").GetValue<bool>())
                FMQMinion();
            
        }

        private void FMQMinion()
        {
            if (_config.Item("fmEnableQ").GetValue<bool>())
            {
                var kMinion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spells[SpellSlot.Q].Range).Find(m => _spells[SpellSlot.Q].IsKillable(m));
                if (kMinion.IsValidTarget(_spells[SpellSlot.Q].Range))
                {
                    _spells[SpellSlot.Q].Cast();
                }
            }
            
        }      
    }    
}
