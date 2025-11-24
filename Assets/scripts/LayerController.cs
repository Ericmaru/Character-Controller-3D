using System.Data.Common;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    private InputAction _lookAction;
    private Vector2 _lookInput;
    private InputAction _aimAction;
    private InputAction _grabAction;
    private InputAction _throwAction;

    //Floats
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private float _smoothTime = 0.2f;
    private float _turnSmoothVelocity;
    [SerializeField] private float _pushForce = 10;
    [SerializeField] private float _throwForce = 50;

    //Gravity
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Vector3 _playerGravity;

    //GroundSensor
    [SerializeField] Transform _sensor;
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] float _sensorRadius;

    public Transform _mainCamera;

    [SerializeField] private Transform _hands;
    [SerializeField] private Transform _grabedObject;
    [SerializeField] private Vector3 _handSensorSize;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _lookAction = InputSystem.actions["Look"];
        _aimAction = InputSystem.actions["Aim"];
        _mainCamera = Camera.main.transform;
        _grabAction = InputSystem.actions["Interact"];
        _throwAction = InputSystem.actions["Throw"];
    }

    void Start()
    {

    }

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        if (_aimAction.IsInProgress())
        {
            AimMovement();
        }
        else
        {
            Movement();
        }
        //MovimientoCutre();
        //Movimiento2();

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }
        Gravity();

        if (_aimAction.WasPerformedThisFrame())
        {
            Attack();
        }

        if (_grabAction.WasPerformedThisFrame())
        {
            GrabObject();
        }

        if (_throwAction.WasPerformedThisFrame())
        {
            Throw();
        }

        RayTest();
    }

    void Movement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            _controller.Move(moveDirection.normalized * _movementSpeed * Time.deltaTime);
        }
    }

    void AimMovement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _mainCamera.eulerAngles.y, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            _controller.Move(moveDirection.normalized * _movementSpeed * Time.deltaTime);
        }
    }

    void Movimiento2()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);
        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 playerForward = hit.point - transform.position;
            Debug.Log(hit.transform.name);
            playerForward.y = 0;
            transform.forward = playerForward;
        }

        if (direction != Vector3.zero)
        {
            _controller.Move(direction.normalized * _movementSpeed * Time.deltaTime);
        }
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
        //return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);

        RaycastHit hit;
        if (Physics.Raycast(_sensor.position, -transform.up, out hit, _sensorRadius, _groundLayer))
        {
            Debug.DrawRay(_sensor.position, -transform.up * _sensorRadius, Color.red);
            return true;
        }
        else
        {
            Debug.DrawRay(_sensor.position, -transform.up * _sensorRadius, Color.green);
            return false; 
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_hands.position, _handSensorSize);
    }

    void Attack()
    {
        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            if (damageable != null)
            {
                //damageable.TakeDamage();
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.gameObject.tag == "empujable")
        {
            Rigidbody rBody = hit.collider.attachedRigidbody;
            //Rigidbody rBody = hit.transform.GetComponent<Rigidbody>();

            if (rBody == null || rBody.isKinematic)
            {
                return;
            }

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            rBody.linearVelocity = pushDirection * _pushForce / rBody.mass;
        }
    }

    void GrabObject()
    {
        if (_grabedObject == null)
        {
            Collider[] objectsToGrab = Physics.OverlapBox(_hands.position, _handSensorSize);

            foreach (Collider item in objectsToGrab)
            {
                IGrabeable grabeableObject = item.GetComponent<IGrabeable>();

                if (grabeableObject != null)
                {
                    _grabedObject = item.transform;
                    _grabedObject.SetParent(_hands);
                    _grabedObject.position = _hands.position;
                    _grabedObject.rotation = _hands.rotation;
                    _grabedObject.GetComponent<Rigidbody>().isKinematic = true;

                    return;
                }
            }
        }
        else
        {
            _grabedObject.SetParent(null);
            _grabedObject.GetComponent<Rigidbody>().isKinematic = false;
            _grabedObject = null;

        }
    }

    void Throw()
    {
        if (_grabedObject == null)
        {
            Rigidbody grabedBody = _grabedObject.GetComponent<Rigidbody>();

            _grabedObject.SetParent(null);
            grabedBody.isKinematic = false;
            grabedBody.AddForce(_mainCamera.transform.forward * _throwForce, ForceMode.Impulse);
            _grabedObject = null;
        }
    }

    void RayTest()
    {
        //raycast simple
        if (Physics.Raycast(transform.position, transform.forward, 5))
        {
            Debug.Log("hit");
            Debug.DrawRay(transform.position, transform.forward * 5, Color.red);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        }

        //raycast avanzado
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 5))
        {
            Debug.Log(hit.transform.name);
            Debug.Log(hit.transform.position);
            Debug.Log(hit.transform.gameObject.layer);
            Debug.Log(hit.transform.tag);

            /*if(hit.transform.tag == "empujable")
            {
                Box box = hit.transform.GetComponent<Box>();
                if(box != null)
                {
                    Debug.Log("cosas");
                }
            }*/

            IDamageable damageable = hit.transform.GetComponent<IDamageable>();

            if (damageable != null)
            {
                //damageable.TakeDamage(5);
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
    }
}   
