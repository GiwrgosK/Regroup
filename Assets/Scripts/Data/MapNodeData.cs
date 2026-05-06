using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable] public class MapNodeData {
    public string nodeID;
    public string assignedEventID;
    public Vector2 position;
    public List<string> connectedNodes;
    public MapNode.NodeType nodeType;
    public bool isVisited;
    public bool isAvailable;
    public bool isCurrentNode;
    public bool isExtra;
}