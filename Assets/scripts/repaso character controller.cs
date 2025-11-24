using UnityEngine;
using UnityEngine.InputSystem;

public class repasocharactercontroller : MonoBehaviour
{
    //instalar cinemachine: package manager, window, unity registry, cinemachine, targeted Cameras, free look camera//
    //como editar la camera: tracking target (personaje o cubo de dentro) orbital follow: orbit style (top, center, bottom) target offset Y, horizontal axis//
    //poner animaciones de mixamo, idle, walk y correr palante patras y laos//
    //loop time//
    //blend tree pa las de andar y ilde y jump separado//
    //bool isjumping//
    //animator y Character controller y el script en el personaje//
    Animator _animator;
    CharacterController _controller;

    InputAction _moveAction;
    InputAction _jumpAction;
    Vector2 _moveValue;

    [SerializeField] float _movementSpeed = 5;
    [SerializeField] float _jumpHeight = 2;
    [SerializeField] float _gravity = -9.8f;
    [SerializeField] Transform _sensorPosition;
    [SerializeField] float _sensorRadius;
    [SerializeField] LayerMask _groundLayer;
    Vector3 playerGravity;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _jumpAction = InputSystem.actions["Jump"];
        _moveAction = InputSystem.actions["Move"];
    }

    void Update()
    {
        _moveValue = _moveAction.ReadValue<Vector2>();

        if(_jumpAction.WasPerformedThisFrame() && IsGrounded())
        {
            Jump();
        }
        Movement();
        Gravity();
    }

    void Movement()
    {
        Vector3 moveDirection = new Vector3(_moveValue.x, 0, _moveValue.y);
        _controller.Move(moveDirection * _movementSpeed * Time.deltaTime);
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensorPosition.position, _sensorRadius, _groundLayer);
    }

    void Jump()
    {
        playerGravity.y = Mathf.Sqrt(_jumpHeight * _gravity * -2);
        _controller.Move(playerGravity * Time.deltaTime);
    }
    
    void Gravity()
    {
        if(!IsGrounded())
        {
            playerGravity.y += _gravity * Time.deltaTime;
        }
        else if(IsGrounded() && playerGravity.y < -1)
        {
            playerGravity.y = _gravity;
        }
        _controller.Move(playerGravity * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_sensorPosition.position, _sensorRadius);
    }

}