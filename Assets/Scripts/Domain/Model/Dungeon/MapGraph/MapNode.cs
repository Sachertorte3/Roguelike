using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using XNode;

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Map")]
    public class MapNode : Node
    {
        private string _map;
        public Id<IMap> Map => new(_map);

        [Input(ShowBackingValue.Never), SerializeField]
        [InfoBox("This map has no previous map.", InfoMessageType.Info, VisibleIf = "_isPrevMapUnconnected")]
        private int _prevMap;
        private bool _isPrevMapUnconnected => !GetInputPort("_prevMap").IsConnected;
        [Input(ShowBackingValue.Never), SerializeField]
        private int _teleportIn;

        public IEnumerable<MapNode> PrevNodes => GetInputPort("_prevMap")
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

        public IEnumerable<MapNode> TeleportInNodes => GetInputPort("_teleportIn")
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

        [Input(connectionType: ConnectionType.Override), SerializeField, Required]
        [InfoBox("Connection to SectionData is required.", InfoMessageType.Error, VisibleIf = "_isSectionUnconnected")]
        private SectionData _sectionData = null;
        private bool _isSectionUnconnected => !GetInputPort("_sectionData").IsConnected;

        public SectionData SectionData => GetInputValue<SectionData>("_sectionData");
        [Input(connectionType: ConnectionType.Override), SerializeField, Required]
        [InfoBox("Connection to FloorData is required.", InfoMessageType.Error, VisibleIf = "_isFloorUnconnected")]
        private FloorData _floorData = null;
        private bool _isFloorUnconnected => !GetInputPort("_floorData").IsConnected;

        public FloorData FloorData => GetInputValue<FloorData>("_floorData");

        [Output, SerializeField]
        [InfoBox("This map has no next map.", InfoMessageType.Info, VisibleIf = "_isNextMapUnconnected")]
        private int _nextMap;

        [Output, SerializeField]
        private int _teleportOut;
        private bool _isNextMapUnconnected => !GetOutputPort("_nextMap").IsConnected;

        public IEnumerable<MapNode> NextNodes => GetOutputPort("_nextMap")
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

        public IEnumerable<MapNode> TeleportOutNodes => GetOutputPort("_teleportOut")
            .GetConnections()
            .Select(connection => connection.node)
            .OfType<MapNode>();

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