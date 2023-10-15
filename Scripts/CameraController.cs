using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Config")]
    [SerializeField] private float _followLerpSpeed;
    [SerializeField] private float _lookLerpSpeed;
    [Range(1, 10)] [SerializeField] private float _xOffsetDamping = 3f;
    [SerializeField] private float _cameraChangeLimit = 0.2f;
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private Transform _cameraPosAtIntro;
    [SerializeField] private Transform _cameraPosAtFollow;
    [Header("Game Config")]
    [SerializeField] private float _trainHeight = 2.5f;
    [Header("Required Components")]
    [SerializeField] Transform _playerTransform;
    [SerializeField] ForceReceiver _playerForceReceiver;

    private Transform _transform;

    private Vector3 _offset;
    private float _cameraHeight;
    private float _startHeight;
    private bool _isFollow = false;
    private Vector3 _cameraLocalPosAtAwake;
    private Quaternion _cameraLocalRotationAtAwake;
    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _cameraLocalPosAtAwake = _transform.localPosition;
        _cameraLocalRotationAtAwake = _transform.localRotation;
    }
    private void Start()
    {
        _transform.position = _cameraPosAtIntro.position;
        _transform.rotation = _cameraPosAtIntro.rotation;
        _transform.SetParent(null);
    }
    private void OnEnable()
    {
        _playerForceReceiver.OnGrounded += HandleOnGrounded;
        GameManager.Instance.OnIntroStarted += HandleOnIntroStarted;
    }
    private void OnDisable()
    {
        _playerForceReceiver.OnGrounded -= HandleOnGrounded;
        GameManager.Instance.OnIntroStarted -= HandleOnIntroStarted;
    }
    private void LateUpdate()
    {
        if (!_isFollow) return;
        if (_playerForceReceiver.IsFalling)
        {
            if ((_playerTransform.position.y <= _trainHeight) && _cameraHeight > _startHeight)
            {
                _cameraHeight = _startHeight;
            }
        }
        Vector3 newPos = new Vector3((_playerTransform.position.x / _xOffsetDamping) + _offset.x, _cameraHeight, _playerTransform.position.z + _offset.z);
        _transform.position = Vector3.Lerp(_transform.position, newPos, _followLerpSpeed * Time.fixedDeltaTime);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_lookAtTarget.transform.position - _transform.position, Vector3.up), _lookLerpSpeed * Time.deltaTime);
    }
    private void HandleOnGrounded(float playerHeight)
    {
        if (Mathf.Abs(playerHeight - _cameraHeight) < _cameraChangeLimit) return;
        float before = _cameraHeight;
        _cameraHeight = playerHeight + _offset.y;
    }
    private void HandleOnIntroStarted()
    {
        StartCoroutine(MoveToFollowPos());
        //_transform.SetParent(_playerTransform);
        //_transform.localPosition = _cameraLocalPosAtAwake;
        //_transform.localRotation =  _cameraLocalRotationAtAwake;
        //_transform.SetParent(null);
        //_offset = _transform.position - _playerTransform.position;
        //_cameraHeight = _startHeight = _playerTransform.position.y + _offset.y;
        //_isFollow = true;
    }
    private IEnumerator MoveToFollowPos()
    {
        yield return new WaitForSeconds(1f);
        _transform.SetParent(null);
        float howFar = 0f;
        do
        {
            howFar += Time.deltaTime * 2f;
            if (howFar > 1f)
                howFar = 1f;
            _transform.position = Vector3.Lerp(_transform.position, _cameraPosAtFollow.position, howFar);
            _transform.rotation = Quaternion.Lerp(_transform.rotation, _cameraPosAtFollow.rotation, howFar);
            yield return null;
        }
        while (howFar != 1f);
        _offset = _transform.position - _playerTransform.position;
        _cameraHeight = _startHeight = _playerTransform.position.y + _offset.y;
        _isFollow = true;
    }
}
