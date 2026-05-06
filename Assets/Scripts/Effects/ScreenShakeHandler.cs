using UnityEngine;

public class ScreenShakeHandler : MonoBehaviour {
    
    private void Start() {
        ShootAction.OnAnyShoot += ShootAction_OnAnyShoot;
        GrenadeProjectile.OnAnyGrenadeThrown += GrenadeProjectile_OnAnyGrenadeThrown;
        MeleeAction.OnAnyMeleeHit += MeleeAction_OnAnyMeleeHit;
    }

    private void OnDestroy() {
        ShootAction.OnAnyShoot -= ShootAction_OnAnyShoot;
        GrenadeProjectile.OnAnyGrenadeThrown -= GrenadeProjectile_OnAnyGrenadeThrown;
        MeleeAction.OnAnyMeleeHit -= MeleeAction_OnAnyMeleeHit;
    }

    private void ShootAction_OnAnyShoot(ShootAction.OnShootEventArgs _) {
        ScreenShake.Instance.Shake(0.5f);
    }

    private void GrenadeProjectile_OnAnyGrenadeThrown() {
        ScreenShake.Instance.Shake(2f);
    }

    private void MeleeAction_OnAnyMeleeHit() {
        ScreenShake.Instance.Shake(1.2f);
    }
}