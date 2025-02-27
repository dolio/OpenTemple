using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;

namespace OpenTemple.Core.Systems.Script.Hooks;

[HookInterface]
public interface ISaveGameHook
{
    void OnAfterSave(string saveDirectory, SaveGameFile saveFile);

    void OnAfterLoad(string saveDirectory, SaveGameFile saveFile);
}