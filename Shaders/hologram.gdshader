shader_type canvas_item;

uniform float glow_intensity = 0.8;   // How strong the glow is
uniform float glow_size = 0.4;       // Size of the glow effect (higher values = more spread)
uniform float distortion_strength = 0.002;  // How strong the distortion is
uniform sampler2D noise_texture;  // Noise texture for distortion (optional)

void fragment() {
    // Get the original texture color and UV coordinates
    vec2 uv = UV;
    vec4 tex_color = texture(TEXTURE, uv);

    // Only apply the effect where the text exists (where alpha > 0)
    if (tex_color.a > 0.1) {
        // Add a glowing fade-out effect toward the edges
        float distance_from_center = length(uv - vec2(0.5, 0.5)); // Distance from the center of UV space
        float glow_factor = smoothstep(0.0, glow_size, 1.0 - distance_from_center); // Fade out toward the edges
        
        // Apply glow to the alpha channel (fade the text at the edges)
        tex_color.a *= glow_factor * glow_intensity;
        
        // Add displacement using noise or sine wave
        vec2 distortion = vec2(sin(TIME + uv.y * 10.0), sin(TIME + uv.x * 15.0)) * distortion_strength;
        
        // Optionally, use noise texture for a more random distortion
        //if (noise_texture != null) {
            vec4 noise_value = texture(noise_texture, uv * 2.0); // Sample noise
            distortion += (noise_value.xy - 0.5) * distortion_strength;
        //}
        
        // Apply the displacement to the UV
        uv += distortion;
        
        // Get the final texture color with the displaced UV
        tex_color = texture(TEXTURE, uv);

        // Glow color (you can adjust the RGB for the holographic look)
        vec3 glow_color = vec3(0.0, 0.8, 1.0); // Holographic blue

        // Blend the glow color with the original texture color
        tex_color.rgb = mix(tex_color.rgb, glow_color, glow_factor * glow_intensity);
    }
    
    // Output the final color
    COLOR = tex_color;
}
