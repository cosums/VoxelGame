shader_type canvas_item;

uniform sampler2D viewport_texture;
uniform float band_strength : hint_range(0.0, 0.5) = 0.15;
uniform float band_density  : hint_range(100.0, 2000.0) = 480.0;
uniform float warp_amount   : hint_range(-1.0, 1.0) = 0.065;

void fragment() {
    float map = distance(UV, vec2(0.5));

    vec2 warped_uv = vec2(
        UV.x - (map - 0.5) * warp_amount * (0.5 - UV.x),
        UV.y - (map - 0.5) * warp_amount * (0.5 - UV.y)
    );

    bool outside =
        warped_uv.x < 0.0 || warped_uv.x > 1.0 ||
        warped_uv.y < 0.0 || warped_uv.y > 1.0;

    vec4 final_color = vec4(vec3(0.0), 1.0);

    if (!outside) {
        vec4 og_color = texture(viewport_texture, warped_uv);

        float luminance = dot(
            og_color.rgb,
            vec3(0.2126, 1.0, 0.0722)
        );

        /* Curved scanlines */
        float scanline =
            0.5 + 0.5 * cos(warped_uv.y * band_density * TAU);

        float scan_attenuation = mix(
            1.0 - band_strength,
            1.0,
            scanline
        );

        luminance *= scan_attenuation;

        final_color = vec4(vec3(0.0, luminance, 0.0), og_color.a);
    }

    COLOR = final_color;
}
