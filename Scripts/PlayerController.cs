using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum Side
{
    Left,
    Right,
    Mid,
    MidLeft,
    MidRight,
}
public enum Direction
{
    Left,
    Right,
    None
}
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _rightSidelineX = 4.6f;
    [SerializeField] private float _leftSidelineX = 4.6f;
    [SerializeField] private float _midSidelineX = 2.64f;
    [SerializeField] private float _jumpForce = 10f;
    [Header("Animation Config")]
    [SerializeField] private float _animationFadeTime = 0.1f;
    [Header("Sound")]
    [SerializeField] private AudioClip[] _sounds;

    private InputReader _inputReader;
    private Animator _animator;
    private CharacterController _characterController;
    private Transform _transform;
    private ForceReceiver _forceReceiver;
    private AudioSource _audioSource;

    private readonly int _animDodgeLeftHash = Animator.StringToHash("dodgeLeft");
    private readonly int _animDodgeRightHash = Animator.StringToHash("dodgeRight");
    private readonly int _animRollHash = Animator.StringToHash("roll");
    private readonly int _animJumpHash = Animator.StringToHash("jump2");
    private readonly int _animRunHash = Animator.StringToHash("runSpeed");
    private readonly int _animInAirDodgeRightHash = Animator.StringToHash("airDodgeRight");
    private readonly int _animInAirDodgeLeftHash = Animator.StringToHash("airDodgeLeft");
    private readonly int _animBoolIsGrounded = Animator.StringToHash("isGrounded");
    private readonly int _animCaughtHash = Animator.StringToHash("caught");
    private readonly int _animDeadHash = Animator.StringToHash("death");
    private readonly int _animIntroRunHash = Animator.StringToHash("introRun");

    private Vector3 _colliderCenterOnRoll;
    private Vector3 _colliderCenterOnStart;
    private float _colliderHeightOnStart;

    private Side _currentSide;
    private Direction _currentDirection;

    private Coroutine _sideMovesCoroutine;
    private WaitForSeconds _rollCooldown = new WaitForSeconds(0.4f);
    private WaitForSeconds _bumpCooldown = new WaitForSeconds(1f);

    private bool _isRolling = false;
    private bool _isChangingSide = false;
    private bool _canBump = true;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _inputReader = GetComponent<InputReader>();
        _characterController = GetComponent<CharacterController>();
        _transform = GetComponent<Transform>();
        _forceReceiver = GetComponent<ForceReceiver>();

        _colliderCenterOnStart = _characterController.center;
        _colliderHeightOnStart = _characterController.height;
        _colliderCenterOnRoll = new Vector3(_characterController.center.x, _characterController.center.y / 2, _characterController.center.z);
    }
    private void OnEnable()
    {
        _canBump = true;
        GameManager.Instance.OnIntroStarted += HandleOnIntroStarted;
        _inputReader.OnGoLeftEvent += HandleOnGoLeftEvent;
        _inputReader.OnGoRightEvent += HandleOnGoRightEvent;
        _inputReader.OnRollEvent += HandleOnRollEvent;
        _inputReader.OnJumpEvent += HandleOnJumpEvent;
        _forceReceiver.OnLanded += HandleOnLanded;
        _forceReceiver.OnFall += HandleOnFall;
        _forceReceiver.OnRunSpeedChange += HandleOnRunSpeedChange;
        //_forceReceiver.OnGrounded += HandleOnGrounded;
        GameManager.Instance.OnLifeless += HandleOnGameEnd;
        GameManager.Instance.OnCrashed += HandleOnCrash;
        _currentSide = Side.Mid;
    }
    private void Start()
    {
        _animator.SetBool(_animBoolIsGrounded, true);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnIntroStarted -= HandleOnIntroStarted;
        _inputReader.OnGoLeftEvent -= HandleOnGoLeftEvent;
        _inputReader.OnGoRightEvent -= HandleOnGoRightEvent;
        _inputReader.OnRollEvent -= HandleOnRollEvent;
        _inputReader.OnJumpEvent -= HandleOnJumpEvent;
        _forceReceiver.OnFall -= HandleOnFall;
        _forceReceiver.OnRunSpeedChange -= HandleOnRunSpeedChange;
        //_forceReceiver.OnGrounded -= HandleOnGrounded;
        GameManager.Instance.OnLifeless -= HandleOnGameEnd;
        GameManager.Instance.OnCrashed -= HandleOnCrash;
    }
    private void HandleOnGoLeftEvent()
    {
        if (_currentSide == Side.Left) return;
        GameManager.Instance.PlaySound(0);
        if (_currentDirection != Direction.Left)
        {
            _currentDirection = Direction.Left;
            if(_forceReceiver.IsInAir)
            {
                _animator.CrossFadeInFixedTime(_animInAirDodgeLeftHash, _animationFadeTime + 0.15f);
            }
            else
            {
                _animator.CrossFadeInFixedTime(_animDodgeLeftHash, _animationFadeTime);
            }
        }

        if(_sideMovesCoroutine != null)
            StopCoroutine(_sideMovesCoroutine);
        if (_currentSide == Side.Mid || _currentSide == Side.MidLeft)
        {
            _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_leftSidelineX, Side.Left));
            _currentSide = Side.MidLeft;
        }
        else if (_currentSide == Side.Right || _currentSide == Side.MidRight)
        {
            _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_midSidelineX, Side.Mid));
            _currentSide = Side.MidRight;
        }
    }
    private void HandleOnGoRightEvent()
    {
        if (_currentSide == Side.Right) return;
        GameManager.Instance.PlaySound(0);
        if (_currentDirection != Direction.Right)
        {
            _currentDirection = Direction.Right;

            if (_forceReceiver.IsInAir)
            {
                _animator.CrossFadeInFixedTime(_animInAirDodgeRightHash, _animationFadeTime + 0.05f);
            }
            else
            {
                _animator.CrossFadeInFixedTime(_animDodgeRightHash, _animationFadeTime);
            }
        }

        if (_sideMovesCoroutine != null)
            StopCoroutine(_sideMovesCoroutine);
        if (_currentSide == Side.Mid || _currentSide == Side.MidRight)
        {
            _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_rightSidelineX, Side.Right));
            _currentSide = Side.MidRight;
        }
        else if(_currentSide == Side.Left || _currentSide == Side.MidLeft)
        {
            _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_midSidelineX, Side.Mid));
            _currentSide = Side.MidLeft;
        }
    }
    private void HandleOnRollEvent()
    {
        if(_isRolling) 
            return;
        if (_forceReceiver.IsInAir)
            _forceReceiver.airRoll = true;
        GameManager.Instance.PlaySound(2);
        _animator.CrossFadeInFixedTime(_animRollHash, _animationFadeTime);
        _characterController.center = _colliderCenterOnRoll;
        _characterController.height /= 2f;
        StartCoroutine(RollCooldown());
    }
    private void HandleOnJumpEvent()
    {
        if (!_forceReceiver.IsGrounded) return;
        GameManager.Instance.PlaySound(1);
        _forceReceiver.Jump(_jumpForce);
        _animator.SetBool(_animBoolIsGrounded, false);
        _animator.CrossFadeInFixedTime(_animJumpHash, _animationFadeTime);
    }
    private void HandleOnLanded()
    {
        _animator.SetBool(_animBoolIsGrounded, true);
    }
    private void HandleOnGrounded(float heighty)
    {
        _animator.SetBool(_animBoolIsGrounded, true);
    }
    private void HandleOnFall()
    {
        _animator.SetBool(_animBoolIsGrounded, false);
    }
    private void HandleOnRunSpeedChange(float speed)
    {
        _animator.SetFloat(_animRunHash, speed);
    }
    private void HandleBumping()
    {
        if (_currentSide == Side.MidLeft)
        {
            if (_currentDirection == Direction.Right)
            {
                StopCoroutine(_sideMovesCoroutine);
                _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_leftSidelineX, Side.Left));
            }
            else if (_currentDirection == Direction.Left)
            {
                StopCoroutine(_sideMovesCoroutine);
                _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_midSidelineX, Side.Mid));
            }
        }
        else if (_currentSide == Side.MidRight)
        {
            if (_currentDirection == Direction.Right)
            {
                StopCoroutine(_sideMovesCoroutine);
                _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_midSidelineX, Side.Mid));
            }
            else if (_currentDirection == Direction.Left)
            {
                StopCoroutine(_sideMovesCoroutine);
                _sideMovesCoroutine = StartCoroutine(MoveOnRightAxis(_rightSidelineX, Side.Right));
            }
        }
    }
    private void HandleOnGameEnd()
    {
        _animator.CrossFadeInFixedTime(_animCaughtHash, _animationFadeTime);
    }
    private void HandleOnCrash()
    {
        _animator.CrossFadeInFixedTime(_animDeadHash, _animationFadeTime);
    }
    private void HandleOnIntroStarted()
    {
        StartCoroutine(GetReady(1f));
    }
    private IEnumerator GetReady(float speed)
    {
        yield return new WaitForSeconds(0.4f);
        _animator.CrossFadeInFixedTime(_animIntroRunHash, _animationFadeTime);
        float howFar = 0f;
        do
        {
            howFar += Time.deltaTime * speed;
            if (howFar > 1f)
                howFar = 1f;
            _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.identity, howFar);
            yield return null;
        }
        while (howFar != 1f);
        _transform.position += Vector3.up * 2f;
        StartCoroutine(MoveOnRightAxis(_midSidelineX, Side.Mid, 0.6f));
    }
    private IEnumerator MoveOnRightAxis(float xPosition, Side newSide)
    {
        float distance;
        _isChangingSide = true;
        do
        {
            float newX = Mathf.Lerp(_transform.position.x, xPosition, Mathf.Pow(Time.deltaTime * _dashSpeed, 2f));
            Vector3 target = new Vector3(newX, _transform.position.y, _transform.position.z);
            _characterController.Move(target - _transform.position);
            distance = Mathf.Abs(_transform.position.x - xPosition);
            //allow player to change side before reaching it
            if (distance < 0.7f)
            {
                _isChangingSide = false;
                _currentDirection = Direction.None;
                _currentSide = newSide;
            }
            yield return null;
        }
        while (distance > 0.06f);
        _sideMovesCoroutine = null;
    }
    private IEnumerator MoveOnRightAxis(float xPosition, Side newSide, float speed)
    {
        float howfar = 0f;
        do
        {
            howfar += Time.deltaTime * speed;
            float newX = Mathf.Lerp(_transform.position.x, xPosition, howfar);
            Vector3 target = new Vector3(newX, _transform.position.y, _transform.position.z);
            if (howfar > 1f)
                howfar = 1f;
            _characterController.Move(target - _transform.position);
            yield return null;
        }
        while (howfar != 1f);
        _isChangingSide = false;
        _currentDirection = Direction.None;
        _currentSide = newSide;
    }
    private IEnumerator RollCooldown()
    {
        _isRolling = true;
        yield return _rollCooldown;
        _characterController.height = _colliderHeightOnStart;
        _characterController.center = _colliderCenterOnStart;
        _isRolling = false;
    }
    private IEnumerator BumpCoolDown()
    {
        _canBump = false;
        yield return _bumpCooldown;
        _canBump = true;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_isChangingSide)
        {
            if (hit.normal.y > 0.5f) return;
            if (hit.normal.z < -0.97f) return;
            if (GameManager.Instance.IsGameEnded) return;
            if (!_canBump) return;
            GameManager.Instance.PlaySound(3);
            StartCoroutine(BumpCoolDown());
            if (hit.collider.CompareTag("Obstacle"))
            {
                GameManager.Instance.ChangingSideCrash();
                HandleBumping();
            }
            else if(hit.collider.CompareTag("LightSignal"))
            {
                GameManager.Instance.ChangingSideCrash();
            }
        }
        else
        {
            if (hit.normal.z > -0.97f) return;
            if (hit.collider.CompareTag("Obstacle"))
            {
                GameManager.Instance.EndGame(false);
            }
        }
    }
    //Animation event
    public void PlaySound(int index)
    {
        _audioSource.PlayOneShot(_sounds[index]);
    }

}
