using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TheBookofDuplication
{
    public class DuplicationConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("$Mods.TheBookofDuplication.PriceSettings.Label")]

        [DefaultValue(2)]
        [Range(1, 30)]
        [ReloadRequired]
        public int PriceMultiplier { get; set; } = 2;
    }
}