shader_type canvas_item;
uniform float radius;
uniform float fade_intensity : hint_range(0.0, 5.0) = 3.0;       // How quickly the alpha value decreases
uniform sampler2D noise_texture: filter_nearest, repeat_enable;  // Noise texture for distortion
uniform float noise_scale : hint_range(0.0, 10.0) = 1.0;         // Scale of the noise texture
uniform float noise_speed : hint_range(0.0, 5.0) = 1.0;          // Speed of noise movement
uniform float z_offset_speed : hint_range(0.0, 5.0) = 1.0;

// Uniforms to control the impact area and intensity
uniform vec2 alpha_focus_point = vec2(0.0, 0.0);                 // Focus point for the impact glow
uniform float alpha_focus_intensity : hint_range(0.0, 10.0) = 2.0; // Intensity of the focus glow
uniform float hit_intensity : hint_range(0.0, 5.0) = 1.0;         // Glow intensity when hit

void fragment() {
    // Use UV directly as a global space, decoupling from the ship's rotation
    vec2 local_coords = UV - vec2(0.5, 0.5);  // Centered UV coordinates
    float distance_from_center = length(local_coords); // Calculate the distance from the center

    // Define the radius of the forcefield
    float circleRadius = radius / float(2);

    // Discard fragments outside the circle
    if (distance_from_center > circleRadius) {
        discard;
    }

    // Calculate the distance from the hit focus point (impact)
    vec2 intensity_point = alpha_focus_point - UV; // Keep using UV directly
    float distance_to_focus = length(intensity_point);

    // Intensify the alpha around the focus point when the forcefield is hit
    float focus_alpha = pow(smoothstep(alpha_focus_intensity, 0.0, distance_to_focus), 1.0) * hit_intensity;

    // Calculate the alpha based on the distance from the center of the shield
    float alpha = pow(smoothstep(0.0, fade_intensity, distance_from_center), 5.0);
    alpha += focus_alpha;

    // Add a dynamic z-offset based on time for the noise texture
    float z_offset = TIME * z_offset_speed;
    vec2 noise_uv = UV * noise_scale + vec2(z_offset, z_offset);

    // Get the color from the noise texture and apply it
    vec4 noise_color = texture(noise_texture, noise_uv);

    // Modulate the alpha by the noise texture
    alpha *= noise_color.a;

    // Set the final color
    vec4 final_color = vec4(noise_color.rgb, alpha);
    COLOR = final_color;
}
