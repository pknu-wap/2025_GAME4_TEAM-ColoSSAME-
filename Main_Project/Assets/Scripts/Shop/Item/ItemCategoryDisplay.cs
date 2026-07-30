using System.Collections.Generic;

public static class ItemCategoryDisplay
{
    private static readonly Dictionary<ItemCategory, string> Names = new Dictionary<ItemCategory, string>
    {
        { ItemCategory.Potion, "물약" },
        { ItemCategory.Material, "강화석" },
        { ItemCategory.Accessory, "악세서리" },
        { ItemCategory.Etc, "기타" }
        
    };
    
    public static string GetName(ItemCategory category) =>
        Names.TryGetValue(category, out var name) ? name : category.ToString();
}