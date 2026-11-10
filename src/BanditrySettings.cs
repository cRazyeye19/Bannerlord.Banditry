using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace Banditry
{
    /// <summary>In-game options, shown in Mod Options (MCM).</summary>
    public sealed class BanditrySettings : AttributeGlobalSettings<BanditrySettings>
    {
        public override string Id => "Banditry_v2";
        public override string DisplayName => "Banditry";
        public override string FolderName => "Banditry";
        public override string FormatType => "json2";

        [SettingPropertyFloatingInteger("Hideout Recruitable Multiplier", 1f, 4f, "0.0",
            HintText = "Max recruits available from a hideout = (bandits currently in the hideout / 10) x this multiplier.",
            Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("Multipliers", GroupOrder = 0)]
        public float HideoutRecruitableMultiplier { get; set; } = 2f;

        [SettingPropertyFloatingInteger("Gang Leader Recruitable Multiplier", 1f, 4f, "0.0",
            HintText = "Recruits per gang leader in a town = sqrt(relation / 10) x this multiplier (minimum 1 per friendly gang leader).",
            Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Multipliers", GroupOrder = 0)]
        public float TownRecruitableMultiplier { get; set; } = 2f;

        [SettingPropertyBool("Remove Modded Data from Save",
            HintText = "Tick this, then load your save once. All Banditry troops are removed from every party and the mod stops saving its own data, so you can safely stop using the mod with that save.",
            Order = 0, RequireRestart = false)]
        [SettingPropertyGroup("Uninstalling", GroupOrder = 1)]
        public bool RemoveModdedData { get; set; } = false;
    }
}
