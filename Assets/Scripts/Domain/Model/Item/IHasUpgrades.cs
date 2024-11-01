using System.Collections.Generic;
using System.Linq;

namespace Domain.Model.Item
{
    public interface IHasUpgrades
    {
        public string UpgradePathName { get; }
        public List<UpgradeData> GetUpgrades();
        public List<string> GetUpgradeNames() => GetUpgrades().Select(upgrade => upgrade.UpgradeName).ToList();
        public List<IHasUpgrades> GetChildren();
    }
}