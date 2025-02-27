
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

[ObjectScript(81)]
public class Spugnoir : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(819)))
        {
            attachee.Attack(triggerer);
            return SkipDefault;
        }

        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 100); // spugnoir in party
        }
        else if ((GetGlobalVar(913) == 32))
        {
            triggerer.BeginDialog(attachee, 140); // have attacked 3 or more farm animals with spugnoir in party
        }
        else if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
        {
            attachee.FloatLine(11004, triggerer); // have lawbreaker or convict or banished from hommlet rep
        }
        else
        {
            triggerer.BeginDialog(attachee, 1); // none of the above
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(712, 0);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        attachee.FloatLine(12014, triggerer);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        Logger.Info("Spugnoir Enter Combat");
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(712) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()) && !PartyAlignment.IsGood())
        {
            attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
            attachee.PendingSpellsToMemorized();
            SetGlobalVar(712, 1);
        }

        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((GetGlobalVar(913) >= 3))
            {
                if ((attachee != null))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(attachee);
                        attachee.FloatLine(22000, triggerer);
                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        Utilities.create_item_in_inventory(4645, attachee);
        Utilities.create_item_in_inventory(4647, attachee);
        Utilities.create_item_in_inventory(4224, attachee);
        Utilities.create_item_in_inventory(12848, attachee);
        DetachScript();
        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetMap() == 5006) && (GetGlobalVar(695) == 1 || GetGlobalVar(695) == 2)))
        {
            attachee.FloatLine(12070, triggerer);
        }
        else if (((attachee.GetMap() == 5024) && (!Utilities.is_daytime())))
        {
            attachee.FloatLine(10019, triggerer);
        }

        return RunDefault;
    }
    public static bool equip_transfer(GameObject attachee, GameObject triggerer)
    {
        var itemA = attachee.FindItemByName(6081);
        if ((itemA != null))
        {
            itemA.Destroy();
            Utilities.create_item_in_inventory(6081, triggerer);
        }

        var itemB = attachee.FindItemByName(6023);
        if ((itemB != null))
        {
            itemB.Destroy();
            Utilities.create_item_in_inventory(6023, triggerer);
        }

        var itemC = attachee.FindItemByName(4060);
        if ((itemC != null))
        {
            itemC.Destroy();
            Utilities.create_item_in_inventory(4060, triggerer);
        }

        Utilities.create_item_in_inventory(7001, attachee);
        return RunDefault;
    }

}