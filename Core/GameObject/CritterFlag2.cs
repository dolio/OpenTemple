using System;

namespace OpenTemple.Core.GameObject
{
    [Flags]
    public enum CritterFlag2 : uint
    {
        ITEM_STOLEN = 0x1,
        AUTO_ANIMATES = 0x2,
        USING_BOOMERANG = 0x4,
        FATIGUE_DRAINING = 0x8,
        SLOW_PARTY = 0x10,
        UNUSED_00000020 = 0x20,
        NO_DECAY = 0x40,
        NO_PICKPOCKET = 0x80,
        NO_BLOOD_SPLOTCHES = 0x100,
        NIGH_INVULNERABLE = 0x200,
        ELEMENTAL = 0x400,
        DARK_SIGHT = 0x800,
        NO_SLIP = 0x1000,
        NO_DISINTEGRATE = 0x2000,
        REACTION_0 = 0x4000,
        REACTION_1 = 0x8000,
        REACTION_2 = 0x10000,
        REACTION_3 = 0x20000,
        REACTION_4 = 0x40000,
        REACTION_5 = 0x80000,
        REACTION_6 = 0x100000,
        TARGET_LOCK = 0x200000,
        ACTION0_PAUSED = 0x400000,
        ACTION1_PAUSED = 0x800000,
        ACTION2_PAUSED = 0x1000000,
        ACTION3_PAUSED = 0x2000000,
        ACTION4_PAUSED = 0x4000000,
        ACTION5_PAUSED = 0x8000000,
        /*ACTION6_PAUSED = 0x10000000,
        OCF2_ = 0x20000000,
        OCF2_ = 0x40000000,
        OCF2_ = 0x80000000*/
    }
}