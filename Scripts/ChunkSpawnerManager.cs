using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawnerManager : MonoBehaviour
{
    //TODO: Get prew chunks
    [SerializeField] GameObject[] _chunks;
    [SerializeField] private float _chunkLenght = 54f;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _spawnDistance = 50f;
    [Header("Pool Config")]
    [SerializeField] private int _size = 10;

    private List<Queue<GameObject>> _chunksQueueList = new List<Queue<GameObject>>(); //poolList for randomizing
    private Vector3 _spawnPos = new Vector3(0f, 0f, 1237f);
    private void Start()
    {
        PoolChunks();
    }
    private void Update()
    {
        if(Vector3.Distance(_playerTransform.position, _spawnPos) < _spawnDistance)
        {
            SpawnRandomChunk();
        }
    }
    private void PoolChunks()
    {
        for (int i = 0; i < _chunks.Length; i++)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            for (int j = 0; j < _size; j++)
            {
                GameObject newObj = Instantiate(_chunks[i].gameObject);
                newObj.gameObject.SetActive(false);
                newPool.Enqueue(newObj);
            }
            _chunksQueueList.Add(newPool);
        }   
    }
    private void SpawnRandomChunk()
    {
        GameObject newChunk = _chunksQueueList[Random.Range(0, _chunksQueueList.Count)].Dequeue();
        newChunk.transform.position = _spawnPos;
        _spawnPos.z += _chunkLenght;
        newChunk.gameObject.SetActive(true);
    }
}
