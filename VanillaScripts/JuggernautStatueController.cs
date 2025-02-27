
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

[ObjectScript(232)]
public class JuggernautStatueController : BaseObjectScript
{

    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        foreach (var statue in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
        {
            if ((statue.GetNameId() == 1618))
            {
                var loc = statue.GetLocation();

                var rot = statue.Rotation;

                statue.Destroy();
                var juggernaut = GameSystems.MapObject.CreateObject(14426, loc);

                juggernaut.Rotation = rot;

                AttachParticles("ef-MinoCloud", juggernaut);
                attachee.Destroy();
                return SkipDefault;
            }

        }

        return SkipDefault;
    }


}