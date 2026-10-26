#include "chunk_data.hpp"
#include "terrain_field.hpp"
#include "world_gen_constants.hpp"
#include <algorithm>

using namespace godot;

void ChunkData::generate_data(const TerrainField &field, Vector3 position, int scale, bool is_lod_chunk) {
    int pad_start = is_lod_chunk ? -1 : 0;
    width = CHUNK_SIZE + (is_lod_chunk ? 3 : 2);
    height = CHUNK_HEIGHT + 1;
    
    samples.resize(width * height * width);
    materials.resize(width * height * width);

    chunk_scale = scale;

    std::vector<float> height_map;
    height_map.resize(width * width);

    for (int x = 0; x < width; x++) {
        float sx = position.x + (x + pad_start) * chunk_scale;
        for (int z = 0; z < width; z++) { 
            float sz = position.z + (z + pad_start) * chunk_scale;

            float column_density = field.sample_height(sx, sz);
            height_map[x * width + z] = column_density;
        }
    }

    for (int x = 0; x < width; x++) {
        float sx = position.x + (x + pad_start) * chunk_scale;
        for (int y = 0; y < height; y++) {
            float sy = position.y + y * chunk_scale;
            for (int z = 0; z < width; z++) {
                float sz = position.z + (z + pad_start) * chunk_scale;

                float density = height_map[x * width + z] - sy;

                if (chunk_scale > 1 && sy < LOD_CUTOFF) {
                    samples[index(x, y, z)] = 1;
                    materials[index(x, y, z)] = 4; // mantlestone
                    continue;
                }

                CaveSample sample = field.sample_cave(sx, sy, sz, density);
                float final_density = std::clamp(sample.density, -1.0f, 1.0f);

                samples[index(x, y, z)] = final_density;
                materials[index(x, y, z)] = sample.material;
            }
        }
    }
}

int ChunkData::index(int x, int y, int z) const {
    return z + width * y + width * height * x;
}