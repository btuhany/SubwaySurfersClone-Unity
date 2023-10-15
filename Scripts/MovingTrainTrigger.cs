using UnityEngine;

public class MovingTrainTrigger : MonoBehaviour
{
    [SerializeField] GameObject[] _gameObjectsToEnable;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            foreach (GameObject item in _gameObjectsToEnable)
            {
                item.SetActive(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            for (int i = 0; i < _gameObjectsToEnable.Length; i++)
            {
                if (_gameObjectsToEnable[i] == other.gameObject)
                    other.gameObject.SetActive(false);
            }
        }
    }
}
