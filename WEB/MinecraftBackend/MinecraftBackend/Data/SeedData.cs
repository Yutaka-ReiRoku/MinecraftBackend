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

                Console.WriteLine(">>> STARTING INTELLIGENT SEED DATA...");

                // 1. SHOP ITEMS (Registry tổng: Chứa cả đồ bán và dữ liệu ẩn)
                if (!context.ShopItems.Any()) 
                {
                    SeedShopItems(context);
                }

                // 2. MONSTERS (Bảng chỉ số quái vật riêng biệt để tính dame/hp)
                if (!context.Monsters.Any()) 
                {
                    SeedMonsters(context);
                }

                // 3. RECIPES (Công thức chế tạo)
                if (!context.Recipes.Any()) 
                {
                    SeedRecipes(context);
                }

                context.SaveChanges();
                Console.WriteLine(">>> DONE! ALL ASSETS MAPPED WITH CORRECT VISIBILITY FLAGS.");
            }
        }

        private static void SeedShopItems(ApplicationDbContext context)
        {
            var items = new List<ShopItem>();

            // =============================================================
            // GROUP A: MERCHANDISE (Hàng hóa - IsShow = TRUE)
            // =============================================================

            // --- 1. ARMOR ---
            string pArm = "/images/armor/";
            items.Add(CreateItem("ARM_IRON_HELMET", "Iron Helmet", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_helmet.png", true));
            items.Add(CreateItem("ARM_IRON_CHEST", "Iron Chestplate", "Armor", "Uncommon", 400, "RES_GOLD", pArm + "iron_chestplate.png", true));
            items.Add(CreateItem("ARM_IRON_LEGS", "Iron Leggings", "Armor", "Uncommon", 300, "RES_GOLD", pArm + "iron_leggings.png", true));
            items.Add(CreateItem("ARM_IRON_BOOTS", "Iron Boots", "Armor", "Uncommon", 150, "RES_GOLD", pArm + "iron_boots.png", true));

            // --- 2. WEAPONS ---
            string pWeap = "/images/weapons/";
            items.Add(CreateItem("WEAP_WOOD_SWORD", "Wooden Sword", "Weapon", "Common", 50, "RES_GOLD", pWeap + "wooden_sword.png", true));
            items.Add(CreateItem("WEAP_STONE_SWORD", "Stone Sword", "Weapon", "Common", 100, "RES_GOLD", pWeap + "stone_sword.png", true));
            items.Add(CreateItem("WEAP_IRON_SWORD", "Iron Sword", "Weapon", "Uncommon", 250, "RES_GOLD", pWeap + "iron_sword.png", true));
            items.Add(CreateItem("WEAP_GOLD_SWORD", "Gold Sword", "Weapon", "Rare", 400, "RES_GOLD", pWeap + "gold_sword.png", true));
            items.Add(CreateItem("WEAP_DIAMOND_SWORD", "Diamond Sword", "Weapon", "Epic", 1000, "RES_GEM", pWeap + "diamond_sword.png", true));
            items.Add(CreateItem("WEAP_NETHER_SWORD", "Netherite Sword", "Weapon", "Legendary", 2500, "RES_GEM", pWeap + "netherite_sword.png", true));

            // --- 3. TOOLS ---
            string pTool = "/images/tools/";
            items.Add(CreateItem("TOOL_AXE", "Stone Axe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_axe.png", true));
            items.Add(CreateItem("TOOL_HOE", "Stone Hoe", "Tool", "Common", 60, "RES_GOLD", pTool + "stone_hoe.png", true));
            items.Add(CreateItem("TOOL_PICK", "Stone Pickaxe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_pickaxe.png", true));
            items.Add(CreateItem("TOOL_SHOVEL", "Stone Shovel", "Tool", "Common", 60, "RES_GOLD", pTool + "stone_shovel.png", true));

            // --- 4. CONSUMABLES ---
            string pCon = "/images/consumables/";
            items.Add(CreateItem("CON_APPLE", "Apple", "Consumable", "Common", 10, "RES_GOLD", pCon + "apple.png", true));
            items.Add(CreateItem("CON_BREAD", "Bread", "Consumable", "Common", 15, "RES_GOLD", pCon + "bread.png", true));
            items.Add(CreateItem("CON_CAKE", "Cake", "Consumable", "Rare", 100, "RES_GOLD", pCon + "cake.png", true));
            items.Add(CreateItem("CON_CARROT", "Carrot", "Consumable", "Common", 10, "RES_GOLD", pCon + "carrot.png", true));
            items.Add(CreateItem("CON_COOKIE", "Cookie", "Consumable", "Common", 5, "RES_GOLD", pCon + "cookie.png", true));
            items.Add(CreateItem("CON_GOLD_APPLE", "Golden Apple", "Consumable", "Epic", 200, "RES_GEM", pCon + "golden_apple.png", true));
            items.Add(CreateItem("CON_MELON", "Melon", "Consumable", "Common", 10, "RES_GOLD", pCon + "melon.png", true));
            items.Add(CreateItem("CON_PORK", "Raw Porkchop", "Consumable", "Common", 20, "RES_GOLD", pCon + "porkchop.png", true));
            items.Add(CreateItem("CON_POTATO", "Potato", "Consumable", "Common", 10, "RES_GOLD", pCon + "potato.png", true));
            items.Add(CreateItem("CON_POTION", "Mysterious Potion", "Consumable", "Rare", 150, "RES_GOLD", pCon + "potion.png", true));
            items.Add(CreateItem("CON_PIE", "Pumpkin Pie", "Consumable", "Uncommon", 60, "RES_GOLD", pCon + "pumpkin_pie.png", true));
            items.Add(CreateItem("CON_STEAK", "Steak", "Consumable", "Uncommon", 50, "RES_GOLD", pCon + "steak.png", true));

            // --- 5. RESOURCES ---
            string pRes = "/images/resources/";
            items.Add(CreateItem("MAT_AMETHYST", "Amethyst Shard", "Material", "Rare", 80, "RES_GOLD", pRes + "amethyst_shard.png", true));
            items.Add(CreateItem("MAT_COAL", "Coal", "Material", "Common", 10, "RES_GOLD", pRes + "coal.png", true));
            items.Add(CreateItem("MAT_COBBLE", "Cobblestone", "Material", "Common", 2, "RES_GOLD", pRes + "cobblestone.png", true));
            items.Add(CreateItem("MAT_COPPER", "Copper Ingot", "Material", "Common", 20, "RES_GOLD", pRes + "copper_ingot.png", true));
            items.Add(CreateItem("MAT_DIAMOND", "Diamond", "Material", "Epic", 200, "RES_GEM", pRes + "diamond.png", true));
            items.Add(CreateItem("MAT_EMERALD", "Emerald", "Material", "Epic", 250, "RES_GEM", pRes + "emerald.png", true));
            items.Add(CreateItem("MAT_GOLD", "Gold Ingot", "Material", "Rare", 60, "RES_GOLD", pRes + "gold_ingot.png", true));
            items.Add(CreateItem("MAT_IRON", "Iron Ingot", "Material", "Uncommon", 30, "RES_GOLD", pRes + "iron_ingot.png", true));
            items.Add(CreateItem("MAT_LAPIS", "Lapis Lazuli", "Material", "Rare", 40, "RES_GOLD", pRes + "lapis_lazuli.png", true));
            items.Add(CreateItem("MAT_SCRAP", "Netherite Scrap", "Material", "Legendary", 500, "RES_GEM", pRes + "netherite_scrap.png", true));
            items.Add(CreateItem("MAT_LOG", "Oak Log", "Material", "Common", 5, "RES_GOLD", pRes + "oak_log.png", true));
            items.Add(CreateItem("MAT_OBSIDIAN", "Obsidian", "Material", "Rare", 100, "RES_GOLD", pRes + "obsidian.png", true));
            items.Add(CreateItem("MAT_QUARTZ", "Quartz", "Material", "Uncommon", 25, "RES_GOLD", pRes + "quartz.png", true));
            items.Add(CreateItem("MAT_REDSTONE", "Redstone Dust", "Material", "Uncommon", 15, "RES_GOLD", pRes + "redstone_dust.png", true));
            items.Add(CreateItem("MAT_SLIME", "Slime Ball", "Material", "Uncommon", 30, "RES_GOLD", pRes + "slime_ball.png", true));

            // --- 6. VEHICLES ---
            string pVeh = "/images/vehicles/";
            items.Add(CreateItem("VEH_BOAT", "Boat", "Vehicle", "Common", 100, "RES_GOLD", pVeh + "boat.png", true));
            items.Add(CreateItem("VEH_DONKEY", "Donkey", "Mount", "Uncommon", 500, "RES_GOLD", pVeh + "donkey.png", true));
            items.Add(CreateItem("VEH_ELYTRA", "Elytra", "Equipment", "Legendary", 5000, "RES_GEM", pVeh + "elytra.png", true));
            items.Add(CreateItem("VEH_HORSE", "Horse", "Mount", "Rare", 1000, "RES_GOLD", pVeh + "horse.png", true));
            items.Add(CreateItem("VEH_LLAMA", "Llama", "Mount", "Uncommon", 800, "RES_GOLD", pVeh + "llama.png", true));
            items.Add(CreateItem("VEH_MINECART", "Minecart", "Vehicle", "Uncommon", 200, "RES_GOLD", pVeh + "minecart.png", true));
            items.Add(CreateItem("VEH_MULE", "Mule", "Mount", "Uncommon", 600, "RES_GOLD", pVeh + "mule.png", true));
            items.Add(CreateItem("VEH_PIG", "Saddled Pig", "Mount", "Rare", 1200, "RES_GOLD", pVeh + "pig.png", true));
            items.Add(CreateItem("VEH_RAFT", "Bamboo Raft", "Vehicle", "Common", 120, "RES_GOLD", pVeh + "raft.png", true));
            items.Add(CreateItem("VEH_STRIDER", "Strider", "Mount", "Epic", 3000, "RES_GEM", pVeh + "strider.png", true));

            // --- 7. BUNDLES ---
            string pBun = "/images/bundles/";
            items.Add(CreateItem("BUN_COST", "Economy Bundle", "Bundle", "Common", 500, "RES_GOLD", pBun + "bundle_cost.png", true));
            items.Add(CreateItem("BUN_LOOT", "Loot Sack", "Bundle", "Rare", 100, "RES_GEM", pBun + "bundle_loot.png", true));
            items.Add(CreateItem("BUN_PROD", "Product Crate", "Bundle", "Uncommon", 1000, "RES_GOLD", pBun + "bundle_product.png", true));

            // --- 8. AVATARS (Skins - Thường là bán, nhưng nếu bạn muốn ẩn thì sửa thành false) ---
            string pAva = "/images/avatars/";
            items.Add(CreateItem("SKIN_CREEPER", "Creeper Skin", "Cosmetic", "Epic", 500, "RES_GEM", pAva + "creeper.png", true));
            items.Add(CreateItem("SKIN_STEVE", "Steve Skin", "Cosmetic", "Common", 0, "RES_GOLD", pAva + "steve.png", true));
            items.Add(CreateItem("SKIN_ZOMBIE", "Zombie Skin", "Cosmetic", "Rare", 300, "RES_GEM", pAva + "zombie.png", true));
            items.Add(CreateItem("SKIN_DEFAULT", "Default Skin", "Cosmetic", "Common", 0, "RES_GOLD", pAva + "default.png", true));

            // =============================================================
            // GROUP B: GAME DATA (Dữ liệu nội bộ - IsShow = FALSE)
            // =============================================================

            // --- 9. BUILDINGS (Không bán, dùng để hiển thị hoặc làm sự kiện) ---
            string pBuild = "/images/buildings/";
            items.Add(CreateItem("DATA_STRUCT_01", "Structure 01", "Building", "Common", 0, "NONE", pBuild + "structure_01.png", false));
            items.Add(CreateItem("DATA_STRUCT_02", "Structure 02", "Building", "Common", 0, "NONE", pBuild + "structure_02.png", false));
            items.Add(CreateItem("DATA_STRUCT_03", "Structure 03", "Building", "Common", 0, "NONE", pBuild + "structure_03.png", false));

            // --- 10. MODES (Icons cho Game Mode, không phải vật phẩm) ---
            string pMode = "/images/modes/";
            items.Add(CreateItem("MODE_ADV", "Adventure", "GameMode", "Common", 0, "NONE", pMode + "adventure.png", false));
            items.Add(CreateItem("MODE_CREATIVE", "Creative", "GameMode", "Common", 0, "NONE", pMode + "creative.png", false));
            items.Add(CreateItem("MODE_HARD", "Hardcore", "GameMode", "Common", 0, "NONE", pMode + "hardcore.png", false));
            items.Add(CreateItem("MODE_SPEC", "Spectator", "GameMode", "Common", 0, "NONE", pMode + "spectator.png", false));
            items.Add(CreateItem("MODE_SURV", "Survival", "GameMode", "Common", 0, "NONE", pMode + "survival.png", false));

            // --- 11. MOBS (Dữ liệu hình ảnh quái vật trong kho, không bán) ---
            string pMob = "/images/mobs/";
            items.Add(CreateItem("DATA_MOB_BLAZE", "Blaze Data", "MobInfo", "Common", 0, "NONE", pMob + "blaze.png", false));
            items.Add(CreateItem("DATA_MOB_CREEPER", "Creeper Data", "MobInfo", "Common", 0, "NONE", pMob + "creeper.png", false));
            items.Add(CreateItem("DATA_MOB_DRAGON", "Dragon Data", "MobInfo", "Legendary", 0, "NONE", pMob + "dragon.png", false));
            items.Add(CreateItem("DATA_MOB_ENDERMAN", "Enderman Data", "MobInfo", "Rare", 0, "NONE", pMob + "enderman.png", false));
            items.Add(CreateItem("DATA_MOB_GHAST", "Ghast Data", "MobInfo", "Rare", 0, "NONE", pMob + "ghast.png", false));
            items.Add(CreateItem("DATA_MOB_SKELETON", "Skeleton Data", "MobInfo", "Common", 0, "NONE", pMob + "skeleton.png", false));
            items.Add(CreateItem("DATA_MOB_SLIME", "Slime Data", "MobInfo", "Common", 0, "NONE", pMob + "slime.png", false));
            items.Add(CreateItem("DATA_MOB_SPIDER", "Spider Data", "MobInfo", "Common", 0, "NONE", pMob + "spider.png", false));
            items.Add(CreateItem("DATA_MOB_WITCH", "Witch Data", "MobInfo", "Uncommon", 0, "NONE", pMob + "witch.png", false));
            items.Add(CreateItem("DATA_MOB_ZOMBIE", "Zombie Data", "MobInfo", "Common", 0, "NONE", pMob + "zombie.png", false));

            // --- 12. OTHERS (Icons UI/System - IsShow = FALSE) ---
            string pOther = "/images/others/";
            items.Add(CreateItem("SYS_EXP", "Experience Orb", "System", "Common", 0, "NONE", pOther + "exp.png", false));
            items.Add(CreateItem("SYS_QUEST", "Quest Icon", "System", "Common", 0, "NONE", pOther + "quest.png", false));
            // Lưu ý: Các file UUID trùng lặp trong folder này ta bỏ qua để tránh rác DB

            context.ShopItems.AddRange(items);
            Console.WriteLine($"> Seeded {items.Count} entries to ShopItems (Hidden & Visible).");
        }

        private static void SeedMonsters(ApplicationDbContext context)
        {
            var monsters = new List<Monster>();
            string pMob = "/images/mobs/";

            monsters.Add(new Monster { Name = "Blaze", HP = 50, Damage = 10, ExpReward = 20, GoldReward = 30, ImageUrl = pMob + "blaze.png" });
            monsters.Add(new Monster { Name = "Creeper", HP = 40, Damage = 40, ExpReward = 15, GoldReward = 20, ImageUrl = pMob + "creeper.png" });
            monsters.Add(new Monster { Name = "Ender Dragon", HP = 500, Damage = 50, ExpReward = 1000, GoldReward = 5000, ImageUrl = pMob + "dragon.png" });
            monsters.Add(new Monster { Name = "Enderman", HP = 80, Damage = 15, ExpReward = 30, GoldReward = 50, ImageUrl = pMob + "enderman.png" });
            monsters.Add(new Monster { Name = "Ghast", HP = 30, Damage = 20, ExpReward = 40, GoldReward = 40, ImageUrl = pMob + "ghast.png" });
            monsters.Add(new Monster { Name = "Skeleton", HP = 40, Damage = 8, ExpReward = 10, GoldReward = 15, ImageUrl = pMob + "skeleton.png" });
            monsters.Add(new Monster { Name = "Slime", HP = 20, Damage = 5, ExpReward = 5, GoldReward = 10, ImageUrl = pMob + "slime.png" });
            monsters.Add(new Monster { Name = "Spider", HP = 30, Damage = 6, ExpReward = 8, GoldReward = 10, ImageUrl = pMob + "spider.png" });
            monsters.Add(new Monster { Name = "Witch", HP = 50, Damage = 10, ExpReward = 25, GoldReward = 30, ImageUrl = pMob + "witch.png" });
            monsters.Add(new Monster { Name = "Zombie", HP = 40, Damage = 5, ExpReward = 5, GoldReward = 5, ImageUrl = pMob + "zombie.png" });

            context.Monsters.AddRange(monsters);
            Console.WriteLine($"> Seeded {monsters.Count} monsters.");
        }

        private static void SeedRecipes(ApplicationDbContext context)
        {
            var recipes = new List<Recipe>();
            string pTool = "/images/tools/";
            string pWeap = "/images/weapons/";
            string pVeh = "/images/vehicles/";
            string pCon = "/images/consumables/";

            recipes.Add(CreateRecipe("R_PICK_STONE", "TOOL_PICK", "Stone Pickaxe", pTool + "stone_pickaxe.png", 10, "MAT_COBBLE:3,MAT_LOG:2"));
            recipes.Add(CreateRecipe("R_AXE_STONE", "TOOL_AXE", "Stone Axe", pTool + "stone_axe.png", 10, "MAT_COBBLE:3,MAT_LOG:2"));
            recipes.Add(CreateRecipe("R_SHOVEL_STONE", "TOOL_SHOVEL", "Stone Shovel", pTool + "stone_shovel.png", 8, "MAT_COBBLE:1,MAT_LOG:2"));
            recipes.Add(CreateRecipe("R_HOE_STONE", "TOOL_HOE", "Stone Hoe", pTool + "stone_hoe.png", 8, "MAT_COBBLE:2,MAT_LOG:2"));

            recipes.Add(CreateRecipe("R_SWORD_WOOD", "WEAP_WOOD_SWORD", "Wooden Sword", pWeap + "wooden_sword.png", 5, "MAT_LOG:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_STONE", "WEAP_STONE_SWORD", "Stone Sword", pWeap + "stone_sword.png", 10, "MAT_COBBLE:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_IRON", "WEAP_IRON_SWORD", "Iron Sword", pWeap + "iron_sword.png", 20, "MAT_IRON:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_GOLD", "WEAP_GOLD_SWORD", "Gold Sword", pWeap + "gold_sword.png", 15, "MAT_GOLD:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_DIAMOND", "WEAP_DIAMOND_SWORD", "Diamond Sword", pWeap + "diamond_sword.png", 60, "MAT_DIAMOND:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_NETHER", "WEAP_NETHER_SWORD", "Netherite Sword", pWeap + "netherite_sword.png", 120, "WEAP_DIAMOND_SWORD:1,MAT_SCRAP:1"));

            recipes.Add(CreateRecipe("R_BOAT", "VEH_BOAT", "Boat", pVeh + "boat.png", 15, "MAT_LOG:5"));
            recipes.Add(CreateRecipe("R_MINECART", "VEH_MINECART", "Minecart", pVeh + "minecart.png", 30, "MAT_IRON:5"));

            recipes.Add(CreateRecipe("R_GOLD_APPLE", "CON_GOLD_APPLE", "Golden Apple", pCon + "golden_apple.png", 60, "MAT_GOLD:8,CON_APPLE:1"));
            recipes.Add(CreateRecipe("R_BREAD", "CON_BREAD", "Bread", pCon + "bread.png", 5, "MAT_WHEAT:3"));

            context.Recipes.AddRange(recipes);
            Console.WriteLine($"> Seeded {recipes.Count} recipes.");
        }

        // --- HELPER METHODS ---

        private static ShopItem CreateItem(string id, string name, string type, string rarity, int price, string currency, string imgUrl, bool isShow)
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
                IsShow = isShow // Quan trọng: Quyết định hiển thị hay ẩn
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