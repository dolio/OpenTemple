using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.D20.Actions;

public class ActionSequence
{
    private static long nextSerial;

    public long Serial { get; } = nextSerial++;

    public D20Action CurrentAction => d20ActArray[d20aCurIdx];

    public List<D20Action> d20ActArray = new();
    public int d20ActArrayNum => d20ActArray.Count;
    public int d20aCurIdx; // inited to -1
    public ActionSequence? prevSeq;
    public ActionSequence? interruptSeq;
    public TurnBasedStatus tbStatus = new();
    public GameObject performer;
    public LocAndOffsets performerLoc;
    public GameObject targetObj;
    public SpellPacketBody spellPktBody = new();
    public D20Action? castSpellAction;
    public bool ignoreLos;

    public ActionSequence Copy()
    {
        var result = new ActionSequence();
        CopyTo(result);
        return result;
    }

    public void CopyTo(ActionSequence otherSequence)
    {
        otherSequence.d20ActArray = new List<D20Action>(d20ActArray.Select(a => a.Copy()));
        otherSequence.d20aCurIdx = d20aCurIdx;
        otherSequence.prevSeq = prevSeq;
        otherSequence.interruptSeq = interruptSeq;
        tbStatus.CopyTo(otherSequence.tbStatus);
        otherSequence.performer = performer;
        otherSequence.performerLoc = performerLoc;
        otherSequence.targetObj = targetObj;
        otherSequence.spellPktBody = spellPktBody;
        otherSequence.castSpellAction = castSpellAction?.Copy();
        otherSequence.ignoreLos = ignoreLos;
        otherSequence.IsPerforming = IsPerforming;
        otherSequence.IsInterrupted = IsInterrupted;
    }

    // See SequenceFlags for save/load
    public bool IsPerforming { get; set; }

    // See SequenceFlags for save/load
    public bool IsInterrupted { get; set; }

    public bool IsLastAction => d20aCurIdx == d20ActArrayNum - 1;

    public override string ToString()
    {
        return $"ActionSequence({performer};{Serial})";
    }

    public void ResetSpell()
    {
        spellPktBody = new SpellPacketBody();
    }
}