
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Scripts.Dialog;

[DialogScript(117)]
public class LodrissDialog : Lodriss, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
            case 4:
                originalScript = "not npc.has_met( pc )";
                return !npc.HasMet(pc);
            case 5:
            case 8:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 6:
            case 7:
                originalScript = "check_skole(npc) == 1";
                return check_skole(npc) == 1;
            case 15:
            case 16:
            case 22:
            case 23:
                originalScript = "game.quests[42].state >= qs_mentioned";
                return GetQuestState(42) >= QuestState.Mentioned;
            case 17:
            case 121:
                originalScript = "game.global_flags[97] == 1";
                return GetGlobalFlag(97);
            case 67:
            case 68:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 9";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9;
            case 81:
            case 82:
            case 106:
            case 107:
            case 135:
            case 261:
            case 262:
                originalScript = "game.quests[42].state == qs_accepted";
                return GetQuestState(42) == QuestState.Accepted;
            case 83:
            case 84:
            case 241:
                originalScript = "anyone( pc.group_list(), \"has_item\", 5804 )";
                return pc.GetPartyMembers().Any(o => o.HasItemByName(5804));
            case 85:
            case 86:
                originalScript = "game.quests[42].state == qs_mentioned";
                return GetQuestState(42) == QuestState.Mentioned;
            case 103:
            case 104:
                originalScript = "game.quests[42].state == qs_mentioned or game.quests[42].state == qs_accepted";
                return GetQuestState(42) == QuestState.Mentioned || GetQuestState(42) == QuestState.Accepted;
            case 122:
            case 123:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 11";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
            case 181:
            case 191:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
            case 301:
            case 302:
                originalScript = "game.global_flags[102] == 1";
                return GetGlobalFlag(102);
            case 303:
            case 304:
                originalScript = "game.global_flags[102] == 0";
                return !GetGlobalFlag(102);
            case 352:
            case 354:
                originalScript = "game.party[0].money_get() > 1000";
                return PartyLeader.GetMoney() > 1000;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 12:
            case 13:
            case 43:
            case 44:
                originalScript = "pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                pc.AddCondition("Fallen_Paladin", 0, 0);
                break;
            case 53:
            case 54:
                originalScript = "npc.attack( pc ); get_rep( npc, pc ); npc.reaction_adj( pc,-20)";
                npc.Attack(pc);
                get_rep(npc, pc);
                npc.AdjustReaction(pc, -20);
                ;
                break;
            case 131:
            case 132:
                originalScript = "pc.condition_add_with_args(\"Fallen_Paladin\",0,0); game.fade_and_teleport( 14400, 0, 0, 5051, 389, 488 ); make_like( npc, pc )";
                pc.AddCondition("Fallen_Paladin", 0, 0);
                FadeAndTeleport(14400, 0, 0, 5051, 389, 488);
                make_like(npc, pc);
                ;
                break;
            case 135:
                originalScript = "game.quests[42].state = qs_completed; kill_lodriss(npc); game.fade_and_teleport( 3600, 0, 0, 5051, 389, 488 )";
                SetQuestState(42, QuestState.Completed);
                kill_lodriss(npc);
                FadeAndTeleport(3600, 0, 0, 5051, 389, 488);
                ;
                break;
            case 183:
            case 193:
            case 221:
                originalScript = "party_transfer_to( npc, 5804 )";
                Utilities.party_transfer_to(npc, 5804);
                break;
            case 200:
            case 270:
                originalScript = "npc.reaction_adj( pc,+10); game.quests[42].state = qs_completed; game.global_flags[102] = 1; kill_skole(npc)";
                npc.AdjustReaction(pc, +10);
                SetQuestState(42, QuestState.Completed);
                SetGlobalFlag(102, true);
                kill_skole(npc);
                ;
                break;
            case 222:
            case 223:
            case 283:
            case 284:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 231:
            case 232:
                originalScript = "game.quests[42].state = qs_completed; game.global_flags[102] = 1; kill_skole(npc)";
                SetQuestState(42, QuestState.Completed);
                SetGlobalFlag(102, true);
                kill_skole(npc);
                ;
                break;
            case 260:
                originalScript = "pc.money_adj(+10000)";
                pc.AdjustMoney(+10000);
                break;
            case 311:
            case 312:
                originalScript = "evac(npc)";
                evac(npc);
                break;
            case 351:
            case 353:
                originalScript = "evac_partial(npc)";
                evac_partial(npc);
                break;
            case 352:
            case 354:
                originalScript = "pc.money_adj(-1000); evac(npc)";
                pc.AdjustMoney(-1000);
                evac(npc);
                ;
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            case 67:
            case 68:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9);
                return true;
            case 122:
            case 123:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                return true;
            case 181:
            case 191:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}