shader_type canvas_item;

uniform vec2 light_position = vec2(0.5, 0.5);  // Center of the light
uniform float light_radius : hint_range(0.0, 1.0) = 0.5;  // Radius of the light (how far the light reaches)
uniform float shadow_density : hint_range(0.0, 5.0) = 1.0;  // Controls how dense the shadow becomes

void fragment() {
    // Get the base texture color
    vec4 base_color = texture(TEXTURE, UV);

    // Calculate the distance from the current fragment to the light source
    vec2 uv = UV;
    float distance_to_light = distance(uv, light_position);

    // Calculate the light intensity based on the distance
    // The light is fully bright at the light position and fades out toward the light radius
    float light_intensity = smoothstep(light_radius, 0.0, distance_to_light) * shadow_density;

    // Invert the light intensity so it decreases as the distance increases
    light_intensity = light_intensity;

    // Apply the shadow effect by multiplying the base texture color with the light intensity
    vec4 final_color = vec4(base_color.rgb * light_intensity, base_color.a);


// Discard everything outside the radius of the texture
	vec2 center = vec2(0.5, 0.5); // Center of the Viewport (normalized coordinates)
	vec2 local_coords = (UV - center); // Adjust for aspect ratio
	float distance = length(local_coords);
	float circleRadius = 1.0 / float(2);
	
	// Discard fragments outside of the circle
	if (distance > circleRadius)
	{
		discard;
	}
	
    // Set the final fragment color
    COLOR = final_color;
}

