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
    }

    void Simulate()
    {
        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
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

        float[,] temp = previous;
        previous = current;
        current = next;
        next = temp;
    }

    public void AddImpulse(int x, int y, float strength)
    {
        if (x >= 1 && x < size - 1 && y >= 1 && y < size - 1)
        {
            current[x, y] += strength;
        }
    }

    public void ResetSimulation()
    {
        InitializeArrays();
    }
}