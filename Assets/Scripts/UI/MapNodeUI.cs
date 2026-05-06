using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MapNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("Map Node UI Configuration")]
    [SerializeField] private Button nodeButton;
    [SerializeField] private TextMeshProUGUI nodeText;

    [Header("Map Node UI Images")]
    [SerializeField] private Image townNodeIcon;
    [SerializeField] private Image extraNodeIcon;
    [SerializeField] private Image endingNodeIcon;
    [SerializeField] private Image isVisitedCircle;
    [SerializeField] private Image visitedXIcon;

    [Header("Map Node UI Sound Effects")]
    [SerializeField] private AudioClip onHoverSoundEffect;
    [SerializeField] private AudioClip onClickSoundEffect;

    private System.Action<MapNode> onClick;
    private MapNode node;
    private Vector3 defaultScale;
    private Outline iconOutline;
    private bool isHovered;
    private readonly float hoverScale = 1.2f;
    private readonly float hoverSpeed = 5f;

    private void Awake() {
        iconOutline = GetComponent<Outline>();
    }

    private void Update() {
        Vector3 targetScale = defaultScale;
        if (isHovered) {
            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.05f;
            targetScale = hoverScale * pulse * defaultScale;
        }
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverSpeed);
    }

    public void Setup(MapNode node, System.Action<MapNode> onClick) {
        this.node = node;
        this.onClick = onClick;
        nodeText.text = node.NodeID;
        nodeButton.onClick.AddListener(OnClick);
        defaultScale = transform.localScale;

        UpdateIcons();
        UpdateVisualState();
    }

    public void UpdateIcons() {
        townNodeIcon.enabled = false;
        extraNodeIcon.enabled = false;
        endingNodeIcon.enabled = false;

        if (node.Type == MapNode.NodeType.Ending) {
            endingNodeIcon.enabled = true;
        } else if (node.IsExtra) {
            extraNodeIcon.enabled = true;
        } else {
            townNodeIcon.enabled = true;
        }
    }

    public void UpdateVisualState() {
        nodeButton.interactable = node.IsAvailable;
        isVisitedCircle.gameObject.SetActive(node.IsVisited && node.IsCurrentNode);
        visitedXIcon.gameObject.SetActive(node.IsVisited && !node.IsCurrentNode);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovered = true;
        AudioManager.Instance.PlayClip(onHoverSoundEffect);
        iconOutline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
        iconOutline.enabled = false;
    }

    private void OnClick() {
        onClick?.Invoke(node);
        AudioManager.Instance.PlayClip(onClickSoundEffect);
    }
}