shader_type canvas_item;

uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 1.0, 1.0); // The color of the glow
uniform float glow_intensity : hint_range(0.0, 5.0) = 1.0;          // The intensity of the glow
uniform float glow_radius : hint_range(1.0, 10.0) = 3.0;            // The radius of the glow
uniform float add_intensity : hint_range(0.0, 5.0) = 1.0;           // The intensity of the additive effect

void fragment() {
    vec4 base_color = texture(TEXTURE, UV); // The object's base texture
    vec4 glow = vec4(0.0);                  // Initialize the glow color
    float count = 0.0;

    // Iterate around the current pixel to create the glow effect
    for (float x = -glow_radius; x <= glow_radius; x++) {
        for (float y = -glow_radius; y <= glow_radius; y++) {
            vec2 offset = vec2(x, y) * TEXTURE_PIXEL_SIZE;
            glow += texture(TEXTURE, UV + offset) * glow_color;
            count += 1.0;
        }
    }

    // Average the glow and apply the intensity
    glow = (glow / count) * glow_intensity;

    // Perform additive blending: add glow to the base color and apply the additive intensity
    vec4 additive_color = base_color + glow * add_intensity;

    // Blend the glow with the base color, but retain the object's original color using mix
    vec4 final_color = mix(base_color, additive_color, glow.a);

    // Set the final color
    COLOR = final_color;
}
