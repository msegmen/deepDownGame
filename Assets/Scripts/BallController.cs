using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BallController : MonoBehaviour
{
    public CircleCollider2D objCollider;
    public int rayCount = 4;

    [HideInInspector]
    public float raySpacing;

    public const float SkinWidth = 0.01f;
    public LayerMask collisionMask;

    
    private List<Vector2> _raycastOrigins;

    public void CreateRaycastOrigins()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);
        
        _raycastOrigins = new List<Vector2>();

        for (float i = 0; i < 360f; i += raySpacing)
        {
            _raycastOrigins.Add(new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad)));
        }
    }
    
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);

        Vector2 pos = transform.position;
        
        int j = 0;
        for (float i = 0; i < 360f; i += raySpacing)
        {
            _raycastOrigins[j] =
                pos + 
                new Vector2(Mathf.Cos(i * Mathf.Deg2Rad),
                            Mathf.Sin(i * Mathf.Deg2Rad)) * objCollider.radius;
            j++;
        }
    }
    
    void CalculateRaySpacing()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);

        rayCount = Mathf.Clamp(rayCount, 2, int.MaxValue);

        raySpacing = 360f / rayCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        objCollider = GetComponent<CircleCollider2D>();
        CalculateRaySpacing();
        CreateRaycastOrigins();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        
        Collisions(ref velocity);
        
        transform.Translate(velocity);
    }

    void Collisions(ref Vector3 velocity)
    {
        Vector2 direction = Vector2.right;
        for (int i = 0; i < _raycastOrigins.Count; i++)
        {
            Vector2 pos = transform.position;
            Vector2 rayDirection = _raycastOrigins[i]-new Vector2(pos.x, pos.y);
            Vector2 v = _raycastOrigins[i];
            Debug.DrawLine(v, 2*(v-pos)+pos);
            
            
        }

        /*
        Vector2 reaction = Vector2.zero;
        for (int i = 0; i < size; i++)
        {
            RaycastHit2D hit = results[i];
            Debug.DrawRay(objCollider.transform.position, hit.centroid);
            reaction -= hit.normal * Vector2.Dot(hit.normal, velocity);
        }

        velocity = new Vector3(reaction.x + velocity.x, reaction.y + velocity.y);
        */
    }
}
