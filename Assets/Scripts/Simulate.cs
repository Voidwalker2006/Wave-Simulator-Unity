using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Simulate : MonoBehaviour
{
    [SerializeField]
    private int vertexCount = 100; // number of vertices per row/column (default 100)
    public WaveSimulation waveSimulation;
    public Material material_force;
    [SerializeField]
    private float scale_amplitude = 1f; // scale factor for wave height (default 1)
    [SerializeField]
    private int size = 10; // wave simulation grid size (default 10)
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;
    private float minValue;
    private float maxValue;
    private float span;

    void Start()
    {
        CreateGrid();
        BuildMesh();
        GetComponent<MeshRenderer>().material = material_force; // drag your material asset into a public variable for this

        // Compute normalization values
        if (waveSimulation != null && waveSimulation.current != null)
        {
            int width = waveSimulation.current.GetLength(0);
            int height = waveSimulation.current.GetLength(1);

            minValue = float.MaxValue;
            maxValue = float.MinValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = waveSimulation.current[x, y];
                    if (v < minValue) minValue = v;
                    if (v > maxValue) maxValue = v;
                }
            }

            span = maxValue - minValue;
            if (span <= Mathf.Epsilon)
            {
                span = 1f; // avoid division by zero
            }
        }
        else
        {
            minValue = 0;
            maxValue = 1;
            span = 1;
        }
        
    }

    private void CreateGrid()
    {
        if (vertexCount > 250)
        {
            Debug.LogWarning($"Vertex count value {vertexCount} is too large; clamping to 250 to avoid mesh overflow.");
            vertexCount = 250;
        }

        int count = Mathf.Clamp(vertexCount, 1, 250);
        vertices = new Vector3[count * count];

        for (int y = 0; y < count; y++)
        {
            for (int x = 0; x < count; x++)
            {
                int index = y * count + x;
                vertices[index] = new Vector3(x, 0f, y);
            }
        }

        triangles = new int[(count - 1) * (count - 1) * 6];
        colors = new Color[vertices.Length];

        int triIndex = 0;
        for (int y = 0; y < count - 1; y++)
        {
            for (int x = 0; x < count - 1; x++)
            {
                int v = y * count + x;

                // Build two triangles from each grid cell (smallest square on x-z plane)
                // using shared vertices so height updates remain smooth.
                // Clockwise winding order for each face (Unity front faces use clockwise by default).
                triangles[triIndex++] = v;
                triangles[triIndex++] = v + count + 1;
                triangles[triIndex++] = v + 1;

                triangles[triIndex++] = v;
                triangles[triIndex++] = v + count;
                triangles[triIndex++] = v + count + 1;
            }
        }
    }

    private void BuildMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh {name = "ProceduralGrid"};
            mesh.MarkDynamic(); 
        }    
        else
            mesh.Clear();

        // // Use 32-bit indices for large grids (more than 65,535 vertices)
        // int count = Mathf.Max(1, vertexCount);
        // if (count * count > 65535)
        // {
        //     mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        // }
        // else
        // {
        //     mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
        // }
        for(int i = 0; i < vertices.Length; i++)
            colors[i] = Color.red; // default color before simulation updates  
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    private void Update()
    {
        if (mesh == null || vertices == null || vertices.Length == 0)
            return;

        if (waveSimulation == null || waveSimulation.current == null)
            return;

        int width = waveSimulation.current.GetLength(0);
        int height = waveSimulation.current.GetLength(1);

        // bool t = true;

        for (int i = 0; i < vertices.Length; i++)
        {
            // colors[i]=Color.red;
            int x = i % vertexCount;
            int y = i / vertexCount;
            float posX = x / (vertexCount - 1.0f) * (size - 1.0f);
            float posZ = y / (vertexCount - 1.0f) * (size - 1.0f);
            float rawHeight = BilinearInterpolate(waveSimulation.current, posX, posZ);

            vertices[i].y = rawHeight * scale_amplitude;
            
            float normalized =Mathf.InverseLerp(-1f,1f,rawHeight); // (rawHeight - minValue) / span; // normalize to 0-1 based on observed min/max
            // if(normalized >1f || normalized < 0f)
            //     t = false;
            colors[i] = Color.Lerp(Color.blue, Color.red, normalized); // blue for low, red for high, with smooth gradient in between
        }
        // if(Time.frameCount % 30 == 0)
        //     Debug.Log(t);
        
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    private float BilinearInterpolate(float[,] grid, float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int x1 = Mathf.Min(x0 + 1, size - 1);
        int y0 = Mathf.FloorToInt(y);
        int y1 = Mathf.Min(y0 + 1, size - 1);

        float fx = x - x0;
        float fy = y - y0;

        float v00 = grid[Mathf.Clamp(x0, 0, size - 1), Mathf.Clamp(y0, 0, size - 1)];
        float v01 = grid[Mathf.Clamp(x0, 0, size - 1), Mathf.Clamp(y1, 0, size - 1)];
        float v10 = grid[Mathf.Clamp(x1, 0, size - 1), Mathf.Clamp(y0, 0, size - 1)];
        float v11 = grid[Mathf.Clamp(x1, 0, size - 1), Mathf.Clamp(y1, 0, size - 1)];

        // float v0 = Mathf.Lerp(v00, v01, fy);
        // float v1 = Mathf.Lerp(v10, v11, fy);
        // return Mathf.Lerp(v0, v1, fx);
        float v0 = SinLerp(v00, v01, fy);
        float v1 = SinLerp(v10, v11, fy);
        return SinLerp(v0, v1, fx);
    }

    private float SinLerp(float a, float b, float t)
    {
        return a + (b - a) * (Mathf.Sin(t * Mathf.PI - Mathf.PI / 2) + 1) / 2;
    }

    private Color RainbowColor(float normalizedHeight)
    {
        // normalizedHeight: 0 (bottom) to 1 (top), with red in top.
        // Hue from ~0.72 (violet) to 0 (red) produces rainbow-like.
        float hue = Mathf.Lerp(0.72f, 0f, Mathf.Clamp01(normalizedHeight));
        Color c = Color.HSVToRGB(hue, 1f, 1f);
        return c;
    }

    private void OnDrawGizmosSelected()
    {
        if (vertices == null || vertices.Length == 0)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.02f);
        }
    }
}
