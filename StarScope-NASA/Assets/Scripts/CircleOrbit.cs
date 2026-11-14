using UnityEngine;

public class CircleOrbit : MonoBehaviour
{
    public Transform sun;
    public float orbitRadius = 10f;
    public int segments = 200;
    private float angle = 0f;
    public float orbitSpeed = 10f;

    public LineRenderer line;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //LineRenderer line = GetComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;

            float x = orbitRadius * Mathf.Cos(angle);
            float z = orbitRadius * Mathf.Sin(angle);

            line.SetPosition(i, new Vector3(x, 0, z));
        }
    }
    
    

    // Update is called once per frame
    void Update()
    {
        angle += orbitSpeed * Time.deltaTime;

        float radians = angle * Mathf.Deg2Rad;

        float x = sun.position.x +
                  orbitRadius * Mathf.Cos(radians);

        float z = sun.position.z +
                  orbitRadius * Mathf.Sin(radians);

        transform.position = new Vector3(
            x,
            sun.position.y,
            z
        );
    }
}
