#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model.Character.Status;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Game
{
    public class GlobalStatistics
    {
        // プレイ・進行
        public int MaxMapLevel { get; private set; }
        private TimeSpan _lastSaveTotalPlayTime;
        private DateTime _sessionStartTime;
        private TimeSpan CurrentSessionTime => DateTime.Now - _sessionStartTime;
        public TimeSpan TotalPlayTime => _lastSaveTotalPlayTime + CurrentSessionTime;
        private readonly ReactiveProperty<int> _totalTurns;
        public ReadOnlyReactiveProperty<int> TotalTurns => _totalTurns;

        // 戦闘・ダメージ
        private int _totalDamageReceived;
        private int _maxDamageReceived;
        private int _totalDamageDealt;
        private int _maxDamageDealt;
        private int _totalHealReceived;
        private int _maxHealReceived;

        // 敵撃破
        private readonly Dictionary<string, int> _enemyTypeKilledCount = new();

        // アイテム
        private readonly ObservableHashSet<string> _knownItemNames;
        public IObservableCollection<string> KnownItemNames => _knownItemNames;
        private readonly Dictionary<string, int> _itemUsedCountByBaseName = new();

        // 死亡
        private readonly Dictionary<string, int> _deathCountByCause = new();
        public GlobalStatistics(GlobalStatisticsMemento memento, GameManager game, World world)
        {
            _knownItemNames = new(memento.KnownItemNames);
            _enemyTypeKilledCount = new Dictionary<string, int>(memento.EnemyTypeKilledCount);
            _lastSaveTotalPlayTime = TimeSpan.FromTicks(memento.TotalPlayTime);
            _sessionStartTime = DateTime.Now;
            _totalTurns = new ReactiveProperty<int>(memento.TotalTurns);
            _totalDamageReceived = memento.TotalDamageReceived;
            _maxDamageReceived = memento.MaxDamageReceived;
            _totalDamageDealt = memento.TotalDamageDealt;
            _maxDamageDealt = memento.MaxDamageDealt;
            _totalHealReceived = memento.TotalHealReceived;
            _maxHealReceived = memento.MaxHealReceived;
            foreach (var kvp in memento.ItemUsedCountByBaseName)
                _itemUsedCountByBaseName[kvp.Key] = kvp.Value;
            foreach (var kvp in memento.DeathCountByCause)
                _deathCountByCause[kvp.Key] = kvp.Value;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var map = mapChanged.Map;
                if (map.Depth > MaxMapLevel)
                    MaxMapLevel = map.Depth;
                map.Player.Character.KnownItemNames.ObserveAdd().Subscribe(item =>
                {
                    _knownItemNames.Add(item.Value);
                });
                map.Characters.SubscribeIncludingCurrentObservables(
                    character => character.Status.OnDamageReceived,
                    (character, msg) =>
                    {
                        if (character.IsPlayer)
                        {
                            _totalDamageReceived += msg.Damage;
                            if (msg.Damage > _maxDamageReceived)
                                _maxDamageReceived = msg.Damage;
                        }
                        else if (map.Player.Character.Affiliation.IsEnemy(character.Affiliation) &&
                                 msg.Attacker?.IsPlayer == true)
                        {
                            _totalDamageDealt += msg.Damage;
                            if (msg.Damage > _maxDamageDealt)
                                _maxDamageDealt = msg.Damage;
                        }
                    }
                );
                map.Characters.SubscribeIncludingCurrentObservables(
                    character => character.Status.OnHealReceived,
                    (character, amount) =>
                    {
                        if (character.IsPlayer)
                        {
                            _totalHealReceived += amount;
                            if (amount > _maxHealReceived)
                                _maxHealReceived = amount;
                        }
                    }
                );
                map.Player.Character.OnItemUsed.Subscribe(baseName =>
                {
                    _itemUsedCountByBaseName.TryGetValue(baseName, out var count);
                    _itemUsedCountByBaseName[baseName] = count + 1;
                });
                map.Player.Character.Entity.OnDestroyed.Subscribe(cause =>
                {
                    _deathCountByCause.TryGetValue(cause, out var count);
                    _deathCountByCause[cause] = count + 1;
                });
                map.Characters.SubscribeIncludingCurrentObservables(
                    character => character.OnDead,
                    (character, _) =>
                    {
                        if (!character.IsPlayer && map.Player.Character.Affiliation.IsEnemy(character.Affiliation))
                        {
                            var enemyName = character.Name;
                            _enemyTypeKilledCount.TryGetValue(enemyName, out var count);
                            _enemyTypeKilledCount[enemyName] = count + 1;
                        }
                    }
                );
            });
            game.OnTurnChanged.Subscribe(_ =>
            {
                _totalTurns.Value++;
            });
        }

        public GlobalStatisticsMemento Serialize()
        {
            return new GlobalStatisticsMemento(MaxMapLevel, _knownItemNames.ToList(), _enemyTypeKilledCount,
                TotalPlayTime.Ticks, TotalTurns.CurrentValue,
                _totalDamageReceived, _maxDamageReceived, _totalDamageDealt, _maxDamageDealt,
                _totalHealReceived, _maxHealReceived, _itemUsedCountByBaseName, _deathCountByCause);
        }
        public static GlobalStatisticsMemento Build()
        {
            return new GlobalStatisticsMemento(1, new(), new Dictionary<string, int>(), 0, 0, 0, 0, 0, 0, 0, 0, new Dictionary<string, int>(), new Dictionary<string, int>());
        }

        public string GetStatisticsText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Global Statistics ===");
            sb.AppendLine("--- プレイ・進行 ---");
            sb.AppendLine($"TotalPlayTime: {TotalPlayTime}");
            sb.AppendLine($"TotalTurns: {TotalTurns.CurrentValue}");
            sb.AppendLine($"MaxMapLevel: {MaxMapLevel}");
            sb.AppendLine("--- 戦闘・ダメージ ---");
            sb.AppendLine($"TotalDamageReceived: {_totalDamageReceived} (Max: {_maxDamageReceived})");
            sb.AppendLine($"TotalDamageDealt: {_totalDamageDealt} (Max: {_maxDamageDealt})");
            sb.AppendLine($"TotalHealReceived: {_totalHealReceived} (Max: {_maxHealReceived})");
            sb.AppendLine("--- 敵撃破 ---");
            sb.AppendLine($"EnemyKilledCount: {_enemyTypeKilledCount.Values.Sum()}");
            foreach (var kvp in _enemyTypeKilledCount.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine("--- アイテム ---");
            sb.AppendLine($"TotalItemUsedCount: {_itemUsedCountByBaseName.Values.Sum()}");
            foreach (var kvp in _itemUsedCountByBaseName.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine($"KnownItemNames Count: {_knownItemNames.Count}");
            foreach (var itemName in _knownItemNames.OrderBy(x => x))
                sb.AppendLine($"  {itemName}");
            sb.AppendLine("--- 死亡 ---");
            sb.AppendLine($"TotalDeathCount: {_deathCountByCause.Values.Sum()}");
            foreach (var kvp in _deathCountByCause.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            return sb.ToString();
        }
    }
}