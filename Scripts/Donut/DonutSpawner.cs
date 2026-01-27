using UnityEngine;
using System.Collections;

public class DonutSpawner : MonoBehaviour
{
    private float spawnInterval = 5f;
    [SerializeField] private GameObject donutPrefab;
    [SerializeField] private MaskManager maskManager;

    private void Start()
    {
        StartCoroutine(SpawnDonutsCoroutine());
    }

    private IEnumerator SpawnDonutsCoroutine()
    {
        while (true)
        {
            SpawnDonut();
            yield return new WaitForSeconds(spawnInterval-0.5f*maskManager.GetMaskState());
        }
    }

    private void SpawnDonut()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-6.5f, 6.5f), 0f, Random.Range(-6.5f, 6.5f));
        GameObject donut = Instantiate(donutPrefab, spawnPosition, Quaternion.identity);
        donut.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
