
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

[SpellScript(152)]
public class Enlarge : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Enlarge OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Enlarge OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;

        var target_item = spell.Targets[0];

        if ((target_item.Object.IsMonsterCategory(MonsterCategory.humanoid)))
        {
            if (target_item.Object.IsFriendly(spell.caster))
            {
                var return_val = target_item.Object.AddCondition("sp-Enlarge", spell.spellId, spell.duration, 0);

                if (return_val)
                {
                    target_item.ParticleSystem = AttachParticles("sp-Enlarge", target_item.Object);

                }

            }
            else
            {
                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    var return_val = target_item.Object.AddCondition("sp-Enlarge", spell.spellId, spell.duration, 0);

                    if (return_val)
                    {
                        target_item.ParticleSystem = AttachParticles("sp-Enlarge", target_item.Object);

                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }

            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Enlarge OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Enlarge OnEndSpellCast");
    }


}