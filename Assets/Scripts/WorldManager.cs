using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float axisLength;

    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void Update()
    {
        
    }

    private void OnRenderObject()
    {
        /*
        Draw Center Axis Lines
        */
        GL.PushMatrix();
        GL.Begin(GL.LINES);
        lineMaterial.SetPass(0);

        // x-axis red
        GL.Color(Color.red);
        GL.Vertex3(-axisLength / 2, 0, 0);
        GL.Vertex3(axisLength / 2, 0, 0);

        // y-axis green
        GL.Color(Color.green);
        GL.Vertex3(0, -axisLength / 2, 0);
        GL.Vertex3(0, axisLength / 2, 0);

        // z-axis blue
        GL.Color(Color.blue);
        GL.Vertex3(0, 0, -axisLength / 2);
        GL.Vertex3(0, 0, axisLength / 2);

        GL.End();
        GL.PopMatrix();

        /*
        Draw Bounding Box
        */
        float halfSize = config.boundSize / 2;
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        GL.Color(Color.black);

        // front face
        GL.Vertex3(-halfSize, -halfSize, halfSize);
        GL.Vertex3(halfSize, -halfSize, halfSize);

        GL.Vertex3(halfSize, -halfSize, halfSize);
        GL.Vertex3(halfSize, halfSize, halfSize);

        GL.Vertex3(halfSize, halfSize, halfSize);
        GL.Vertex3(-halfSize, halfSize, halfSize);

        GL.Vertex3(-halfSize, halfSize, halfSize);
        GL.Vertex3(-halfSize, -halfSize, halfSize);

        // rear face
        GL.Vertex3(-halfSize, -halfSize, -halfSize);
        GL.Vertex3(halfSize, -halfSize, -halfSize);

        GL.Vertex3(halfSize, -halfSize, -halfSize);
        GL.Vertex3(halfSize, halfSize, -halfSize);

        GL.Vertex3(halfSize, halfSize, -halfSize);
        GL.Vertex3(-halfSize, halfSize, -halfSize);

        GL.Vertex3(-halfSize, halfSize, -halfSize);
        GL.Vertex3(-halfSize, -halfSize, -halfSize);

        // connecting edges
        GL.Vertex3(-halfSize, -halfSize, halfSize);
        GL.Vertex3(-halfSize, -halfSize, -halfSize);

        GL.Vertex3(halfSize, -halfSize, halfSize);
        GL.Vertex3(halfSize, -halfSize, -halfSize);

        GL.Vertex3(halfSize, halfSize, halfSize);
        GL.Vertex3(halfSize, halfSize, -halfSize);

        GL.Vertex3(-halfSize, halfSize, halfSize);
        GL.Vertex3(-halfSize, halfSize, -halfSize);

        GL.End();
        GL.PopMatrix();
    }
}
