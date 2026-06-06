using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.Audio;
using Terraria.UI;
using Terraria.Localization;

namespace TheBookofDuplication
{
    public class DuplicationGlobalItem : GlobalItem
    {
        private static DuplicationConfig Config => ModContent.GetInstance<DuplicationConfig>();

        public override void Load()
        {
            var method = typeof(ItemLoader).GetMethod("RightClick", BindingFlags.Static | BindingFlags.Public);
            if (method != null)
            {
                MonoModHooks.Add(method, DuplicationModifyRightClick);
            }
            
            On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
        }

        private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item)
        {
            if (BookOfDuplication.Active) return;
            orig.Invoke(item);
        }

        public delegate void orig_RightClick(Item item, Player player);

        private static bool IsItemDuplicable(Item item)
        {
            if (item.type == ModContent.ItemType<BookOfDuplication>()) return false;
            if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin) return false;
            if (item.type == ModContent.ItemType<UnloadedItem>()) return false;
            if (Main.ItemDropsDB.GetRulesForItemID(item.type).Count != 0) return false;
            return true;
        }

        public static void DuplicationModifyRightClick(orig_RightClick orig, Item item, Player player)
        {
            if (player.talkNPC != -1)
            {
                orig(item, player);
                return;
            }

            if (BookOfDuplication.Active && IsItemDuplicable(item))
            {
                long price = (long)(item.GetStoreValue() * 0.2 * Config.PriceMultiplier);
                if (price < 0) price = 0;
                
                if (player.CanAfford(price))
                {
                    if (Main.stackSplit > 1) return;
                    
                    int m = Main.superFastStack + 1;
                    
                    for (int n = 0; n < m; n++)
                    {
                        bool canStack = Main.mouseItem.type == item.type && Main.mouseItem.stack < Main.mouseItem.maxStack;
                        bool canPlace = Main.mouseItem.type == ItemID.None;
                        
                        if (!(canStack || canPlace)) break;

                        player.BuyItem(price);

                        if (canStack)
                        {
                            Main.mouseItem.stack++;
                        }
                        else if (canPlace)
                        {
                            Main.mouseItem = item.Clone();
                            Main.mouseItem.stack = 1;
                        }

                        if (n == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Coins);
                            ItemSlot.RefreshStackSplitCooldown();
                        }
                    }
                    return;
                }
            }
            
            orig(item, player);
        }

        public override bool CanRightClick(Item item)
        {
            if (item.type == ItemID.None) return false;
            if (Main.LocalPlayer.talkNPC != -1) return false;
            if (!BookOfDuplication.Active) return false;
            if (!IsItemDuplicable(item)) return false;
            
            long price = (long)(item.GetStoreValue() * 0.2 * Config.PriceMultiplier);
            if (price < 0) price = 0;
            if (!Main.LocalPlayer.CanAfford(price)) return false;
            
            return Main.mouseRightRelease = true;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!BookOfDuplication.Active) return;

            bool inShop = Main.LocalPlayer.talkNPC != -1;

            TooltipLine nameLine = null;
            foreach (var tip in tooltips)
            {
                if (tip.Name == "ItemName")
                    nameLine = tip;
            }
            tooltips.Clear();
            if (nameLine != null)
                tooltips.Add(nameLine);

            if (inShop)
            {
                tooltips.Add(new TooltipLine(Mod, "ShopCannotDuplicate",
                    Language.GetTextValue("Mods.TheBookofDuplication.ShopCannotDuplicate"))
                {
                    OverrideColor = Color.Gray
                });
                return;
            }

            if (item.type == ModContent.ItemType<BookOfDuplication>())
            {
                tooltips.Add(new TooltipLine(Mod, "ExitInstruction",
                    Language.GetTextValue("Mods.TheBookofDuplication.ExitInstruction"))
                {
                    OverrideColor = Color.LightGreen
                });
                return;
            }

            if (IsItemDuplicable(item))
            {
                long price = (long)(item.GetStoreValue() * 0.2 * Config.PriceMultiplier);
                if (price < 0) price = 0;
                
                if (item.GetStoreValue() == 0 || price == 0)
                {
                    tooltips.Add(new TooltipLine(Mod, "DuplicatePriceFree",
                        Language.GetTextValue("Mods.TheBookofDuplication.PriceFree"))
                    {
                        OverrideColor = Color.LightGreen
                    });
                }
                else
                {
                    string priceText = FormatPrice(price);
                    tooltips.Add(new TooltipLine(Mod, "DuplicatePrice",
                        Language.GetTextValue("Mods.TheBookofDuplication.DuplicatePriceFormat", priceText))
                    {
                        OverrideColor = GetPriceColor(price)
                    });
                }

                tooltips.Add(new TooltipLine(Mod, "DuplicateInstruction",
                    Language.GetTextValue("Mods.TheBookofDuplication.DuplicateInstruction"))
                {
                    OverrideColor = Color.LightGreen
                });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "CannotDuplicate",
                    Language.GetTextValue("Mods.TheBookofDuplication.CannotDuplicate"))
                {
                    OverrideColor = Color.Gray
                });
            }
        }

        private string FormatPrice(long price)
        {
            long platinum = price / 1000000;
            price -= platinum * 1000000;
            long gold = price / 10000;
            price -= gold * 10000;
            long silver = price / 100;
            long copper = price % 100;

            string text = "";
            if (platinum > 0) text += $"{platinum} {Language.GetTextValue("Mods.TheBookofDuplication.Platinum")} ";
            if (gold > 0) text += $"{gold} {Language.GetTextValue("Mods.TheBookofDuplication.Gold")} ";
            if (silver > 0) text += $"{silver} {Language.GetTextValue("Mods.TheBookofDuplication.Silver")} ";
            if (copper > 0) text += $"{copper} {Language.GetTextValue("Mods.TheBookofDuplication.Copper")} ";
            return text.Trim();
        }

        private Color GetPriceColor(long price)
        {
            if (price >= 1000000) return new Color(220, 220, 198);
            if (price >= 10000) return new Color(224, 201, 92);
            if (price >= 100) return new Color(181, 192, 193);
            return new Color(246, 138, 96);
        }
    }
}