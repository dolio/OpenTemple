
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(536)]
    public class WindWall : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Wind Wall OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Wind Wall OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var target_item = spell.Targets[0];
            // put sp-Wind Wall condition on target
            var spell_obj_partsys_id = AttachParticles("sp-Wind Wall", target_item.Object);
            target_item.Object.AddCondition("sp-Wind Wall", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
        }
        // target_item.obj.condition_add_arg_x( 3, spell_obj_partsys_id )
        // objectevent_id = target_item.obj.condition_get_arg_x( 2 )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Wind Wall OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Wind Wall OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Wind Wall OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Wind Wall OnSpellStruck");
        }

    }
}
