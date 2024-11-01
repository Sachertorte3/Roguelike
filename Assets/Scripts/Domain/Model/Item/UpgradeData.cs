namespace Domain.Model.Item
{
    public record UpgradeData(string UpgradeName, System.Action Upgrade, System.Action Downgrade);
}