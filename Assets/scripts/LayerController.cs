using UnityEngine;
using UnityEngine.InputSystem;

public class LayerController : MonoBehaviour
{
    //Components
    private CharacterController _controller;

    //Inputs
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private InputAction _jumpAction;

    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private float _smoothTime = 0.2f;
    private float _turnSmoothVelocity;

    //Gravity
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Vector3 _playerGravity;

    //GroundSensor
    [SerializeField] Transform _sensor;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] float _sensorRadius;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
    }

    void Start()
    {

    }

    void Update()
    {
        MovimientoCutre();

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }
        Gravity();
    }

    void MovimientoCutre()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            _controller.Move(direction.normalized * _movementSpeed * Time.deltaTime);   
        }
    }

    void Jump()
    {
        _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);
        _controller.Move(_playerGravity * Time.deltaTime);
    }

    void Gravity()
    {
        if (!IsGrounded())
        {
            _playerGravity.y += _gravity * Time.deltaTime;
        }
        else if (IsGrounded() && _playerGravity.y < 0)
        {
            _playerGravity.y = 0;
        }
         _controller.Move(_playerGravity * Time.deltaTime);
    
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
}
