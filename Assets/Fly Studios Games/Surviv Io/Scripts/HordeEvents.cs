using UnityEngine;
using System;

public class HordeEvents : MonoBehaviour
{
    public static HordeEvents Instance;
    public event Action<Vector3> OnGunshot;
    public event Action<Vector3> OnZombieKilled;

    void Awake() => Instance = this;

    public void EmitGunshot(Vector3 pos)
    {
        OnGunshot?.Invoke(pos);
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.NotifyGunshot(pos);
    }

    public void EmitZombieKilled(Vector3 pos)
    {
        OnZombieKilled?.Invoke(pos);
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.NotifyGunshot(pos); // reuse surge or create dedicated
    }
}
