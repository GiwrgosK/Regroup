using UnityEngine;
using System.Collections;

public class UnitAnimator : MonoBehaviour {
	[Header("Unit Animator Configuration")]
	[SerializeField] private Animator unitAnimator;

	[Header("Unit Animator Transforms")]
	[SerializeField] private Transform bulletProjectilePrefab;
	[SerializeField] private Transform shootPointTransform;
	[SerializeField] private Transform rifleTransform;
	[SerializeField] private Transform knifeTransform;

	private MoveAction moveAction;
    private ShootAction shootAction;
    private MeleeAction meleeAction;
    private GrenadeAction grenadeAction;
	private SuppressAction suppressAction;

	private void Awake() {
		TryGetComponent(out moveAction);
		TryGetComponent(out shootAction);
		TryGetComponent(out meleeAction);
		TryGetComponent(out grenadeAction);
		TryGetComponent(out suppressAction);
	}

	private void Start() {
		if (moveAction != null) {
			moveAction.OnStartMoving += MoveAction_OnStartMoving;
			moveAction.OnStopMoving += MoveAction_OnStopMoving;
			moveAction.OnGotBehindCover += MoveAction_OnGotBehindCover;
		}

		if (shootAction != null) {
			shootAction.OnShoot += ShootAction_OnShoot;
		}

		if (meleeAction != null) {
			meleeAction.OnMeleeActionStart += MeleeAction_OnMeleeActionStart;
			meleeAction.OnMeleeActionEnd += MeleeAction_OnMeleeActionEnd;
		}

		if (grenadeAction != null) {
			grenadeAction.OnGrenadeThrown += GrenadeAction_OnGrenadeThrown;
		}

		if (suppressAction != null) {
			suppressAction.OnSuppressing += SuppressAction_OnSuppressing;
		}

		StartCoroutine(EnableRifle(0f));
	}

	private void OnDestroy() {
		if (moveAction != null) {
			moveAction.OnStartMoving -= MoveAction_OnStartMoving;
			moveAction.OnStopMoving -= MoveAction_OnStopMoving;
			moveAction.OnGotBehindCover -= MoveAction_OnGotBehindCover;
		}

		if (shootAction != null) {
			shootAction.OnShoot -= ShootAction_OnShoot;
		}

		if (meleeAction != null) {
			meleeAction.OnMeleeActionStart -= MeleeAction_OnMeleeActionStart;
			meleeAction.OnMeleeActionEnd -= MeleeAction_OnMeleeActionEnd;
		}

		if (grenadeAction != null) {
			grenadeAction.OnGrenadeThrown -= GrenadeAction_OnGrenadeThrown;
		}

		if (suppressAction != null) {
			suppressAction.OnSuppressing -= SuppressAction_OnSuppressing;
		}
	}

	private void EquipKnife() {
		rifleTransform.gameObject.SetActive(false);
		knifeTransform.gameObject.SetActive(true);
	}

	private void MoveAction_OnStartMoving() {
		unitAnimator.SetBool("InCover", false);
		unitAnimator.SetBool("IsWalking", true);
	}
	
	private void MoveAction_OnStopMoving() {
		unitAnimator.SetBool("IsWalking", false);
	}

	private void MoveAction_OnGotBehindCover() {
		unitAnimator.SetBool("InCover", true);
		unitAnimator.SetTrigger("TakeCover");
	}
	
	private void ShootAction_OnShoot(ShootAction.OnShootEventArgs e) {
		if (unitAnimator.GetBool("InCover")) {
			unitAnimator.SetTrigger("ShootFromCover");
			unitAnimator.SetTrigger("TakeCover");
		} else {
			unitAnimator.SetTrigger("Shoot");
		}

		if (e.shooter.FiresBurst) {
			StartCoroutine(FireBurst(e));
		} else if (e.shooter.FiresShotgun) {
			FireShotgun(e);
		} else {
			FireSingle(e);
		}
	}

	private void FireSingle(ShootAction.OnShootEventArgs e) {
		Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
		BulletProjectile bullet = bulletProjectileTransform.GetComponent<BulletProjectile>();
		Vector3 targetWorldPosition = e.target.GetWorldPosition();

		if (e.hit) {
			targetWorldPosition.y = shootPointTransform.position.y;
			bullet.Setup(targetWorldPosition);
		} else {
			targetWorldPosition += new Vector3(0, 1.5f, 0);

			Vector3 missOffset = new Vector3(
				Random.Range(-1f, 1f),
				Random.Range(-0.8f, 1.2f),
				Random.Range(-0.8f, 0.8f)
			);

			Vector3 missTargetPosition = targetWorldPosition + missOffset;
			bullet.Setup(missTargetPosition);
		}
	}

