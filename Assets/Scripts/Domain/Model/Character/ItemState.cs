#nullable enable
namespace Domain.Model.Items
{
    public enum ItemState
    {
        ShopItem,
        UsedShopItem,
        Stolen,
        None
    }
    public static class ItemStateExtensions
    {
        public static string GetDescription(this ItemState state)
        {
            return state switch
            {
                ItemState.ShopItem => "[売品]",
                ItemState.UsedShopItem => "[売品(使用済み)]",
                ItemState.Stolen => "[盗品]",
                ItemState.None => "",
            };
        }
    }
}