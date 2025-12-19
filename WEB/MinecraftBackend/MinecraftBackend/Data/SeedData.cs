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

                Console.WriteLine(">>> STARTING SMART SEED DATA...");

                // 1. ITEMS (Vừa là hàng hóa, vừa là dữ liệu game)
                if (!context.ShopItems.Any()) 
                {
                    SeedItems(context);
                }

                // 2. MONSTERS (Dữ liệu quái vật - Không liên quan đến Shop)
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
                Console.WriteLine(">>> SEED COMPLETED! (Hidden items marked as IsShow=false)");
            }
        }

        private static void SeedItems(ApplicationDbContext context)
        {
            var items = new List<ShopItem>();

            // --- 1. ARMOR (Đồ sắt thì bán, đồ xịn hơn sau này user tự chế) ---
            string pArm = "/images/armor/";
            items.Add(CreateItem("ARM_IRON_HELMET", "Iron Helmet", "Armor", "Uncommon", 200, "RES_GOLD", pArm + "iron_helmet.png", true));
            items.Add(CreateItem("ARM_IRON_CHEST", "Iron Chestplate", "Armor", "Uncommon", 400, "RES_GOLD", pArm + "iron_chestplate.png", true));
            items.Add(CreateItem("ARM_IRON_LEGS", "Iron Leggings", "Armor", "Uncommon", 300, "RES_GOLD", pArm + "iron_leggings.png", true));
            items.Add(CreateItem("ARM_IRON_BOOTS", "Iron Boots", "Armor", "Uncommon", 150, "RES_GOLD", pArm + "iron_boots.png", true));

            // --- 2. WEAPONS (Cơ bản thì bán, Xịn thì phải chế tạo/Craft Only) ---
            string pWeap = "/images/weapons/";
            // Bán đồ gỗ, đá, sắt
            items.Add(CreateItem("WEAP_WOOD_SWORD", "Wooden Sword", "Weapon", "Common", 50, "RES_GOLD", pWeap + "wooden_sword.png", true));
            items.Add(CreateItem("WEAP_STONE_SWORD", "Stone Sword", "Weapon", "Common", 100, "RES_GOLD", pWeap + "stone_sword.png", true));
            items.Add(CreateItem("WEAP_IRON_SWORD", "Iron Sword", "Weapon", "Uncommon", 250, "RES_GOLD", pWeap + "iron_sword.png", true));
            
            // Đồ Vàng, Kim cương, Netherite -> ẨN KHỎI SHOP (IsShow = false), User phải tự chế tạo
            items.Add(CreateItem("WEAP_GOLD_SWORD", "Gold Sword", "Weapon", "Rare", 0, "RES_GOLD", pWeap + "gold_sword.png", false)); 
            items.Add(CreateItem("WEAP_DIAMOND_SWORD", "Diamond Sword", "Weapon", "Epic", 0, "RES_GEM", pWeap + "diamond_sword.png", false));
            items.Add(CreateItem("WEAP_NETHER_SWORD", "Netherite Sword", "Weapon", "Legendary", 0, "RES_GEM", pWeap + "netherite_sword.png", false));

            // --- 3. TOOLS (Tương tự vũ khí) ---
            string pTool = "/images/tools/";
            items.Add(CreateItem("TOOL_AXE", "Stone Axe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_axe.png", true));
            items.Add(CreateItem("TOOL_HOE", "Stone Hoe", "Tool", "Common", 60, "RES_GOLD", pTool + "stone_hoe.png", true));
            items.Add(CreateItem("TOOL_PICK", "Stone Pickaxe", "Tool", "Common", 80, "RES_GOLD", pTool + "stone_pickaxe.png", true));
            items.Add(CreateItem("TOOL_SHOVEL", "Stone Shovel", "Tool", "Common", 60, "RES_GOLD", pTool + "stone_shovel.png", true));

            // --- 4. CONSUMABLES (Thức ăn/Potion - Bán hết để hỗ trợ newbie) ---
            string pCon = "/images/consumables/";
            items.Add(CreateItem("CON_APPLE", "Apple", "Consumable", "Common", 10, "RES_GOLD", pCon + "apple.png", true));
            items.Add(CreateItem("CON_BREAD", "Bread", "Consumable", "Common", 15, "RES_GOLD", pCon + "bread.png", true));
            items.Add(CreateItem("CON_CAKE", "Cake", "Consumable", "Rare", 100, "RES_GOLD", pCon + "cake.png", true));
            // ... (Golden Apple quá xịn, ẨN để user tự chế)
            items.Add(CreateItem("CON_GOLD_APPLE", "Golden Apple", "Consumable", "Epic", 0, "RES_GEM", pCon + "golden_apple.png", false));
            items.Add(CreateItem("CON_POTION", "Mysterious Potion", "Consumable", "Rare", 150, "RES_GOLD", pCon + "potion.png", true));
            // Các món cơ bản khác
            items.Add(CreateItem("CON_STEAK", "Steak", "Consumable", "Uncommon", 50, "RES_GOLD", pCon + "steak.png", true));

            // --- 5. RESOURCES (Nguyên liệu) ---
            // Một số nguyên liệu cơ bản có thể bán, nhưng nguyên liệu hiếm thì chỉ Drop từ quái
            string pRes = "/images/resources/";
            items.Add(CreateItem("MAT_COAL", "Coal", "Material", "Common", 10, "RES_GOLD", pRes + "coal.png", true)); // Bán than
            items.Add(CreateItem("MAT_IRON", "Iron Ingot", "Material", "Uncommon", 30, "RES_GOLD", pRes + "iron_ingot.png", true));
            items.Add(CreateItem("MAT_DIAMOND", "Diamond", "Material", "Epic", 500, "RES_GEM", pRes + "diamond.png", true)); // Bán kim cương giá đắt
            
            // Nguyên liệu DROP ONLY (Không bán)
            items.Add(CreateItem("MAT_SCRAP", "Netherite Scrap", "Material", "Legendary", 0, "RES_GEM", pRes + "netherite_scrap.png", false));
            items.Add(CreateItem("MAT_SLIME", "Slime Ball", "Material", "Uncommon", 0, "RES_GOLD", pRes + "slime_ball.png", false)); // Drop từ Slime
            items.Add(CreateItem("MAT_OBSIDIAN", "Obsidian", "Material", "Rare", 0, "RES_GOLD", pRes + "obsidian.png", false)); 
            items.Add(CreateItem("MAT_AMETHYST", "Amethyst Shard", "Material", "Rare", 0, "RES_GOLD", pRes + "amethyst_shard.png", false));

            // --- 6. VEHICLES ---
            string pVeh = "/images/vehicles/";
            items.Add(CreateItem("VEH_BOAT", "Boat", "Vehicle", "Common", 100, "RES_GOLD", pVeh + "boat.png", true));
            items.Add(CreateItem("VEH_HORSE", "Horse", "Mount", "Rare", 1000, "RES_GOLD", pVeh + "horse.png", true));
            items.Add(CreateItem("VEH_ELYTRA", "Elytra", "Equipment", "Legendary", 0, "RES_GEM", pVeh + "elytra.png", false)); // Elytra chỉ tìm thấy, không bán

            // --- 7. OTHERS (Quest Item, Token - Chỉ dùng cho logic game) ---
            string pOther = "/images/others/";
            items.Add(CreateItem("MSC_EXP", "Experience Bottle", "Consumable", "Rare", 0, "RES_GOLD", pOther + "exp.png", false)); // Phần thưởng
            items.Add(CreateItem("MSC_QUEST", "Quest Scroll", "Item", "Uncommon", 0, "RES_GOLD", pOther + "quest.png", false)); // Vật phẩm nhiệm vụ

            // --- 8. AVATARS & BUILDINGS (Giữ nguyên bán trong Shop) ---
            string pAva = "/images/avatars/";
            items.Add(CreateItem("SKIN_CREEPER", "Creeper Skin", "Cosmetic", "Epic", 500, "RES_GEM", pAva + "creeper.png", true));
            items.Add(CreateItem("SKIN_STEVE", "Steve Skin", "Cosmetic", "Common", 0, "RES_GOLD", pAva + "steve.png", true));
            
            string pBuild = "/images/buildings/";
            items.Add(CreateItem("BP_STRUCT_01", "Basic House", "Blueprint", "Common", 1000, "RES_GOLD", pBuild + "structure_01.png", true));


            context.ShopItems.AddRange(items);
            Console.WriteLine($"> Seeded {items.Count} items (Mixed: Shop & GameData).");
        }

        private static void SeedMonsters(ApplicationDbContext context)
        {
            // Logic Mobs giữ nguyên, vì Mobs nằm bảng riêng, không liên quan ShopItem
            var monsters = new List<Monster>();
            string pMob = "/images/mobs/";

            monsters.Add(new Monster { Name = "Blaze", HP = 50, Damage = 10, ExpReward = 20, GoldReward = 30, ImageUrl = pMob + "blaze.png" });
            monsters.Add(new Monster { Name = "Creeper", HP = 40, Damage = 40, ExpReward = 15, GoldReward = 20, ImageUrl = pMob + "creeper.png" });
            monsters.Add(new Monster { Name = "Ender Dragon", HP = 500, Damage = 50, ExpReward = 1000, GoldReward = 5000, ImageUrl = pMob + "dragon.png" });
            monsters.Add(new Monster { Name = "Zombie", HP = 40, Damage = 5, ExpReward = 5, GoldReward = 5, ImageUrl = pMob + "zombie.png" });
            // ... (Bạn có thể thêm tiếp list cũ nếu muốn đầy đủ)

            context.Monsters.AddRange(monsters);
            Console.WriteLine($"> Seeded {monsters.Count} monsters.");
        }

        private static void SeedRecipes(ApplicationDbContext context)
        {
            var recipes = new List<Recipe>();
            string pWeap = "/images/weapons/";
            string pCon = "/images/consumables/";

            // Crafting Sword
            recipes.Add(CreateRecipe("R_SWORD_DIAMOND", "WEAP_DIAMOND_SWORD", "Diamond Sword", pWeap + "diamond_sword.png", 60, "MAT_DIAMOND:2,MAT_LOG:1"));
            // Netherite Sword (Item này IsShow=false bên shop, chỉ có thể lấy qua recipe này)
            recipes.Add(CreateRecipe("R_SWORD_NETHER", "WEAP_NETHER_SWORD", "Netherite Sword", pWeap + "netherite_sword.png", 120, "WEAP_DIAMOND_SWORD:1,MAT_SCRAP:1"));
            
            // Golden Apple
            recipes.Add(CreateRecipe("R_GOLD_APPLE", "CON_GOLD_APPLE", "Golden Apple", pCon + "golden_apple.png", 60, "MAT_GOLD:8,CON_APPLE:1"));

            context.Recipes.AddRange(recipes);
            Console.WriteLine($"> Seeded {recipes.Count} recipes.");
        }

        // --- HELPER METHODS ---

        // Thêm tham số isForSale để quyết định IsShow
        private static ShopItem CreateItem(string id, string name, string type, string rarity, int price, string currency, string imgUrl, bool isForSale)
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
                IsShow = isForSale // <--- QUAN TRỌNG: True = Hiện trong Shop, False = Chỉ là data game
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