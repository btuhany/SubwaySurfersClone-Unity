using UnityEngine;

public class CoinController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.Instance.PlaySound(7);
            ScoreManager.Instance.IncreaseCoin(1);
            gameObject.SetActive(false);
        }
    }
}
