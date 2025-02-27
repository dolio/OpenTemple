using System;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet.Portrait;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Inventory;

public enum ItemSlotMode
{
    /// <summary>
    /// A slot belonging to one's own inventory.
    /// </summary>
    Inventory,

    /// <summary>
    /// A slot belonging to a container or corpse that is being looted.
    /// </summary>
    Looting,

    /// <summary>
    /// A slot belonging to a vendor's inventory who is being bartered with.
    /// </summary>
    Bartering
}

public class ItemSlotBehavior
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly WidgetBase _slotWidget;

    private readonly Func<GameObject?> _currentItemSupplier;

    private readonly Func<GameObject?> _actingCritterSupplier;

    private GameObject? CurrentItem => _currentItemSupplier();

    private GameObject? ActingCritter => _actingCritterSupplier();

    public bool AllowShowInfo { get; set; }

    public ItemSlotMode Mode { get; set; } = ItemSlotMode.Inventory;

    private bool _dragging;

    public ItemSlotBehavior(WidgetBase slotWidget,
        Func<GameObject?> currentItemSupplier,
        Func<GameObject?> actingCritterSupplier)
    {
        _slotWidget = slotWidget;
        _currentItemSupplier = currentItemSupplier;
        _actingCritterSupplier = actingCritterSupplier;

        slotWidget.OnMouseDown += HandleMouseDown;
        slotWidget.OnMouseMove += e =>
        {
            if (e.IsLeftButtonHeld)
            {
                _dragging = true;
            }
        };
        slotWidget.OnMouseUp += HandleMouseUp;
        slotWidget.OnMouseLeave += _ => UiSystems.CharSheet.Help.ClearHelpText();
    }

    private void HandleMouseDown(MouseEvent e)
    {
        if (e.Button == MouseButton.Left)
        {
            var currentItem = CurrentItem;

            // Other operations are in progress, or this slot is empty
            if (currentItem == null
                || UiSystems.CharSheet.Looting.IsIdentifying
                || UiSystems.CharSheet.State == CharInventoryState.CastingSpell)
            {
                return;
            }

            // Determine where inside the widget the user clicked.
            var relPos = e.GetLocalPos(_slotWidget);

            UiSystems.CharSheet.Inventory.DraggedObject = currentItem;
            var texturePath = currentItem.GetInventoryIconPath();
            var iconSize = _slotWidget.LayoutBox.Size;
            iconSize.Height -= 4;
            iconSize.Width -= 4;
            Tig.Mouse.SetDraggedIcon(texturePath, new PointF(-relPos.X + 4, -relPos.Y + 4), iconSize);

            _slotWidget.SetMouseCapture();
            _dragging = false;
        }
    }
    
    private void HandleMouseUp(MouseEvent e)
    {
        _slotWidget.ReleaseMouseCapture();
        
        var currentItem = CurrentItem;

        if (e.Button == MouseButton.Left)
        {
            if (_dragging)
            {
                var droppedOn = Globals.UiManager.PickWidget(e.X, e.Y);
                var item = UiSystems.CharSheet.Inventory.DraggedObject;

                UiSystems.CharSheet.Inventory.DraggedObject = null;
                Tig.Mouse.ClearDraggedIcon();

                if (item == null || droppedOn == _slotWidget)
                {
                    return;
                }

                DropOn(droppedOn);
            }
            else
            {
                if (UiSystems.CharSheet.State == CharInventoryState.CastingSpell)
                {
                    UiSystems.CharSheet.CallItemPickCallback(CurrentItem);
                    return;
                }

                if (UiSystems.CharSheet.Looting.IsIdentifying)
                {
                    if (currentItem == null || GameSystems.Item.IsIdentified(currentItem) ||
                        GameSystems.Party.GetPartyMoney() < 10000)
                    {
                        return;
                    }

                    GameSystems.Party.RemovePartyMoney(0, 100, 0, 0);
                    currentItem.SetItemFlag(ItemFlag.IDENTIFIED, true);
                }
                else
                {
                    // Shift+Click will show extra info about an item
                    if (AllowShowInfo
                        && e.IsShiftHeld
                        && GameSystems.MapObject.HasLongDescription(currentItem))
                    {
                        UiSystems.CharSheet.ShowItemDetailsPopup(currentItem, ActingCritter);
                    }

                    UiSystems.CharSheet.Inventory.DraggedObject = null;
                    Tig.Mouse.ClearDraggedIcon();
                }
            }
        }
        else if (e.Button == MouseButton.Right)
        {
            var critter = ActingCritter;
            if (currentItem != null)
            {
                // If the item is in another container, then we'll simply try to take it
                if (GameSystems.Item.GetParent(currentItem) != critter)
                {
                }
                // If the item is currently equipped, unequip it,
                else if (GameSystems.Item.IsEquipped(currentItem))
                {
                    UnequipItem(currentItem, critter);
                }
                // otherwise equip it
                else
                {
                    EquipItem(currentItem, critter);
                }
            }
        }
    }

    private void DropOn(WidgetBase? droppedOn)
    {
        if (droppedOn is PaperdollSlotWidget targetSlotWidget)
        {
            Insert(ActingCritter, CurrentItem, targetSlotWidget);
        }
        else if (droppedOn == UiSystems.CharSheet.Inventory.UseItemWidget)
        {
            UseItem(ActingCritter, CurrentItem);
        }
        else if (droppedOn == UiSystems.CharSheet.Inventory.DropItemWidget)
        {
            DropItem(ActingCritter, CurrentItem);
        }
        else if (UiSystems.CharSheet.State == CharInventoryState.Looting &&
                 UiSystems.CharSheet.Looting.TryGetInventoryIdxForWidget(droppedOn, out var lootingInvIdx))
        {
            InsertIntoLootContainer(ActingCritter, CurrentItem, lootingInvIdx);
        }
        else if (UiSystems.CharSheet.State == CharInventoryState.Bartering &&
                 UiSystems.CharSheet.Looting.TryGetInventoryIdxForWidget(droppedOn, out var barterInvIdx))
        {
            InsertIntoBarterContainer(ActingCritter, CurrentItem, barterInvIdx);
        }
        else if (UiSystems.Party.TryGetPartyMemberByWidget(droppedOn, out var partyMember))
        {
            InsertIntoPartyPortrait(ActingCritter, CurrentItem, partyMember);
        }
        else if (UiSystems.CharSheet.Inventory.TryGetInventoryIdxForWidget(droppedOn, out var invIdx))
        {
            InsertIntoInventorySlot(ActingCritter, CurrentItem, invIdx);
        }
    }

    private static bool IsSplittable(GameObject item, out int quantity)
    {
        return item.TryGetQuantity(out quantity) && quantity > 1 && item.type != ObjectType.money;
    }

    private void Insert(GameObject critter, GameObject item, PaperdollSlotWidget targetSlotWidget)
    {
        if (!AttemptEquipmentChangeInCombat(critter, item))
        {
            return;
        }

        var errorCode = GameSystems.Item.ItemTransferWithFlags(
            item,
            critter,
            targetSlotWidget.InventoryIndex,
            ItemInsertFlag.Allow_Swap | ItemInsertFlag.Use_Wield_Slots,
            null);
        if (errorCode != ItemErrorCode.OK)
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(errorCode);
        }
    }

    private void UnequipItem(GameObject item, GameObject critter)
    {
        if (!AttemptEquipmentChangeInCombat(critter, item))
        {
            return;
        }

        var err = GameSystems.Item.ItemTransferWithFlags(
            item, critter, -1, ItemInsertFlag.Unk4, null);
        if (err != ItemErrorCode.OK)
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(err);
        }
    }

    private void EquipItem(GameObject item, GameObject critter)
    {
        if (!AttemptEquipmentChangeInCombat(critter, item))
        {
            return;
        }

        if (IsSplittable(item, out var quantity))
        {
            var iconPath = item.GetInventoryIconPath();
            UiSystems.CharSheet.SplitItem(item, critter, 0, quantity,
                iconPath, 2, -1, 0, ItemInsertFlag.Allow_Swap | ItemInsertFlag.Use_Wield_Slots);
        }

        var err = GameSystems.Item.ItemTransferWithFlags(item, critter, -1,
            ItemInsertFlag.Allow_Swap | ItemInsertFlag.Use_Wield_Slots, null);
        if (err != ItemErrorCode.OK)
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(err);
        }
    }

    private static void UseItem(GameObject critter, GameObject item)
    {
        Logger.Info("Use item via dragging.");
        var soundId = GameSystems.SoundMap.GetSoundIdForItemEvent(item, critter, null, ItemSoundEffect.Use);
        GameSystems.SoundGame.PositionalSound(soundId, 1, critter);

        if (GameSystems.Script.ExecuteObjectScript(critter, item, 0, ObjScriptEvent.Use) == 0)
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.ItemCannotBeUsed);
            return;
        }

        if (item.type == ObjectType.written)
        {
            UiSystems.Written.Show(item);
            return;
        }

        if (item.type == ObjectType.food || item.type == ObjectType.generic || item.type == ObjectType.scroll)
        {
            if (GameSystems.D20.Actions.TurnBasedStatusInit(critter))
            {
                GameSystems.D20.Actions.CurSeqReset(critter);
                GameSystems.D20.Actions.GlobD20ActnInit();
                if (GameSystems.D20.Actions.DoUseItemAction(critter, null, item))
                {
                    GameSystems.D20.Actions.sequencePerform();
                    UiSystems.CharSheet.Hide();
                    return;
                }
            }

            UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.FailedToUseItem);
        }
    }

    private static void DropItem(GameObject critter, GameObject item)
    {
        Logger.Info("Dropping item via drag&drop");

        var soundId = GameSystems.SoundMap.GetSoundIdForItemEvent(item, critter, null, ItemSoundEffect.Drop);
        GameSystems.SoundGame.PositionalSound(soundId, 1, critter);

        if (IsSplittable(item, out var quantity))
        {
            if (!UiSystems.CharSheet.SplitItem(item, null, 0, quantity, item.GetInventoryIconPath(),
                    1, -1, 0, 0))
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
            }
        }
        else
        {
            if (!GameSystems.Item.ItemDrop(item))
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
            }
        }
    }

    private void InsertIntoLootContainer(GameObject critter, GameObject item, int msg)
    {
        var target = UiSystems.CharSheet.Looting.LootingContainer;
        ItemInsertFlag insertFlags;
        if (target != null && target.IsCritter())
        {
            insertFlags = ItemInsertFlag.Unk4;
        }
        else if (target?.type == ObjectType.container)
        {
            insertFlags = ItemInsertFlag.Unk4 | ItemInsertFlag.Use_Max_Idx_200;
        }
        else
        {
            return;
        }

        if (GameSystems.Item.GetItemAtInvIdx(target, msg) != null)
        {
            msg = -1;
        }

        if (IsSplittable(item, out var quantity) && critter != target)
        {
            var iconTexture = item.GetInventoryIconPath();
            if (!UiSystems.CharSheet.SplitItem(item, target, 0, quantity,
                    iconTexture, 2, msg, 0, insertFlags))
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
            }
        }
        else
        {
            var err = GameSystems.Item.ItemTransferWithFlags(
                item,
                target,
                msg,
                insertFlags,
                null);

            if (err != ItemErrorCode.OK)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(err);
            }
        }
    }

    private void InsertIntoBarterContainer(GameObject critter, GameObject item, int msg)
    {
        var buyer = UiSystems.CharSheet.Looting.LootingContainer;
        if (buyer == null || !buyer.IsCritter())
        {
            buyer = UiSystems.CharSheet.Looting.Vendor;
        }

        var appraiser = GameSystems.Party.GetMemberWithHighestSkill(0);
        var appraisedWorth = GameSystems.Item.GetAppraisedWorth(item, appraiser, buyer);

        if (buyer == null)
        {
            return;
        }

        if (appraisedWorth <= 0)
        {
            GameSystems.Dialog.TryGetWontSellVoiceLine(buyer, critter, out _, out var soundId);
            UiSystems.Dialog.PlayVoiceLine(buyer, critter, soundId);
            return;
        }

        if (IsSplittable(item, out var quantity))
        {
            var icon = item.GetInventoryIconPath();
            if (!UiSystems.CharSheet.SplitItem(item, buyer, 0, quantity, icon, 4, msg,
                    appraisedWorth, ItemInsertFlag.Unk4 | ItemInsertFlag.Use_Max_Idx_200))
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Item_Cannot_Be_Dropped);
            }
        }
        else
        {
            var err = GameSystems.Item.ItemTransferWithFlags(item,
                buyer, msg, ItemInsertFlag.Unk4 | ItemInsertFlag.Use_Max_Idx_200, null);
            if (err != ItemErrorCode.OK)
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(err);
                return;
            }

            GameSystems.Item.SplitMoney(appraisedWorth,
                true, out var platinum,
                true, out var gold,
                true, out var silver,
                out var copper);
            GameSystems.Party.AddPartyMoney(platinum, gold, silver, copper);
        }
    }

    private void InsertIntoPartyPortrait(GameObject critter, GameObject item, GameObject partyMember)
    {
        if (partyMember == critter
            || !UiSystems.CharSheet.Inventory.IsCloseEnoughToTransferItem(critter, partyMember)
            || GameSystems.Combat.IsCombatActive())
        {
            return;
        }

        if (IsSplittable(item, out var quantity))
        {
            var icon = item.GetInventoryIconPath();
            if (!UiSystems.CharSheet.SplitItem(item, partyMember, 0, quantity, icon, 2, -1,
                    0, ItemInsertFlag.Unk4))
            {
                UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode
                    .Item_Cannot_Be_Dropped);
            }

            return;
        }

        var err = GameSystems.Item.ItemTransferWithFlags(item,
            partyMember, -1, ItemInsertFlag.Unk4, null);
        if (err != ItemErrorCode.OK)
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(err);
        }
    }

    private void InsertIntoInventorySlot(GameObject critter, GameObject item, int invIdx)
    {
        if (!AttemptEquipmentChangeInCombat(critter, item))
        {
            return;
        }

        var itemInsertFlag = ItemInsertFlag.Unk4;
        if (GameSystems.Combat.IsCombatActive())
        {
            itemInsertFlag |= ItemInsertFlag.Allow_Swap;
        }

        var err = GameSystems.Item.ItemTransferWithFlags(item, critter, invIdx, itemInsertFlag, null);
        if (err != ItemErrorCode.OK)
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(err);
        }
    }

    private bool AttemptEquipmentChangeInCombat(GameObject critter, GameObject item)
    {
        if (!GameSystems.Combat.IsCombatActive())
        {
            return true;
        }

        if (!UiSystems.CharSheet.Inventory.EquippingIsAction(item))
        {
            UiSystems.CharSheet.ItemTransferErrorPopup(ItemErrorCode.Cannot_Transfer);
            return false;
        }

        var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
        if (!GameSystems.Feat.HasFeat(critter, FeatId.QUICK_DRAW)
            && tbStatus.hourglassState < HourglassState.MOVE &&
            !tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.ChangedWornItem))
        {
            Logger.Info("Cannot change equipment, not enough time left!");
            return false;
        }

        if (!GameSystems.Feat.HasFeat(critter, FeatId.QUICK_DRAW))
        {
            if (!tbStatus.tbsFlags.HasFlag(TurnBasedStatusFlags.ChangedWornItem))
            {
                tbStatus.hourglassState =
                    GameSystems.D20.Actions.GetHourglassTransition(
                        tbStatus.hourglassState,
                        ActionCostType.Move);
                tbStatus.tbsFlags |= TurnBasedStatusFlags.ChangedWornItem;
            }
        }

        return true;
    }
}