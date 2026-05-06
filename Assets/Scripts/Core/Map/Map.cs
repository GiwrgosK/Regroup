using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour {
    public static Map Instance { get; private set; }

    [Header("Map Configuration")]
    [SerializeField] private TextAsset JSONFile;
    [SerializeField] private RectTransform mapContainer;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Texture2D placementMask;

    private List<MapNode> allNodes = new List<MapNode>();
    private Dictionary<string, MapNode> allNodesDictionary = new Dictionary<string, MapNode>(); //For quick searches.
    private Dictionary<string, MapNodeUI> allNodesUI = new Dictionary<string, MapNodeUI>();
    private List<GameObject> activeConnections = new List<GameObject>();
    private List<string> placedEventIDs = new List<string>();

    private Vector2 maskSize = new Vector2(600f, 435f);
    private readonly float lineWidth = 2f;
    private readonly float pixelsPerUnitMultiplier = 1f;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        InitializeMap();
    }

    public void OpenMap() => mapContainer.gameObject.SetActive(true);
    public void CloseMap() => mapContainer.gameObject.SetActive(false);

    private void InitializeMap() {
        allNodes.Clear();
        allNodesDictionary.Clear();
        allNodesUI.Clear();
        placedEventIDs.Clear();

        if (GameManager.Instance.campaignNodes.Count == 0) {
            HandleNodeSetup();
            HandleConnectivityBetweenNodes(allNodes);
            SaveNodes();
        } else {
            LoadNodes();
        }
        HandleNodeUISetup();
    }

    private void HandleNodeSetup() {
        TownList townList = JsonUtility.FromJson<TownList>(JSONFile.text);
        int estimatedTotalNodes = townList.towns.Count + (townList.towns.Count * 1);
        Queue<EventData> eventDeck = EventManager.Instance.GetEventDeck(estimatedTotalNodes);
        int nodeIndex = 0;

        foreach (Town town in townList.towns) {
            Vector2 townPosition = ConvertMapCoordinatesToUI(town.position.x, town.position.y);

            if (nodeIndex == 0) {
                EventData intro = EventManager.Instance.GetIntroEvent();
                CreateNode(townPosition, intro.ID, MapNode.NodeType.Tutorial, ref nodeIndex, false);
                continue;
            }

            if (town.name == "Cherbourg" || town.name == "Dieppe") {
                EventData ending = EventManager.Instance.GetEndingEvent(town.name);
                if (ending != null) {
                    CreateNode(townPosition, ending.ID, MapNode.NodeType.Ending, ref nodeIndex, false);
                }
                continue;
            }

            if (IsPositionValid(townPosition)) {
                GetEventDataFromDeckOrDuplicate(eventDeck, out string eventID, out MapNode.NodeType nodeType);
                CreateNode(townPosition, eventID, nodeType, ref nodeIndex, false);

                int extraNodesPerTown = UnityEngine.Random.Range(0, 3);
                int maxAttempts = 10;

                for (int i = 0; i < extraNodesPerTown; i++) {
                    for (int j = 0; j < maxAttempts; j++) {
                        float minDistance = 30f; 
                        float maxDistance = 60f;
                        float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
                        float radius = UnityEngine.Random.Range(minDistance, maxDistance);
                        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                        Vector2 extraNodePosition = townPosition + offset;

                        if (IsPositionValid(extraNodePosition)) {
                            GetEventDataFromDeckOrDuplicate(eventDeck, out string extraEventID, out MapNode.NodeType extraNodeType);
                            CreateNode(extraNodePosition, extraEventID, extraNodeType, ref nodeIndex, true);
                            break; 
                        }
                    }
                }
            }
        }
    }

    private void GetEventDataFromDeckOrDuplicate(Queue<EventData> deck, out string id, out MapNode.NodeType type) {
        if (deck.Count > 0) {
            EventData nextEvent = deck.Dequeue();
            id = nextEvent.ID;
            if (Enum.TryParse(nextEvent.NodeType, out MapNode.NodeType parsedType)) type = parsedType;
            else type = MapNode.NodeType.Event;
            placedEventIDs.Add(id);
        } else {
            if (placedEventIDs.Count > 0) {
                id = placedEventIDs[UnityEngine.Random.Range(0, placedEventIDs.Count)];
                type = MapNode.NodeType.Event;
            } else {
                id = "Battle_Basic";
                type = MapNode.NodeType.Event;
            }
        }
    }

    private void CreateNode(Vector2 position, string eventID, MapNode.NodeType type, ref int index, bool isExtra) {
        GameObject mapNodeGameObject = Instantiate(nodePrefab, mapContainer);
        MapNode mapNode = mapNodeGameObject.GetComponent<MapNode>();
        string uniqueNodeID = $"{type} + {index++}";

        mapNode.InitializeMapNode(uniqueNodeID, eventID, position, new List<string>(), type, isExtra);
        mapNode.SetAvailable(false);

        MapNodeUI mapNodeUI = mapNodeGameObject.GetComponent<MapNodeUI>();
        mapNodeUI.Setup(mapNode, OnNodeClicked);
        mapNodeGameObject.GetComponent<RectTransform>().anchoredPosition = position;

        allNodes.Add(mapNode);
        allNodesDictionary.Add(mapNode.NodeID, mapNode);
        allNodesUI.Add(mapNode.NodeID, mapNodeUI);
    }

    private void HandleNodeUISetup() {
        MapNode startNode = allNodes[0];
        startNode.SetAvailable(true);

        if (!GameManager.Instance.HasInitializedCampaign) {
            OnNodeClicked(startNode);
            GameManager.Instance.SetCampaignFlagTrue();
        } else {
            foreach (var node in allNodes) {
                if (node.IsAvailable || node.IsCurrentNode) allNodesUI[node.NodeID].UpdateVisualState();
            }
        }

        foreach (var node in allNodes) {
            if (!allNodesUI.TryGetValue(node.NodeID, out var fromUI)) continue;
            foreach (string connectedNodeID in node.ConnectedNodes) {
                if (!allNodesUI.TryGetValue(connectedNodeID, out var toUI)) continue;
                if (string.Compare(node.NodeID, connectedNodeID) > 0) continue;
                DrawConnectionLine(fromUI, toUI);
            }
        }
    }

    private Vector2 ConvertMapCoordinatesToUI(float x, float y) {
        float mapWidth = 600f;
        float mapHeight = 435f;
        float flippedY = mapHeight - y;
        float newX = x - (mapWidth / 2f);
        float newY = flippedY - (mapHeight / 2f);
        return new Vector2(newX, newY);
    }

    private void DrawConnectionLine(MapNodeUI fromUI, MapNodeUI toUI) {
        var line = Instantiate(linePrefab, lineContainer);
        var lineImage = line.GetComponent<Image>();
        var lineRect = line.GetComponent<RectTransform>();
        var fromRect = fromUI.GetComponent<RectTransform>();
        var toRect = toUI.GetComponent<RectTransform>();

        Vector2 startPos = fromRect.anchoredPosition;
        Vector2 endPos = toRect.anchoredPosition;
        Vector2 direction = endPos - startPos;

        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineRect.sizeDelta = new Vector2(distance, lineWidth);
        lineRect.anchoredPosition = startPos + direction / 2f;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        lineImage.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier; 
        line.transform.SetAsFirstSibling(); 
        activeConnections.Add(line);
    }

    private void OnNodeClicked(MapNode clickedNode) {
        if (!clickedNode.IsAvailable) return;
        bool wasAlreadyVisited = clickedNode.IsVisited;

        foreach (MapNode node in allNodes) {
            node.SetCurrentNode(node == clickedNode);
            node.SetAvailable(false);
        }

        clickedNode.SetVisited(true);
        allNodesUI[clickedNode.NodeID].UpdateVisualState();

        foreach (string connectedID in clickedNode.ConnectedNodes) {
            MapNode connectedNode = allNodesDictionary[connectedID];
            connectedNode.SetAvailable(true);
        }
        foreach (MapNodeUI nodeUI in allNodesUI.Values) {
            nodeUI.UpdateVisualState();
        }

        GameManager.Instance.IncrementNodesVisited();
        SaveNodes();
        if (!wasAlreadyVisited) {
            EventManager.Instance.TriggerEventByID(clickedNode.AssignedEventID);
        }
    }
    
    private void HandleConnectivityBetweenNodes(List<MapNode> nodes) {
        float connectionRadius = 70f;
        int maxConnectionsPerNode = 4;

        List<Edge> allEdges = new List<Edge>();
        for (int i = 0; i < nodes.Count; i++) {
            for (int j = i + 1; j < nodes.Count; j++) {
                float distance = Vector2.Distance(nodes[i].Position, nodes[j].Position);
                if (distance <= connectionRadius * 1.5f) {
                    allEdges.Add(new Edge(i, j, distance));
                }
            }
        }

        allEdges.Sort((a, b) => a.distance.CompareTo(b.distance));

        UnionFind unionFind = new UnionFind(nodes.Count);
        List<Edge> acceptedEdges = new List<Edge>();

        foreach (Edge edge in allEdges) {
            if (unionFind.Find(edge.from) != unionFind.Find(edge.to)) {
                unionFind.Union(edge.from, edge.to);
                ConnectNodes(nodes[edge.from], nodes[edge.to]);
                acceptedEdges.Add(edge);
            }
        }

        foreach (Edge edge in allEdges) {
            MapNode nodeA = nodes[edge.from];
            MapNode nodeB = nodes[edge.to];

            if (nodeA.ConnectedNodes.Contains(nodeB.NodeID)) continue;
            if (edge.distance > connectionRadius) continue;
            if (nodeA.ConnectedNodes.Count >= maxConnectionsPerNode || nodeB.ConnectedNodes.Count >= maxConnectionsPerNode) continue;

            bool createsCrossing = false;
            foreach (Edge existing in acceptedEdges) {
                if (DoLinesIntersect(nodeA.Position, nodeB.Position, nodes[existing.from].Position, nodes[existing.to].Position)) {
                    createsCrossing = true;
                    break;
                }
            }

            if (!createsCrossing) {
                ConnectNodes(nodeA, nodeB);
                acceptedEdges.Add(edge);
            }
        }
    }

    private bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
        if (p1 == p3 || p1 == p4 || p2 == p3 || p2 == p4) return false;

        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        if (denominator == 0) return false;

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        return ua > 0f && ua < 1f && ub > 0f && ub < 1f;
    }

    private void ConnectNodes(MapNode a, MapNode b) {
        if (!a.ConnectedNodes.Contains(b.NodeID)) a.ConnectedNodes.Add(b.NodeID);
        if (!b.ConnectedNodes.Contains(a.NodeID)) b.ConnectedNodes.Add(a.NodeID);
    }

    private bool IsPositionValid(Vector2 position) {
        float x = (position.x + (maskSize.x / 2f)) / maskSize.x;
        float y = (position.y + (maskSize.y / 2f)) / maskSize.y;
        if (x < 0f || x > 1f || y < 0f || y > 1f) return false;
        Color pixel = placementMask.GetPixelBilinear(x, y);
        return pixel.r > 0.5f;
    }

    private void SaveNodes() {
        GameManager.Instance.campaignNodes.Clear();
        foreach (var node in allNodes) {
            var nodeData = new MapNodeData {
                nodeID = node.NodeID,
                assignedEventID = node.AssignedEventID,
                position = node.Position,
                connectedNodes = new List<string>(node.ConnectedNodes),
                nodeType = node.Type,
                isVisited = node.IsVisited,
                isAvailable = node.IsAvailable,
                isCurrentNode = node.IsCurrentNode,
                isExtra = node.IsExtra
            };
            GameManager.Instance.campaignNodes.Add(nodeData);
        }
    }

    private void LoadNodes() {
        foreach (MapNodeData mapNodeData in GameManager.Instance.campaignNodes) {
            GameObject mapNodeGameObject = Instantiate(nodePrefab, mapContainer);
            MapNode mapNode = mapNodeGameObject.GetComponent<MapNode>();

            mapNode.InitializeMapNode(mapNodeData.nodeID, mapNodeData.assignedEventID, mapNodeData.position, mapNodeData.connectedNodes, mapNodeData.nodeType, mapNodeData.isExtra);
            
            mapNode.SetVisited(mapNodeData.isVisited);
            mapNode.SetAvailable(mapNodeData.isAvailable);
            mapNode.SetCurrentNode(mapNodeData.isCurrentNode);

            MapNodeUI mapNodeUI = mapNodeGameObject.GetComponent<MapNodeUI>();
            mapNodeUI.Setup(mapNode, OnNodeClicked);
            mapNodeGameObject.GetComponent<RectTransform>().anchoredPosition = mapNodeData.position;

            allNodes.Add(mapNode);
            allNodesDictionary.Add(mapNode.NodeID, mapNode);
            allNodesUI.Add(mapNode.NodeID, mapNodeUI);
        }
    }

    public MapNode GetCurrentNode() => allNodes.Find(node => node.IsCurrentNode);

    private class Edge {
        public int from, to;
        public float distance;
        public Edge(int f, int t, float d) { from = f; to = t; distance = d; }
    }

    private class UnionFind {
        private int[] parent;
        public UnionFind(int size) {
            parent = new int[size];
            for (int i = 0; i < size; i++) parent[i] = i;
        }
        public int Find(int x) {
            if (parent[x] != x) parent[x] = Find(parent[x]);
            return parent[x];
        }
        public void Union(int x, int y) => parent[Find(x)] = Find(y);
    }
}