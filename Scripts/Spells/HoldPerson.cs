
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(228)]
    public class HoldPerson : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Hold Person OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Hold Person OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var target = spell.Targets[0];
            var npc = spell.caster; // added so NPC's will choose valid targets
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                if (target.Object.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(target.Object) < SizeCategory.Large && Utilities.critter_is_unconscious(target.Object) != 1 && !target.Object.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    npc = spell.caster;
                }
                else
                {
                    SetGlobalFlag(811, false);
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 5 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 10 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 15 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 20 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 25 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 30 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.DistanceTo(npc) <= 100 && Utilities.critter_is_unconscious(obj) != 1 && obj.IsMonsterCategory(MonsterCategory.humanoid) && GameSystems.Stat.DispatchGetSizeCategory(obj) < SizeCategory.Large && !GetGlobalFlag(811) && !obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            target.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                }

            }

            if (target.Object.IsMonsterCategory(MonsterCategory.humanoid))
            {
                if (GameSystems.Stat.DispatchGetSizeCategory(target.Object) < SizeCategory.Large)
                {
                    // allow Will saving throw to negate
                    if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw successful
                        target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        AttachParticles("Fizzle", target.Object);
                        spell.RemoveTarget(target.Object);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        // HTN - apply condition HOLD (paralyzed)
                        target.Object.AddCondition("sp-Hold Person", spell.spellId, spell.duration, 0);
                        target.ParticleSystem = AttachParticles("sp-Hold Person", target.Object);
                    }

                }
                else
                {
                    // not medium sized or smaller
                    target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                    target.Object.FloatMesFileLine("mes/spell.mes", 31005);
                    AttachParticles("Fizzle", target.Object);
                    spell.RemoveTarget(target.Object);
                }

            }
            else
            {
                // not a person
                target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target.Object.FloatMesFileLine("mes/spell.mes", 31004);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Hold Person OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Hold Person OnEndSpellCast");
        }

    }
}
