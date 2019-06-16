using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.Anim
{
    // Has to be 0x10 in size
    public struct AnimParam
    {
        private object value;

        public GameObjectBody obj
        {
            get => (GameObjectBody) value;
            set => this.value = value;
        }

        public LocAndOffsets location
        {
            get => (LocAndOffsets) value;
            set => this.value = value;
        }

        public int number
        {
            get => (int) value;
            set => this.value = value;
        }

        public int spellId
        {
            get => (int) value;
            set => this.value = value;
        }

        public float floatNum
        {
            get
            {
                if (value is int i && i == 0)
                {
                    return 0.0f;
                }
                return (float) value;
            }
            set => this.value = value;
        }
    }

    [Flags]
    public enum AnimPathFlag
    {
        UNK_1 = 1,
        UNK_2 = 2,
        UNK_4 = 4,
        UNK_8 = 8,
        UNK_20 = 0x20,
    }

    public struct AnimPath
    {
        public AnimPathFlag flags;
        public sbyte[] deltas; // xy delta pairs describing deltas for drawing a line in screenspace
        public int range;
        public CompassDirection fieldD0;
        public int fieldD4; // Current index
        public int deltaIdxMax;
        public int fieldDC;
        public int maxPathLength;
        public int fieldE4;
        public locXY objLoc;
        public locXY tgtLoc;

        public AnimPath Empty => new AnimPath
        {
            deltas = new sbyte[200]
        };
    }

    public class AnimSlot
    {
        public AnimSlotId id;
        public AnimSlotFlag flags;
        public int currentState;
        public int field_14; // Compared against currentGoal
        public GameTime nextTriggerTime;
        public GameObjectBody animObj;
        public int currentGoal;
        public int field_2C;
        public List<AnimSlotGoalStackEntry> goals = new List<AnimSlotGoalStackEntry>();
        public AnimSlotGoalStackEntry pCurrentGoal;
        public uint unk1; // field_1134
        public AnimPath animPath;
        public PathQueryResult path = new PathQueryResult();
        public AnimParam param1; // Used as parameters for goal states
        public AnimParam param2; // Used as parameters for goal states
        public int stateFlagData;
        public uint[] unknown = new uint[5];
        public TimePoint gametimeSth;
        public uint currentPing; // I.e. used in
        public uint uniqueActionId; // ID assigned when triggered by a D20 action

        public bool IsActive => flags.HasFlag(AnimSlotFlag.ACTIVE);

        public bool IsStopProcessing => flags.HasFlag(AnimSlotFlag.STOP_PROCESSING);

        public bool IsStackFull => currentGoal >= 7;

        public bool IsStackEmpty => currentGoal < 0;

        public void Clear()
        {
            id.Clear();
            pCurrentGoal = null;
            animObj = null;
            flags = 0;
            currentGoal = -1;
            animPath.flags = 0;
        }

        public void ClearPath()
        {
            path.flags &= ~PathFlags.PF_COMPLETE;
            GameSystems.Raycast.GoalDestinationsRemove(path.mover);
        }
    }
}