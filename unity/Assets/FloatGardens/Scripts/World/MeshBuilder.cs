// Geometría combinada: muchos edificios en pocas mallas (pocas llamadas de dibujo, ideal para celular).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FloatGardens
{
    public class MeshBuilder
    {
        readonly List<Vector3> verts = new List<Vector3>();
        readonly List<Vector3> normals = new List<Vector3>();
        readonly List<Vector2> uvs = new List<Vector2>();
        readonly List<int> tris = new List<int>();

        public int VertexCount => verts.Count;

        /// <summary>Cuadrilátero con normal dada; ordena el triángulo para que la cara mire hacia la normal.</summary>
        public void Quad(Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl, Vector3 n, Vector2 uv0, Vector2 uv1)
        {
            int i = verts.Count;
            verts.Add(bl); verts.Add(br); verts.Add(tr); verts.Add(tl);
            for (int k = 0; k < 4; k++) normals.Add(n);
            uvs.Add(new Vector2(uv0.x, uv0.y)); uvs.Add(new Vector2(uv1.x, uv0.y));
            uvs.Add(new Vector2(uv1.x, uv1.y)); uvs.Add(new Vector2(uv0.x, uv1.y));
            // Unity: la cara frontal es la que cumple Cross(b - a, c - a) en la dirección de la normal
            bool front = Vector3.Dot(Vector3.Cross(br - bl, tr - bl), n) > 0;
            if (front) { tris.Add(i); tris.Add(i + 1); tris.Add(i + 2); tris.Add(i); tris.Add(i + 2); tris.Add(i + 3); }
            else { tris.Add(i); tris.Add(i + 2); tris.Add(i + 1); tris.Add(i); tris.Add(i + 3); tris.Add(i + 2); }
        }

        /// <summary>Paredes de una caja con base en y0. uw / vh = metros por repetición de textura.</summary>
        public void Walls(float x, float z, float y0, float w, float h, float d, float uw = 4f, float vh = 3f)
        {
            float y1 = y0 + h;
            Face(new Vector3(1, 0, 0), new Vector3(0, 0, 1), x + w / 2, z, d);
            Face(new Vector3(-1, 0, 0), new Vector3(0, 0, -1), x - w / 2, z, d);
            Face(new Vector3(0, 0, 1), new Vector3(-1, 0, 0), x, z + d / 2, w);
            Face(new Vector3(0, 0, -1), new Vector3(1, 0, 0), x, z - d / 2, w);

            void Face(Vector3 n, Vector3 right, float cx, float cz, float width)
            {
                Vector3 c = new Vector3(cx, 0, cz);
                Vector3 bl = c - right * width / 2; bl.y = y0;
                Vector3 br = c + right * width / 2; br.y = y0;
                Vector3 tr = br; tr.y = y1;
                Vector3 tl = bl; tl.y = y1;
                Quad(bl, br, tr, tl, n, new Vector2(0, y0 / vh), new Vector2(width / uw, y1 / vh));
            }
        }

        public void Top(float x, float z, float y, float w, float d, float scale = 8f)
        {
            float x0 = x - w / 2, x1 = x + w / 2, z0 = z - d / 2, z1 = z + d / 2;
            Quad(new Vector3(x0, y, z0), new Vector3(x1, y, z0), new Vector3(x1, y, z1), new Vector3(x0, y, z1), Vector3.up,
                new Vector2(x0 / scale, z0 / scale), new Vector2(x1 / scale, z1 / scale));
        }

        public void Box(float x, float z, float y0, float w, float h, float d, float uw = 4f, float vh = 3f)
        {
            Walls(x, z, y0, w, h, d, uw, vh);
            Top(x, z, y0 + h, w, d);
        }

        /// <summary>Agrega una malla existente transformada (árboles, columnas, cerros...).</summary>
        public void Add(Mesh mesh, Matrix4x4 m)
        {
            int i0 = verts.Count;
            var v = mesh.vertices;
            var n = mesh.normals;
            var uv = mesh.uv;
            var t = mesh.triangles;
            Matrix4x4 nm = m.inverse.transpose;
            for (int k = 0; k < v.Length; k++)
            {
                verts.Add(m.MultiplyPoint3x4(v[k]));
                normals.Add(n.Length > k ? nm.MultiplyVector(n[k]).normalized : Vector3.up);
                uvs.Add(uv.Length > k ? uv[k] : Vector2.zero);
            }
            bool flip = m.determinant < 0;
            for (int k = 0; k < t.Length; k += 3)
            {
                if (flip) { tris.Add(i0 + t[k]); tris.Add(i0 + t[k + 2]); tris.Add(i0 + t[k + 1]); }
                else { tris.Add(i0 + t[k]); tris.Add(i0 + t[k + 1]); tris.Add(i0 + t[k + 2]); }
            }
        }

        public GameObject Build(string name, Material mat, Transform parent, bool castShadows = true)
        {
            var mesh = new Mesh { name = name };
            if (verts.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            go.isStatic = true;
            return go;
        }

        // ---------- Mallas primitivas útiles ----------
        static readonly Dictionary<PrimitiveType, Mesh> prims = new Dictionary<PrimitiveType, Mesh>();

        public static Mesh Primitive(PrimitiveType type)
        {
            if (prims.TryGetValue(type, out var m)) return m;
            var go = GameObject.CreatePrimitive(type);
            m = go.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(go);
            prims[type] = m;
            return m;
        }

        /// <summary>Cono low-poly con sombreado plano (cerros, copas de árbol, sombrillas).</summary>
        public static Mesh Cone(int segments, float radius, float height, bool flat = true)
        {
            var b = new List<Vector3>();
            var tr = new List<int>();
            Vector3 tip = new Vector3(0, height, 0);
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2 / segments, a1 = (i + 1) * Mathf.PI * 2 / segments;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * radius, 0, Mathf.Sin(a0) * radius);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, 0, Mathf.Sin(a1) * radius);
                int k = b.Count;
                b.Add(p0); b.Add(tip); b.Add(p1);
                tr.Add(k); tr.Add(k + 1); tr.Add(k + 2);
            }
            var mesh = new Mesh { name = "Cono" };
            mesh.SetVertices(b);
            mesh.SetTriangles(tr, 0);
            mesh.SetUVs(0, new List<Vector2>(new Vector2[b.Count]));
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
