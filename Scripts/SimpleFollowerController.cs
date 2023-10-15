using UnityEngine;

public class SimpleFollowerController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private bool _followAtZ;
    [SerializeField] private bool _followAtY;
    [SerializeField] private bool _followAtX;
    [SerializeField] private bool _setLimitY = false;
    [SerializeField] private float _yLimit = 5f;
    private Vector3 _distance;
    private Transform _transform;
    private bool _isAboveLimit = false;
    private bool _isFollow;
    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    private void OnEnable()
    {
        GameManager.Instance.OnGameStarted += HandleOnGameStart;
    }
    private void Update()
    {
        if (!_isFollow) return;
        Vector3 newPos = _transform.position;
        if (_followAtZ)
        {
            newPos.z = _target.transform.position.z + _distance.z;
        }

        if (_followAtY)
        {
            if(_setLimitY)
            {
                if (_target.position.y > _yLimit)
                {
                    newPos.y = _target.transform.position.y + _distance.y;
                    _isAboveLimit = true;
                }
                else if(_isAboveLimit)
                {
                    _isAboveLimit= false;
                    newPos.y = _target.transform.position.y - _distance.y;
                }
            }
            else
            {
                newPos.y = _target.transform.position.y + _distance.y;
            }
        }

        if (_followAtX)
        {
            newPos.x = _target.transform.position.x + _distance.x;
        }
        _transform.position = newPos;
    }
    private void HandleOnGameStart()
    {
        _distance = transform.position - _target.transform.position;
        _transform.SetParent(null);
        _isFollow = true;
    }
}
