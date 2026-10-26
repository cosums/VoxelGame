#include "terrain_field.hpp"
#include <godot_cpp/classes/fast_noise_lite.hpp>

TerrainField::TerrainField(int seed) {
    terrain_noise.instantiate();
    terrain_noise->set_seed(seed);
    terrain_noise->set_noise_type(godot::FastNoiseLite::TYPE_SIMPLEX);

    cave_noise.instantiate();
    cave_noise->set_seed(seed);
    cave_noise->set_frequency(cave_frequency);
    cave_noise->set_noise_type(godot::FastNoiseLite::TYPE_SIMPLEX);
}

float TerrainField::sample_height(float x, float z) const {
    float noise_sum = 0;
    float frequency = base_frequency;
    float amplitude = 1;
    
    for (int i = 0; i < num_layers; i++) {
        noise_sum += (terrain_noise->get_noise_2d(x * frequency, z * frequency) + 1) * 0.5f * amplitude;
        frequency *= roughness;
        amplitude *= persistence;
    }

    return noise_sum * strength + base_height;
}

CaveSample TerrainField::sample_cave(float x, float y, float z, float surface_density) const {
    CaveSample result;
    result.density = surface_density;

    if (surface_density > 0) {
        float cave_value = cave_noise->get_noise_3d(x, y, z);
    
        if (cave_value > cave_threshold) {
            result.density = -cave_strength * (cave_value - cave_threshold);
        }
    }

    if (y < 10) {
        result.density += 500.0f / (y + 1.0f);
    }

    if (result.density > 0)
    {
        if (y < 10)
            result.material = 4; // mantleshell
        else if (y < 180)
            result.material = 1; // stone
        else if (y < 194)
            result.material = 2; // sand
        else
            result.material = 3; // grass
    }
    else
    {
        result.material = 0;
    }

    return result;
}