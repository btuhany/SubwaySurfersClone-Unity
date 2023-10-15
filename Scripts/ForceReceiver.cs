using UnityEngine;

public class ForceReceiver : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheckTransform;
    [SerializeField] private float _checkRadius;
    [SerializeField] private LayerMask _groundMask;

    [Header("Gravity Config")]
    [SerializeField] private float _gravityScaleOnFall = 2.2f;
    [SerializeField] private float _gravityScaleOnAirRoll = 10f;

    private CharacterController _characterController;
    private Vector3 _verticalVelocity = Vector3.zero;

    private readonly float _gravity = Physics.gravity.y;
    public bool IsGrounded => Physics.CheckSphere(_groundCheckTransform.position, _checkRadius, _groundMask);

    private float _runSpeed;
    public bool airRoll = false;
    private bool _isInAir = false;
    private bool _isFalling = false;
    public bool IsInAir { get => _isInAir;}
    public bool IsFalling { get => _isFalling;}

    public event System.Action OnLanded;
    public event System.Action<float> OnGrounded;  //Sends the player height info.
    public event System.Action OnFall;
    public event System.Action<float> OnRunSpeedChange;
    private Transform _transform;
    
    private void Awake()
    {
        _characterController= GetComponent<CharacterController>();
        _transform = GetComponent<Transform>();
    }
    private void OnEnable()
    {
        GameManager.Instance.OnGameEnd += HandleOnGameEnd;
    }
    private void Start()
    {
        ChangeRunSpeed(0);
    }
    private void Update()
    {
        if (airRoll)
        {
            _verticalVelocity.y += _gravity * Time.deltaTime * _gravityScaleOnAirRoll;
        }
        else
        {
            _verticalVelocity.y += _gravity * Time.deltaTime * _gravityScaleOnFall;
        }
        
        if (IsGrounded && _verticalVelocity.y < 0f)
        {
            _verticalVelocity.y = -2f;
        }

        if(IsGrounded) 
        {
            OnGrounded?.Invoke(_transform.position.y);
        }
        else
        {
            _isInAir = true;
            if (_verticalVelocity.y < 0f)
            {
                _isFalling = true;
                OnFall?.Invoke();
            }
        }

        if(IsGrounded && _isInAir && _verticalVelocity.y < 0f)
        {
            _isInAir = false;
            _isFalling = false;
            OnLanded?.Invoke();
        }

        if(IsGrounded && airRoll  && _verticalVelocity.y < 0f)
        {
            airRoll = false;
        }

        Vector3 forwardDir = Vector3.forward * _runSpeed;
        _characterController.Move((_verticalVelocity + forwardDir) * Time.deltaTime);
    }
    private void HandleOnGameEnd()
    {
        ChangeRunSpeed(0f);
    }

    public void Jump(float jumpHeight)
    {
        _verticalVelocity.y = Mathf.Sqrt(jumpHeight * (-2f) * _gravity);
        _isInAir = true;
    }
    public void ChangeRunSpeed(float newSpeed)
    {
        _runSpeed = newSpeed;
        OnRunSpeedChange?.Invoke(newSpeed);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(_groundCheckTransform.position, _checkRadius);
    }
}
