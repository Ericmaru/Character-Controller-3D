using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class FPScontroller : MonoBehaviour
{

    //Components
    private CharacterController _controller;
    
    //Inputs
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private InputAction _lookAction;
    private Vector2 _lookInput;
    private InputAction _jumpAction;
    private InputAction _aimAction;

    //Floats
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private float _smoothTime = 0.2f;
    private float _turnSmoothVelocity;
    [SerializeField] private float _cameraSensivility = 10;
    [SerializeField] Transform _lookAtCamera;
    float _xRotation;
    
    //G
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Vector3 _playerGravity;

    //GS
    [SerializeField] Transform _sensor;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] float _sensorRadius;

    //Camara
    [SerializeField] private InputAxis xAxis;
    [SerializeField] private InputAxis yAxis;


    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _aimAction = InputSystem.actions["Aim"];
        _lookAction = InputSystem.actions["Look"];
    }

    void Start()
    {

    }
    
    void Movement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        float mouseX = _lookInput.x * _cameraSensivility * Time.deltaTime;
        float mouseY = _lookInput.y * _cameraSensivility * Time.deltaTime;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90, 90);
        transform.Rotate(Vector3.up, mouseX);
        _lookAtCamera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        //_lookAtCamera.Rotate(Vector3.right, mouseY);

        if(direcrtion != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            _controller.Move(moveDirection * _movementSpeed * _smoothTime.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }
        Gravity();
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
