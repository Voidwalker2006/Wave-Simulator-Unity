using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Simulate : MonoBehaviour
{
    [Header("Mesh Resolution")]
    [SerializeField] private int vertexCount = 100;

    [Header("References")]
    public WaveSimulation waveSimulation;
    public Material material_force;

    [Header("Visual Settings")]
    [SerializeField] private float scale_amplitude = 1f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        if (waveSimulation == null)
        {
            Debug.LogError("WaveSimulation reference is missing in Simulate.cs");
            return;
        }

        CreateGrid();
        BuildMesh();

        if (material_force != null)
        {
            GetComponent<MeshRenderer>().material = material_force;
        }
    }

    private void CreateGrid()
    {
        vertexCount = Mathf.Clamp(vertexCount, 2, 250);

        vertices = new Vector3[vertexCount * vertexCount];

        float simSize = waveSimulation.size - 1;

        for (int y = 0; y < vertexCount; y++)
        {
            for (int x = 0; x < vertexCount; x++)
            {
                int index = y * vertexCount + x;

                float posX = (x / (vertexCount - 1f)) * simSize;
                float posZ = (y / (vertexCount - 1f)) * simSize;

                vertices[index] = new Vector3(posX, 0f, posZ);
            }
        }

        triangles = new int[(vertexCount - 1) * (vertexCount - 1) * 6];

        int triIndex = 0;
        for (int y = 0; y < vertexCount - 1; y++)
        {
            for (int x = 0; x < vertexCount - 1; x++)
            {
                int v = y * vertexCount + x;

                triangles[triIndex++] = v;
                triangles[triIndex++] = v + vertexCount + 1;
                triangles[triIndex++] = v + 1;

                triangles[triIndex++] = v;
                triangles[triIndex++] = v + vertexCount;
                triangles[triIndex++] = v + vertexCount + 1;
            }
        }
    }

    private void BuildMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh { name = "ProceduralGrid" };
            mesh.MarkDynamic();
        }
        else
        {
            mesh.Clear();
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    void Update()
    {
        if (mesh == null || vertices == null || vertices.Length == 0)
            return;

        if (waveSimulation == null || waveSimulation.current == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            float posX = vertices[i].x;
            float posZ = vertices[i].z;

            float rawHeight = BilinearInterpolate(waveSimulation.current, posX, posZ);
            vertices[i].y = rawHeight * scale_amplitude;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // IMPORTANT: update collider so clicking works correctly
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    private float BilinearInterpolate(float[,] grid, float x, float y)
    {
        int gridSizeX = grid.GetLength(0);
        int gridSizeY = grid.GetLength(1);

        int x0 = Mathf.FloorToInt(x);
        int x1 = Mathf.Min(x0 + 1, gridSizeX - 1);
        int y0 = Mathf.FloorToInt(y);
        int y1 = Mathf.Min(y0 + 1, gridSizeY - 1);

        float fx = x - x0;
        float fy = y - y0;

        float v00 = grid[Mathf.Clamp(x0, 0, gridSizeX - 1), Mathf.Clamp(y0, 0, gridSizeY - 1)];
        float v01 = grid[Mathf.Clamp(x0, 0, gridSizeX - 1), Mathf.Clamp(y1, 0, gridSizeY - 1)];
        float v10 = grid[Mathf.Clamp(x1, 0, gridSizeX - 1), Mathf.Clamp(y0, 0, gridSizeY - 1)];
        float v11 = grid[Mathf.Clamp(x1, 0, gridSizeX - 1), Mathf.Clamp(y1, 0, gridSizeY - 1)];

        float v0 = SinLerp(v00, v01, fy);
        float v1 = SinLerp(v10, v11, fy);

        return SinLerp(v0, v1, fx);
    }

    private float SinLerp(float a, float b, float t)
    {
        return a + (b - a) * (Mathf.Sin(t * Mathf.PI - Mathf.PI / 2f) + 1f) / 2f;
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