shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0) = 0.5; // Progress percentage (0 to 1)
uniform vec2 center = vec2(0.5, 0.5); // Center of the circle
uniform float radius : hint_range(0.0, 1.0) = 0.4; // Radius of the circle
uniform float thickness : hint_range(0.0, 0.5) = 0.05; // Thickness of the arc
uniform vec4 color : source_color = vec4(0.0, 1.0, 0.0, 1.0); // Arc color

void fragment() {
    // Normalize the UV coordinates relative to the center
    vec2 uv = UV - center;
    float distance = length(uv);

    // Calculate the angle of the current fragment
    float angle = atan(uv.y, uv.x); 
    float normalized_angle = (angle + 3.14159) / (2.0 * 3.14159); // Map -PI to PI -> 0 to 1

    // Check if the fragment is within the radius and progress range
    if (distance > radius - thickness && distance < radius + thickness && normalized_angle <= progress) {
        COLOR = color;
    } else {
        discard; // Discard fragments outside the arc
    }
}
