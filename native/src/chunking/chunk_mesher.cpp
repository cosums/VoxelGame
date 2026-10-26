#include "chunk_mesher.hpp"
#include "chunk_data.hpp"
#include "world_gen_constants.hpp"
#include <cmath>

using namespace godot;

static const int CORNER_OFFSETS[8][3] = {
    {0, 0, 0},
    {1, 0, 0},
    {0, 1, 0},
    {1, 1, 0},
    {0, 0, 1},
    {1, 0, 1},
    {0, 1, 1},
    {1, 1, 1},
};

static const int EDGE_PAIRS[12][2] = {
    {0, 1}, {1, 3}, {3, 2}, {2, 0},
    {4, 5}, {5, 7}, {7, 6}, {6, 4},
    {0, 4}, {1, 5}, {2, 6}, {3, 7},
};

static Vector3 centroid(const Vector3 *positions, int count) {
    Vector3 sum(0,0,0);

    for (int i = 0; i < count; i++) {
        sum += positions[i];
    }

    return sum / count;
}

static void add_triangle(int a, int b, int c, MaterialId mat, const std::vector<Vector3> &vertex_list, ArrayMeshData &mesh) {
    int start_index = static_cast<int>(mesh.verts.size());

    Vector3 v0 = vertex_list[a];
    Vector3 v1 = vertex_list[b];
    Vector3 v2 = vertex_list[c];
    mesh.verts.push_back(v0);
    mesh.verts.push_back(v1);
    mesh.verts.push_back(v2);

    Vector3 normal = (v2 - v0).cross(v1 - v0).normalized();
    mesh.normals.push_back(normal);
    mesh.normals.push_back(normal);
    mesh.normals.push_back(normal);

    std::vector<int32_t> &list = mesh.material_indices[mat];
    list.push_back(start_index);
    list.push_back(start_index + 1);
    list.push_back(start_index + 2);
}

static void add_quad(int a, int b, int c, int d, bool flip, MaterialId mat, const std::vector<Vector3> &vertex_list, ArrayMeshData &mesh) {
    if (flip) {
        add_triangle(a, b, c, mat, vertex_list, mesh);
        add_triangle(c, d, a, mat, vertex_list, mesh);
    } else {
        add_triangle(c, b, a, mat, vertex_list, mesh);
        add_triangle(a, d, c, mat, vertex_list, mesh);
    }
}

static Vector3 calculate_vertex(const ChunkData &data, int x, int y, int z) {
    Vector3 edge_hits[12];
    int count = 0;

    int mask = 0;
    for (int i = 0; i < 8; i++) {
        int cx = x + CORNER_OFFSETS[i][0];
        int cy = y + CORNER_OFFSETS[i][1];
        int cz = z + CORNER_OFFSETS[i][2];
        if (data.samples[data.index(cx, cy, cz)] > 0) 
            mask |= (1 << i);
    }
    if (mask == 0 || mask == 255) return Vector3(NAN, NAN, NAN);

    for (int i = 0; i < 12; i++) {
        int edge1 = EDGE_PAIRS[i][0];
        int edge2 = EDGE_PAIRS[i][1];
        
        int ax = x + CORNER_OFFSETS[edge1][0];
        int ay = y + CORNER_OFFSETS[edge1][1];
        int az = z + CORNER_OFFSETS[edge1][2];

        int bx = x + CORNER_OFFSETS[edge2][0];
        int by = y + CORNER_OFFSETS[edge2][1];
        int bz = z + CORNER_OFFSETS[edge2][2];

        bool a_solid = (mask >> edge1) & 1;
        bool b_solid = (mask >> edge2) & 1;

        if (a_solid != b_solid) {
            float valA = data.samples[data.index(ax, ay, az)];
            float valB = data.samples[data.index(bx, by, bz)];
            Vector3 a(ax, ay, az);
            Vector3 b(bx, by, bz);
            Vector3 pos_inter = a + (b - a) * (valA / (valA - valB));
            edge_hits[count++] = pos_inter * data.chunk_scale; 
        }
    }

    if (count == 0) {
        return Vector3(NAN, NAN, NAN);
    }

    return centroid(edge_hits, count);
}

ArrayMeshData ChunkMesher::generate_mesh(const ChunkData &data) {
    std::vector<Vector3> vertex_list(data.width * data.height * data.width);

    ArrayMeshData mesh_data;

    mesh_data.verts.reserve(8192);
    mesh_data.normals.reserve(8192);

    // Vertext placement
    for (int x = 0; x < data.width - 1; x++) {
        for (int y = 0; y < data.height - 1; y++) {
            for (int z = 0; z < data.width - 1; z++) {
                Vector3 vertex = calculate_vertex(data, x, y, z);
                vertex_list[data.index(x, y, z)] = vertex;
            } 
        }
    }

    // mesh construction
    for (int x = 1; x < data.width - 1; x++) {
        for (int y = 1; y < data.height - 1; y++) {
            for (int z = 1; z < data.width - 1; z++) {
                // exit out when not on surface
                int index = data.index(x, y, z);
                if (std::isnan(vertex_list[index].x)) continue;
                
                bool solidVert = data.samples[data.index(x, y, z)] > 0;
                
                // X faces
                bool solidX = data.samples[data.index(x + 1, y, z)] > 0;
                if (solidVert != solidX) {
                    int v0 = data.index(x - 0, y - 1, z - 1);
                    int v1 = data.index(x - 0, y - 0, z - 1);
                    int v2 = data.index(x - 0, y - 0, z - 0);
                    int v3 = data.index(x - 0, y - 1, z - 0);

                    MaterialId mat = solidX ? data.materials[data.index(x + 1, y, z)] : data.materials[data.index(x, y, z)];
                    add_quad(v0, v1, v2, v3, solidX, mat, vertex_list, mesh_data);
                }

                // Y faces
                bool solidY = data.samples[data.index(x, y + 1, z)] > 0;
                if (solidVert != solidY) {
                    int v0 = data.index(x - 1, y - 0, z - 1);
                    int v1 = data.index(x - 1, y - 0, z - 0);
                    int v2 = data.index(x - 0, y - 0, z - 0);
                    int v3 = data.index(x - 0, y - 0, z - 1);

                    MaterialId mat = solidY ? data.materials[data.index(x, y + 1, z)] : data.materials[data.index(x, y, z)];
                    add_quad(v0, v1, v2, v3, solidY, mat, vertex_list, mesh_data);
                }

                // Z faces
                bool solidZ = data.samples[data.index(x, y, z + 1)] > 0;
                if (solidVert != solidZ) {
                    int v0 = data.index(x - 1, y - 1, z - 0);
                    int v1 = data.index(x - 0, y - 1, z - 0);
                    int v2 = data.index(x - 0, y - 0, z - 0);
                    int v3 = data.index(x - 1, y - 0, z - 0);
                    
                    MaterialId mat = solidZ ? data.materials[data.index(x, y, z + 1)] : data.materials[data.index(x, y, z)];
                    add_quad(v0, v1, v2, v3, solidZ, mat, vertex_list, mesh_data);
                }
            }
        }
    }

    return mesh_data;
}