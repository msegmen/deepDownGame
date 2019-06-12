using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;
    
    private float moveSpeed = 6; 
    private float _gravity = -20;
    private float _jumpVelocity = 8;
    private Vector3 _velocity;
    private float accelerationTimeAirborne = .2f;
    private float accelerationTimeGrounded = .1f;
    private float _velocityXSmoothing;
    
    private Controller2D _controller;
    
    void Start()
    { 
        _controller = GetComponent<Controller2D>();

        _gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity * timeToJumpApex);
    }
    
    void Update()
    {
        if (_controller.collisions.above || _controller.collisions.below)
        {
            _velocity.y = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && _controller.collisions.below)
        {
            _velocity.y = _jumpVelocity;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _velocity.y += _gravity * Time.deltaTime;
        float targetVelocity = input.x * moveSpeed;
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocity, ref _velocityXSmoothing, 
            _controller.collisions.below?accelerationTimeGrounded:accelerationTimeAirborne);
        
        // velocity refers to the movement we want to perform
        _controller.Move(_velocity*Time.deltaTime);
    }
}
