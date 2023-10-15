using UnityEngine;

public class MovingTrains : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    private Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + Vector3.back * _speed * Time.fixedDeltaTime);
    }
}
