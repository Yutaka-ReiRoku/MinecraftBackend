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
                // Tự động tạo DB nếu chưa có
                context.Database.EnsureCreated();

                Console.WriteLine(">>> STARTING SEED DATA WITH FULL ASSETS...");

                // 1. SHOP ITEMS (Quét toàn bộ ảnh item/block/icon để bán)
                if (!context.ShopItems.Any()) 
                {
                    SeedShopItems(context);
                }

                // 2. MONSTERS (Quét folder mobs)
                if (!context.Monsters.Any()) 
                {
                    SeedMonsters(context);
                }

                // 3. RECIPES (Công thức ghép đồ)
                if (!context.Recipes.Any()) 
                {
                    SeedRecipes(context);
                }

                context.SaveChanges();
                Console.WriteLine(">>> SEED DATA COMPLETED! NO ASSET LEFT BEHIND.");
            }
        }

        private static void SeedShopItems(ApplicationDbContext context)
        {
            var items = new List<ShopItem>();

            // --- 1. ARMOR (/images/armor/) ---
            string pArm = "/images/armor/";
            items.Add(CreateItem("ARM_IRON_HELMET", "Iron Helmet", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_helmet.png"));
            items.Add(CreateItem("ARM_IRON_CHEST", "Iron Chestplate", "Armor", "Uncommon", 400, "RES_GOLD", pArm + "iron_chestplate.png"));
            items.Add(CreateItem("ARM_IRON_LEGS", "Iron Leggings", "Armor", "Uncommon", 300, "RES_GOLD", pArm + "iron_leggings.png"));
            items.Add(CreateItem("ARM_IRON_BOOTS", "Iron Boots", "Armor", "Uncommon", 150, "RES_GOLD", pArm + "iron_boots.png"));

            // --- 2. WEAPONS (/images/weapons/) [NEW SECTION] ---
            string pWeap = "/images/weapons/";
            items.Add(CreateItem("WEAP_WOOD_SWORD", "Wooden Sword", "Weapon", "Common", 50, "RES_GOLD", pWeap + "wooden_sword.png"));
            items.Add(CreateItem("WEAP_STONE_SWORD", "Stone Sword", "Weapon", "Common", 100, "RES_GOLD", pWeap + "stone_sword.png"));
            items.Add(CreateItem("WEAP_IRON_SWORD", "Iron Sword", "Weapon", "Uncommon", 250, "RES_GOLD", pWeap + "iron_sword.png"));
            items.Add(CreateItem("WEAP_GOLD_SWORD", "Gold Sword", "Weapon", "Rare", 400, "RES_GOLD", pWeap + "gold_sword.png"));
            items.Add(CreateItem("WEAP_DIAMOND_SWORD", "Diamond Sword", "Weapon", "Epic", 1000, "RES_GEM", pWeap + "diamond_sword.png"));
            items.Add(CreateItem("WEAP_NETHER_SWORD", "Netherite Sword", "Weapon", "Legendary", 2500, "RES_GEM", pWeap + "netherite_sword.png"));

            // --- 3. TOOLS (/images/tools/) ---
            string pTool = "/images/tools/";
            items.Add(CreateItem("TOOL_AXE", "Stone Axe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_axe.png"));
            items.Add(CreateItem("TOOL_HOE", "Stone Hoe", "Tool", "Common", 60, "RES_GOLD", pTool + "stone_hoe.png"));
            items.Add(CreateItem("TOOL_PICK", "Stone Pickaxe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_pickaxe.png"));
            items.Add(CreateItem("TOOL_SHOVEL", "Stone Shovel", "Tool", "Common", 60, "RES_GOLD", pTool + "stone_shovel.png"));

            // --- 4. CONSUMABLES (/images/consumables/) ---
            string pCon = "/images/consumables/";
            items.Add(CreateItem("CON_APPLE", "Apple", "Consumable", "Common", 10, "RES_GOLD", pCon + "apple.png"));
            items.Add(CreateItem("CON_BREAD", "Bread", "Consumable", "Common", 15, "RES_GOLD", pCon + "bread.png"));
            items.Add(CreateItem("CON_CAKE", "Cake", "Consumable", "Rare", 100, "RES_GOLD", pCon + "cake.png"));
            items.Add(CreateItem("CON_CARROT", "Carrot", "Consumable", "Common", 10, "RES_GOLD", pCon + "carrot.png"));
            items.Add(CreateItem("CON_COOKIE", "Cookie", "Consumable", "Common", 5, "RES_GOLD", pCon + "cookie.png"));
            items.Add(CreateItem("CON_GOLD_APPLE", "Golden Apple", "Consumable", "Epic", 200, "RES_GEM", pCon + "golden_apple.png"));
            items.Add(CreateItem("CON_MELON", "Melon", "Consumable", "Common", 10, "RES_GOLD", pCon + "melon.png"));
            items.Add(CreateItem("CON_PORK", "Raw Porkchop", "Consumable", "Common", 20, "RES_GOLD", pCon + "porkchop.png"));
            items.Add(CreateItem("CON_POTATO", "Potato", "Consumable", "Common", 10, "RES_GOLD", pCon + "potato.png"));
            items.Add(CreateItem("CON_POTION", "Mysterious Potion", "Consumable", "Rare", 150, "RES_GOLD", pCon + "potion.png"));
            items.Add(CreateItem("CON_PIE", "Pumpkin Pie", "Consumable", "Uncommon", 60, "RES_GOLD", pCon + "pumpkin_pie.png"));
            items.Add(CreateItem("CON_STEAK", "Steak", "Consumable", "Uncommon", 50, "RES_GOLD", pCon + "steak.png"));

            // --- 5. RESOURCES (/images/resources/) ---
            string pRes = "/images/resources/";
            items.Add(CreateItem("MAT_AMETHYST", "Amethyst Shard", "Material", "Rare", 80, "RES_GOLD", pRes + "amethyst_shard.png"));
            items.Add(CreateItem("MAT_COAL", "Coal", "Material", "Common", 10, "RES_GOLD", pRes + "coal.png"));
            items.Add(CreateItem("MAT_COBBLE", "Cobblestone", "Material", "Common", 2, "RES_GOLD", pRes + "cobblestone.png"));
            items.Add(CreateItem("MAT_COPPER", "Copper Ingot", "Material", "Common", 20, "RES_GOLD", pRes + "copper_ingot.png"));
            items.Add(CreateItem("MAT_DIAMOND", "Diamond", "Material", "Epic", 200, "RES_GEM", pRes + "diamond.png"));
            items.Add(CreateItem("MAT_EMERALD", "Emerald", "Material", "Epic", 250, "RES_GEM", pRes + "emerald.png"));
            items.Add(CreateItem("MAT_GOLD", "Gold Ingot", "Material", "Rare", 60, "RES_GOLD", pRes + "gold_ingot.png"));
            items.Add(CreateItem("MAT_IRON", "Iron Ingot", "Material", "Uncommon", 30, "RES_GOLD", pRes + "iron_ingot.png"));
            items.Add(CreateItem("MAT_LAPIS", "Lapis Lazuli", "Material", "Rare", 40, "RES_GOLD", pRes + "lapis_lazuli.png"));
            items.Add(CreateItem("MAT_SCRAP", "Netherite Scrap", "Material", "Legendary", 500, "RES_GEM", pRes + "netherite_scrap.png"));
            items.Add(CreateItem("MAT_LOG", "Oak Log", "Material", "Common", 5, "RES_GOLD", pRes + "oak_log.png"));
            items.Add(CreateItem("MAT_OBSIDIAN", "Obsidian", "Material", "Rare", 100, "RES_GOLD", pRes + "obsidian.png"));
            items.Add(CreateItem("MAT_QUARTZ", "Quartz", "Material", "Uncommon", 25, "RES_GOLD", pRes + "quartz.png"));
            items.Add(CreateItem("MAT_REDSTONE", "Redstone Dust", "Material", "Uncommon", 15, "RES_GOLD", pRes + "redstone_dust.png"));
            items.Add(CreateItem("MAT_SLIME", "Slime Ball", "Material", "Uncommon", 30, "RES_GOLD", pRes + "slime_ball.png"));

            // --- 6. VEHICLES (/images/vehicles/) ---
            string pVeh = "/images/vehicles/";
            items.Add(CreateItem("VEH_BOAT", "Boat", "Vehicle", "Common", 100, "RES_GOLD", pVeh + "boat.png"));
            items.Add(CreateItem("VEH_DONKEY", "Donkey", "Mount", "Uncommon", 500, "RES_GOLD", pVeh + "donkey.png"));
            items.Add(CreateItem("VEH_ELYTRA", "Elytra", "Equipment", "Legendary", 5000, "RES_GEM", pVeh + "elytra.png"));
            items.Add(CreateItem("VEH_HORSE", "Horse", "Mount", "Rare", 1000, "RES_GOLD", pVeh + "horse.png"));
            items.Add(CreateItem("VEH_LLAMA", "Llama", "Mount", "Uncommon", 800, "RES_GOLD", pVeh + "llama.png"));
            items.Add(CreateItem("VEH_MINECART", "Minecart", "Vehicle", "Uncommon", 200, "RES_GOLD", pVeh + "minecart.png"));
            items.Add(CreateItem("VEH_MULE", "Mule", "Mount", "Uncommon", 600, "RES_GOLD", pVeh + "mule.png"));
            items.Add(CreateItem("VEH_PIG", "Saddled Pig", "Mount", "Rare", 1200, "RES_GOLD", pVeh + "pig.png"));
            items.Add(CreateItem("VEH_RAFT", "Bamboo Raft", "Vehicle", "Common", 120, "RES_GOLD", pVeh + "raft.png"));
            items.Add(CreateItem("VEH_STRIDER", "Strider", "Mount", "Epic", 3000, "RES_GEM", pVeh + "strider.png"));

            // --- 7. BUILDINGS (/images/buildings/) ---
            string pBuild = "/images/buildings/";
            items.Add(CreateItem("BP_STRUCT_01", "Basic House", "Blueprint", "Common", 1000, "RES_GOLD", pBuild + "structure_01.png"));
            items.Add(CreateItem("BP_STRUCT_02", "Watch Tower", "Blueprint", "Rare", 2500, "RES_GOLD", pBuild + "structure_02.png"));
            items.Add(CreateItem("BP_STRUCT_03", "Grand Castle", "Blueprint", "Epic", 5000, "RES_GEM", pBuild + "structure_03.png"));

            // --- 8. BUNDLES (/images/bundles/) ---
            string pBun = "/images/bundles/";
            items.Add(CreateItem("BUN_COST", "Economy Bundle", "Bundle", "Common", 500, "RES_GOLD", pBun + "bundle_cost.png"));
            items.Add(CreateItem("BUN_LOOT", "Loot Sack", "Bundle", "Rare", 100, "RES_GEM", pBun + "bundle_loot.png"));
            items.Add(CreateItem("BUN_PROD", "Product Crate", "Bundle", "Uncommon", 1000, "RES_GOLD", pBun + "bundle_product.png"));

            // --- 9. AVATARS (/images/avatars/) [NEW SECTION - MAPPED AS COSMETICS] ---
            string pAva = "/images/avatars/";
            items.Add(CreateItem("SKIN_CREEPER", "Creeper Skin", "Cosmetic", "Epic", 500, "RES_GEM", pAva + "creeper.png"));
            items.Add(CreateItem("SKIN_STEVE", "Steve Skin", "Cosmetic", "Common", 0, "RES_GOLD", pAva + "steve.png"));
            items.Add(CreateItem("SKIN_ZOMBIE", "Zombie Skin", "Cosmetic", "Rare", 300, "RES_GEM", pAva + "zombie.png"));
            items.Add(CreateItem("SKIN_DEFAULT", "Default Skin", "Cosmetic", "Common", 0, "RES_GOLD", pAva + "default.png"));

            // --- 10. MODES (/images/modes/) [NEW SECTION - MAPPED AS TICKETS] ---
            string pMode = "/images/modes/";
            items.Add(CreateItem("TICKET_ADV", "Adventure Pass", "Ticket", "Common", 100, "RES_GOLD", pMode + "adventure.png"));
            items.Add(CreateItem("TICKET_CREATIVE", "Creative License", "Ticket", "Legendary", 10000, "RES_GEM", pMode + "creative.png"));
            items.Add(CreateItem("TICKET_HARD", "Hardcore Token", "Ticket", "Epic", 500, "RES_GEM", pMode + "hardcore.png"));
            items.Add(CreateItem("TICKET_SPEC", "Spectator Orb", "Ticket", "Rare", 200, "RES_GEM", pMode + "spectator.png"));
            items.Add(CreateItem("TICKET_SURV", "Survival Guide", "Ticket", "Common", 50, "RES_GOLD", pMode + "survival.png"));

            // --- 11. OTHERS (/images/others/) [NEW SECTION - CREATIVE MAPPING] ---
            string pOther = "/images/others/";
            items.Add(CreateItem("MSC_EXP", "Bottle o' Enchanting", "Consumable", "Rare", 300, "RES_GOLD", pOther + "exp.png"));
            items.Add(CreateItem("MSC_QUEST", "Quest Scroll", "Item", "Uncommon", 100, "RES_GOLD", pOther + "quest.png"));
            // Lưu ý: Các file UUID trong others và resources thường là file rác hoặc user upload, ta bỏ qua để tránh lỗi.

            context.ShopItems.AddRange(items);
            Console.WriteLine($"> Seeded {items.Count} items to Shop (Covering all folders).");
        }

        private static void SeedMonsters(ApplicationDbContext context)
        {
            var monsters = new List<Monster>();
            string pMob = "/images/mobs/";

            // Map chính xác các file trong folder mobs
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

            // --- TOOLS ---
            recipes.Add(CreateRecipe("R_PICK_STONE", "TOOL_PICK", "Stone Pickaxe", pTool + "stone_pickaxe.png", 10, "MAT_COBBLE:3,MAT_LOG:2"));
            recipes.Add(CreateRecipe("R_AXE_STONE", "TOOL_AXE", "Stone Axe", pTool + "stone_axe.png", 10, "MAT_COBBLE:3,MAT_LOG:2"));
            recipes.Add(CreateRecipe("R_SHOVEL_STONE", "TOOL_SHOVEL", "Stone Shovel", pTool + "stone_shovel.png", 8, "MAT_COBBLE:1,MAT_LOG:2"));
            recipes.Add(CreateRecipe("R_HOE_STONE", "TOOL_HOE", "Stone Hoe", pTool + "stone_hoe.png", 8, "MAT_COBBLE:2,MAT_LOG:2"));

            // --- WEAPONS (NEW) ---
            recipes.Add(CreateRecipe("R_SWORD_WOOD", "WEAP_WOOD_SWORD", "Wooden Sword", pWeap + "wooden_sword.png", 5, "MAT_LOG:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_STONE", "WEAP_STONE_SWORD", "Stone Sword", pWeap + "stone_sword.png", 10, "MAT_COBBLE:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_IRON", "WEAP_IRON_SWORD", "Iron Sword", pWeap + "iron_sword.png", 20, "MAT_IRON:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_GOLD", "WEAP_GOLD_SWORD", "Gold Sword", pWeap + "gold_sword.png", 15, "MAT_GOLD:2,MAT_LOG:1"));
            recipes.Add(CreateRecipe("R_SWORD_DIAMOND", "WEAP_DIAMOND_SWORD", "Diamond Sword", pWeap + "diamond_sword.png", 60, "MAT_DIAMOND:2,MAT_LOG:1"));
            // Netherite cần Sword Diamond + Scrap
            recipes.Add(CreateRecipe("R_SWORD_NETHER", "WEAP_NETHER_SWORD", "Netherite Sword", pWeap + "netherite_sword.png", 120, "WEAP_DIAMOND_SWORD:1,MAT_SCRAP:1"));

            // --- VEHICLES ---
            recipes.Add(CreateRecipe("R_BOAT", "VEH_BOAT", "Boat", pVeh + "boat.png", 15, "MAT_LOG:5"));
            recipes.Add(CreateRecipe("R_MINECART", "VEH_MINECART", "Minecart", pVeh + "minecart.png", 30, "MAT_IRON:5"));

            // --- FOOD ---
            recipes.Add(CreateRecipe("R_GOLD_APPLE", "CON_GOLD_APPLE", "Golden Apple", pCon + "golden_apple.png", 60, "MAT_GOLD:8,CON_APPLE:1"));
            recipes.Add(CreateRecipe("R_BREAD", "CON_BREAD", "Bread", pCon + "bread.png", 5, "MAT_WHEAT:3")); // (Giả sử wheat là logic ẩn hoặc dùng item khác)

            context.Recipes.AddRange(recipes);
            Console.WriteLine($"> Seeded {recipes.Count} recipes.");
        }

        // --- HELPER METHODS ---

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