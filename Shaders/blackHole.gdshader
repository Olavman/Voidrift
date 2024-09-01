shader_type canvas_item;

// Declare the SCREEN_TEXTURE with the necessary hints
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform sampler2D curve;
uniform float strength : hint_range(-1, 1) = 0.5;

void fragment() {
    // Calculate vector to the center of the screen
    vec2 vecToCenter = vec2(0.5, 0.5) - UV;

    // Calculate distance to the center
    float distToCenter = length(vecToCenter);

    // Sample the curve texture based on the distance
    float curveVal = texture(curve, vec2(distToCenter)).r;

    // Calculate the distortion based on strength and curve value
    vec2 diff = normalize(vecToCenter) * strength * curveVal;

    // Apply the distortion to the screen texture
    vec2 distortedUV = SCREEN_UV - diff;
    COLOR = texture(SCREEN_TEXTURE, distortedUV);
}