	private void FireShotgun(ShootAction.OnShootEventArgs e) {
		Vector3 targetWorldPosition = e.target.GetWorldPosition();

		if (e.hit) {
			targetWorldPosition.y = shootPointTransform.position.y;
		} else {
			targetWorldPosition += new Vector3(0, 1.5f, 0);
			targetWorldPosition += new Vector3(
				Random.Range(-1f, 1f),
				Random.Range(-0.8f, 1.2f),
				Random.Range(-0.8f, 0.8f)
			);
		}

		for (int i = 0; i < 5; i++) {
			Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
			BulletProjectile bullet = bulletProjectileTransform.GetComponent<BulletProjectile>();

			Vector3 pelletSpreadOffset = new Vector3(
				Random.Range(-1.5f, 1.5f),
				Random.Range(-1.5f, 1.5f) * 0.5f,
				Random.Range(-1.5f, 1.5f)
			);

			bullet.Setup(targetWorldPosition + pelletSpreadOffset);
		}
	}

	private void MeleeAction_OnMeleeActionStart() {
		EquipKnife();
		if (unitAnimator.GetBool("InCover")) {
			unitAnimator.SetTrigger("MeleeFromCover");
			unitAnimator.SetTrigger("TakeCover");
		} else {
			unitAnimator.SetTrigger("Melee");
		}
	}

	private void MeleeAction_OnMeleeActionEnd() {
		knifeTransform.gameObject.SetActive(false);
		StartCoroutine(EnableRifle(1f));
	}

	private void GrenadeAction_OnGrenadeThrown() {
		if (unitAnimator.GetBool("InCover")) {
			unitAnimator.SetTrigger("GrenadeFromCover");
		} else {
			unitAnimator.SetTrigger("Grenade");
		}
		rifleTransform.gameObject.SetActive(false);
		StartCoroutine(EnableRifle(2.7f));
	}

	private void SuppressAction_OnSuppressing(Unit targetUnit) {
		if (unitAnimator.GetBool("InCover")) {
            unitAnimator.SetTrigger("ShootFromCover");
            unitAnimator.SetTrigger("TakeCover");
        } else {
            unitAnimator.SetTrigger("Shoot");
        }
        Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
        BulletProjectile bullet = bulletProjectileTransform.GetComponent<BulletProjectile>();
        Vector3 targetPosition = targetUnit.GetWorldPosition();
        targetPosition.y = shootPointTransform.position.y;
        bullet.Setup(targetPosition);
	}

	private IEnumerator EnableRifle(float delay) {
		yield return new WaitForSeconds(delay);
		rifleTransform.gameObject.SetActive(true);
		if (unitAnimator.GetBool("InCover")) {
			unitAnimator.SetTrigger("TakeCover");
		}
	}

	private IEnumerator FireBurst(ShootAction.OnShootEventArgs e) {
		int bulletCount = 3;
		float fireRateDelay = 0.1f;
		float recoilJitter = 0.4f;
		Vector3 targetPosition = e.target.GetWorldPosition();

		for (int i = 0; i < bulletCount; i++) {
			Transform bulletProjectileTransform = Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);
			BulletProjectile bullet = bulletProjectileTransform.GetComponent<BulletProjectile>();
			Vector3 jitter = new Vector3(
				Random.Range(-recoilJitter, recoilJitter),
				Random.Range(-recoilJitter, recoilJitter),
				Random.Range(-recoilJitter, recoilJitter)
			);

			if (e.hit) {
				targetPosition.y = shootPointTransform.position.y;
				bullet.Setup(targetPosition + jitter);
			} else {
				targetPosition += new Vector3(0, 1.5f, 0);
				Vector3 missOffset = new Vector3(
					Random.Range(-1f, 1f),
					Random.Range(-0.8f, 1.2f),
					Random.Range(-0.8f, 0.8f)
				);
				bullet.Setup(targetPosition + missOffset + jitter);
			}
			yield return new WaitForSeconds(fireRateDelay);
		}
	}
}