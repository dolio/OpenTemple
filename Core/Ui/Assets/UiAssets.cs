using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Assets;

public enum UiAssetType
{
    Portraits = 0,
    Inventory,
    Generic, // Textures
    GenericLarge // IMG files
}

public enum UiGenericAsset
{
    AcceptHover = 0,
    AcceptNormal,
    AcceptPressed,
    DeclineHover,
    DeclineNormal,
    DeclinePressed,
    DisabledNormal,
    GenericDialogueCheck
}

public class UiAssets
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    /// <summary>
    /// Appended to translation keys this will cause the text to be uppercased.
    /// </summary>
    private const string UpperSuffix = ":upper";
    
    private readonly Dictionary<string, Dictionary<int, string>> _translationFiles = new();
    
    public UiAssets(IFileSystem fileSystem)
    {
        _translationFiles["main_menu"] = fileSystem.ReadMesFile("mes/mainmenu.mes");
        _translationFiles["pc_creation"] = fileSystem.ReadMesFile("mes/pc_creation.mes");
        _translationFiles["party_pool"] = fileSystem.ReadMesFile("mes/party_pool.mes");
        _translationFiles["stat"] = fileSystem.ReadMesFile("mes/stat.mes");
        _translationFiles["options"] = fileSystem.ReadMesFile("mes/options_text.mes");
        _translationFiles["char_ui_inventory"] = fileSystem.ReadMesFile("mes/5_char_inventory_ui_text.mes");
        _translationFiles["char_ui_spells"] = fileSystem.ReadMesFile("mes/14_char_spells_ui_text.mes");
        _translationFiles["char_ui_skills"] = fileSystem.ReadMesFile("mes/15_char_skills_ui_text.mes");
        _translationFiles["loadgame"] = fileSystem.ReadMesFile("mes/loadgame_ui.mes");
        _translationFiles["savegame"] = fileSystem.ReadMesFile("mes/savegame_ui.mes");
        _translationFiles["townmap"] = fileSystem.ReadMesFile("mes/townmap_ui_text.mes");
        _translationFiles["worldmap_locations"] = fileSystem.ReadMesFile("mes/worldmap_location_names_text.mes");
        _translationFiles["townmap_markers"] = fileSystem.ReadMesFile("mes/townmap_ui_placed_flag_text.mes");
        _translationFiles["map_names"] = fileSystem.ReadMesFile("mes/map_names.mes");
        _translationFiles["logbook_keys"] = fileSystem.ReadMesFile("mes/logbook_ui_keys_text.mes");
    }

    /**
         * Replaces placeholders of the form #{main_menu:123} with the key 123 from the mes file registered
         * as main_menu.
         */
    public string ApplyTranslation(string text)
    {
        StringBuilder result = new StringBuilder(text.Length);
        StringBuilder mesFilename = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            if (!IsStartOfTranslation(text, i))
            {
                result.Append(text[i]);
                continue;
            }

            var firstToken = i; // If parsing fails, we append the original
            mesFilename.Clear();

            // Start pushing back tokens until we reach the marker or end of translation
            bool terminated = false;
            for (i = i + 2; i < text.Length; i++)
            {
                if (text[i] == ':' || text[i] == '}')
                {
                    terminated = text[i] == ':';
                    break;
                }
                else
                {
                    mesFilename.Append(text[i]);
                }
            }

            if (!terminated)
            {
                result.Append(text.Substring(firstToken, i - firstToken));
                continue;
            }


            if (!_translationFiles.TryGetValue(mesFilename.ToString(), out var translationDict))
            {
                result.Append(text.Substring(firstToken, i - firstToken));
                continue;
            }

            // Parse the mes id now
            terminated = false;
            var mesLine = new StringBuilder();
            for (i = i + 1; i < text.Length; i++)
            {
                if (text[i] == '}')
                {
                    terminated = true;
                    break;
                }
                else
                {
                    mesLine.Append(text[i]);
                }
            }

            if (!terminated)
            {
                result.Append(text.Substring(firstToken, i - firstToken));
                continue;
            }

            var keyIdStr = mesLine.ToString();
            var toUpper = false;
            if (keyIdStr.EndsWith(UpperSuffix))
            {
                keyIdStr = keyIdStr.Substring(0, keyIdStr.Length - UpperSuffix.Length);
                toUpper = true;
            }

            if (!int.TryParse(keyIdStr, out var mesLineNo))
            {
                result.Append(text.Substring(firstToken, i - firstToken));
                continue;
            }

            string translation;
            if (!translationDict.TryGetValue(mesLineNo, out translation))
            {
                Logger.Warn("Missing translation: {0}:{1}", mesFilename, mesLineNo);
                translation = "!" + mesFilename + ":" + mesLineNo + "!";
            }

            if (toUpper)
            {
                translation = translation.ToUpper();
            }

            result.Append(translation);
        }

        return result.ToString();
    }

    /* TODO
    public bool GetAsset(UiAssetType assetType, UiGenericAsset assetIndex, out int textureIdOut) {
        static var ui_get_common_texture_id = temple.GetPointer<signed int(UiAssetType assetType, UiGenericAsset assetIdx, int& textureIdOut, int a4)>(0x1004a360);
        return ui_get_common_texture_id(assetType, assetIndex, textureIdOut, 0) == 0;
    }*/

    // Loads a .img file.
    [TempleDllLocation(0x101e8320)]
    public ResourceRef<ITexture> LoadImg(string filename)
    {
        return Tig.Textures.Resolve(filename, false);
    }

    public static bool IsStartOfTranslation(ReadOnlySpan<char> text, int pos)
    {
        // #{} Is minimal
        if (pos + 2 >= text.Length)
        {
            return false;
        }

        return text[pos] == '#' && text[pos + 1] == '{';
    }
}
