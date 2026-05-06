using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GrenadeAction : BaseAction {
    public event Action OnGrenadeThrown;

    [Header("Grenade Action Configuration")]
    [SerializeField] private Transform grenadeProjectilePrefab;
    [SerializeField] private GameObject rangeIndicatorPrefab;
    [SerializeField] private AudioClip grenadeExplosionSoundEffect;
    [SerializeField] private LayerMask obstacleLayerMask;

    protected override string ActionName => "Frag Grenade";
    protected override string ActionDescription => "Throw a Mk 2 frag grenade. Deals 50 area damage and destroys crates and barrels.";
    protected override int ActionCost => 1;
    protected override int Range => 6;

    private Dictionary<GameObject, int> outlinedObjects = new Dictionary<GameObject, int>();
    private GameObject rangeIndicatorInstance;
    private Vector3 lastCenter;
    private readonly string outlineLayerName = "Outline";
    private readonly float outlineBuffer = 0.25f;
    private readonly float explosionRadius = 4f;
    private float lastRadius;
    private int amount = -1;

    public int GrenadeAmount {
        get {
            if (amount == -1) {
                amount = Unit.Data.roleData.grenadeAmount;
            }
            return amount;
        }
    }

    private void Update() {
        if (!isActive) return;

        Vector3 mouseWorldPosition = MouseHandler.GetPosition();
        if (rangeIndicatorInstance != null) {
            rangeIndicatorInstance.transform.position = mouseWorldPosition;   
        }
        UpdateOutlines(mouseWorldPosition);
    }

    private void OnDestroy() {
        if (rangeIndicatorInstance != null) {
            Destroy(rangeIndicatorInstance);
            rangeIndicatorInstance = null;
        }

        ClearAllOutlines();
    }

    public override List<GridPosition> GetValidActionGridPositionList() {
        List<GridPosition>  validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = Unit.UnitGridPosition;
        
        for (int i = -Range; i <= Range; i++) {
			for (int j = -Range; j<= Range; j++) {
				GridPosition offsetGridPosition = new GridPosition(i, j);
				GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

				if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) {
					continue;
				}

				int testDistance = Mathf.Abs(i) + Mathf.Abs(j);
				if (testDistance > Range) {
					continue;
				}

                if (testGridPosition == unitGridPosition) {
                    continue;
                }

                if (!Pathfinding.Instance.IsGridPositionWalkable(testGridPosition)) {
                    continue;
                }

                if (!HasLineOfSight(unitGridPosition, testGridPosition)) {
                    continue;
                }

				validGridPositionList.Add(testGridPosition);
			}
		}

		return validGridPositionList;
    }

    public override bool IsActionAvailable() {
        return GrenadeAmount > 0;
    }

    private bool HasLineOfSight(GridPosition from, GridPosition to) {
        Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(from) + Vector3.up * 1.5f;
        Vector3 targetWorldPosition = LevelGrid.Instance.GetWorldPosition(to) + Vector3.up * 1.5f;
        Vector3 direction = targetWorldPosition - unitWorldPosition;
        float distance = Vector3.Distance(unitWorldPosition, targetWorldPosition);

        if (Physics.Raycast(unitWorldPosition, direction.normalized, distance, obstacleLayerMask)) {
            return false;
        }
        return true;
    }

    public override void OnSelected() {
        base.OnSelected();

        rangeIndicatorInstance = Instantiate(rangeIndicatorPrefab);
        float diameter = 8f;

        rangeIndicatorInstance.transform.localScale = new Vector3(diameter, diameter, diameter);
        UpdateOutlines(MouseHandler.GetPosition());
    }

    public override void OnDeselected() {
        base.OnDeselected();

        if (rangeIndicatorInstance != null) {
            Destroy(rangeIndicatorInstance);
            rangeIndicatorInstance = null;
        }

        ClearAllOutlines();
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition) {
        if (GrenadeAmount <= 0) return null;

        int totalTargetsHit = 0;
        int friendlyFireCount = 0;
        Vector3 impactPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        Collider[] colliderArray = Physics.OverlapSphere(impactPosition, explosionRadius);
        foreach (Collider collider in colliderArray) {
            if (collider.TryGetComponent(out Unit hitUnit)) {
                if (hitUnit.IsEnemy != Unit.IsEnemy) {
                    totalTargetsHit++;
                } else {
                    friendlyFireCount++;
                }
            }
        }

        if (friendlyFireCount > 0) return null;

        if (totalTargetsHit > 0) {
            int actionValue = 80;
            if (totalTargetsHit >= 2) actionValue += 100;
            if (totalTargetsHit >= 3) actionValue += 200;
            return new EnemyAIAction {
                gridPosition = gridPosition,
                actionValue = actionValue
            };
        }
        return null;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete) {
        if (GrenadeAmount <= 0) {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        OnDeselected();
        StartCoroutine(RotateThenThrowGrenade(gridPosition, onActionComplete));
    }

    private void OnGrenadeActionComplete() {
        AudioManager.Instance.PlayClip(grenadeExplosionSoundEffect);
        ActionComplete();
        if (GrenadeAmount <= 0) {
            OnDeselected();
        } else {
            OnSelected();
        }
    }

    private void UpdateOutlines(Vector3 center) {
        if (rangeIndicatorInstance == null) return;
        float radius = rangeIndicatorInstance.transform.localScale.x * 0.44f;
        if (Vector3.Distance(center, lastCenter) < 1f && Mathf.Abs(radius - lastRadius) < 1f) return;

        lastCenter = center;
        lastRadius = radius;

        Collider[] hits = Physics.OverlapSphere(center, radius + outlineBuffer);

        int unitLayer = LayerMask.NameToLayer("Unit");
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");

        HashSet<GameObject> newInRange = new HashSet<GameObject>();
        foreach (Collider hit in hits) {
            GameObject root = hit.transform.root.gameObject;
            if (root.layer != unitLayer && root.layer != obstacleLayer) continue;
            newInRange.Add(root);
        }
        foreach (GameObject root in newInRange) {
            if (!outlinedObjects.ContainsKey(root)) {
                outlinedObjects[root] = root.layer;
                int outlineLayer = LayerMask.NameToLayer(outlineLayerName);
                if (outlineLayer != -1) {
                    SetLayerRecursively(root, outlineLayer);
                }
            }
        }
        List<GameObject> previouslyOutlinedObjects = new List<GameObject>(outlinedObjects.Keys);
        foreach (GameObject previouslyOutlined in previouslyOutlinedObjects) {
            if (previouslyOutlined == null) {
                outlinedObjects.Remove(previouslyOutlined);
                continue;
            }
            float distance = Vector3.Distance(center, previouslyOutlined.transform.position);
            if (distance > radius + outlineBuffer) {
                SetLayerRecursively(previouslyOutlined, outlinedObjects[previouslyOutlined]);
                outlinedObjects.Remove(previouslyOutlined);
            }
        }
    }

    private void ClearAllOutlines() {
        foreach (var outlinedObject in outlinedObjects) {
            if (outlinedObject.Key != null) {
                SetLayerRecursively(outlinedObject.Key, outlinedObject.Value);
            }
        }
        outlinedObjects.Clear();
    }

    private void SetLayerRecursively(GameObject gameObject, int layerNumber) {
        gameObject.layer = layerNumber;
        foreach (Transform child in gameObject.transform) {
            SetLayerRecursively(child.gameObject, layerNumber);
        }
    }

    private IEnumerator RotateThenThrowGrenade(GridPosition gridPosition, Action onActionComplete) {
        yield return StartCoroutine(RotateTowardsTarget(LevelGrid.Instance.GetWorldPosition(gridPosition)));
    
        OnGrenadeThrown?.Invoke();
        amount--;
        yield return new WaitForSeconds(1.7f);
        Transform grenadeProjectileTransform = Instantiate(grenadeProjectilePrefab, Unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeActionComplete);
    }

    private IEnumerator RotateTowardsTarget(Vector3 targetPosition) {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float rotateSpeed = 720f;
        while (Quaternion.Angle(Unit.transform.rotation, targetRotation) > 0.5f) {
            Unit.transform.rotation = Quaternion.RotateTowards(Unit.transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
}