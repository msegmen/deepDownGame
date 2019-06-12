using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public GameObject gameObject;
    private Vector2 screenBounds;
    public int numObjects;
    
    // Start is called before the first frame update
    void Start()
    {

        screenBounds =
            Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        
        RaycastHit2D[] results = new RaycastHit2D[1];
        Vector2 position = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y);

        Instantiate(gameObject, position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
