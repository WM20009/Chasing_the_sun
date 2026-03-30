using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; } // 新增单例

    [SerializeField] private Transform defaultSpawnPoint;
    [SerializeField] private FadeOverlay fadeOverlay;
    [SerializeField] private float fadeDuration = 0.18f;
    [SerializeField] private float blackScreenHoldDuration = 0.12f;

    private readonly List<RespawnableDynamic> _dynamics = new List<RespawnableDynamic>();

    private PlayerController2D _player;
    private CampfireCheckpoint _activeCheckpoint;
    private bool _isRespawning;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlayer(PlayerController2D player) 
    {
        _player = player;
    }

    public void RegisterDynamic(RespawnableDynamic dynamicObject)
    {
        if (dynamicObject == null || _dynamics.Contains(dynamicObject)) return;
        _dynamics.Add(dynamicObject);
    }

    public void UnregisterDynamic(RespawnableDynamic dynamicObject)
    {
        if (dynamicObject == null) return;
        _dynamics.Remove(dynamicObject);
    }

    public void SetCheckpoint(CampfireCheckpoint checkpoint)
    {
        _activeCheckpoint = checkpoint;
    }

    public void KillPlayer(PlayerController2D player)
    {
        if (_isRespawning) return;

        var targetPlayer = player != null ? player : _player;
        if (targetPlayer == null) return;

        StartCoroutine(RespawnRoutine(targetPlayer));
    }

    private IEnumerator RespawnRoutine(PlayerController2D player)
    {
        _isRespawning = true;
        player.HandleDeathStarted();

        if (fadeOverlay != null)
        {
            // 防卡死：避免 FadeOverlay 脚本抛错导致无法重生
            var fadeCor = fadeOverlay.FadeTo(1f, fadeDuration);
            if (fadeCor != null) yield return fadeCor;
        }

        if (blackScreenHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(blackScreenHoldDuration);
        }

        for (var i = 0; i < _dynamics.Count; i++)
        {
            if (_dynamics[i] != null)
            {
                _dynamics[i].RestoreRespawnState();
            }
        }

        player.RespawnAt(ResolveRespawnPosition(player));

        if (fadeOverlay != null)
        {
            var fadeCor = fadeOverlay.FadeTo(0f, fadeDuration);
            if (fadeCor != null) yield return fadeCor;
        }

        player.HandleDeathFinished();
        _isRespawning = false;
    }

    private Vector3 ResolveRespawnPosition(PlayerController2D player)
    {
        if (_activeCheckpoint != null)
        {
            return _activeCheckpoint.GetSpawnPosition();
        }

        if (defaultSpawnPoint != null)
        {
            return defaultSpawnPoint.position;
        }

        return player != null ? player.InitialSpawnPosition : Vector3.zero;
    }
}
