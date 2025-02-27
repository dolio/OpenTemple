
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

[SpellScript(560)]
public class CallLightningStorm : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Call Lightning Storm OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Call Lightning Storm OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 10 * spell.casterLevel;
        // check if outdoors
        Dice dam;
        if ((GameSystems.Map.IsCurrentMapOutdoors()))
        {
            dam = Dice.Parse("5d10");
        }
        else
        {
            dam = Dice.Parse("5d6");
        }

        // play fx
        GameSystems.Vfx.CallLightning(spell.aoeCenter);
        // damage all initial targets
        foreach (var target_item in spell.Targets)
        {
            AttachParticles("sp-Call Lightning", target_item.Object);
            if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                // saving throw unsuccessful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            }

            remove_list.Add(target_item.Object);
        }

        spell.RemoveTargets(remove_list);
        // spell.spell_end( spell.id )
        // add call-lightning condition, which allows additional bolts to be called
        spell.caster.AddCondition("sp-Call Lightning Storm", spell.spellId, spell.duration, (spell.casterLevel - 1));
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Call Lightning Storm OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Call Lightning Storm OnEndSpellCast");
    }

}