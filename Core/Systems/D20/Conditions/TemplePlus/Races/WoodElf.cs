
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class WoodElf
    {
        private static readonly RaceId Id = RaceId.elf + (5 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec
        {
            helpTopic = "TAG_WOOD_ELF",
            conditionName = "Wood Elf",
            heightMale = (53, 65),
            heightFemale = (53, 65),
            weightMale = (87, 121),
            weightFemale = (82, 116),
            statModifiers =
            {
                (Stat.strength, 2), (Stat.dexterity, 2),
                (Stat.constitution, -2), (Stat.intelligence, -2),
            },
            protoId = 13030,
            materialOffset = 2, // offset into rules/material_ext.mes file,
            feats = {FeatId.SIMPLE_WEAPON_PROFICIENCY_ELF},
            useBaseRaceForDeity = true
        };

        public static void ElvenSaveBonusEnchantment(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var flags = dispIo.flags;
            if ((flags & D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT) != 0)
            {
                dispIo.bonlist.AddBonus(2, 31, 139); // Racial Bonus
            }
        }

        public static void ConditionImmunityOnPreAdd(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            var val = dispIo.condStruct == SpellEffects.SpellSleep;
            if (val)
            {
                dispIo.outputFlag = false;
                evt.objHndCaller.FloatMesFileLine("mes/combat.mes", 5059, TextFloaterColor.Red); // "Sleep Immunity"
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(31, evt.objHndCaller, null);
            }
        }

        public static readonly ConditionSpec raceSpecObj = ConditionSpec.Create(RaceSpec.conditionName)
            .SetUnique()
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses(
                    (SkillId.listen, 2),
                    (SkillId.search, 2),
                    (SkillId.spot, 2)
            )
            .AddBaseMoveSpeed(30)
            .AddHandler(DispatcherType.SaveThrowLevel, ElvenSaveBonusEnchantment)
            .AddFavoredClassHook(Stat.level_ranger)
            .AddHandler(DispatcherType.ConditionAddPre, ConditionImmunityOnPreAdd)
            .Build();

    }
}
