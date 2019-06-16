using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Config
{
    public enum HpOnLevelUpMode
    {
        Max,
        Average,
        Roll
    }

    public class GameConfig
    {
        /// <summary>
        /// The folder where Tempe of Elemental Evil is installed.
        /// </summary>
        public string InstallationFolder { get; set; }

        public RenderingConfig Rendering { get; set; } = new RenderingConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public bool SkipLegal { get; set; } = true;
        public bool laxRules { get; set; }

        public bool disableAlignmentRestrictions { get; set; }

        public bool drawObjCylinders { get; set; }

        public bool softShadows { get; set; }

        public string fogOfWar { get; set; } = "";

        public HpOnLevelUpMode HpOnLevelUpMode { get; set; }

        public bool MaxHpForNpcHitdice { get; set; }

        public bool monstrousRaces { get; set; }

        public bool forgottenRealmsRaces { get; set; }

        public bool newRaces { get; set; }

        public int renderWidth { get; set; }

        public int renderHeight { get; set; }

        public int ScrollButter => GetVanillaInt("scroll_butter");

        public int MaxPCs { set; get; } = 6;

        public bool MaxPCsFlexible { get; set; } = false;

        public bool ShowNpcStats { get; set; }

        public int MaxLevel { get; set; } = 20;

        public Dictionary<string, string> VanillaSettings = new Dictionary<string, string>();

        public bool animCatchup { get; set; }

        public bool AutoAttack { get; set; }

        public bool ViolenceFilter { get; set; }

        public bool AlwaysRun { get; set; }

        public bool pathfindingDebugMode { get; set; }

        public TimeSpan AStarMaxWindow { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan AStarMaxTime { get; set; } = TimeSpan.FromSeconds(4);

        public string GetVanillaString(string name) => VanillaSettings[name];

        public int GetVanillaInt(string name) => int.Parse(GetVanillaString(name));

        public void SetVanillaInt(string name, int value) => VanillaSettings[name] = value.ToString();

        public void AddVanillaSetting(string name, string defaultValue, Action changeCallback = null)
        {
            if (VanillaSettings.ContainsKey(name))
            {
                return;
            }

            VanillaSettings[name] = defaultValue;
            // TODO
        }

        public void RemoveVanillaCallback(string name)
        {
            // TODO
        }
    }
}