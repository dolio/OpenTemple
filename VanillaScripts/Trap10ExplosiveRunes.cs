
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

namespace VanillaScripts;

[ObjectScript(32009)]
public class Trap10ExplosiveRunes : BaseObjectScript
{

    public override bool OnTrap(TrapSprungEvent trap, GameObject triggerer)
    {
        AttachParticles(trap.Type.ParticleSystemId, trap.Object);
        foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
        {
            if ((obj.DistanceTo(trap.Object) <= 10))
            {
                if ((obj.HasLineOfSight(trap.Object)))
                {
                    foreach (var dmg in trap.Type.Damage)
                    {
                        if ((dmg.Type == DamageType.Poison))
                        {
                            if ((!obj.SavingThrow(15, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                            {
                                obj.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                            }

                        }
                        else if ((obj == triggerer))
                        {
                            obj.Damage(trap.Object, dmg.Type, dmg.Dice);
                        }
                        else
                        {
                            obj.ReflexSaveAndDamage(trap.Object, 15, D20SavingThrowReduction.Half, D20SavingThrowFlag.SPELL_DESCRIPTOR_FORCE, dmg.Dice, dmg.Type, D20AttackPower.NORMAL);
                        }

                    }

                }

            }

        }

        return SkipDefault;
    }


}