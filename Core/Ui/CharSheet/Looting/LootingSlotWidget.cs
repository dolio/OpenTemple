using System;
using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.CharSheet.Inventory;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Looting;

public class LootingSlotWidget : WidgetContainer
{
    public static readonly Size SlotSize = new(50, 51);
    public const int MarginRight = 3;
    public const int MarginBottom = 3;

    public int Index { get; }

    /// <summary>
    /// This was previously stored in flag 2.
    /// </summary>
    public bool EquipmentSlot { get; set; }

    public int InventorySlot { get; set; }

    private GameObject? _item;

    private readonly WidgetImage _icon;

    private readonly WidgetText _quantityLabel;

    private readonly ItemSlotBehavior _behavior;

    public LootingSlotWidget(int index, Point position) : base(new RectangleF(position, SlotSize))
    {
        Index = index;
        Reset();
        // slotWidget.OnHandleMessage += 0x101406d0;
        // slotWidget.OnBeforeRender += 0x1013faf0;
        // slotWidget.OnRenderTooltip += 0x1013fea0;

        _quantityLabel = InventorySlotWidget.CreateQuantityLabel();

        _icon = new WidgetImage();
        AddContent(_icon);

        _behavior = new ItemSlotBehavior(this,
            () => _item,
            () => UiSystems.CharSheet.CurrentCritter);
        _behavior.AllowShowInfo = true;
    }

    public void Reset()
    {
        InventorySlot = -1;
        EquipmentSlot = false;
        _item = null;
    }

    public override void Render(UiRenderContext context)
    {
        // don't render if the item in this slot is currently being dragged
        if (UiSystems.CharSheet.Inventory.DraggedObject == _item || _item == null)
        {
            return;
        }

        base.Render(context);
    }

    public void SetItem(GameObject item)
    {
        _item = item;

        if (UiSystems.CharSheet.State == CharInventoryState.Looting)
        {
            _behavior.Mode = ItemSlotMode.Looting;
        }
        else if (UiSystems.CharSheet.State == CharInventoryState.Looting)
        {
            _behavior.Mode = ItemSlotMode.Bartering;
        }

        if ((item.GetItemFlags() & ItemFlag.NO_LOOT) != 0)
        {
            _item = null;
            return;
        }

        var artId = item.GetInt32(obj_f.item_inv_aid);
        var texturePath = GameSystems.UiArtManager.GetInventoryIconPath(artId);
        _icon.SetTexture(texturePath);

        UpdateQuantity();
    }

    private void UpdateQuantity()
    {
        // Renders the stack size on top of the slot
        if (_item != null && _item.TryGetQuantity(out var quantity))
        {
            var quantityText = quantity.ToString(CultureInfo.InvariantCulture);
            _quantityLabel.Text = quantityText;

            // Position the label in the lower right corner
            var textSize = _quantityLabel.GetPreferredSize();
            _quantityLabel.X = PaddingArea.Width - 2 - textSize.Width;
            _quantityLabel.Y = PaddingArea.Height - 2 - textSize.Height;
            _quantityLabel.Visible = true;
        }
        else
        {
            _quantityLabel.Visible = false;
        }
    }

    protected override void HandleTooltip(TooltipEvent e)
    {
        if (Pressed)
        {
            return;
        }

        if (_item == null || UiSystems.CharSheet.Inventory.DraggedObject == _item)
        {
            return;
        }

        UiSystems.CharSheet.Help.ShowItemDescription(_item, UiSystems.CharSheet.CurrentCritter);

        var tooltip = ItemTooltipBuilder.BuildItemTooltip(UiSystems.CharSheet.CurrentCritter, _item);
        if (tooltip != null)
        {
            e.TextContent = tooltip;
        }
    }
}