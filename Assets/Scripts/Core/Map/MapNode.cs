using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour {
    public enum NodeType {
        Tutorial,
        Encounter,
        Event,
        Resupply,
        Ending
    }
    
    private string nodeID;
    private string assignedEventID;
    private Vector2 position;
    private List<string> connectedNodes;
    private NodeType nodeType;
    private bool isVisited;
    private bool isAvailable;
    private bool isExtra;
    private bool isCurrentNode;

    public string NodeID => nodeID;
    public string AssignedEventID => assignedEventID;
    public Vector2 Position => position;
    public List<string> ConnectedNodes => connectedNodes;
    public NodeType Type => nodeType;
    public bool IsVisited => isVisited;
    public bool IsAvailable => isAvailable;
    public bool IsExtra => isExtra;
    public bool IsCurrentNode => isCurrentNode;

    public void InitializeMapNode(string id, string eventID, Vector2 pos, List<string> connections, NodeType type, bool extra) {
        nodeID = id;
        assignedEventID = eventID;
        position = pos;
        connectedNodes = new List<string>(connections);
        nodeType = type;
        isVisited = false;
        isAvailable = false;
        isExtra = extra;
    }

    public void SetVisited(bool visited) => isVisited = visited;
    public void SetAvailable(bool available) => isAvailable = available;
    public void SetCurrentNode(bool current) => isCurrentNode = current;
}