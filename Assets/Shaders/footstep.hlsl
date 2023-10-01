void footstep_float(
	float2 uv, float step_mask, float progress, float3 footstep_color,
	float pixels, out float3 out_color, out float out_alpha
) {
/*
	uv *= pixels;
	uv = round(uv);
	float2 pixel_grid = uv;
	uv /= pixels;

	uint seed = 0x578437adU;
	float perlin_noise = perlinNoise(uv, 3, 3, 0.5, 2.0, seed) * 0.5 + 0.5;

	uv -= 0.5;

	float2 direction = float2(0,0) - uv;
	float distance = length(direction) * 2 + perlin_noise * 0.25;

	float wave_distance = distance - progress;
	float wave = smoothstep(-wave_width, 0, wave_distance);
	if (wave >= 1) wave = 0;

	wave *= smoothstep(1, 0.75, progress);
	float4 wave_shading = get_shaded_waves(pixel_grid, wave, wave_color);

	out_color = wave_shading.rgb;
	out_alpha = wave_shading.a;
*/

	out_color = footstep_color;
	out_alpha = step_mask /* * (1-progress)*/;
}
