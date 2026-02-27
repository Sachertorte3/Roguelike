#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model;
using Domain.Model.Character.Status;
using Domain.Model.Memento;
using R3;
using Unity.Logging;
using Utilities;

namespace Game
{
    public class WorldStatistics : ISerializable<StatisticsMemento>
    {
        // プレイ・進行
        private TimeSpan _lastSavePlayTime;
        private DateTime _stateChangedAt;
        private readonly ReadOnlyReactiveProperty<GameState> _state;
        private TimeSpan CurrentSessionTime => _state.CurrentValue == GameState.Dungeon
            ? DateTime.Now - _stateChangedAt
            : TimeSpan.Zero;
        public TimeSpan PlayTime => _lastSavePlayTime + CurrentSessionTime;
        private readonly ReactiveProperty<int> _turn;
        public ReadOnlyReactiveProperty<int> Turn => _turn;
        public int MaxMapLevel { get; private set; }
        public bool IsCheating { get; set; }

        // 戦闘・ダメージ
        private int _totalDamageReceived;
        private int _maxDamageReceived;
        private int _totalDamageDealt;
        private int _maxDamageDealt;
        private int _totalHealReceived;
        private int _maxHealReceived;

        // 敵撃破
        private readonly Dictionary<string, int> _enemyTypeKilledCount = new();

        // アイテム使用
        private readonly Dictionary<string, int> _itemUsedCountByBaseName = new();

        // 死亡
        private readonly Dictionary<string, int> _deathCountByCause = new();
        public WorldStatistics(StatisticsMemento memento, GameManager game, World world)
        {
            _state = game.State;
            _lastSavePlayTime = TimeSpan.FromTicks(memento.PlayTime);
            _stateChangedAt = DateTime.Now;
            _turn = new(memento.Turn);
            MaxMapLevel = memento.MaxMapLevel;
            IsCheating = memento.IsCheating;
            _enemyTypeKilledCount = new Dictionary<string, int>(memento.EnemyTypeKilledCount);
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
                _turn.Value++;
            });

            _state.Pairwise().Subscribe(state =>
            {
                if (state.Previous == GameState.Dungeon)
                {
                    _lastSavePlayTime += DateTime.Now - _stateChangedAt;
                }
                _stateChangedAt = DateTime.Now;
            });
        }
        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, _turn.Value, MaxMapLevel, IsCheating,
                _totalDamageReceived, _maxDamageReceived, _totalDamageDealt, _maxDamageDealt,
                _totalHealReceived, _maxHealReceived, _itemUsedCountByBaseName, _deathCountByCause, _enemyTypeKilledCount);
        }
        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, 1, false, 0, 0, 0, 0, 0, 0, new Dictionary<string, int>(), new Dictionary<string, int>(), new Dictionary<string, int>());
        }

        public string GetStatisticsText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== World Statistics ===");
            sb.AppendLine("--- プレイ・進行 ---");
            sb.AppendLine($"PlayTime: {PlayTime}");
            sb.AppendLine($"Turn: {Turn.CurrentValue}");
            sb.AppendLine($"MaxMapLevel: {MaxMapLevel}");
            sb.AppendLine($"IsCheating: {IsCheating}");
            sb.AppendLine("--- 戦闘・ダメージ ---");
            sb.AppendLine($"TotalDamageReceived: {_totalDamageReceived} (Max: {_maxDamageReceived})");
            sb.AppendLine($"TotalDamageDealt: {_totalDamageDealt} (Max: {_maxDamageDealt})");
            sb.AppendLine($"TotalHealReceived: {_totalHealReceived} (Max: {_maxHealReceived})");
            sb.AppendLine("--- 敵撃破 ---");
            sb.AppendLine($"EnemyKilledCount: {_enemyTypeKilledCount.Values.Sum()}");
            foreach (var kvp in _enemyTypeKilledCount.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine("--- アイテム使用 ---");
            sb.AppendLine($"TotalItemUsedCount: {_itemUsedCountByBaseName.Values.Sum()}");
            foreach (var kvp in _itemUsedCountByBaseName.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine("--- 死亡 ---");
            sb.AppendLine($"TotalDeathCount: {_deathCountByCause.Values.Sum()}");
            foreach (var kvp in _deathCountByCause.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            return sb.ToString();
        }
    }
}