shader_type canvas_item;

uniform vec4 glow_color: source_color = vec4(0.2, 0.6, 0.3, 1.0);
uniform float glow_power: hint_range(0.0, 8.0, 0.1) = 2.0;
uniform float glow_shift: hint_range(0.0, 10.0, 0.1) = 1.0;
uniform float glow_radius: hint_range(1.0, 10.0, 1.0) = 1.0;
uniform sampler2D screen_texture: hint_screen_texture;

void fragment() {
	vec4 original_color = texture(TEXTURE, UV);

	vec4 glow = vec4(0.0);
	float count = 0.0;
	for (float x = -glow_radius; x <= glow_radius; x+= 1.0)
	{
		for (float y = -glow_radius; y <= glow_radius; y+= 1.0)
		{
			vec2 offset = vec2(x, y) * glow_shift * TEXTURE_PIXEL_SIZE;
			glow += texture(TEXTURE, UV + offset) * glow_color;
			count += 1.0;
		}
	}
	glow *= glow_power / count;

	// Access the background using SCREEN_TEXTURE
	vec4 background = texture(screen_texture, SCREEN_UV);

	// Additive blending of glow and background
	vec4 result_color = original_color + glow * glow.a;

	// add the background to the result color (ensures additive blending with the scene)
	result_color.rgb += background.rgb;

	// Mix the original texture with the glow
	COLOR = vec4(result_color.rgb, original_color.a);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
