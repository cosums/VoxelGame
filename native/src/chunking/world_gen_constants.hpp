#pragma once

#include <cstdint>

inline constexpr int CHUNK_SIZE = 16;
inline constexpr int POINT_SIZE = CHUNK_SIZE + 1;
inline constexpr int SAMPLE_SIZE = CHUNK_SIZE + 2;

inline constexpr int CHUNK_HEIGHT = 512;

inline constexpr int LOD_CUTOFF = 150;

using MaterialId = uint8_t; // if you ever need more mats just up this to match the C# side

// inline int get_flat_index(int x, int y, int z) {
//     return z + SAMPLE_SIZE * y + SAMPLE_SIZE * SAMPLE_SIZE * x;
// }