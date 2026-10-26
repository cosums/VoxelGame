#pragma once

#include "world_gen_constants.hpp"
#include <godot_cpp/classes/fast_noise_lite.hpp>

struct CaveSample {
    float density = 0.0f;
    MaterialId material = 0;
};

class TerrainField {
    public:
        TerrainField(int seed);
        float sample_height(float x, float z) const;
        CaveSample sample_cave(float x, float y, float z, float surface_density) const;
    private:
        godot::Ref<godot::FastNoiseLite> terrain_noise;
        godot::Ref<godot::FastNoiseLite> cave_noise;
        float base_height = 154.0f;
        int num_layers = 4;
        float base_frequency = 0.1f;
        float strength = 40;
        float persistence = 0.5f;
        float roughness = 2;
        float cave_frequency = 0.01f;
        float cave_threshold = 0.4f;
        float cave_strength = 500;
};