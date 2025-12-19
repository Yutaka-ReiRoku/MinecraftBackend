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
                // Đảm bảo Database đã được tạo
                context.Database.EnsureCreated();

                Console.WriteLine(">>> BẮT ĐẦU SEEDING DATA (FULL CHECK)...");

                // --- 1. SHOP ITEMS (Bán mọi thứ có thể mua/cầm nắm) ---
                if (!context.ShopItems.Any())
                {
                    SeedShopItems(context);
                }

                // --- 2. MONSTERS (Toàn bộ mob trong folder mobs) ---
                if (!context.Monsters.Any())
                {
                    SeedMonsters(context);
                }

                // --- 3. RECIPES (Công thức ghép đồ) ---
                if (!context.Recipes.Any())
                {
                    SeedRecipes(context);
                }

                context.SaveChanges();
                Console.WriteLine(">>> HOÀN TẤT SEEDING! KHÔNG BỎ SÓT FILE NÀO.");
            }
        }

        private static void SeedShopItems(ApplicationDbContext context)
        {
            var items = new List<ShopItem>();

            // ==================================================================================
            // 1. WEAPONS (Vũ khí) - Folder: /images/weapons/
            // ==================================================================================
            string pWep = "/images/weapons/";

            // Swords (Kiếm)
            items.Add(CreateItem("WEP_WOOD_SWORD", "Wooden Sword", "Weapon", "Common", 50, "RES_GOLD", pWep + "wooden_sword.png"));
            items.Add(CreateItem("WEP_STONE_SWORD", "Stone Sword", "Weapon", "Common", 100, "RES_GOLD", pWep + "stone_sword.png"));
            items.Add(CreateItem("WEP_IRON_SWORD", "Iron Sword", "Weapon", "Uncommon", 300, "RES_GOLD", pWep + "iron_sword.png"));
            items.Add(CreateItem("WEP_GOLD_SWORD", "Golden Sword", "Weapon", "Rare", 500, "RES_GOLD", pWep + "golden_sword.png"));
            items.Add(CreateItem("WEP_DIAMOND_SWORD", "Diamond Sword", "Weapon", "Epic", 1000, "RES_GEM", pWep + "diamond_sword.png"));
            items.Add(CreateItem("WEP_NETHERITE_SWORD", "Netherite Sword", "Weapon", "Legendary", 2000, "RES_GEM", pWep + "netherite_sword.png"));

            // Ranged (Tầm xa)
            items.Add(CreateItem("WEP_BOW", "Bow", "Weapon", "Uncommon", 150, "RES_GOLD", pWep + "bow.png"));
            items.Add(CreateItem("WEP_CROSSBOW", "Crossbow", "Weapon", "Rare", 300, "RES_GOLD", pWep + "crossbow.png"));
            items.Add(CreateItem("WEP_TRIDENT", "Trident", "Weapon", "Legendary", 2500, "RES_GEM", pWep + "trident.png"));
            items.Add(CreateItem("WEP_ARROW", "Arrow", "Ammo", "Common", 5, "RES_GOLD", pWep + "arrow.png")); // Thêm mũi tên nếu có

            // ==================================================================================
            // 2. TOOLS (Công cụ) - Folder: /images/tools/
            // ==================================================================================
            string pTool = "/images/tools/";

            // Pickaxes (Cúp)
            items.Add(CreateItem("TOOL_WOOD_PICK", "Wooden Pickaxe", "Tool", "Common", 40, "RES_GOLD", pTool + "wooden_pickaxe.png"));
            items.Add(CreateItem("TOOL_STONE_PICK", "Stone Pickaxe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_pickaxe.png"));
            items.Add(CreateItem("TOOL_IRON_PICK", "Iron Pickaxe", "Tool", "Uncommon", 200, "RES_GOLD", pTool + "iron_pickaxe.png"));
            items.Add(CreateItem("TOOL_GOLD_PICK", "Golden Pickaxe", "Tool", "Rare", 350, "RES_GOLD", pTool + "golden_pickaxe.png"));
            items.Add(CreateItem("TOOL_DIAMOND_PICK", "Diamond Pickaxe", "Tool", "Epic", 800, "RES_GEM", pTool + "diamond_pickaxe.png"));
            items.Add(CreateItem("TOOL_NETHERITE_PICK", "Netherite Pickaxe", "Tool", "Legendary", 1500, "RES_GEM", pTool + "netherite_pickaxe.png"));

            // Axes (Rìu)
            items.Add(CreateItem("TOOL_WOOD_AXE", "Wooden Axe", "Tool", "Common", 40, "RES_GOLD", pTool + "wooden_axe.png"));
            items.Add(CreateItem("TOOL_STONE_AXE", "Stone Axe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_axe.png"));
            items.Add(CreateItem("TOOL_IRON_AXE", "Iron Axe", "Tool", "Uncommon", 200, "RES_GOLD", pTool + "iron_axe.png"));
            items.Add(CreateItem("TOOL_DIAMOND_AXE", "Diamond Axe", "Tool", "Epic", 800, "RES_GEM", pTool + "diamond_axe.png"));

            // Shovels (Xẻng) & Hoes (Cuốc)
            items.Add(CreateItem("TOOL_IRON_SHOVEL", "Iron Shovel", "Tool", "Uncommon", 150, "RES_GOLD", pTool + "iron_shovel.png"));
            items.Add(CreateItem("TOOL_DIAMOND_SHOVEL", "Diamond Shovel", "Tool", "Epic", 600, "RES_GEM", pTool + "diamond_shovel.png"));
            items.Add(CreateItem("TOOL_IRON_HOE", "Iron Hoe", "Tool", "Uncommon", 150, "RES_GOLD", pTool + "iron_hoe.png"));

            // Misc Tools (Khác)
            items.Add(CreateItem("TOOL_FISHING_ROD", "Fishing Rod", "Tool", "Common", 100, "RES_GOLD", pTool + "fishing_rod.png"));
            items.Add(CreateItem("TOOL_FLINT_STEEL", "Flint and Steel", "Tool", "Uncommon", 120, "RES_GOLD", pTool + "flint_and_steel.png"));
            items.Add(CreateItem("TOOL_SHEARS", "Shears", "Tool", "Common", 80, "RES_GOLD", pTool + "shears.png"));
            items.Add(CreateItem("TOOL_SHIELD", "Shield", "Equipment", "Uncommon", 300, "RES_GOLD", pTool + "shield.png"));

            // ==================================================================================
            // 3. ARMOR (Giáp) - Folder: /images/armor/
            // ==================================================================================
            string pArm = "/images/armor/";

            // Leather
            items.Add(CreateItem("ARM_LEATHER_HELMET", "Leather Cap", "Armor", "Common", 50, "RES_GOLD", pArm + "leather_helmet.png"));
            items.Add(CreateItem("ARM_LEATHER_CHEST", "Leather Tunic", "Armor", "Common", 80, "RES_GOLD", pArm + "leather_chestplate.png"));
            items.Add(CreateItem("ARM_LEATHER_LEGS", "Leather Pants", "Armor", "Common", 70, "RES_GOLD", pArm + "leather_leggings.png"));
            items.Add(CreateItem("ARM_LEATHER_BOOTS", "Leather Boots", "Armor", "Common", 50, "RES_GOLD", pArm + "leather_boots.png"));

            // Iron
            items.Add(CreateItem("ARM_IRON_HELMET", "Iron Helmet", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_helmet.png"));
            items.Add(CreateItem("ARM_IRON_CHEST", "Iron Chestplate", "Armor", "Uncommon", 400, "RES_GOLD", pArm + "iron_chestplate.png"));
            items.Add(CreateItem("ARM_IRON_LEGS", "Iron Leggings", "Armor", "Uncommon", 300, "RES_GOLD", pArm + "iron_leggings.png"));
            items.Add(CreateItem("ARM_IRON_BOOTS", "Iron Boots", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_boots.png"));

            // Golden
            items.Add(CreateItem("ARM_GOLD_HELMET", "Golden Helmet", "Armor", "Rare", 300, "RES_GOLD", pArm + "golden_helmet.png"));
            items.Add(CreateItem("ARM_GOLD_CHEST", "Golden Chestplate", "Armor", "Rare", 600, "RES_GOLD", pArm + "golden_chestplate.png"));

            // Diamond
            items.Add(CreateItem("ARM_DIAMOND_HELMET", "Diamond Helmet", "Armor", "Epic", 1000, "RES_GEM", pArm + "diamond_helmet.png"));
            items.Add(CreateItem("ARM_DIAMOND_CHEST", "Diamond Chestplate", "Armor", "Epic", 2000, "RES_GEM", pArm + "diamond_chestplate.png"));
            items.Add(CreateItem("ARM_DIAMOND_LEGS", "Diamond Leggings", "Armor", "Epic", 1500, "RES_GEM", pArm + "diamond_leggings.png"));
            items.Add(CreateItem("ARM_DIAMOND_BOOTS", "Diamond Boots", "Armor", "Epic", 1000, "RES_GEM", pArm + "diamond_boots.png"));

            // Netherite
            items.Add(CreateItem("ARM_NETHER_HELMET", "Netherite Helmet", "Armor", "Legendary", 2500, "RES_GEM", pArm + "netherite_helmet.png"));
            items.Add(CreateItem("ARM_NETHER_CHEST", "Netherite Chestplate", "Armor", "Legendary", 5000, "RES_GEM", pArm + "netherite_chestplate.png"));

            // ==================================================================================
            // 4. CONSUMABLES (Thức ăn & Thuốc) - Folder: /images/consumables/
            // ==================================================================================
            string pCon = "/images/consumables/";

            // Food
            items.Add(CreateItem("CON_APPLE", "Apple", "Consumable", "Common", 5, "RES_GOLD", pCon + "apple.png"));
            items.Add(CreateItem("CON_BREAD", "Bread", "Consumable", "Common", 10, "RES_GOLD", pCon + "bread.png"));
            items.Add(CreateItem("CON_PORKCHOP", "Raw Porkchop", "Consumable", "Common", 15, "RES_GOLD", pCon + "porkchop.png"));
            items.Add(CreateItem("CON_COOKED_PORK", "Cooked Porkchop", "Consumable", "Uncommon", 40, "RES_GOLD", pCon + "cooked_porkchop.png"));
            items.Add(CreateItem("CON_BEEF", "Raw Beef", "Consumable", "Common", 15, "RES_GOLD", pCon + "beef.png"));
            items.Add(CreateItem("CON_STEAK", "Steak", "Consumable", "Uncommon", 50, "RES_GOLD", pCon + "cooked_beef.png")); // Check if file is steak.png or cooked_beef.png
            items.Add(CreateItem("CON_CHICKEN", "Raw Chicken", "Consumable", "Common", 12, "RES_GOLD", pCon + "chicken.png"));
            items.Add(CreateItem("CON_COOKED_CHICKEN", "Cooked Chicken", "Consumable", "Uncommon", 35, "RES_GOLD", pCon + "cooked_chicken.png"));
            items.Add(CreateItem("CON_CARROT", "Carrot", "Consumable", "Common", 8, "RES_GOLD", pCon + "carrot.png"));
            items.Add(CreateItem("CON_POTATO", "Potato", "Consumable", "Common", 8, "RES_GOLD", pCon + "potato.png"));
            items.Add(CreateItem("CON_BAKED_POTATO", "Baked Potato", "Consumable", "Uncommon", 25, "RES_GOLD", pCon + "baked_potato.png"));
            items.Add(CreateItem("CON_MELON", "Melon Slice", "Consumable", "Common", 5, "RES_GOLD", pCon + "melon_slice.png"));
            items.Add(CreateItem("CON_SWEET_BERRIES", "Sweet Berries", "Consumable", "Common", 5, "RES_GOLD", pCon + "sweet_berries.png"));

            // Rare Food
            items.Add(CreateItem("CON_GOLDEN_APPLE", "Golden Apple", "Consumable", "Epic", 200, "RES_GEM", pCon + "golden_apple.png"));
            items.Add(CreateItem("CON_ENCHANTED_APPLE", "Enchanted Apple", "Consumable", "Legendary", 1000, "RES_GEM", pCon + "enchanted_golden_apple.png"));
            items.Add(CreateItem("CON_CAKE", "Cake", "Consumable", "Rare", 150, "RES_GOLD", pCon + "cake.png"));
            items.Add(CreateItem("CON_PUMPKIN_PIE", "Pumpkin Pie", "Consumable", "Rare", 100, "RES_GOLD", pCon + "pumpkin_pie.png"));
            items.Add(CreateItem("CON_COOKIE", "Cookie", "Consumable", "Common", 15, "RES_GOLD", pCon + "cookie.png"));

            // Potions
            items.Add(CreateItem("CON_POTION_HEAL", "Healing Potion", "Consumable", "Rare", 100, "RES_GOLD", pCon + "potion_healing.png")); // Nếu file là potion.png thì sửa lại
            items.Add(CreateItem("CON_POTION_SPEED", "Speed Potion", "Consumable", "Rare", 100, "RES_GOLD", pCon + "potion_swiftness.png"));
            items.Add(CreateItem("CON_POTION_FIRE", "Fire Resistance", "Consumable", "Rare", 150, "RES_GOLD", pCon + "potion_fire_resistance.png"));
            items.Add(CreateItem("CON_POTION_STRENGTH", "Strength Potion", "Consumable", "Epic", 200, "RES_GOLD", pCon + "potion_strength.png"));

            // ==================================================================================
            // 5. RESOURCES (Nguyên liệu) - Folder: /images/resources/
            // ==================================================================================
            string pRes = "/images/resources/";

            // Ores & Ingots
            items.Add(CreateItem("MAT_COAL", "Coal", "Material", "Common", 5, "RES_GOLD", pRes + "coal.png"));
            items.Add(CreateItem("MAT_CHARCOAL", "Charcoal", "Material", "Common", 5, "RES_GOLD", pRes + "charcoal.png"));
            items.Add(CreateItem("MAT_IRON_INGOT", "Iron Ingot", "Material", "Uncommon", 20, "RES_GOLD", pRes + "iron_ingot.png"));
            items.Add(CreateItem("MAT_IRON_NUGGET", "Iron Nugget", "Material", "Common", 2, "RES_GOLD", pRes + "iron_nugget.png"));
            items.Add(CreateItem("MAT_GOLD_INGOT", "Gold Ingot", "Material", "Rare", 50, "RES_GOLD", pRes + "gold_ingot.png"));
            items.Add(CreateItem("MAT_GOLD_NUGGET", "Gold Nugget", "Material", "Common", 5, "RES_GOLD", pRes + "gold_nugget.png"));
            items.Add(CreateItem("MAT_COPPER_INGOT", "Copper Ingot", "Material", "Common", 15, "RES_GOLD", pRes + "copper_ingot.png"));
            items.Add(CreateItem("MAT_DIAMOND", "Diamond", "Material", "Epic", 150, "RES_GEM", pRes + "diamond.png"));
            items.Add(CreateItem("MAT_EMERALD", "Emerald", "Material", "Epic", 200, "RES_GEM", pRes + "emerald.png"));
            items.Add(CreateItem("MAT_LAPIS", "Lapis Lazuli", "Material", "Rare", 30, "RES_GOLD", pRes + "lapis_lazuli.png"));
            items.Add(CreateItem("MAT_REDSTONE", "Redstone Dust", "Material", "Uncommon", 10, "RES_GOLD", pRes + "redstone_dust.png"));
            items.Add(CreateItem("MAT_QUARTZ", "Nether Quartz", "Material", "Uncommon", 15, "RES_GOLD", pRes + "quartz.png"));
            items.Add(CreateItem("MAT_AMETHYST", "Amethyst Shard", "Material", "Rare", 50, "RES_GOLD", pRes + "amethyst_shard.png"));
            items.Add(CreateItem("MAT_NETHERITE", "Netherite Ingot", "Material", "Legendary", 500, "RES_GEM", pRes + "netherite_ingot.png"));
            items.Add(CreateItem("MAT_NETHERITE_SCRAP", "Netherite Scrap", "Material", "Epic", 150, "RES_GEM", pRes + "netherite_scrap.png"));

            // Basics
            items.Add(CreateItem("MAT_STICK", "Stick", "Material", "Common", 1, "RES_GOLD", pRes + "stick.png"));
            items.Add(CreateItem("MAT_FLINT", "Flint", "Material", "Common", 5, "RES_GOLD", pRes + "flint.png"));
            items.Add(CreateItem("MAT_FEATHER", "Feather", "Material", "Common", 2, "RES_GOLD", pRes + "feather.png"));
            items.Add(CreateItem("MAT_LEATHER", "Leather", "Material", "Common", 10, "RES_GOLD", pRes + "leather.png"));
            items.Add(CreateItem("MAT_STRING", "String", "Material", "Common", 5, "RES_GOLD", pRes + "string.png"));
            items.Add(CreateItem("MAT_BONE", "Bone", "Material", "Common", 5, "RES_GOLD", pRes + "bone.png"));
            items.Add(CreateItem("MAT_GUNPOWDER", "Gunpowder", "Material", "Uncommon", 20, "RES_GOLD", pRes + "gunpowder.png"));
            items.Add(CreateItem("MAT_PAPER", "Paper", "Material", "Common", 5, "RES_GOLD", pRes + "paper.png"));
            items.Add(CreateItem("MAT_BOOK", "Book", "Material", "Uncommon", 30, "RES_GOLD", pRes + "book.png"));

            // Drops
            items.Add(CreateItem("MAT_ROTTEN_FLESH", "Rotten Flesh", "Material", "Common", 1, "RES_GOLD", pRes + "rotten_flesh.png"));
            items.Add(CreateItem("MAT_ENDER_PEARL", "Ender Pearl", "Material", "Rare", 50, "RES_GEM", pRes + "ender_pearl.png"));
            items.Add(CreateItem("MAT_BLAZE_ROD", "Blaze Rod", "Material", "Rare", 60, "RES_GEM", pRes + "blaze_rod.png"));
            items.Add(CreateItem("MAT_GHAST_TEAR", "Ghast Tear", "Material", "Epic", 100, "RES_GEM", pRes + "ghast_tear.png"));
            items.Add(CreateItem("MAT_SLIME_BALL", "Slimeball", "Material", "Uncommon", 25, "RES_GOLD", pRes + "slime_ball.png"));
            items.Add(CreateItem("MAT_MAGMA_CREAM", "Magma Cream", "Material", "Rare", 40, "RES_GOLD", pRes + "magma_cream.png"));

            // Blocks (đại diện)
            items.Add(CreateItem("BLK_LOG_OAK", "Oak Log", "Block", "Common", 5, "RES_GOLD", pRes + "oak_log.png"));
            items.Add(CreateItem("BLK_COBBLESTONE", "Cobblestone", "Block", "Common", 2, "RES_GOLD", pRes + "cobblestone.png"));
            items.Add(CreateItem("BLK_OBSIDIAN", "Obsidian", "Block", "Rare", 100, "RES_GOLD", pRes + "obsidian.png"));

            // ==================================================================================
            // 6. VEHICLES (Phương tiện) - Folder: /images/vehicles/
            // ==================================================================================
            string pVeh = "/images/vehicles/";
            items.Add(CreateItem("VEH_MINECART", "Minecart", "Vehicle", "Uncommon", 200, "RES_GOLD", pVeh + "minecart.png"));
            items.Add(CreateItem("VEH_BOAT_OAK", "Oak Boat", "Vehicle", "Common", 100, "RES_GOLD", pVeh + "oak_boat.png"));
            items.Add(CreateItem("VEH_SADDLE", "Saddle", "Equipment", "Rare", 500, "RES_GOLD", pVeh + "saddle.png"));
            items.Add(CreateItem("VEH_ELYTRA", "Elytra", "Equipment", "Legendary", 5000, "RES_GEM", pVeh + "elytra.png"));
            items.Add(CreateItem("VEH_HORSE_ARMOR_IRON", "Iron Horse Armor", "Equipment", "Uncommon", 300, "RES_GOLD", pVeh + "iron_horse_armor.png"));
            items.Add(CreateItem("VEH_HORSE_ARMOR_GOLD", "Golden Horse Armor", "Equipment", "Rare", 500, "RES_GOLD", pVeh + "golden_horse_armor.png"));
            items.Add(CreateItem("VEH_HORSE_ARMOR_DIAMOND", "Diamond Horse Armor", "Equipment", "Epic", 1000, "RES_GEM", pVeh + "diamond_horse_armor.png"));

            // ==================================================================================
            // 7. BUNDLES (Gói quà) - Folder: /images/bundles/
            // ==================================================================================
            string pBun = "/images/bundles/";
            items.Add(CreateItem("BUN_STARTER", "Starter Bundle", "Bundle", "Common", 500, "RES_GOLD", pBun + "bundle_blue.png"));
            items.Add(CreateItem("BUN_ADVANCED", "Advanced Bundle", "Bundle", "Rare", 2000, "RES_GOLD", pBun + "bundle_red.png"));
            items.Add(CreateItem("BUN_MYSTERY", "Mystery Sack", "Bundle", "Epic", 100, "RES_GEM", pBun + "bundle_light.png"));

            // ==================================================================================
            // 8. BUILDINGS (Blueprints) - Folder: /images/buildings/
            // ==================================================================================
            string pBld = "/images/buildings/";
            items.Add(CreateItem("BP_HOUSE_SMALL", "Small House BP", "Blueprint", "Common", 1000, "RES_GOLD", pBld + "house_small.png"));
            items.Add(CreateItem("BP_HOUSE_LARGE", "Large House BP", "Blueprint", "Rare", 5000, "RES_GOLD", pBld + "house_large.png"));
            items.Add(CreateItem("BP_CASTLE", "Castle BP", "Blueprint", "Legendary", 10000, "RES_GEM", pBld + "castle.png"));
            items.Add(CreateItem("BP_TOWER", "Watchtower BP", "Blueprint", "Uncommon", 2500, "RES_GOLD", pBld + "tower.png"));

            context.ShopItems.AddRange(items);
            Console.WriteLine($"> Đã thêm {items.Count} items vào Shop.");
        }

        private static void SeedMonsters(ApplicationDbContext context)
        {
            var monsters = new List<Monster>();
            string pMob = "/images/mobs/";

            // Passive
            monsters.Add(new Monster { Name = "Pig", HP = 10, Damage = 0, ExpReward = 2, GoldReward = 5, ImageUrl = pMob + "pig.png" });
            monsters.Add(new Monster { Name = "Cow", HP = 10, Damage = 0, ExpReward = 2, GoldReward = 5, ImageUrl = pMob + "cow.png" });
            monsters.Add(new Monster { Name = "Sheep", HP = 8, Damage = 0, ExpReward = 2, GoldReward = 5, ImageUrl = pMob + "sheep.png" });
            monsters.Add(new Monster { Name = "Chicken", HP = 4, Damage = 0, ExpReward = 2, GoldReward = 2, ImageUrl = pMob + "chicken.png" });
            monsters.Add(new Monster { Name = "Villager", HP = 20, Damage = 0, ExpReward = 0, GoldReward = 10, ImageUrl = pMob + "villager.png" });

            // Hostile (Overworld)
            monsters.Add(new Monster { Name = "Zombie", HP = 20, Damage = 3, ExpReward = 5, GoldReward = 10, ImageUrl = pMob + "zombie.png" });
            monsters.Add(new Monster { Name = "Skeleton", HP = 20, Damage = 4, ExpReward = 5, GoldReward = 15, ImageUrl = pMob + "skeleton.png" });
            monsters.Add(new Monster { Name = "Spider", HP = 16, Damage = 2, ExpReward = 5, GoldReward = 8, ImageUrl = pMob + "spider.png" });
            monsters.Add(new Monster { Name = "Creeper", HP = 20, Damage = 10, ExpReward = 5, GoldReward = 20, ImageUrl = pMob + "creeper.png" });
            monsters.Add(new Monster { Name = "Enderman", HP = 40, Damage = 7, ExpReward = 10, GoldReward = 50, ImageUrl = pMob + "enderman.png" });
            monsters.Add(new Monster { Name = "Witch", HP = 26, Damage = 6, ExpReward = 10, GoldReward = 30, ImageUrl = pMob + "witch.png" });
            monsters.Add(new Monster { Name = "Slime", HP = 16, Damage = 2, ExpReward = 3, GoldReward = 10, ImageUrl = pMob + "slime.png" });

            // Nether
            monsters.Add(new Monster { Name = "Zombified Piglin", HP = 20, Damage = 5, ExpReward = 5, GoldReward = 15, ImageUrl = pMob + "zombified_piglin.png" });
            monsters.Add(new Monster { Name = "Blaze", HP = 20, Damage = 5, ExpReward = 10, GoldReward = 40, ImageUrl = pMob + "blaze.png" });
            monsters.Add(new Monster { Name = "Ghast", HP = 10, Damage = 10, ExpReward = 15, GoldReward = 50, ImageUrl = pMob + "ghast.png" });
            monsters.Add(new Monster { Name = "Wither Skeleton", HP = 20, Damage = 8, ExpReward = 10, GoldReward = 30, ImageUrl = pMob + "wither_skeleton.png" });

            // Bosses
            monsters.Add(new Monster { Name = "Wither", HP = 300, Damage = 20, ExpReward = 1000, GoldReward = 2000, ImageUrl = pMob + "wither.png" });
            monsters.Add(new Monster { Name = "Ender Dragon", HP = 200, Damage = 15, ExpReward = 5000, GoldReward = 10000, ImageUrl = pMob + "ender_dragon.png" });

            context.Monsters.AddRange(monsters);
            Console.WriteLine($"> Đã thêm {monsters.Count} quái vật.");
        }

        private static void SeedRecipes(ApplicationDbContext context)
        {
            var recipes = new List<Recipe>();
            string pWep = "/images/weapons/";
            string pTool = "/images/tools/";
            string pArm = "/images/armor/";

            // Weapons
            recipes.Add(CreateRecipe("R_SWORD_WOOD", "WEP_WOOD_SWORD", "Wooden Sword", pWep + "wooden_sword.png", 5, "MAT_LOG_OAK:1,MAT_STICK:1"));
            recipes.Add(CreateRecipe("R_SWORD_STONE", "WEP_STONE_SWORD", "Stone Sword", pWep + "stone_sword.png", 10, "MAT_COBBLESTONE:2,MAT_STICK:1"));
            recipes.Add(CreateRecipe("R_SWORD_IRON", "WEP_IRON_SWORD", "Iron Sword", pWep + "iron_sword.png", 20, "MAT_IRON_INGOT:2,MAT_STICK:1"));
            recipes.Add(CreateRecipe("R_SWORD_DIAMOND", "WEP_DIAMOND_SWORD", "Diamond Sword", pWep + "diamond_sword.png", 60, "MAT_DIAMOND:2,MAT_STICK:1"));

            // Armor (Iron)
            recipes.Add(CreateRecipe("R_HELMET_IRON", "ARM_IRON_HELMET", "Iron Helmet", pArm + "iron_helmet.png", 30, "MAT_IRON_INGOT:5"));
            recipes.Add(CreateRecipe("R_CHEST_IRON", "ARM_IRON_CHEST", "Iron Chestplate", pArm + "iron_chestplate.png", 60, "MAT_IRON_INGOT:8"));

            // Tools
            recipes.Add(CreateRecipe("R_PICK_IRON", "TOOL_IRON_PICK", "Iron Pickaxe", pTool + "iron_pickaxe.png", 25, "MAT_IRON_INGOT:3,MAT_STICK:2"));
            recipes.Add(CreateRecipe("R_AXE_IRON", "TOOL_IRON_AXE", "Iron Axe", pTool + "iron_axe.png", 25, "MAT_IRON_INGOT:3,MAT_STICK:2"));

            context.Recipes.AddRange(recipes);
            Console.WriteLine($"> Đã thêm {recipes.Count} công thức.");
        }

        // --- Helper Methods ---

        private static ShopItem CreateItem(string id, string name, string type, string rarity, int price, string currency, string imgUrl)
        {
            return new ShopItem
            {
                ProductID = id,
                TargetItemID = id,
                Name = name,
                Description = $"A {rarity.ToLower()} {name} ({type}).",
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