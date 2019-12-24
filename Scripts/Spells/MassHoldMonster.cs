
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
    [SpellScript(585)]
    public class MassHoldMonster : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mass Hold Monster OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Mass Hold Monster OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            foreach (var target_item in spell.Targets)
            {
                if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
                {
                    // allow Will saving throw to negate
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target_item.Object);
                        spell.RemoveTarget(target_item.Object);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        // HTN - apply condition HOLD (paralyzed)
                        target_item.Object.AddCondition("sp-Hold Monster", spell.spellId, spell.duration, 0);
                        target_item.ParticleSystem = AttachParticles("sp-Hold Monster", target_item.Object);
                    }

                }
                else
                {
                    // not a critter
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 31001);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Mass Hold Monster OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mass Hold Monster OnEndSpellCast");
        }

    }
}
