using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class BallRaycastController : MonoBehaviour
{

    public CircleCollider2D objCollider; 
    public int rayCount = 4;
    
    [HideInInspector] public float raySpacing;
    
    public const float SkinWidth = 0.01f;
    public LayerMask collisionMask;
    public List<Vector2> raycastOrigins;
    
    // Start is called before the first frame update
    void Start()
    {
        objCollider = GetComponent<CircleCollider2D>();
        raycastOrigins = new List<Vector2>();
        CalculateRaySpacing();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void UpdateRaycastOrigins()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);
        
        for (float i = 0; i < 360f; i += raySpacing)
        {
            raycastOrigins.Add(new Vector2(
                Mathf.Cos(i*Mathf.Deg2Rad)*bounds.size.y*0.5f,
                Mathf.Sin(i*Mathf.Deg2Rad)*bounds.size.y*0.5f));
        }
    }
    
    void CalculateRaySpacing()
    {
        Bounds bounds = objCollider.bounds;
        bounds.Expand(SkinWidth*-2);

        rayCount = Mathf.Clamp(rayCount, 2, int.MaxValue);

        raySpacing =  360f / (rayCount - 1);
    }
}
