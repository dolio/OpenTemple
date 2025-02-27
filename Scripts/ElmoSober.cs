
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

[ObjectScript(270)]
public class ElmoSober : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((GetGlobalVar(900) == 32))
        {
            triggerer.BeginDialog(attachee, 190); // have attacked 3 or more farm animals with elmo in party
        }
        else if ((attachee.HasMet(triggerer)))
        {
            if ((attachee.GetLeader() == null))
            {
                triggerer.BeginDialog(attachee, 90); // have met and elmo not in party
            }

            triggerer.BeginDialog(attachee, 200); // elmo in party
        }
        else
        {
            triggerer.BeginDialog(attachee, 1); // have not met
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        attachee.FloatLine(12014, triggerer);
        SetGlobalFlag(934, true);
        if ((!GetGlobalFlag(236)))
        {
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(92);
            }

        }
        else
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(934, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            // if (game.global_vars[900] >= 3):
            // if (attachee != OBJ_HANDLE_NULL):
            // leader = attachee.leader_get()
            // if (leader != OBJ_HANDLE_NULL):
            // leader.follower_remove(attachee)
            // attachee.float_line(22000,triggerer)
            // else:
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    if ((!attachee.HasMet(obj)))
                    {
                        obj.BeginDialog(attachee, 1);
                    }

                }

            }

        }

        // game.new_sid = 0
        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(236, true);
        Logger.Info("elmo joins");
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(236, false);
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        var randy1 = RandomRange(1, 16);
        if (((attachee.GetMap() == 5052 || attachee.GetMap() == 5007) && randy1 >= 15))
        {
            var leader = attachee.GetLeader();
            if ((leader != null))
            {
                if ((!GetGlobalFlag(934)))
                {
                    attachee.FloatLine(12200, triggerer);
                }

            }

        }

        return RunDefault;
    }
    public static bool make_otis_talk(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8014);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 320);
        }

        return SkipDefault;
    }
    public static bool make_lila_talk(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 14001);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 320);
        }

        return SkipDefault;
    }
    public static bool make_Fruella_talk(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 14037);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }

        return SkipDefault;
    }
    public static bool make_saduj_talk(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 14689);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }

        return SkipDefault;
    }
    public static bool switch_to_thrommel(GameObject attachee, GameObject triggerer)
    {
        var npc = Utilities.find_npc_near(attachee, 8031);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, 40);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 570);
        }

        return SkipDefault;
    }
    // edited by darmagon for sober elmo

    public static bool elmo_joins_first_time(GameObject attachee, GameObject triggerer, bool sober)
    {
        // def elmo_joins_first_time( attachee, triggerer, sober ):	#edited by darmagon for sober elmo
        GameObject new_elmo;
        if (sober)
        {
            var loc = attachee.GetLocation();
            var rot = attachee.Rotation;
            attachee.Destroy();
            new_elmo = GameSystems.MapObject.CreateObject(14723, loc);
            new_elmo.Rotation = rot;
        }
        else
        {
            new_elmo = attachee;
        }

        triggerer.AdjustMoney(-20000);
        var rchain = Utilities.create_item_in_inventory(6049, new_elmo);
        rchain.SetItemFlag(ItemFlag.NO_TRANSFER);
        var mshield = Utilities.create_item_in_inventory(6051, new_elmo);
        mshield.SetItemFlag(ItemFlag.NO_TRANSFER);
        var maxe = Utilities.create_item_in_inventory(4098, new_elmo);
        maxe.SetItemFlag(ItemFlag.NO_TRANSFER);
        var magd = attachee.FindItemByName(4058);
        if (magd != null)
        {
            magd.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        new_elmo.WieldBestInAllSlots();
        if (sober)
        {
            triggerer.BeginDialog(new_elmo, 70);
        }

        return SkipDefault;
    }
    public static void equip_transfer(GameObject attachee, GameObject triggerer)
    {
        var rchain = attachee.FindItemByName(6049);
        if (rchain != null)
        {
            rchain.ClearItemFlag(ItemFlag.NO_TRANSFER);
        }

        var mshield = attachee.FindItemByName(6051);
        if (mshield != null)
        {
            mshield.ClearItemFlag(ItemFlag.NO_TRANSFER);
        }

        var maxe = attachee.FindItemByName(4098);
        if (maxe != null)
        {
            maxe.ClearItemFlag(ItemFlag.NO_TRANSFER);
        }

        return;
    }

}