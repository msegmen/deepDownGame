using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Controller2D : PlayerRaycastController
{
    private const float MaxClimbAngle = 72f;
    private const float MaxDescendAngle = 75f;
    
    public CollisionInfo collisions;
    public override void Start()
    {
        base.Start();
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;
        
        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        // we check if any collisions occur, if so, we constrain movement
        if (velocity.x != 0) {
            HorizontalCollisions (ref velocity);
        }
        if (velocity.y != 0) {
            VerticalCollisions (ref velocity);
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
                // handles v shaped slopes
                if (collisions.descendingSlope)
                {
                    collisions.descendingSlope = false;
                    velocity = collisions.velocityOld;
                }

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

    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
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

    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = directionX == -1 ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= MaxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
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

        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SkinWidth;
            Vector2 rayOrigin = 
                ((directionX == -1) ? 
                    _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - SkinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }

        }
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public Vector3 velocityOld;
        public float slopeAngle, slopeAngleOld;
        
        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
