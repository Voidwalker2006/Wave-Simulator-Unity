using UnityEngine;

public class WaveSimulation : MonoBehaviour
{
    public int size = 100;     // grid size
    public float c = 0.5f;       // wave speed
    public float damping = 1f;
    public float dt = 0.01f;

    public float[,] current;
    float[,] previous;
    float[,] next;

    void Start() // Initialize Arrays
    {
        current = new float[size, size];
        previous = new float[size, size];
        next = new float[size, size];

        current[size / 2, size / 2] = 1f;
        previous[size / 2, size / 2] = 1f;

    }




    void Update()
    {
        Simulate();

        // 🔍 DEBUG: track max amplitude
        if (Time.frameCount % 30 == 0) // print every ~0.5 sec
        {
            float maxVal = 0f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    maxVal = Mathf.Max(maxVal, Mathf.Abs(current[x, y]));
                }
            }

            Debug.Log(maxVal);
        }
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
                    4 * current[x, y];

                next[x, y] = 2 * current[x, y] - previous[x, y]
                            + c * c * dt * dt * laplacian;

                next[x, y] *= damping;
            }
        }


        ApplyBoundary(next);


        // Swap arrays
        var temp = previous;
        previous = current;
        current = next;
        next = temp;
    }

    void ApplyBoundary(float[,] grid)
    {
        // Left & Right
        for (int y = 0; y < size; y++)
        {
            grid[0, y] = grid[1, y];
            grid[size - 1, y] = grid[size - 2, y];
        }

        // Top & Bottom
        for (int x = 0; x < size; x++)
        {
            grid[x, 0] = grid[x, 1];
            grid[x, size - 1] = grid[x, size - 2];
        }
    }

    public void AddImpulse(int x, int y, float strength)
    {
        if (x > 1 && x < size - 1 && y > 1 && y < size - 1)
        {
            current[x, y] += strength;
        }
    }

    public void SetSource(int x, int y, float value)
    {
        if (x > 1 && x < size - 1 && y > 1 && y < size - 1)
        {
            current[x, y] = value;
        }
    }

    public void ResetSimulation()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                current[x, y] = 0;
                previous[x, y] = 0;
                next[x, y] = 0;
            }
        }
    }

}