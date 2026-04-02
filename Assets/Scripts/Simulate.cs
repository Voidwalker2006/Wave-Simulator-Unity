using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Simulate : MonoBehaviour
{
    [SerializeField]
    private int poly = 100; // number of vertices per row/column (default 100)
    public WaveSimulation waveSimulation;
    public Material material_force;
    [SerializeField]
    private float scale_amplitude = 1f; // scale factor for wave height (default 1)
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    void Start()
    {
        CreateGrid();
        BuildMesh();
        GetComponent<MeshRenderer>().material = material_force; // drag your material asset into a public variable for this

        
    }

    private void CreateGrid()
    {
        if (poly > 250)
        {
            Debug.LogWarning($"Poly value {poly} is too large; clamping to 250 to avoid mesh overflow.");
            poly = 250;
        }

        int count = Mathf.Clamp(poly, 1, 250);
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
        // int count = Mathf.Max(1, poly);
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

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float v = waveSimulation.current[x, y];
                if (v < minValue) minValue = v;
                if (v > maxValue) maxValue = v;
            }
        }

        float span = maxValue - minValue;
        bool hasSpan = span > Mathf.Epsilon;

        // bool t = true;

        for (int i = 0; i < vertices.Length; i++)
        {
            // colors[i]=Color.red;
            int x = i % poly;
            int y = i / poly;
            float rawHeight = waveSimulation.current[x, y];

            vertices[i].y = rawHeight * scale_amplitude;
            
            float normalized = hasSpan ? (rawHeight - minValue) / span : 0f;
            // if(normalized >1f || normalized < 0f)
            //     t = false;
            colors[i] = RainbowColor(normalized);
        }
        // if(Time.frameCount % 30 == 0)
        //     Debug.Log(t);
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
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
