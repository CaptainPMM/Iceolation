void footstep_float(
	float2 uv, float step_mask, float progress, float3 footstep_color,
	float pixels, out float3 out_color, out float out_alpha
) {
	// maybe i'll add a "particle" effect if there's time
	out_color = footstep_color;
	out_alpha = step_mask * (1-progress);
}
