using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(BallController))]
public class Ball : MonoBehaviour
{
    private Vector2 _displacement;
    public float ballSpeed;

    private BallController _controller;
    // Start is called before the first frame update
    void Start()
    {
        _displacement = Vector2.down * ballSpeed;
        _controller = GetComponent<BallController>();
    }
    
    // Update is called once per frame
    void Update()
    {
        _controller.Move(_displacement*Time.deltaTime);
    }
    
    
}
