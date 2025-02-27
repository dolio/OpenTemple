
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

namespace VanillaScripts.Dialog;

[DialogScript(111)]
public class YDeyDialog : YDey, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
            case 4:
            case 5:
                originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)";
                return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
            case 7:
            case 8:
                originalScript = "game.quests[31].state == qs_accepted and group_average_level(pc) <= 5 and anyone( pc.group_list(), \"has_follower\", 8014 )";
                return GetQuestState(31) == QuestState.Accepted && Utilities.group_average_level(pc) <= 5 && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8014));
            case 53:
            case 54:
                originalScript = "((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state == 3)";
                return ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState == 3);
            case 71:
            case 72:
                originalScript = "(game.story_state == 3)";
                return (StoryState == 3);
            case 182:
            case 184:
            case 191:
            case 193:
            case 392:
            case 394:
                originalScript = "game.party_npc_size() == 1";
                return GameSystems.Party.NPCFollowersSize == 1;
            case 183:
            case 185:
            case 192:
            case 194:
            case 393:
            case 395:
                originalScript = "game.party_npc_size() >= 2";
                return GameSystems.Party.NPCFollowersSize >= 2;
            case 211:
            case 212:
                originalScript = "game.global_flags[306] == 0";
                return !GetGlobalFlag(306);
            case 215:
            case 216:
                originalScript = "game.global_flags[306] == 1 and game.global_flags[98] == 0 and game.quests[11].state == qs_accepted";
                return GetGlobalFlag(306) && !GetGlobalFlag(98) && GetQuestState(11) == QuestState.Accepted;
            case 221:
            case 222:
                originalScript = "game.party_alignment == NEUTRAL_GOOD";
                return PartyAlignment == Alignment.NEUTRAL_GOOD;
            case 223:
            case 224:
            case 245:
            case 246:
                originalScript = "game.global_flags[98] == 0 and game.quests[11].state == qs_accepted";
                return !GetGlobalFlag(98) && GetQuestState(11) == QuestState.Accepted;
            case 252:
            case 253:
            case 254:
            case 255:
                originalScript = "game.global_flags[100] == 1";
                return GetGlobalFlag(100);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 80:
                originalScript = "game.areas[4] = 1; game.story_state = 4";
                MakeAreaKnown(4);
                StoryState = 4;
                ;
                break;
            case 151:
                originalScript = "buttin(npc,pc,450)";
                buttin(npc, pc, 450);
                break;
            case 186:
            case 187:
            case 195:
            case 196:
            case 396:
            case 397:
                originalScript = "buttin(npc,pc,460)";
                buttin(npc, pc, 460);
                break;
            case 220:
                originalScript = "game.global_flags[306] = 1";
                SetGlobalFlag(306, true);
                break;
            case 260:
                originalScript = "game.global_flags[100] = 1; pc.follower_add(npc)";
                SetGlobalFlag(100, true);
                pc.AddFollower(npc);
                ;
                break;
            case 383:
            case 384:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 401:
                originalScript = "pc.follower_remove(npc)";
                pc.RemoveFollower(npc);
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
            default:
                skillChecks = default;
                return false;
        }
    }
}