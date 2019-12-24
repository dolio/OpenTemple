using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20.Conditions.TemplePlus;
using OpenTemple.Core.Systems.D20.Conditions.TemplePlus.Races;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20
{
    public static class D20RaceSystem
    {
        private static readonly Dictionary<RaceId, RaceSpec> _races = new Dictionary<RaceId, RaceSpec>();

        public static readonly RaceId[] VanillaRaceIds =
        {
            RaceId.human, RaceId.dwarf, RaceId.elf,
            RaceId.gnome, RaceId.halfelf, RaceId.half_orc,
            RaceId.halfling
        };

        static D20RaceSystem()
        {
            _races[RaceId.human] = new RaceSpec {
                conditionName = "Human",
                flags = RaceDefinitionFlags.RDF_Vanilla,
            };

            _races[RaceId.dwarf] = new RaceSpec {
                conditionName = "Dwarf",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.constitution, 2), (Stat.charisma, -2)}
            };

            _races[RaceId.elf] = new RaceSpec {
                conditionName = "Elf",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.constitution, -2), (Stat.dexterity, 2)}
            };

            _races[RaceId.gnome] = new RaceSpec {
                conditionName = "Gnome",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.constitution, 2), (Stat.strength, -2)}
            };

            _races[RaceId.halfelf] = new RaceSpec {
                conditionName = "Halfelf",
                flags = RaceDefinitionFlags.RDF_Vanilla,
            };

            _races[RaceId.half_orc] = new RaceSpec {
                conditionName = "Halforc",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.strength, 2), (Stat.charisma, -2)}
            };

            _races[RaceId.halfling] = new RaceSpec {
                conditionName = "Halfling",
                flags = RaceDefinitionFlags.RDF_Vanilla,
                statModifiers = {(Stat.dexterity, 2), (Stat.strength, -2)}
            };

            // Extension races (TODO: Change over to how TP registers races)
            _races[Aasimar.Id] = Aasimar.RaceSpec;
        }

        public static string GetConditionName(RaceId raceId)
        {
            return _races[raceId].conditionName;
        }

        public static Dictionary<SpellStoreData, int> GetSpellLikeAbilities(RaceId raceId)
        {
            return _races[raceId].spellLikeAbilities;
        }

        public static bool HasFeat(RaceId raceId, FeatId featId)
        {
            var raceSpec = GetRaceSpec(raceId);
            return raceSpec.feats.Contains(featId);
        }

        public static Dice GetHitDice(RaceId race)
        {
            var raceSpec = GetRaceSpec(race);
            return raceSpec.hitDice;
        }

        private static RaceSpec GetRaceSpec(RaceId raceId) => _races[raceId];

        public static bool IsVanillaRace(RaceId race) => race >= RaceId.human && race >= RaceId.halfling;

        public static int GetRaceMaterialOffset(RaceId race)
        {
            switch (race)
            {
                case RaceId.human:
                    return 0;
                case RaceId.dwarf:
                    return 6;
                case RaceId.elf:
                    return 2;
                case RaceId.gnome:
                    return 8;
                case RaceId.half_elf:
                    return 10;
                case RaceId.half_orc:
                    return 4;
                case RaceId.halfling:
                    return 12;
                default:
                    var raceSpec = GetRaceSpec(race);
                    return raceSpec.materialOffset;
            }
        }

        public static int GetLevelAdjustment(RaceId race)
        {
            return GetRaceSpec(race).effectiveLevel;
        }
    }

    [Flags]
    public enum RaceDefinitionFlags
    {
        RDF_Vanilla = 1,
        RDF_Monstrous = 2,
        RDF_ForgottenRealms = 4
    }

    public enum RaceBase
    {
        human = 0,
        dwarf = 1,
        elf = 2,
        gnome = 3,
        halfelf = 4,
        half_elf = 4,
        halforc = 5,
        half_orc = 5,
        halfling = 6,

        goblin = 7,
        bugbear = 8,
        gnoll = 9,
        hill_giant = 10,
        troll = 11,
        hobgoblin = 12,
        lizardman = 13,
    }

    public class RaceSpec
    {
        // Pairs of the modified Stat, and the modifier
        public List<(Stat, int)> statModifiers = new List<(Stat, int)>();
        public int effectiveLevel = 0; // modifier for Effective Character Level (determines XP requirement)
        public string helpTopic; // helpsystem id ("TAG_xxx")
        public RaceDefinitionFlags flags;
        public Dice hitDice = new Dice(0, 0, 0);
        public int naturalArmor = 0;
        public List<FeatId> feats = new List<FeatId>(); // feat enums; for entering new-style feats in python, use tpdp.hash
        public Dictionary<SpellStoreData, int> spellLikeAbilities = new Dictionary<SpellStoreData, int>();

        public int protoId; // protos.tab entry (male; female is +1)
        public int materialOffset = 0; // offset for rules/materials.mes (or materials_ext.mes for non-vanilla races)

        public (int, int) weightMale = (80, 100);
        public (int, int) weightFemale = (80, 100);
        public (int, int) heightMale = (70, 80);
        public (int, int) heightFemale = (70, 80);

        public bool bonusFirstLevelFeat = false; //Grants an extra feat at first level if true
        public bool useBaseRaceForDeity = false; //Treats the subrace as the main race for deity selection if true

        // Main Race
        public RaceBase baseRace = RaceBase.human;
        public bool isBaseRace = true;

        public List<RaceId> subraces = new List<RaceId>();

        // Subrace
        public Subrace subrace = Subrace.none;

        public string conditionName;

        public bool IsEnabled()
        {
            if (flags.HasFlag(RaceDefinitionFlags.RDF_Vanilla))
                return true;
            if (flags.HasFlag(RaceDefinitionFlags.RDF_Monstrous) && !Globals.Config.monstrousRaces)
                return false;
            if (flags.HasFlag(RaceDefinitionFlags.RDF_ForgottenRealms) && !Globals.Config.forgottenRealmsRaces)
                return false;
            return Globals.Config.newRaces;
        }
    }
}