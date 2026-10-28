// Utilidades de geometría: fusiona piezas estáticas en una sola malla con colores por vértice
// para dibujar personajes y objetos con una llamada de dibujo en lugar de una por pieza.
(function () {
  const tmpColor = new THREE.Color();
  const normalMatrix = new THREE.Matrix3();
  const v = new THREE.Vector3();

  // parts: [{ geo, matrix, color }] -> BufferGeometry no indexada con position, normal, color (y uv si todas la tienen)
  function merge(parts) {
    let count = 0;
    const flat = parts.map((p) => {
      const g = p.geo.index ? p.geo.toNonIndexed() : p.geo;
      count += g.attributes.position.count;
      return { g, p };
    });
    const withUv = flat.every(({ g }) => g.attributes.uv);
    const pos = new Float32Array(count * 3);
    const nor = new Float32Array(count * 3);
    const col = new Float32Array(count * 3);
    const uv = withUv ? new Float32Array(count * 2) : null;
    let o = 0;
    for (const { g, p } of flat) {
      const P = g.attributes.position, N = g.attributes.normal;
      const m = p.matrix;
      normalMatrix.getNormalMatrix(m);
      tmpColor.set(p.color === undefined ? '#ffffff' : p.color);
      for (let i = 0; i < P.count; i++, o++) {
        v.fromBufferAttribute(P, i).applyMatrix4(m);
        pos[o * 3] = v.x; pos[o * 3 + 1] = v.y; pos[o * 3 + 2] = v.z;
        v.fromBufferAttribute(N, i).applyMatrix3(normalMatrix).normalize();
        nor[o * 3] = v.x; nor[o * 3 + 1] = v.y; nor[o * 3 + 2] = v.z;
        col[o * 3] = tmpColor.r; col[o * 3 + 1] = tmpColor.g; col[o * 3 + 2] = tmpColor.b;
        if (uv) { uv[o * 2] = g.attributes.uv.getX(i); uv[o * 2 + 1] = g.attributes.uv.getY(i); }
      }
      if (g !== p.geo) g.dispose();
    }
    const out = new THREE.BufferGeometry();
    out.setAttribute('position', new THREE.BufferAttribute(pos, 3));
    out.setAttribute('normal', new THREE.BufferAttribute(nor, 3));
    out.setAttribute('color', new THREE.BufferAttribute(col, 3));
    if (uv) out.setAttribute('uv', new THREE.BufferAttribute(uv, 2));
    out.computeBoundingSphere();
    return out;
  }

  // Matriz a partir de posición, rotación (x, y, z) y escala
  const e = new THREE.Euler();
  const q = new THREE.Quaternion();
  function matrix(x, y, z, rx, ry, rz, sx, sy, sz) {
    e.set(rx || 0, ry || 0, rz || 0);
    q.setFromEuler(e);
    const s = sx === undefined ? 1 : sx;
    return new THREE.Matrix4().compose(
      new THREE.Vector3(x || 0, y || 0, z || 0), q,
      new THREE.Vector3(s, sy === undefined ? s : sy, sz === undefined ? s : sz),
    );
  }

  // Deja fija la matriz de un objeto que ya no se mueve (ahorra recalcularla en cada cuadro)
  function freeze(obj) {
    obj.updateMatrix();
    obj.matrixAutoUpdate = false;
    return obj;
  }

  // Material de colores por vértice compartido (Lambert, como el resto del mundo)
  const vcMats = new Map();
  function vcMat(opts) {
    const key = JSON.stringify(opts || {});
    if (!vcMats.has(key)) vcMats.set(key, new THREE.MeshLambertMaterial(Object.assign({ vertexColors: true }, opts)));
    return vcMats.get(key);
  }

  W.Geom = { merge, matrix, freeze, vcMat };
})();
