using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Table;
using XNode;

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Map")]
    public class MapNode : Node
    {
        [SerializeField, HideInInspector] private string _map;
        public Id<IMap> Map => new(_map);

        [Input(ShowBackingValue.Never, connectionType: ConnectionType.Override), SerializeField, Required]
        [InfoBox("Connection to SectionData is required.", InfoMessageType.Error, VisibleIf = nameof(_isSectionUnconnected))]
        private SectionData _sectionData = null;
        [Input(ShowBackingValue.Never, connectionType: ConnectionType.Override), SerializeField, Required]
        [InfoBox("Connection to FloorData is required.", InfoMessageType.Error, VisibleIf = nameof(_isFloorUnconnected))]
        private FloorData _floorData = null;
        [Input(ShowBackingValue.Never, connectionType: ConnectionType.Override), SerializeField]
        [InfoBox("No enemies spawn on this map.", InfoMessageType.Info, VisibleIf = nameof(_isEnemiesUnconnected))]
        private Table<EnemyData> _enemies = null;
        public List<EnemyData> Boss;

        [Input(ShowBackingValue.Never), SerializeField]
        [InfoBox("This map has no previous map.", InfoMessageType.Info, VisibleIf = nameof(_isPrevMapUnconnected))]
        private int _prevMap;
        [Output(ShowBackingValue.Never), SerializeField]
        [InfoBox("This map has no next map.", InfoMessageType.Info, VisibleIf = nameof(_isNextMapUnconnected))]
        private int _nextMap;
        [Input(ShowBackingValue.Never), SerializeField]
        private int _teleportIn;
        [Output, SerializeField]
        private int _teleportOut;

        private bool _isPrevMapUnconnected => !GetInputPort(nameof(_prevMap)).IsConnected;
        private bool _isNextMapUnconnected => !GetOutputPort("_nextMap").IsConnected;
        private bool _isSectionUnconnected => !GetInputPort("_sectionData").IsConnected;
        private bool _isFloorUnconnected => !GetInputPort("_floorData").IsConnected;
        private bool _isEnemiesUnconnected => !GetInputPort("_enemies").IsConnected;

        public IEnumerable<MapNode> PrevNodes => GetInputPort(nameof(_prevMap))
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

        public IEnumerable<MapNode> TeleportInNodes => GetInputPort(nameof(_teleportIn))
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();
        public IEnumerable<MapNode> NextNodes => GetOutputPort("_nextMap")
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

        public IEnumerable<MapNode> TeleportOutNodes => GetOutputPort("_teleportOut")
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

        public SectionData SectionData => GetInputValue<SectionData>(nameof(_sectionData));
        public FloorData FloorData => GetInputValue<FloorData>(nameof(_floorData));
        public Table<EnemyData> Enemies => GetInputValue<Table<EnemyData>>(nameof(_enemies), new());
        [ReadOnly, ShowInInspector] public bool IsStartMap => PrevNodes.Count() == 0 && TeleportInNodes.Count() == 0;
        [ReadOnly, ShowInInspector] public int Depth => PrevNodes.Select(node => node.Depth).DefaultIfEmpty(0).Max() + 1;
        protected override void Init()
        {
            if (string.IsNullOrEmpty(_map))
            {
                _map = Id<IMap>.Generate().ToString();
            }
        }
    }
}