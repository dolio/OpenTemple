using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7990)]
internal class GenderSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_GENDER";

    public ChargenStage Stage => ChargenStage.Gender;

    public WidgetContainer Container { get; }

    private readonly WidgetButton _maleButton;

    private readonly WidgetButton _femaleButton;

    private CharEditorSelectionPacket _pkt;

    [TempleDllLocation(0x1018a420)]
    public GenderSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/gender_ui.json");
        Container = doc.GetRootContainer();

        _maleButton = doc.GetButton("maleButton");
        _maleButton.AddClickListener(() => ChooseGender(Gender.Male));

        _femaleButton = doc.GetButton("femaleButton");
        _femaleButton.AddClickListener(() => ChooseGender(Gender.Female));

        // NOTE: Vanilla previously tried showing gender-specific help texts, but those texts actually don't exist
    }

    private void ChooseGender(Gender gender)
    {
        _pkt.genderId = gender;
        UpdateButtons();
        UiSystems.PCCreation.UpdatePlayerDescription();
        UiSystems.PCCreation.ResetSystemsAfter(ChargenStage.Gender);
    }

    [TempleDllLocation(0x10189c70)]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        pkt.genderId = null;
        _pkt = pkt;
        UpdateButtons();
    }

    [TempleDllLocation(0x10189cc0)]
    public bool CheckComplete()
    {
        return _pkt.genderId.HasValue;
    }

    [TempleDllLocation(0x10189cd0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:68")]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        Trace.Assert(_pkt.raceId.HasValue);
        Trace.Assert(_pkt.genderId.HasValue);
        Trace.Assert(playerObj == null);

        var protoId = D20RaceSystem.GetProtoId(_pkt.raceId.Value, _pkt.genderId.Value);

        var protoObj = GameSystems.Proto.GetProtoById(protoId);
        playerObj = GameSystems.Object.CreateFromProto(protoObj, LocAndOffsets.Zero);

        for (var i = 0; i < 6; i++)
        {
            playerObj.SetBaseStat(Stat.strength + i, _pkt.abilityStats[i]);
        }

        var animHandle = playerObj.GetOrCreateAnimHandle();
        var animParams = AnimatedModelParams.Default;
        animHandle.Advance(1.0f, 0, 0, animParams);

        if (_pkt.isPointbuy)
        {
            playerObj.SetInt32(obj_f.pc_roll_count, -Globals.Config.PointBuyBudget);
        }
        else
        {
            playerObj.SetInt32(obj_f.pc_roll_count, _pkt.numRerolls);
        }
    }

    public bool CompleteForTesting(Dictionary<string, object> props)
    {
        var gender = GameSystems.Random.GetBool() ? Gender.Female : Gender.Male;
        ChooseGender(gender);
        return true;
    }

    private void UpdateButtons()
    {
        var gender = _pkt.genderId;
        _maleButton.SetActive(gender.HasValue && gender.Value == Gender.Male);
        _femaleButton.SetActive(gender.HasValue && gender.Value == Gender.Female);
    }

}