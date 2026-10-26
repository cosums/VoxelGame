#pragma once

#include <memory>
#include <godot_cpp/classes/ref_counted.hpp>
#include <godot_cpp/classes/wrapped.hpp>
#include "terrain_field.hpp"

class ChunkGenerator : public godot::RefCounted {
    GDCLASS(ChunkGenerator, godot::RefCounted)

    public:
        void set_seed(int seed);
        godot::Dictionary generate_chunk(godot::Vector3 Position, int chunk_scale, bool is_lod_chunk);

    protected:
        static void _bind_methods();

    private:
        std::unique_ptr<TerrainField> field;
};