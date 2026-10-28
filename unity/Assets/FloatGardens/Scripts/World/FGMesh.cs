// Fusión de piezas: un objeto hecho de varias primitivas (bus, dron, cultivo, kiosco...) se convierte en UNA malla
// con un submesh por material. Menos GameObjects, Transforms y renderers que recorrer en cada cuadro, y las mallas
// se comparten entre todos los objetos iguales.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FloatGardens
{
    public static class FGMesh
    {
        public struct Part
        {
            public Mesh mesh;
            public Matrix4x4 matrix;
            public Material mat;
        }

        public static Part P(PrimitiveType type, Vector3 pos, Vector3 scale, Material mat) =>
            P(MeshBuilder.Primitive(type), pos, Quaternion.identity, scale, mat);

        public static Part P(PrimitiveType type, Vector3 pos, Quaternion rot, Vector3 scale, Material mat) =>
            P(MeshBuilder.Primitive(type), pos, rot, scale, mat);

        public static Part P(Mesh mesh, Vector3 pos, Quaternion rot, Vector3 scale, Material mat) =>
            new Part { mesh = mesh, matrix = Matrix4x4.TRS(pos, rot, scale), mat = mat };

        static readonly Dictionary<string, KeyValuePair<Mesh, Material[]>> cache = new Dictionary<string, KeyValuePair<Mesh, Material[]>>();

        /// <summary>
        /// Crea un hijo de parent con todas las piezas en una sola malla. Con cacheKey, la malla se arma una vez y se
        /// comparte entre todos los objetos que usen la misma clave (los materiales deben ser los mismos).
        /// </summary>
        public static GameObject Bake(Transform parent, string name, IList<Part> parts, string cacheKey = null,
            ShadowCastingMode shadows = ShadowCastingMode.On)
        {
            Mesh mesh;
            Material[] mats;
            if (cacheKey != null && cache.TryGetValue(cacheKey, out var hit) && hit.Key != null && System.Array.TrueForAll(hit.Value, m => m != null))
            {
                mesh = hit.Key;
                mats = hit.Value;
            }
            else
            {
                mesh = Combine(name, parts, out mats);
                if (cacheKey != null) cache[cacheKey] = new KeyValuePair<Mesh, Material[]>(mesh, mats);
            }
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterials = mats;
            r.shadowCastingMode = shadows;
            return go;
        }

        /// <summary>Junta las piezas en una malla con un submesh por material (en el orden en que aparecen).</summary>
        public static Mesh Combine(string name, IList<Part> parts, out Material[] materials)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var order = new List<Material>();
            var tris = new Dictionary<Material, List<int>>();
            foreach (var p in parts)
            {
                if (!tris.TryGetValue(p.mat, out var t))
                {
                    t = new List<int>();
                    tris[p.mat] = t;
                    order.Add(p.mat);
                }
                Append(p.mesh, p.matrix, verts, normals, uvs, t);
            }
            var mesh = new Mesh { name = name };
            if (verts.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = order.Count;
            for (int i = 0; i < order.Count; i++) mesh.SetTriangles(tris[order[i]], i);
            mesh.RecalculateBounds();
            mesh.UploadMeshData(true); // ya no hace falta la copia en la memoria de la CPU
            materials = order.ToArray();
            return mesh;
        }

        /// <summary>Copia una malla transformada; uv opcional fija todos los UV (paleta de colores).</summary>
        public static void Append(Mesh src, Matrix4x4 m, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<int> tris, Vector2? uv = null)
        {
            int i0 = verts.Count;
            var v = src.vertices;
            var n = src.normals;
            var u = src.uv;
            var t = src.triangles;
            Matrix4x4 nm = m.inverse.transpose;
            for (int k = 0; k < v.Length; k++)
            {
                verts.Add(m.MultiplyPoint3x4(v[k]));
                normals.Add(n.Length > k ? nm.MultiplyVector(n[k]).normalized : Vector3.up);
                uvs.Add(uv ?? (u.Length > k ? u[k] : Vector2.zero));
            }
            bool flip = m.determinant < 0;
            for (int k = 0; k < t.Length; k += 3)
            {
                tris.Add(i0 + t[k]);
                tris.Add(i0 + (flip ? t[k + 2] : t[k + 1]));
                tris.Add(i0 + (flip ? t[k + 1] : t[k + 2]));
            }
        }
    }
}
