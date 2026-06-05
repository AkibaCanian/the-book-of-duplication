using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Localization;

namespace TheBookofDuplication
{
    public class BookOfDuplication : ModItem
    {
        public static bool Active = false;
        
        private static DuplicationConfig Config => ModContent.GetInstance<DuplicationConfig>();

        public override string Texture => "TheBookofDuplication/BookOfDuplication";

        public override LocalizedText DisplayName => Language.GetText("Mods.TheBookofDuplication.DisplayName");

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.value = 0;
            Item.rare = 11;
            Item.useStyle = ItemUseStyleID.None;
            Item.useTime = 0;
            Item.useAnimation = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 1)
                .AddIngredient(ItemID.FallenStar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            Active = !Active;
            SoundEngine.PlaySound(SoundID.Item4);
        }

        public override bool ConsumeItem(Player player)
        {
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int multiplier = Config.PriceMultiplier;
            
            tooltips.Add(new TooltipLine(Mod, "Description", 
                Language.GetTextValue("Mods.TheBookofDuplication.Description")));
            
            tooltips.Add(new TooltipLine(Mod, "Instruction", 
                Language.GetTextValue("Mods.TheBookofDuplication.InstructionFormat", multiplier)));
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, 
            Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Active)
            {
                float factor = Main.GlobalTimeWrappedHourly % 1;
                float alpha = 0.5f - (float)Math.Cos(factor * Math.PI * 2) * 0.5f;
                float scaleMultiplier = 1 + 0.5f * (float)Math.Pow(factor, 3);
                
                spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, 
                    new Color(0, 150, 255) * alpha, 0, origin, scale * scaleMultiplier, 0, 0);
            }
            base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (Active)
            {
                float factor = Main.GlobalTimeWrappedHourly % 1;
                float alpha = 0.5f - (float)Math.Cos(factor * Math.PI * 2) * 0.5f;
                float scaleMultiplier = 1 + 0.5f * (float)Math.Pow(factor, 3);
                
                spriteBatch.Draw(TextureAssets.Item[Type].Value, Item.Center - Main.screenPosition, null, 
                    new Color(0, 150, 255) * alpha, rotation, new Vector2(14, 18), scale * scaleMultiplier, 0, 0);
            }
            base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
        }
    }
}