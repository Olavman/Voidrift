shader_type canvas_item;

uniform vec4 color = vec4(0.0, 0.5, 1.0, 1.0); // This defines the color of the force field
uniform float glow_strength = 0.5; // The strength of the glow effect
uniform float radius = 0.5; // The radius of the force field
uniform float thickness = 0.1; // The thickness of the glowing edge

void fragment() {
    vec2 uv = FRAGCOORD.xy / SCREEN_PIXEL_SIZE;
    float dist = length(uv - vec2(0.5, 0.5));
    float edge = smoothstep(radius - thickness, radius, dist);
    COLOR = mix(vec4(0.0), color * glow_strength, edge);
}
