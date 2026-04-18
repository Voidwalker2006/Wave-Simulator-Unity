using UnityEngine;

public class WaveSimulation : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int size = 100;
    public float c = 2.5f;
    public float damping = 0.996f;
    public float dt = 0.08f;

    [HideInInspector] public float[,] current;
    private float[,] previous;
    private float[,] next;

    public bool[,] obstacle; //  for diffraction

    void Start()
    {
        InitializeArrays();
    }

    void Update()
    {
        Simulate();
    }

    void InitializeArrays()
    {
        current = new float[size, size];
        previous = new float[size, size];
        next = new float[size, size];
        obstacle = new bool[size, size];
    }

    void Simulate()
    {
        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                if (obstacle[x, y])
                {
                    next[x, y] = 0f;
                    continue;
                }

                float laplacian =
                    current[x + 1, y] +
                    current[x - 1, y] +
                    current[x, y + 1] +
                    current[x, y - 1] -
                    4f * current[x, y];

                next[x, y] =
                    2f * current[x, y]
                    - previous[x, y]
                    + (c * c * dt * dt * laplacian);

                next[x, y] *= damping;
            }
        }

        var temp = previous;
        previous = current;
        current = next;
        next = temp;
    }

    public void AddImpulse(int x, int y, float strength)
    {
        if (x >= 1 && x < size - 1 && y >= 1 && y < size - 1)
        {
            current[x, y] += strength;
            previous[x, y] += strength; //  instant response
        }
    }

    public void SetSource(int x, int y, float value)
    {
        if (x >= 1 && x < size - 1 && y >= 1 && y < size - 1)
        {
            current[x, y] = value;
        }
    }

    public void ResetSimulation()
    {
        InitializeArrays();
    }

    //  DIFFRACTION SLIT
    public void CreateSlit()
    {
        int wallX = size / 2;

        for (int y = 0; y < size; y++)
        {
            obstacle[wallX, y] = true;
        }

        int gapStart = size / 2 - 5;
        int gapEnd = size / 2 + 5;

        for (int y = gapStart; y <= gapEnd; y++)
        {
            obstacle[wallX, y] = false;
        }
    }
}