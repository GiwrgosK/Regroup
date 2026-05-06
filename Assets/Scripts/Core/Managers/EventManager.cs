using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventManager : MonoBehaviour {
    public static EventManager Instance { get; private set;}

    [Header("Event Manager Configuration")]
    [SerializeField] private TextAsset eventsJSON;

    private const string CROSSROADS_MAP = "CrossroadsCombatScene";
    private const string RIVER_MAP = "RiverCombatScene";
    private const string TRAINYARD_MAP = "TrainyardCombatScene";
    private const string ABANDONEDTOWN_MAP = "AbandonedTownCombatScene";

    private List<EventData> allEvents;
    private EventData introEvent;
    private Dictionary<string, EventData> endingEvents = new Dictionary<string, EventData>();
    private List<EventData> poolableEvents = new List<EventData>();
    private readonly List<string> allMaps = new List<string> {
        CROSSROADS_MAP,
        RIVER_MAP,
        TRAINYARD_MAP,
        ABANDONEDTOWN_MAP
    };
    
    private bool isReturningFromCombat = false;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this; 
        DontDestroyOnLoad(gameObject);
        LoadAndCategorizeEvents();
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += SceneManager_SceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
    }

    private void LoadAndCategorizeEvents() {
        allEvents = JsonUtilityWrapper.FromJsonList<EventData>(eventsJSON.text);
        foreach (EventData events in allEvents) {
            if (events.NodeType == "Tutorial") {
                introEvent = events;
            } else if (events.NodeType == "Ending") {
                if (events.ID.Contains("Cherbourg")) endingEvents["Cherbourg"] = events;
                if (events.ID.Contains("Dieppe")) endingEvents["Dieppe"] = events;
            } else {
                poolableEvents.Add(events);
            }
        }
    }

    public Queue<EventData> GetEventDeck(int numberOfNodesNeeded) {
        List<EventData> shuffled = new List<EventData>(poolableEvents);
        ShuffleList(shuffled);

        Queue<EventData> deck = new Queue<EventData>();
        for (int i = 0; i < numberOfNodesNeeded; i++) {
            if (i < shuffled.Count) {
                deck.Enqueue(shuffled[i]);
            } else {
                EventData randomDuplicate = poolableEvents[Random.Range(0, poolableEvents.Count)];
                deck.Enqueue(randomDuplicate);
            }
        }
        return deck;
    }

    public EventData GetIntroEvent() {
        return introEvent;
    }

    public EventData GetEndingEvent(string townName) {
        if (endingEvents.TryGetValue(townName, out EventData evt)) {
            return evt;
        }
        return null;
    }

    private void ShuffleList<T>(List<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void TriggerEventByID(string eventId) {
        var foundEvent = allEvents.FirstOrDefault(e => e.ID == eventId);
        if (foundEvent == null) {
            Debug.LogWarning($"No event found with id {eventId}");
            return;
        }
        EventUI.Instance.Show(foundEvent);
    }

    public void ResolveEvent(EventOption eventOption) {
        bool isCombat = eventOption.Consequences.Any(c => c.Type == "StartCombat");
        if (isCombat) {
            isReturningFromCombat = true;
            if (EventUI.Instance != null) {
                EventUI.Instance.Hide();
            }
        }

        foreach (Consequence consequence in eventOption.Consequences) {
            consequence.Apply();
        }

        if (!isCombat) {
            string resultText = eventOption.ResultText;
            if (string.IsNullOrEmpty(resultText)) {
                EventUI.Instance.Hide();
                Map.Instance.OpenMap();
            } else {
                EventUI.Instance.ShowResult(resultText, () => {
                    Map.Instance.OpenMap();
                });
            }
        }
    }

    public void InitiateCombat() {
        AudioManager.Instance.PlaySceneChangeSoundEffect();
        int randomMap = Random.Range(0, allMaps.Count);
        string chosenMap = allMaps[randomMap];
        SceneTransitionManager.Instance.LoadScene(chosenMap);
    }

    private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "CampaignMapScene" && isReturningFromCombat) {
            StartCoroutine(OpenMapDelay());
        }

        if (allMaps.Contains(scene.name)) {
            if (CombatManager.Instance != null) {
                CombatManager.Instance.SetupCombat();
            }
        }
    }

    private IEnumerator OpenMapDelay() {
        yield return null; 
        isReturningFromCombat = false;
        Map.Instance.OpenMap();
    }
}