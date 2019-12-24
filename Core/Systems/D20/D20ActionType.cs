namespace OpenTemple.Core.Systems.D20
{
    public enum D20ActionType
    {
        NONE = -1,
        UNSPECIFIED_MOVE = 0,
        UNSPECIFIED_ATTACK = 1,
        STANDARD_ATTACK = 2,
        FULL_ATTACK = 3,
        STANDARD_RANGED_ATTACK = 4,
        RELOAD = 5,
        FIVEFOOTSTEP = 6,
        MOVE = 7,
        DOUBLE_MOVE = 8,
        RUN = 9,
        CAST_SPELL = 10,
        HEAL = 11,
        CLEAVE = 12,
        ATTACK_OF_OPPORTUNITY = 13,
        WHIRLWIND_ATTACK = 14,
        TOUCH_ATTACK = 15,
        TOTAL_DEFENSE = 16,
        CHARGE = 17,
        FALL_TO_PRONE = 18,
        STAND_UP = 19,
        TURN_UNDEAD = 20,
        DEATH_TOUCH = 21,
        PROTECTIVE_WARD = 22,
        FEAT_OF_STRENGTH = 23,
        BARDIC_MUSIC = 24,
        PICKUP_OBJECT = 25,
        COUP_DE_GRACE = 26,
        USE_ITEM = 27,
        BARBARIAN_RAGE = 28,
        STUNNING_FIST = 29,
        SMITE_EVIL = 30, // 30
        LAY_ON_HANDS_SET = 31,
        DETECT_EVIL = 32,
        STOP_CONCENTRATION = 33,
        BREAK_FREE = 34,
        TRIP = 35,
        REMOVE_DISEASE = 36,
        ITEM_CREATION = 37,
        WHOLENESS_OF_BODY_SET = 38,
        USE_MAGIC_DEVICE_DECIPHER_WRITTEN_SPELL = 39,
        TRACK = 40,
        ACTIVATE_DEVICE_STANDARD = 41,
        SPELL_CALL_LIGHTNING = 42,
        AOO_MOVEMENT = 43,
        CLASS_ABILITY_SA = 44,
        ACTIVATE_DEVICE_FREE = 45,
        OPEN_INVENTORY = 46,
        ACTIVATE_DEVICE_SPELL = 47, // 47
        DISABLE_DEVICE = 48,
        SEARCH = 49,
        SNEAK = 50,
        TALK = 51,
        OPEN_LOCK = 52, // verified
        SLEIGHT_OF_HAND = 53,
        OPEN_CONTAINER = 54, // verified; DLL string are accurate at least up to here
        THROW = 55,
        THROW_GRENADE = 56,
        FEINT = 57, // verified; this was missing in the priginal python table, so that must be where it went off track
        READY_SPELL = 58,
        READY_COUNTERSPELL = 59,
        READY_ENTER = 60,
        READY_EXIT = 61,
        COPY_SCROLL = 62, // verified
        READIED_INTERRUPT = 63,
        LAY_ON_HANDS_USE = 64,
        WHOLENESS_OF_BODY_USE = 65,
        DISMISS_SPELLS = 66,
        FLEE_COMBAT = 67,
        USE_POTION = 68, // vanilla actions are up to here
        DIVINE_MIGHT = 69,
        DISARM = 70,
        SUNDER = 71,
        BULLRUSH = 72,
        TRAMPLE = 73,
        GRAPPLE = 74,
        PIN = 75,
        OVERRUN = 76,
        SHIELD_BASH = 77,
        DISARMED_WEAPON_RETRIEVE = 78,
        AID_ANOTHER_WAKE_UP = 79,
        EMPTY_BODY = 80, // monk ability
        QUIVERING_PALM = 81, // monk ability
        PYTHON_ACTION = 82,
        NUMACTIONS = 83, // always keep this last. Not counting NONE since it is unused (all the d20 action functions start cycling from UNSPECIFIED_MOVE)
        UNASSIGNED = -2 // used for hotkey binds (TODO: Get rid of this)
    }
}