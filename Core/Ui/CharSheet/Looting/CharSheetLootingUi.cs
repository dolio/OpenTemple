using System;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Ui.CharSheet.Looting
{
    public class CharSheetLootingUi : IDisposable
    {
        [TempleDllLocation(0x10BE6EE8)]
        private bool _visible;

        [TempleDllLocation(0x101412a0)]
        public CharSheetLootingUi()
        {
        }

        [TempleDllLocation(0x1013dd50)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013dd20)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013f6c0)]
        public void Show(GameObjectBody target)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013f880)]
        public void Hide()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1013de00)]
        public int GetLootingState()
        {
            if (!_visible)
            {
                return 0;
            }

            return (int) UiSystems.CharSheet.State;
        }

        [TempleDllLocation(0x1013de30)]
        [TempleDllLocation(0x10BE6EC0)]
        public GameObjectBody Target { get; private set; }
    }
}