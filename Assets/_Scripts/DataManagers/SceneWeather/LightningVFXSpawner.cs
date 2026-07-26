#region

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

#endregion

public class LightningVFXSpawner : MonoBehaviour
{
    [SerializeField] private List<Transform> _allLightingVFXSpawningPositions;

    private readonly List<Transform> _allSpawnedLightnings = new();

    public void ShowLightning(Transform lightningVFXPrefab, float showTime)
    {
        int lightningSpawningPositionIndex = Random.Range(0, _allLightingVFXSpawningPositions.Count);
        Vector3 lightningSpawningPosition = _allLightingVFXSpawningPositions[lightningSpawningPositionIndex].position;
        SpawnAndDestroyLightningVFX(lightningVFXPrefab, lightningSpawningPosition, showTime).Forget();
    }

    private async UniTaskVoid SpawnAndDestroyLightningVFX(Transform lightningPrefab, Vector3 spawningPosition,
        float lifetime)
    {
        Transform spawnedLightning =
            Instantiate(lightningPrefab, spawningPosition, lightningPrefab.rotation, transform);

        _allSpawnedLightnings.Add(spawnedLightning);

        await UniTask.WaitForSeconds(lifetime);

        _allSpawnedLightnings.Remove(spawnedLightning);

        Destroy(spawnedLightning);
    }

    public void ClearCurrentLightnings()
    {
        foreach (Transform spawnedLightning in _allSpawnedLightnings)
        {
            Destroy(spawnedLightning);
        }

        _allSpawnedLightnings.Clear();
    }
}