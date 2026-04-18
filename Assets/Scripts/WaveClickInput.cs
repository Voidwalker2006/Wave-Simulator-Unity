using UnityEngine;

public class WaveClickInput : MonoBehaviour
{
    public Camera cam;
    public WaveSimulation sim;

    [Header("Ripple Settings")]
    public float impulseStrength = 0.8f;
    public int rippleRadius = 3;
    public float falloff = 1f;

    [Header("Drag Settings")]
    public float clickCooldown = 0.05f;

    private float lastClickTime = 0f;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time - lastClickTime > clickCooldown)
        {
            HandleClick();
            lastClickTime = Time.time;
        }
    }

    void HandleClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 point = hit.point;

            int cx = Mathf.Clamp(Mathf.RoundToInt(point.x), 1, sim.size - 2);
            int cy = Mathf.Clamp(Mathf.RoundToInt(point.z), 1, sim.size - 2);

            AddRipple(cx, cy);
        }
    }

    void AddRipple(int centerX, int centerY)
    {
        for (int dx = -rippleRadius; dx <= rippleRadius; dx++)
        {
            for (int dy = -rippleRadius; dy <= rippleRadius; dy++)
            {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= rippleRadius)
                {
                    float strength = impulseStrength * Mathf.Exp(-falloff * dist);
                    sim.AddImpulse(centerX + dx, centerY + dy, strength);
                }
            }
        }
    }
}