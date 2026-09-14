using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    /// <summary>
    /// Tiện ích sinh Mesh vòng tròn và đĩa tròn phẳng sát mặt đất (AoE Ground Indicators)
    /// Đảm bảo tối ưu hiệu năng 60+ FPS, không phụ thuộc Texture ngoài, tương thích hoàn toàn với URP & Built-in.
    /// </summary>
    public static class AoECircleMeshUtility
    {
        /// <summary>
        /// Tạo vật liệu Unlit bán trong suốt (Transparent Unlit) tương thích URP và chuẩn Unity
        /// </summary>
        public static Material CreateIndicatorMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Hidden/Internal-Colored");

            Material mat = new Material(shader)
            {
                hideFlags = HideFlags.DontSave
            };

            // Thiết lập chế độ Transparent cho URP Unlit
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // 1 = Transparent
                mat.SetFloat("_Blend", 0);   // 0 = Alpha
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 10;
            }

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;

            return mat;
        }

        /// <summary>
        /// Sinh Mesh vành đai tròn phẳng (Ring Mesh) trên mặt phẳng XZ
        /// </summary>
        public static Mesh CreateRingMesh(float outerRadius, float thickness, int segments = 64)
        {
            Mesh mesh = new Mesh
            {
                name = "AoE_RingMesh",
                hideFlags = HideFlags.DontSave
            };

            float innerRadius = Mathf.Max(0.01f, outerRadius - thickness);
            List<Vector3> vertices = new List<Vector3>((segments + 1) * 2);
            List<int> triangles = new List<int>(segments * 6);

            float angleStep = (2f * Mathf.PI) / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                vertices.Add(new Vector3(cos * outerRadius, 0, sin * outerRadius));
                vertices.Add(new Vector3(cos * innerRadius, 0, sin * innerRadius));
            }

            for (int i = 0; i < segments; i++)
            {
                int o1 = i * 2;
                int i1 = o1 + 1;
                int o2 = (i + 1) * 2;
                int i2 = o2 + 1;

                triangles.Add(o1);
                triangles.Add(i1);
                triangles.Add(o2);

                triangles.Add(o2);
                triangles.Add(i1);
                triangles.Add(i2);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Sinh Mesh đĩa tròn phẳng đặc (Filled Disc Mesh) trên mặt phẳng XZ
        /// </summary>
        public static Mesh CreateDiscMesh(float radius, int segments = 64)
        {
            Mesh mesh = new Mesh
            {
                name = "AoE_DiscMesh",
                hideFlags = HideFlags.DontSave
            };

            List<Vector3> vertices = new List<Vector3>(segments + 2)
            {
                Vector3.zero // Tâm đĩa
            };
            List<int> triangles = new List<int>(segments * 3);

            float angleStep = (2f * Mathf.PI) / segments;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
            }

            for (int i = 1; i <= segments; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
