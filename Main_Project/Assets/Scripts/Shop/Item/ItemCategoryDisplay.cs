using System.Collections.Generic;

public static class ItemCategoryDisplay
{
    private static readonly Dictionary<ItemCategory, string> Names = new Dictionary<ItemCategory, string>
    {
        { ItemCategory.Potion, "item_category_potion" },
        { ItemCategory.Material, "item_category_material" },
        { ItemCategory.Accessory, "item_category_accessory" },
        { ItemCategory.Etc, "item_category_etc" }
        
    };
    
    public static string GetName(ItemCategory category) =>
        Names.TryGetValue(category, out var key) ? Loc.Get(key) : category.ToString();
}