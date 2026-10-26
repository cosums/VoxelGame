#pragma once

#include "world_gen_constants.hpp"
#include "terrain_field.hpp"
#include <godot_cpp/variant/vector3.hpp>
#include <vector>

class ChunkData {
    public:
        std::vector<float> samples;
        std::vector<MaterialId> materials;
        void generate_data(const TerrainField &field, godot::Vector3 position, int scale, bool is_lod_chunk);
        int width = 0;
        int height = 0;
        int chunk_scale;
        int index(int x, int y, int z) const;
};