
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

namespace Scripts;

[ObjectScript(32000)]
public class BasicTrap : BaseObjectScript
{
    public override bool OnTrap(TrapSprungEvent trap, GameObject triggerer)
    {
        AttachParticles(trap.Type.ParticleSystemId, trap.Object);
        foreach (var dmg in trap.Type.Damage)
        {
            triggerer.Damage(trap.Object, dmg.Type, dmg.Dice);
        }

        return SkipDefault;
    }

}