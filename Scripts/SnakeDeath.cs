
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

[ObjectScript(239)]
public class SnakeDeath : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((attachee.GetMap() == 5066))
        {
            SetGlobalFlag(117, true);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5066))
        {
            SetGlobalFlag(117, false);
        }

        return RunDefault;
    }

}