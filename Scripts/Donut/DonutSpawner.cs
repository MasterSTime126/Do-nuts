using UnityEngine;
using System.Collections;

public class DonutSpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        Center,     // Happiness - spawn anywhere in play area
        Border      // Sadness, Fear, Anger, Disgust - spawn near borders
    }

    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 5f;
    [SerializeField] private GameObject donutPrefab;
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Center;

    [Header("Play Area")]
    [SerializeField] private float playAreaSize = 6.5f;
    [SerializeField] private float borderSpawnOffset = 0.5f; // How far from the edge to spawn

    [Header("Happiness Mode")]
    [SerializeField] private int happinessDonutLimit = 10;
    private int donutsSpawnedInHappiness = 0;

    private MaskManager maskManager;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        maskManager = MaskManager.Instance;
        UpdateSpawnMode();
        spawnCoroutine = StartCoroutine(SpawnDonutsCoroutine());
    }

    private void UpdateSpawnMode()
    {
        if (maskManager == null) return;

        MaskManager.MaskState state = maskManager.GetMaskState();
        spawnMode = (state == MaskManager.MaskState.Happiness) ? SpawnMode.Center : SpawnMode.Border;
    }

    private IEnumerator SpawnDonutsCoroutine()
    {
        while (true)
        {
            if (maskManager == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            MaskManager.MaskState state = maskManager.GetMaskState();

            // Don't spawn during Fear (mask is already placed) or TheEnd
            if (state == MaskManager.MaskState.Fear || state == MaskManager.MaskState.TheEnd)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Happiness: Limited spawns
            if (state == MaskManager.MaskState.Happiness)
            {
                if (donutsSpawnedInHappiness < happinessDonutLimit)
                {
                    SpawnDonut();
                    donutsSpawnedInHappiness++;
                }
                yield return new WaitForSeconds(baseSpawnInterval);
            }
            else
            {
                // Other modes: spawn faster as levels progress
                SpawnDonut();
                float interval = Mathf.Max(1f, baseSpawnInterval - 0.5f * (int)state);
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private void SpawnDonut()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject donut = Instantiate(donutPrefab, spawnPosition, Quaternion.identity);
        donut.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private Vector3 GetSpawnPosition()
    {
        switch (spawnMode)
        {
            case SpawnMode.Center:
                return GetRandomCenterPosition();

            case SpawnMode.Border:
                return GetRandomBorderPosition();

            default:
                return Vector3.zero;
        }
    }

    private Vector3 GetRandomCenterPosition()
    {
        return new Vector3(
            Random.Range(-playAreaSize, playAreaSize),
            0f,
            Random.Range(-playAreaSize, playAreaSize)
        );
    }

    private Vector3 GetRandomBorderPosition()
    {
        // Choose a random side (0=left, 1=right, 2=top, 3=bottom)
        int side = Random.Range(0, 4);
        float posAlongEdge = Random.Range(-playAreaSize, playAreaSize);
        float borderPos = playAreaSize - borderSpawnOffset;

        switch (side)
        {
            case 0: // Left
                return new Vector3(-borderPos, 0f, posAlongEdge);
            case 1: // Right
                return new Vector3(borderPos, 0f, posAlongEdge);
            case 2: // Top
                return new Vector3(posAlongEdge, 0f, borderPos);
            case 3: // Bottom
                return new Vector3(posAlongEdge, 0f, -borderPos);
            default:
                return Vector3.zero;
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void ResumeSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnDonutsCoroutine());
        }
    }
}
