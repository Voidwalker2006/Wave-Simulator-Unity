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

            int gridX = Mathf.Clamp(Mathf.RoundToInt(posX), 0, waveSimulation.size - 1);
            int gridY = Mathf.Clamp(Mathf.RoundToInt(posZ), 0, waveSimulation.size - 1);

            //  Diffraction wall visualization
            if (waveSimulation.obstacle != null && waveSimulation.obstacle[gridX, gridY])
            {
                vertices[i].y = 0.5f;
            }
            else
            {
                vertices[i].y = rawHeight * scale_amplitude;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        //  Optimized collider update (not every frame)
        if (meshCollider != null && Time.frameCount % 5 == 0)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    private float BilinearInterpolate(float[,] grid, float x, float y)
    {
        int gridSizeX = grid.GetLength(0);
        int gridSizeY = grid.GetLength(1);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        int x0 = Mathf.FloorToInt(x);
        int x1 = Mathf.Min(x0 + 1, gridSizeX - 1);
        int y0 = Mathf.FloorToInt(y);
        int y1 = Mathf.Min(y0 + 1, gridSizeY - 1);

        float fx = x - x0;
        float fy = y - y0;

        // Convert linear fractions to sinusoidal eased weights (0..1)
        float sfx = (Mathf.Sin(fx * Mathf.PI - Mathf.PI / 2f) + 1f) / 2f;
        float sfy = (Mathf.Sin(fy * Mathf.PI - Mathf.PI / 2f) + 1f) / 2f;

        float v00 = grid[x0, y0];
        float v10 = grid[x1, y0];
        float v01 = grid[x0, y1];
        float v11 = grid[x1, y1];

        // Bilinear combination using sinusoidal eased fractions
        float w00 = (1f - sfx) * (1f - sfy);
        float w10 = sfx * (1f - sfy);
        float w01 = (1f - sfx) * sfy;
        float w11 = sfx * sfy;

        return v00 * w00 + v10 * w10 + v01 * w01 + v11 * w11;
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