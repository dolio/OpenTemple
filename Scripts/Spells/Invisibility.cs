
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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

namespace Scripts.Spells;

[SpellScript(253)]
public class Invisibility : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Invisibility OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-illusion-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Invisibility OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;
        var target = spell.Targets[0];
        // check if target is friendly (willing target)
        if (target.Object.IsFriendly(spell.caster))
        {
            // HTN - apply condition INVISIBLE
            if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
            {
                target.Object.AddCondition("sp-Invisibility", spell.spellId, spell.duration, 0);
                target.ParticleSystem = AttachParticles("sp-Invisibility", target.Object);
            }
            else
            {
                // not a critter
                target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target.Object.FloatMesFileLine("mes/spell.mes", 31001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

        }
        else
        {
            // HTN - apply condition INVISIBLE
            if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
            {
                // allow Will saving throw to negate
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target.Object);
                    spell.RemoveTarget(target.Object);
                }
                else
                {
                    // saving throw unsuccessful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target.Object.AddCondition("sp-Invisibility", spell.spellId, spell.duration, 0);
                    target.ParticleSystem = AttachParticles("sp-Invisibility", target.Object);
                }

            }
            else
            {
                // not a critter
                target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target.Object.FloatMesFileLine("mes/spell.mes", 31001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Invisibility OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Invisibility OnEndSpellCast");
    }

}