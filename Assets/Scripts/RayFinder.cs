using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ray
{
    public Vector3 start;
    public Vector3 end;

    public Ray(Vector3 startSet, Vector2 endSet) 
    {
        start = startSet;
        end = endSet;
    }
}

public class RayFinder : MonoBehaviour
{
    public Ray Reflection(Ray ray)
    {
        // Use raycast hit normal to caculate the reflected ray
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
