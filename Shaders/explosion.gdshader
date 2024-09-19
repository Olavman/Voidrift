shader_type canvas_item;

void fragment() {
    vec4 texColor = texture(TEXTURE, UV);

    // Additive blending: add the particle's color to the background
    COLOR = texColor * COLOR;
    COLOR.rgb *= COLOR.a;  // Make sure the alpha affects the brightness
}
