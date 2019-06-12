using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerRaycastController : MonoBehaviour
{
    [FormerlySerializedAs("_collider")] public BoxCollider2D objCollider;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    
    [HideInInspector]
    public float _horizontalRaySpacing;
    [HideInInspector]
    public float _verticalRaySpacing;
    
    public const float SkinWidth = 0.01f;
    public LayerMask collisionMask;

    
    public RaycastOrigins _raycastOrigins;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        objCollider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }

   
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);
        
        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        _horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    
    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
