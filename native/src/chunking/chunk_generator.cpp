#include "chunk_generator.hpp"
#include "chunk_data.hpp"
#include "chunk_mesher.hpp"
#include "godot_cpp/variant/dictionary.hpp"
#include "godot_cpp/variant/packed_int32_array.hpp"
#include "godot_cpp/variant/packed_vector3_array.hpp"
#include "godot_cpp/variant/utility_functions.hpp"
#include "terrain_field.hpp"
#include <chrono>

#include <godot_cpp/core/class_db.hpp>

using namespace godot;

void ChunkGenerator::_bind_methods() {
    ClassDB::bind_method(D_METHOD("set_seed", "seed"), &ChunkGenerator::set_seed);
    ClassDB::bind_method(D_METHOD("generate_chunk", "position", "chunk_scale", "is_lod_chunk"), &ChunkGenerator::generate_chunk);
}

void ChunkGenerator::set_seed(int seed) {
    field = std::make_unique<TerrainField>(seed);
}

godot::Dictionary ChunkGenerator::generate_chunk(godot::Vector3 position, int chunk_scale, bool is_lod_chunk) {
    if (!field) {
        godot::UtilityFunctions::push_error("[ChunkGenerator]: call set seed first!");
        return godot::Dictionary();
    }
    
    auto start_total = std::chrono::high_resolution_clock::now();

    ChunkData data;
    data.generate_data(*field, position, chunk_scale, is_lod_chunk);
    
    auto start_meshing = std::chrono::high_resolution_clock::now();

    ChunkMesher mesher;
    ArrayMeshData mesh_data = mesher.generate_mesh(data);

    auto start_packing = std::chrono::high_resolution_clock::now();

    PackedVector3Array vertices;
    vertices.resize(mesh_data.verts.size());
    if (!mesh_data.verts.empty()) {
        auto write_ptr = vertices.ptrw();
        std::copy(mesh_data.verts.begin(), mesh_data.verts.end(), write_ptr);
    }

    PackedVector3Array normals;
    normals.resize(mesh_data.normals.size());
    if (!mesh_data.normals.empty()) {
        auto write_ptr = normals.ptrw();
        std::copy(mesh_data.normals.begin(), mesh_data.normals.end(), write_ptr);
    }

    Dictionary materials;
    for (const auto &entry : mesh_data.material_indices) {
        PackedInt32Array arr;
        arr.resize(entry.second.size());
        if (!entry.second.empty()) {
            auto write_ptr = arr.ptrw();
            std::copy(entry.second.begin(), entry.second.end(), write_ptr);
        }
        materials[(int)entry.first] = arr;
    }

    Dictionary result;
    result["vertices"] = vertices;
    result["normals"] = normals;
    result["material_indices"] = materials;
    
    std::chrono::duration<float, std::milli> data_ms = start_meshing - start_total;
    std::chrono::duration<float, std::milli> mesh_ms = start_packing - start_meshing;

    godot::UtilityFunctions::print("[Timer] Data: ", data_ms.count(), "ms | Meshing: ", mesh_ms.count(), "ms");

    return result;
}