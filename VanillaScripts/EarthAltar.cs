
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

[ObjectScript(227)]
public class EarthAltar : BaseObjectScript
{

    public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
    {
        if (((triggerer.GetNameId() == 1203) && (!GetGlobalFlag(109))))
        {
            UiSystems.CharSheet.Hide();
            attachee.Destroy();
            SetGlobalFlag(109, true);
            AttachParticles("DesecrateEarth", triggerer);
        }

        return RunDefault;
    }


}