using UnityEngine;

namespace SB12
{
    [RequireComponent(typeof(LineRenderer))]
    public class WireRuntime : MonoBehaviour
    {
        [Header("Endpoints")]
        public Transform anchor;
        public Transform plug;

        [Header("Look")]
        [Range(2,128)] public int segments = 32;
        [Range(0f,1f)] public float slack = 0.25f; // 0 = straight, 1 = max sag
        public float width = 0.01f;
        public Color color = new Color(0.05f, 0.05f, 0.05f);

        private LineRenderer lr;

        private void Awake()
        {
            lr = GetComponent<LineRenderer>();
            if (lr.material == null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                lr.material = mat;
            }
            lr.positionCount = Mathf.Max(2, segments);
            lr.startWidth = width;
            lr.endWidth = width;
            lr.useWorldSpace = true;
        }

        private void Update()
        {
            if (anchor == null || plug == null || lr == null) return;
            if (lr.positionCount != Mathf.Max(2, segments)) lr.positionCount = Mathf.Max(2, segments);

            Vector3 a = anchor.position;
            Vector3 b = plug.position;
            Vector3 ab = b - a;
            float dist = ab.magnitude;
            Vector3 dir = (dist > 1e-5f) ? ab / dist : Vector3.forward;
            Vector3 up = Vector3.up;

            for (int i = 0; i < lr.positionCount; i++)
            {
                float t = i / (float)(lr.positionCount - 1);
                Vector3 p = Vector3.Lerp(a, b, t);

                // Parabolic sag: maximum at midpoint
                float sag = slack * dist * 0.15f; // scale sag with distance
                float k = (t - 0.5f);
                float parabola = -(k * k) + 0.25f; // max 0.25 at center
                p += up * (-sag * parabola * 4f);

                lr.SetPosition(i, p);
            }
        }

        public void SetColor(Color c)
        {
            color = c;
            if (lr != null && lr.material != null)
            {
                lr.material.color = c;
            }
        }
    }
}
