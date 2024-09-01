shader_type canvas_item;
uniform float radius;

void fragment() {
	// Calculate the distance from the center of the Viewport to the current fragment
	vec2 center = vec2(0.5, 0.5); // Center of the Viewport (normalized coordinates)
	vec2 local_coords = (UV - center); // Adjust for aspect ratio
	float distance = length(local_coords);
	
	// Define the radius of the circle (0.5 for a circle that fits within the viewport)
	float circleRadius = radius / float(2);
	
	// Discard fragments outside of the circle
	if (distance > circleRadius)
	{
		discard;
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
