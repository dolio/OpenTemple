
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

[SpellScript(555)]
public class AcidSplash : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Acid Splash OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Acid Splash OnSpellEffect");
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Acid Splash OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Acid Splash OnBeginProjectile");
        SetProjectileParticles(projectile, AttachParticles("sp-Melfs Acid Arrow Projectile", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Acid Splash OnEndProjectile");
        var damage_dice = Dice.D3;

        spell.duration = 0;

        EndProjectileParticles(projectile);
        var target_item = spell.Targets[0];

        var return_val = spell.caster.PerformTouchAttack(target_item.Object);

        if (return_val == D20CAF.HIT)
        {
            AttachParticles("sp-Acid Splash-Hit", target_item.Object);
            target_item.Object.DealSpellDamage(spell.caster, DamageType.Acid, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
        }
        else if (return_val == D20CAF.CRITICAL)
        {
            AttachParticles("sp-Acid Splash-Hit", target_item.Object);
            damage_dice = damage_dice.WithCount(2);
            target_item.Object.DealSpellDamage(spell.caster, DamageType.Cold, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
        }
        else
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
            AttachParticles("Fizzle", target_item.Object);
        }

        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Acid Splash OnEndSpellCast");
    }


}