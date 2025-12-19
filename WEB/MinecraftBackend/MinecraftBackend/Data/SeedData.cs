using Microsoft.EntityFrameworkCore;
using MinecraftBackend.Models;
using System;
using System.Collections.Generic;
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

                var items = new List<ShopItem>();

                
                items.Add(CreateItem("WEP_SWORD_WOOD", "Wooden Sword", "Weapon", "Common", 50, "RES_GOLD", "/images/weapons/wooden_sword.png"));
                items.Add(CreateItem("WEP_SWORD_STONE", "Stone Sword", "Weapon", "Common", 150, "RES_GOLD", "/images/weapons/stone_sword.png"));
                items.Add(CreateItem("WEP_SWORD_IRON", "Iron Sword", "Weapon", "Uncommon", 500, "RES_GOLD", "/images/weapons/iron_sword.png"));
                items.Add(CreateItem("WEP_SWORD_GOLD", "Golden Sword", "Weapon", "Rare", 800, "RES_GOLD", "/images/weapons/gold_sword.png"));
                items.Add(CreateItem("WEP_SWORD_DIAMOND", "Diamond Sword", "Weapon", "Epic", 500, "RES_GEM", "/images/weapons/diamond_sword.png"));
                items.Add(CreateItem("WEP_SWORD_NETHERITE", "Netherite Sword", "Weapon", "Legendary", 1000, "RES_GEM", "/images/weapons/netherite_sword.png"));

                
                items.Add(CreateItem("TOOL_PICKAXE_STONE", "Stone Pickaxe", "Tool", "Common", 100, "RES_GOLD", "/images/tools/stone_pickaxe.png"));
                items.Add(CreateItem("TOOL_AXE_STONE", "Stone Axe", "Tool", "Common", 100, "RES_GOLD", "/images/tools/stone_axe.png"));
                items.Add(CreateItem("TOOL_SHOVEL_STONE", "Stone Shovel", "Tool", "Common", 80, "RES_GOLD", "/images/tools/stone_shovel.png"));
                items.Add(CreateItem("TOOL_HOE_STONE", "Stone Hoe", "Tool", "Common", 80, "RES_GOLD", "/images/tools/stone_hoe.png"));

                
                items.Add(CreateItem("ARM_HELMET_IRON", "Iron Helmet", "Armor", "Uncommon", 300, "RES_GOLD", "/images/armor/iron_helmet.png"));
                items.Add(CreateItem("ARM_CHEST_IRON", "Iron Chestplate", "Armor", "Uncommon", 500, "RES_GOLD", "/images/armor/iron_chestplate.png"));
                items.Add(CreateItem("ARM_LEGS_IRON", "Iron Leggings", "Armor", "Uncommon", 400, "RES_GOLD", "/images/armor/iron_leggings.png"));
                items.Add(CreateItem("ARM_BOOTS_IRON", "Iron Boots", "Armor", "Uncommon", 300, "RES_GOLD", "/images/armor/iron_boots.png"));

                
                items.Add(CreateItem("CON_APPLE", "Red Apple", "Consumable", "Common", 10, "RES_GOLD", "/images/consumables/apple.png"));
                items.Add(CreateItem("CON_BREAD", "Wheat Bread", "Consumable", "Common", 20, "RES_GOLD", "/images/consumables/bread.png"));
                items.Add(CreateItem("CON_CARROT", "Carrot", "Consumable", "Common", 15, "RES_GOLD", "/images/consumables/carrot.png"));
                items.Add(CreateItem("CON_POTATO", "Potato", "Consumable", "Common", 15, "RES_GOLD", "/images/consumables/potato.png"));
                items.Add(CreateItem("CON_MELON", "Melon Slice", "Consumable", "Common", 10, "RES_GOLD", "/images/consumables/melon.png"));
                items.Add(CreateItem("CON_COOKIE", "Cookie", "Consumable", "Common", 25, "RES_GOLD", "/images/consumables/cookie.png"));
                items.Add(CreateItem("CON_PORKCHOP", "Raw Porkchop", "Consumable", "Uncommon", 40, "RES_GOLD", "/images/consumables/porkchop.png"));
                items.Add(CreateItem("CON_STEAK", "Steak", "Consumable", "Rare", 80, "RES_GOLD", "/images/consumables/steak.png"));
                items.Add(CreateItem("CON_PUMPKIN_PIE", "Pumpkin Pie", "Consumable", "Rare", 100, "RES_GOLD", "/images/consumables/pumpkin_pie.png"));
                items.Add(CreateItem("CON_CAKE", "Cake", "Consumable", "Epic", 200, "RES_GOLD", "/images/consumables/cake.png"));
                items.Add(CreateItem("CON_GOLDEN_APPLE", "Golden Apple", "Consumable", "Legendary", 50, "RES_GEM", "/images/consumables/golden_apple.png"));
                items.Add(CreateItem("CON_POTION", "Magic Potion", "Consumable", "Epic", 30, "RES_GEM", "/images/consumables/potion.png"));

                
                
                items.Add(CreateItem("RES_COAL", "Coal", "Resource", "Common", 20, "RES_GOLD", "/images/resources/coal.png"));
                items.Add(CreateItem("RES_IRON_INGOT", "Iron Ingot", "Resource", "Uncommon", 50, "RES_GOLD", "/images/resources/iron_ingot.png"));
                items.Add(CreateItem("RES_GOLD_INGOT", "Gold Ingot", "Resource", "Rare", 100, "RES_GOLD", "/images/resources/gold_ingot.png"));
                items.Add(CreateItem("RES_COPPER", "Copper Ingot", "Resource", "Common", 30, "RES_GOLD", "/images/resources/copper_ingot.png"));
                items.Add(CreateItem("RES_DIAMOND", "Diamond", "Resource", "Epic", 100, "RES_GEM", "/images/resources/diamond.png"));
                items.Add(CreateItem("RES_EMERALD", "Emerald", "Resource", "Epic", 150, "RES_GEM", "/images/resources/emerald.png"));
                items.Add(CreateItem("RES_NETHERITE", "Netherite Scrap", "Resource", "Legendary", 500, "RES_GEM", "/images/resources/netherite_scrap.png"));
                items.Add(CreateItem("RES_LAPIS", "Lapis Lazuli", "Resource", "Rare", 50, "RES_GOLD", "/images/resources/lapis_lazuli.png"));
                items.Add(CreateItem("RES_QUARTZ", "Nether Quartz", "Resource", "Uncommon", 40, "RES_GOLD", "/images/resources/quartz.png"));
                items.Add(CreateItem("RES_REDSTONE", "Redstone Dust", "Resource", "Uncommon", 30, "RES_GOLD", "/images/resources/redstone_dust.png"));
                items.Add(CreateItem("RES_AMETHYST", "Amethyst Shard", "Resource", "Rare", 80, "RES_GOLD", "/images/resources/amethyst_shard.png"));

                
                items.Add(CreateItem("MAT_OAK_LOG", "Oak Log", "Material", "Common", 10, "RES_GOLD", "/images/resources/oak_log.png"));
                items.Add(CreateItem("MAT_COBBLESTONE", "Cobblestone", "Material", "Common", 5, "RES_GOLD", "/images/resources/cobblestone.png"));
                items.Add(CreateItem("MAT_OBSIDIAN", "Obsidian", "Material", "Rare", 200, "RES_GOLD", "/images/resources/obsidian.png"));
                items.Add(CreateItem("MAT_SLIME", "Slime Ball", "Material", "Uncommon", 60, "RES_GOLD", "/images/resources/slime_ball.png"));

                
                items.Add(CreateItem("VEH_HORSE", "Horse", "Mount", "Epic", 5000, "RES_GOLD", "/images/vehicles/horse.png"));
                items.Add(CreateItem("VEH_DONKEY", "Donkey", "Mount", "Rare", 3000, "RES_GOLD", "/images/vehicles/donkey.png"));
                items.Add(CreateItem("VEH_MULE", "Mule", "Mount", "Rare", 3500, "RES_GOLD", "/images/vehicles/mule.png"));
                items.Add(CreateItem("VEH_PIG", "Saddled Pig", "Mount", "Uncommon", 1000, "RES_GOLD", "/images/vehicles/pig.png"));
                items.Add(CreateItem("VEH_LLAMA", "Llama", "Mount", "Rare", 2500, "RES_GOLD", "/images/vehicles/llama.png"));
                items.Add(CreateItem("VEH_STRIDER", "Strider", "Mount", "Epic", 4000, "RES_GOLD", "/images/vehicles/strider.png"));
                items.Add(CreateItem("VEH_ELYTRA", "Elytra Wings", "Equipment", "Legendary", 2000, "RES_GEM", "/images/vehicles/elytra.png"));
                items.Add(CreateItem("VEH_BOAT", "Oak Boat", "Vehicle", "Common", 200, "RES_GOLD", "/images/vehicles/boat.png"));
                items.Add(CreateItem("VEH_RAFT", "Bamboo Raft", "Vehicle", "Common", 200, "RES_GOLD", "/images/vehicles/raft.png"));
                items.Add(CreateItem("VEH_MINECART", "Minecart", "Vehicle", "Uncommon", 500, "RES_GOLD", "/images/vehicles/minecart.png"));

                
                
                items.Add(CreateItem("EGG_CREEPER", "Creeper Essence", "Summon", "Rare", 50, "RES_GEM", "/images/mobs/creeper.png"));
                items.Add(CreateItem("EGG_ENDERMAN", "Enderman Essence", "Summon", "Epic", 100, "RES_GEM", "/images/mobs/enderman.png"));
                items.Add(CreateItem("EGG_SKELETON", "Skeleton Skull", "Summon", "Uncommon", 30, "RES_GEM", "/images/mobs/skeleton.png"));
                items.Add(CreateItem("EGG_ZOMBIE", "Zombie Head", "Summon", "Uncommon", 30, "RES_GEM", "/images/mobs/zombie.png"));
                items.Add(CreateItem("EGG_BLAZE", "Blaze Rod", "Summon", "Rare", 60, "RES_GEM", "/images/mobs/blaze.png"));
                items.Add(CreateItem("EGG_DRAGON", "Dragon Egg", "Artifact", "Legendary", 9999, "RES_GEM", "/images/mobs/dragon.png"));
                items.Add(CreateItem("EGG_WITCH", "Witch Hat", "Summon", "Rare", 70, "RES_GEM", "/images/mobs/witch.png"));
                items.Add(CreateItem("EGG_GHAST", "Ghast Tear", "Summon", "Epic", 90, "RES_GEM", "/images/mobs/ghast.png"));
                items.Add(CreateItem("EGG_SLIME", "Big Slime", "Summon", "Uncommon", 40, "RES_GEM", "/images/mobs/slime.png"));
                items.Add(CreateItem("EGG_SPIDER", "Spider Eye", "Summon", "Common", 20, "RES_GEM", "/images/mobs/spider.png"));

                
                items.Add(CreateItem("BLUEPRINT_HOUSE", "Basic House Blueprint", "Blueprint", "Common", 1000, "RES_GOLD", "/images/buildings/structure_01.png"));
                items.Add(CreateItem("BLUEPRINT_TOWER", "Watchtower Blueprint", "Blueprint", "Rare", 5000, "RES_GOLD", "/images/buildings/structure_02.png"));
                items.Add(CreateItem("BLUEPRINT_CASTLE", "Castle Blueprint", "Blueprint", "Epic", 500, "RES_GEM", "/images/buildings/structure_03.png"));

                
                items.Add(CreateItem("BUNDLE_STARTER", "Starter Bundle", "Bundle", "Common", 500, "RES_GOLD", "/images/bundles/bundle_cost.png"));
                items.Add(CreateItem("BUNDLE_LOOT", "Lucky Loot Bag", "Bundle", "Rare", 50, "RES_GEM", "/images/bundles/bundle_loot.png"));
                items.Add(CreateItem("BUNDLE_MYSTERY", "Mystery Box", "Bundle", "Epic", 100, "RES_GEM", "/images/bundles/bundle_product.png"));

                
                context.ShopItems.AddRange(items);
                context.SaveChanges();
            }
        }

        
        private static ShopItem CreateItem(string id, string name, string type, string rarity, int price, string currency, string imgUrl)
        {
            return new ShopItem
            {
                ProductID = id,
                TargetItemID = id, 
                Name = name,
                Description = $"A {rarity.ToLower()} {name.ToLower()} ({type}). Used for crafting or equipping.",
                ImageURL = imgUrl, 
                PriceAmount = price,
                PriceCurrency = currency,
                ItemType = type,
                Rarity = rarity,
                IsShow = true
            };
        }
    }
}