using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    private BoxCollider2D _collider;
    public LayerMask collisionMask;

    private const float MoveThreshold = 0.01f;
    private const float MaxClimbAngle = 72f;
    private const float SkinWidth = 0.01f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;
    
    private RaycastOrigins _raycastOrigins;

    public CollisionInfo collisions;
    void Start()
    {
        _collider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();

    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        // we check if any collisions occur, if so, we constrain movement
        if (Math.Abs(velocity.x)>= MoveThreshold)
        {
            HorizontalCollisions(ref velocity);
        }

        if (Math.Abs(velocity.y) >= MoveThreshold)
        {
            VerticalCollisions(ref velocity);
        }
        
        // in the end, we perform the possibly modified movement
        transform.Translate(velocity);
    }

    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SkinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (int) directionX == -1 ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            // since we are checking horizontal collisions first, we do not need to include the velocity.y here
            rayOrigin += (i * _horizontalRaySpacing) * Vector2.up; 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, directionX * Vector2.right, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, directionX*Vector2.right, Color.cyan);

            if (!hit) continue;

            float slopeAngle = Vector2.Angle(Vector2.up, hit.normal);
            
            // for the first ray, within the climbable range
            if (i == 0 && slopeAngle < MaxClimbAngle)
            {
                float distanceToSlopeStart = 0;
                // if a new slope is encountered
                if (slopeAngle != collisions.slopeAngleOld)
                {
                    // adjust distance to the slope
                    distanceToSlopeStart = hit.distance - SkinWidth;
                    velocity.x -= distanceToSlopeStart * directionX;
                }
                
                ClimbSlope(ref velocity, slopeAngle);
                // for the upcoming calculations, we get the previous vel. back
                velocity.x += distanceToSlopeStart * directionX;
            }
            
            if (!collisions.climbingSlope || slopeAngle > MaxClimbAngle)
            {
                // perform movement right where we collide 
                velocity.x = (hit.distance - SkinWidth) * directionX;
                rayLength = hit.distance;

                // we have a wall on the slope
                if (collisions.climbingSlope)
                {
                    // movement in the y axis is constrained by the slope 
                    velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad)*Mathf.Abs(velocity.x);
                }

                collisions.left = directionX == -1;
                collisions.right = directionX == 1;   
            }
        }
    }

    public void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveX = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveX;

        if (climbVelocityY >= velocity.y)
        {
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveX * Mathf.Sign(velocity.x);
            velocity.y = climbVelocityY;
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SkinWidth;
        
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (int)directionY== -1 ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            rayOrigin += (i * _verticalRaySpacing + velocity.x) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, directionY *Vector2.up, rayLength, collisionMask);
            
            Debug.DrawRay(_raycastOrigins.bottomLeft + i*_verticalRaySpacing*Vector2.right, Vector2.up*-1, Color.red);

            if (!hit) continue;
            
            velocity.y = (hit.distance-SkinWidth) * directionY;
            rayLength = hit.distance;
            collisions.above = directionY == 1;
            collisions.below = directionY == -1;
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(SkinWidth*-2);
        
        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(SkinWidth*-2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public float slopeAngle, slopeAngleOld;
        
        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
