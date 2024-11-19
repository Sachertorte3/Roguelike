using System;

namespace Domain.Model.Item
{
    public record UpgradeData(string UpgradeName, Action Upgrade, Action Downgrade);
}