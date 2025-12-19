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


                
                if (!context.ShopItems.Any()) SeedAllShopItems(context);

                
                if (!context.Monsters.Any()) SeedAllMonsters(context);

                
                if (!context.Recipes.Any()) SeedAllRecipes(context);

                context.SaveChanges();
            }
        }

        private static void SeedAllShopItems(ApplicationDbContext context)
        {
            var items = new List<ShopItem>();

            
            
            
            string pWep = "/images/weapons/";
            
            items.Add(CreateShopItem("WEP_SWORD_WOOD", "Wooden Sword", "Weapon", "Common", 50, "RES_GOLD", pWep + "wooden_sword.png"));
            items.Add(CreateShopItem("WEP_SWORD_STONE", "Stone Sword", "Weapon", "Common", 100, "RES_GOLD", pWep + "stone_sword.png"));
            items.Add(CreateShopItem("WEP_SWORD_IRON", "Iron Sword", "Weapon", "Uncommon", 300, "RES_GOLD", pWep + "iron_sword.png"));
            items.Add(CreateShopItem("WEP_SWORD_GOLD", "Golden Sword", "Weapon", "Rare", 500, "RES_GOLD", pWep + "golden_sword.png"));
            items.Add(CreateShopItem("WEP_SWORD_DIAMOND", "Diamond Sword", "Weapon", "Epic", 1000, "RES_GEM", pWep + "diamond_sword.png"));
            items.Add(CreateShopItem("WEP_SWORD_NETHERITE", "Netherite Sword", "Weapon", "Legendary", 2000, "RES_GEM", pWep + "netherite_sword.png"));
            
            items.Add(CreateShopItem("WEP_BOW", "Bow", "Weapon", "Uncommon", 200, "RES_GOLD", pWep + "bow.png"));
            items.Add(CreateShopItem("WEP_CROSSBOW", "Crossbow", "Weapon", "Rare", 400, "RES_GOLD", pWep + "crossbow.png"));
            items.Add(CreateShopItem("WEP_TRIDENT", "Trident", "Weapon", "Legendary", 2500, "RES_GEM", pWep + "trident.png"));

            
            
            
            string pTool = "/images/tools/";
            
            items.Add(CreateShopItem("TOOL_PICK_WOOD", "Wooden Pickaxe", "Tool", "Common", 40, "RES_GOLD", pTool + "wooden_pickaxe.png"));
            items.Add(CreateShopItem("TOOL_PICK_STONE", "Stone Pickaxe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_pickaxe.png"));
            items.Add(CreateShopItem("TOOL_PICK_IRON", "Iron Pickaxe", "Tool", "Uncommon", 250, "RES_GOLD", pTool + "iron_pickaxe.png"));
            items.Add(CreateShopItem("TOOL_PICK_DIAMOND", "Diamond Pickaxe", "Tool", "Epic", 800, "RES_GEM", pTool + "diamond_pickaxe.png"));
            
            items.Add(CreateShopItem("TOOL_AXE_WOOD", "Wooden Axe", "Tool", "Common", 40, "RES_GOLD", pTool + "wooden_axe.png"));
            items.Add(CreateShopItem("TOOL_AXE_IRON", "Iron Axe", "Tool", "Uncommon", 250, "RES_GOLD", pTool + "iron_axe.png"));
            items.Add(CreateShopItem("TOOL_AXE_DIAMOND", "Diamond Axe", "Tool", "Epic", 800, "RES_GEM", pTool + "diamond_axe.png"));
            
            items.Add(CreateShopItem("TOOL_SHOVEL_IRON", "Iron Shovel", "Tool", "Uncommon", 200, "RES_GOLD", pTool + "iron_shovel.png"));
            items.Add(CreateShopItem("TOOL_HOE_IRON", "Iron Hoe", "Tool", "Uncommon", 200, "RES_GOLD", pTool + "iron_hoe.png"));

            
            
            
            string pArm = "/images/armor/";
            
            items.Add(CreateShopItem("ARM_HELMET_LEATHER", "Leather Cap", "Armor", "Common", 50, "RES_GOLD", pArm + "leather_helmet.png"));
            items.Add(CreateShopItem("ARM_CHEST_LEATHER", "Leather Tunic", "Armor", "Common", 80, "RES_GOLD", pArm + "leather_chestplate.png"));
            items.Add(CreateShopItem("ARM_LEGS_LEATHER", "Leather Pants", "Armor", "Common", 70, "RES_GOLD", pArm + "leather_leggings.png"));
            items.Add(CreateShopItem("ARM_BOOTS_LEATHER", "Leather Boots", "Armor", "Common", 50, "RES_GOLD", pArm + "leather_boots.png"));
            
            items.Add(CreateShopItem("ARM_HELMET_IRON", "Iron Helmet", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_helmet.png"));
            items.Add(CreateShopItem("ARM_CHEST_IRON", "Iron Chestplate", "Armor", "Uncommon", 400, "RES_GOLD", pArm + "iron_chestplate.png"));
            items.Add(CreateShopItem("ARM_LEGS_IRON", "Iron Leggings", "Armor", "Uncommon", 300, "RES_GOLD", pArm + "iron_leggings.png"));
            items.Add(CreateShopItem("ARM_BOOTS_IRON", "Iron Boots", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_boots.png"));
            
            items.Add(CreateShopItem("ARM_HELMET_DIAMOND", "Diamond Helmet", "Armor", "Epic", 1000, "RES_GEM", pArm + "diamond_helmet.png"));
            items.Add(CreateShopItem("ARM_CHEST_DIAMOND", "Diamond Chestplate", "Armor", "Epic", 2000, "RES_GEM", pArm + "diamond_chestplate.png"));
            items.Add(CreateShopItem("ARM_LEGS_DIAMOND", "Diamond Leggings", "Armor", "Epic", 1500, "RES_GEM", pArm + "diamond_leggings.png"));
            items.Add(CreateShopItem("ARM_BOOTS_DIAMOND", "Diamond Boots", "Armor", "Epic", 1000, "RES_GEM", pArm + "diamond_boots.png"));

            
            
            
            string pCon = "/images/consumables/";
            items.Add(CreateShopItem("CON_APPLE", "Apple", "Consumable", "Common", 10, "RES_GOLD", pCon + "apple.png"));
            items.Add(CreateShopItem("CON_BREAD", "Bread", "Consumable", "Common", 15, "RES_GOLD", pCon + "bread.png"));
            items.Add(CreateShopItem("CON_PORKCHOP", "Raw Porkchop", "Consumable", "Common", 20, "RES_GOLD", pCon + "porkchop.png"));
            items.Add(CreateShopItem("CON_STEAK", "Steak", "Consumable", "Uncommon", 50, "RES_GOLD", pCon + "cooked_beef.png"));
            items.Add(CreateShopItem("CON_CHICKEN", "Cooked Chicken", "Consumable", "Uncommon", 40, "RES_GOLD", pCon + "cooked_chicken.png"));
            items.Add(CreateShopItem("CON_GOLDEN_APPLE", "Golden Apple", "Consumable", "Epic", 200, "RES_GEM", pCon + "golden_apple.png"));
            items.Add(CreateShopItem("CON_ENCHANTED_APPLE", "Enchanted Apple", "Consumable", "Legendary", 500, "RES_GEM", pCon + "enchanted_golden_apple.png"));
            items.Add(CreateShopItem("CON_POTION_HEAL", "Healing Potion", "Consumable", "Rare", 100, "RES_GOLD", pCon + "potion_red.png"));
            items.Add(CreateShopItem("CON_POTION_SPEED", "Speed Potion", "Consumable", "Rare", 100, "RES_GOLD", pCon + "potion_blue.png"));

            
            
            
            string pRes = "/images/resources/";
            items.Add(CreateShopItem("MAT_STICK", "Stick", "Material", "Common", 2, "RES_GOLD", pRes + "stick.png"));
            items.Add(CreateShopItem("MAT_COAL", "Coal", "Material", "Common", 10, "RES_GOLD", pRes + "coal.png"));
            items.Add(CreateShopItem("MAT_IRON_INGOT", "Iron Ingot", "Material", "Uncommon", 30, "RES_GOLD", pRes + "iron_ingot.png"));
            items.Add(CreateShopItem("MAT_GOLD_INGOT", "Gold Ingot", "Material", "Rare", 60, "RES_GOLD", pRes + "gold_ingot.png"));
            items.Add(CreateShopItem("MAT_DIAMOND", "Diamond", "Material", "Epic", 150, "RES_GEM", pRes + "diamond.png"));
            items.Add(CreateShopItem("MAT_EMERALD", "Emerald", "Material", "Epic", 200, "RES_GEM", pRes + "emerald.png"));
            items.Add(CreateShopItem("MAT_NETHERITE", "Netherite Ingot", "Material", "Legendary", 500, "RES_GEM", pRes + "netherite_ingot.png"));
            items.Add(CreateShopItem("MAT_LEATHER", "Leather", "Material", "Common", 10, "RES_GOLD", pRes + "leather.png"));

            
            
            
            string pVeh = "/images/vehicles/";
            items.Add(CreateShopItem("VEH_MINECART", "Minecart", "Vehicle", "Uncommon", 200, "RES_GOLD", pVeh + "minecart.png"));
            items.Add(CreateShopItem("VEH_BOAT", "Oak Boat", "Vehicle", "Common", 100, "RES_GOLD", pVeh + "oak_boat.png"));
            items.Add(CreateShopItem("VEH_ELYTRA", "Elytra Wings", "Equipment", "Legendary", 3000, "RES_GEM", pVeh + "elytra.png"));
            items.Add(CreateShopItem("VEH_SADDLE", "Saddle", "Equipment", "Rare", 500, "RES_GOLD", pVeh + "saddle.png"));

            
            
            
            string pBun = "/images/bundles/";
            items.Add(CreateShopItem("BUNDLE_STARTER", "Starter Pack", "Bundle", "Common", 500, "RES_GOLD", pBun + "bundle_blue.png"));
            items.Add(CreateShopItem("BUNDLE_EXPERT", "Expert Pack", "Bundle", "Epic", 1000, "RES_GEM", pBun + "bundle_red.png"));

            
            
            
            string pBuild = "/images/buildings/";
            items.Add(CreateShopItem("BLUE_HOUSE_S", "Small House BP", "Blueprint", "Common", 1000, "RES_GOLD", pBuild + "house_small.png"));
            items.Add(CreateShopItem("BLUE_CASTLE", "Castle BP", "Blueprint", "Legendary", 5000, "RES_GEM", pBuild + "castle.png"));

            context.ShopItems.AddRange(items);
            Console.WriteLine($">>> Shop Items: Seeded {items.Count} items.");
        }

        private static void SeedAllMonsters(ApplicationDbContext context)
        {
            var monsters = new List<Monster>();
            string pMob = "/images/mobs/";

            
            monsters.Add(new Monster { Name = "Pig", HP = 10, Damage = 0, ExpReward = 5, GoldReward = 5, ImageUrl = pMob + "pig.png" });
            monsters.Add(new Monster { Name = "Cow", HP = 10, Damage = 0, ExpReward = 5, GoldReward = 5, ImageUrl = pMob + "cow.png" });
            monsters.Add(new Monster { Name = "Sheep", HP = 8, Damage = 0, ExpReward = 5, GoldReward = 5, ImageUrl = pMob + "sheep.png" });
            monsters.Add(new Monster { Name = "Chicken", HP = 4, Damage = 0, ExpReward = 2, GoldReward = 2, ImageUrl = pMob + "chicken.png" });

            
            monsters.Add(new Monster { Name = "Zombie", HP = 20, Damage = 3, ExpReward = 15, GoldReward = 10, ImageUrl = pMob + "zombie.png" });
            monsters.Add(new Monster { Name = "Skeleton", HP = 20, Damage = 4, ExpReward = 20, GoldReward = 15, ImageUrl = pMob + "skeleton.png" });
            monsters.Add(new Monster { Name = "Creeper", HP = 20, Damage = 20, ExpReward = 25, GoldReward = 20, ImageUrl = pMob + "creeper.png" });
            monsters.Add(new Monster { Name = "Spider", HP = 16, Damage = 2, ExpReward = 10, GoldReward = 8, ImageUrl = pMob + "spider.png" });
            monsters.Add(new Monster { Name = "Witch", HP = 26, Damage = 6, ExpReward = 40, GoldReward = 30, ImageUrl = pMob + "witch.png" });
            monsters.Add(new Monster { Name = "Enderman", HP = 40, Damage = 7, ExpReward = 50, GoldReward = 50, ImageUrl = pMob + "enderman.png" });

            
            monsters.Add(new Monster { Name = "Blaze", HP = 20, Damage = 5, ExpReward = 60, GoldReward = 40, ImageUrl = pMob + "blaze.png" });
            monsters.Add(new Monster { Name = "Ghast", HP = 10, Damage = 10, ExpReward = 80, GoldReward = 60, ImageUrl = pMob + "ghast.png" });
            monsters.Add(new Monster { Name = "Piglin", HP = 16, Damage = 5, ExpReward = 30, GoldReward = 25, ImageUrl = pMob + "piglin.png" });

            
            monsters.Add(new Monster { Name = "Wither", HP = 300, Damage = 15, ExpReward = 2000, GoldReward = 1000, ImageUrl = pMob + "wither.png" });
            monsters.Add(new Monster { Name = "Ender Dragon", HP = 200, Damage = 20, ExpReward = 5000, GoldReward = 5000, ImageUrl = pMob + "ender_dragon.png" });

            context.Monsters.AddRange(monsters);
            Console.WriteLine($">>> Monsters: Seeded {monsters.Count} mobs.");
        }

        private static void SeedAllRecipes(ApplicationDbContext context)
        {
            var recipes = new List<Recipe>();
            string pWep = "/images/weapons/";
            string pTool = "/images/tools/";
            string pArm = "/images/armor/";

            
            
            recipes.Add(CreateRecipe("R_SWORD_WOOD", "WEP_SWORD_WOOD", "Wooden Sword", pWep + "wooden_sword.png", 5, "MAT_STICK:1")); 
            recipes.Add(CreateRecipe("R_SWORD_STONE", "WEP_SWORD_STONE", "Stone Sword", pWep + "stone_sword.png", 10, "MAT_STICK:1"));
            recipes.Add(CreateRecipe("R_SWORD_IRON", "WEP_SWORD_IRON", "Iron Sword", pWep + "iron_sword.png", 20, "MAT_IRON_INGOT:2,MAT_STICK:1"));
            recipes.Add(CreateRecipe("R_SWORD_DIAMOND", "WEP_SWORD_DIAMOND", "Diamond Sword", pWep + "diamond_sword.png", 60, "MAT_DIAMOND:2,MAT_STICK:1"));

            
            recipes.Add(CreateRecipe("R_HELMET_IRON", "ARM_HELMET_IRON", "Iron Helmet", pArm + "iron_helmet.png", 30, "MAT_IRON_INGOT:5"));
            recipes.Add(CreateRecipe("R_CHEST_IRON", "ARM_CHEST_IRON", "Iron Chestplate", pArm + "iron_chestplate.png", 60, "MAT_IRON_INGOT:8"));
            recipes.Add(CreateRecipe("R_LEGS_IRON", "ARM_LEGS_IRON", "Iron Leggings", pArm + "iron_leggings.png", 45, "MAT_IRON_INGOT:7"));
            recipes.Add(CreateRecipe("R_BOOTS_IRON", "ARM_BOOTS_IRON", "Iron Boots", pArm + "iron_boots.png", 30, "MAT_IRON_INGOT:4"));

            
            recipes.Add(CreateRecipe("R_PICK_IRON", "TOOL_PICK_IRON", "Iron Pickaxe", pTool + "iron_pickaxe.png", 25, "MAT_IRON_INGOT:3,MAT_STICK:2"));
            recipes.Add(CreateRecipe("R_PICK_DIAMOND", "TOOL_PICK_DIAMOND", "Diamond Pickaxe", pTool + "diamond_pickaxe.png", 60, "MAT_DIAMOND:3,MAT_STICK:2"));

            context.Recipes.AddRange(recipes);
            Console.WriteLine($">>> Recipes: Seeded {recipes.Count} recipes.");
        }

        

        private static ShopItem CreateShopItem(string id, string name, string type, string rarity, int price, string currency, string imgUrl)
        {
            return new ShopItem
            {
                ProductID = id,
                TargetItemID = id,
                Name = name,
                Description = $"A {rarity} {name} ({type}).",
                ImageURL = imgUrl, 
                PriceAmount = price,
                PriceCurrency = currency,
                ItemType = type, 
                Rarity = rarity,
                IsShow = true 
            };
        }

        private static Recipe CreateRecipe(string rid, string resId, string name, string img, int time, string ingredients)
        {
            return new Recipe
            {
                RecipeId = rid,
                ResultItemId = resId,
                ResultItemName = name,
                ResultItemImage = img,
                CraftingTime = time,
                Ingredients = ingredients
            };
        }
    }
}