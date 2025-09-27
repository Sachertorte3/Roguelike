#nullable enable
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public class StorageItem : BaseItem, ISerializable<StorageItemMemento>
    {
        private readonly Option<InventoryTargetSkill> _skillOnUse;
        private readonly Storage _itemStorage;
        private readonly CompositeDisposable _disposables = new();

        public override string RevealedName => BaseName;
        public override ItemCategory Category => ItemCategory.Storage;
        public override bool UseOnDeath => false;
        public override Option<IStorage> ItemStorage => _itemStorage.ToOption().Map(storage => (IStorage)storage);
        public override bool CannotUseIfCursed => true;
        public override bool CannotDropIfCursed => false;
        public override bool IdentifyIfGot => false;
        public override bool IdentifyIfUsed => true;
        public override bool AutoDestroyWhenDisabled => false;
        public override Option<ISkill> SkillOnUse => _skillOnUse.Map(skill => (ISkill)skill);
        public override Option<ISkill> SkillOnThrow => Option.None<ISkill>();
        protected override bool HasSameEffect => false;
        protected override bool HasSameSkill => false;

        public StorageItem(StorageItemData data) : this(Build(data))
        {
        }

        public StorageItem(StorageItemMemento data) : base(data.BaseItem)
        {
            _skillOnUse = data.SkillOnUse.Map(skill => new InventoryTargetSkill(skill));
            var itemStorage = new Storage(data.Storage);
            itemStorage.OnItemChanged.Subscribe(_ =>
            {
                _onItemUpdated.OnNext(Unit.Default);
            }).AddTo(_disposables);
            itemStorage.OnItemUpdated.Subscribe(_ =>
            {
                _onItemUpdated.OnNext(Unit.Default);
            }).AddTo(_disposables);
            _itemStorage = itemStorage;
        }

        public StorageItemMemento Serialize()
        {
            var json = JsonUtility.ToJson(new StorageItemMemento
            (
                baseItem: SerializeBase(),
                skillOnUse: _skillOnUse.Map(skill => skill.Serialize()),
                storage: _itemStorage.Serialize()
            ));
            return JsonUtility.FromJson<StorageItemMemento>(json);
        }

        public static StorageItemMemento Build(StorageItemData data, bool isCursed = false, ItemState state = ItemState.None)
        {
            var skillOnUse = data.InventoryEffect.ToOption().Map(effect => new InventoryTargetSkill(InventoryTargetSkill.Build(effect)).Serialize());

            var json = JsonUtility.ToJson(new StorageItemMemento
            (
                baseItem: BuildBase(
                    baseName: data.name,
                    icon: data.Icon,
                    isShiny: data.IsShiny,
                    additionalPrice: data.AdditionalPrice,
                    multiplyPrice: data.MultiplyPrice,
                    state: state,
                    maxUsages: data.UsageLimit,
                    isCursed: isCursed,
                    upgradeLimit: data.UpgradeLimit,
                    conditions: data.PassiveConditions
                ),
                skillOnUse: skillOnUse,
                storage: Storage.Build(data.StorageCapacity, false, data.CanRemoveItem)
            ));
            return JsonUtility.FromJson<StorageItemMemento>(json); //MEMO: To break the sharing of references
        }

        protected override string FullInfoImpl() => "";
    }
}