using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Models;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace MinecraftBackend.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                context.Database.EnsureCreated();
                if (context.ShopItems.Any())
                {
                    return; 
                }

                Console.WriteLine(">>> Seeding Sample Data for Shop & Inventory Master Data...");

                context.ShopItems.AddRange(
                    new ShopItem
                    {
                        ProductID = "WEP_WOODEN_SWORD",
                        TargetItemID = "WEP_WOODEN_SWORD",
                        Name = "Wooden Sword",
                        Description = "A fragile sword for beginners. 5 ATK.",
                        ImageURL = "/images/weapons/wooden_sword.png",
                        PriceAmount = 50,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Weapon",
                        Rarity = "Common",
                        IsShow = true
                    },
                    new ShopItem
                    {
                        ProductID = "WEP_IRON_SWORD",
                        TargetItemID = "WEP_IRON_SWORD",
                        Name = "Iron Sword",
                        Description = "Standard soldier weapon. 15 ATK.",
                        ImageURL = "/images/weapons/iron_sword.png",
                        PriceAmount = 250,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Weapon",
                        Rarity = "Rare",
                        IsShow = true
                    },
                    new ShopItem
                    {
                        ProductID = "WEP_DIAMOND_SWORD",
                        TargetItemID = "WEP_DIAMOND_SWORD",
                        Name = "Diamond Sword",
                        Description = "Legendary blade. 50 ATK.",
                        ImageURL = "/images/weapons/diamond_sword.png",
                        PriceAmount = 50,
                        PriceCurrency = "RES_GEM",
                        ItemType = "Weapon",
                        Rarity = "Legendary",
                        IsShow = true
                    },

                    new ShopItem
                    {
                        ProductID = "ARM_IRON_HELMET",
                        TargetItemID = "ARM_IRON_HELMET",
                        Name = "Iron Helmet",
                        Description = "Protects your head. +2 DEF.",
                        ImageURL = "/images/armor/iron_helmet.png",
                        PriceAmount = 150,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Armor",
                        Rarity = "Common",
                        IsShow = true
                    },
                    new ShopItem
                    {
                        ProductID = "ARM_IRON_CHEST",
                        TargetItemID = "ARM_IRON_CHEST",
                        Name = "Iron Chestplate",
                        Description = "Heavy protection. +6 DEF.",
                        ImageURL = "/images/armor/iron_chestplate.png",
                        PriceAmount = 400,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Armor",
                        Rarity = "Rare",
                        IsShow = true
                    },

                    new ShopItem
                    {
                        ProductID = "CON_APPLE",
                        TargetItemID = "CON_APPLE",
                        Name = "Red Apple",
                        Description = "Restores 10 Health.",
                        ImageURL = "/images/resources/apple.png",
                        PriceAmount = 10,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Consumable",
                        Rarity = "Common",
                        IsShow = true
                    },
                    new ShopItem
                    {
                        ProductID = "CON_BREAD",
                        TargetItemID = "CON_BREAD",
                        Name = "Bread",
                        Description = "Restores 25 Hunger.",
                        ImageURL = "/images/resources/bread.png",
                        PriceAmount = 20,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Consumable",
                        Rarity = "Common",
                        IsShow = true
                    },
                    new ShopItem
                    {
                        ProductID = "CON_POTION_HEAL",
                        TargetItemID = "CON_POTION_HEAL",
                        Name = "Healing Potion",
                        Description = "Instantly restores 50 HP.",
                        ImageURL = "/images/consumables/potion_red.png",
                        PriceAmount = 10,
                        PriceCurrency = "RES_GEM",
                        ItemType = "Consumable",
                        Rarity = "Rare",
                        IsShow = true
                    },

                    new ShopItem
                    {
                        ProductID = "RES_DIAMOND",
                        TargetItemID = "RES_DIAMOND",
                        Name = "Diamond",
                        Description = "A precious crafting material.",
                        ImageURL = "/images/resources/diamond.png",
                        PriceAmount = 100,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Resource",
                        Rarity = "Rare",
                        IsShow = true
                    },
                    new ShopItem
                    {
                        ProductID = "RES_IRON_INGOT",
                        TargetItemID = "RES_IRON_INGOT",
                        Name = "Iron Ingot",
                        Description = "Used to craft tools and armor.",
                        ImageURL = "/images/resources/iron_ingot.png",
                        PriceAmount = 20,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Resource",
                        Rarity = "Common",
                        IsShow = true
                    },

                    new ShopItem
                    {
                        ProductID = "BOX_STARTER",
                        TargetItemID = "BOX_STARTER",
                        Name = "Starter Bundle",
                        Description = "Get a head start with basic gear.",
                        ImageURL = "/images/others/bundle_starter.png",
                        PriceAmount = 100,
                        PriceCurrency = "RES_GOLD",
                        ItemType = "Bundle",
                        Rarity = "Common",
                        IsShow = true
                    }
                );

                context.SaveChanges();
                Console.WriteLine(">>> Seeding Completed Successfully!");
            }
        }
    }
}