#pragma once

#include "chunk_data.hpp"
#include "world_gen_constants.hpp"
#include <godot_cpp/variant/vector3.hpp>
#include <unordered_map>
#include <vector>
#include <cstdint>

struct ArrayMeshData {
    std::vector<godot::Vector3> verts;
    std::vector<godot::Vector3> normals;
    std::unordered_map<MaterialId, std::vector<int32_t>> material_indices;
};

class ChunkMesher {
public:
    ArrayMeshData generate_mesh(const ChunkData &data);
};