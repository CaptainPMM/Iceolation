// implementation of MurmurHash (https://sites.google.com/site/murmurhash/) for a 
// single unsigned integer.

uint hash(uint x, uint seed) {
    const uint m = 0x5bd1e995U;
    uint hash = seed;
    // process input
    uint k = x;
    k *= m;
    k ^= k >> 24;
    k *= m;
    hash *= m;
    hash ^= k;
    // some final mixing
    hash ^= hash >> 13;
    hash *= m;
    hash ^= hash >> 15;
    return hash;
}

// implementation of MurmurHash (https://sites.google.com/site/murmurhash/) for a  
// 2-dimensional unsigned integer input vector.

uint hash(float2 x, uint seed){
    const uint m = 0x5bd1e995U;
    uint hash = seed;
    // process first vector element
    uint k = x.x; 
    k *= m;
    k ^= k >> 24;
    k *= m;
    hash *= m;
    hash ^= k;
    // process second vector element
    k = x.y; 
    k *= m;
    k ^= k >> 24;
    k *= m;
    hash *= m;
    hash ^= k;
	// some final mixing
    hash ^= hash >> 13;
    hash *= m;
    hash ^= hash >> 15;
    return hash;
}


float2 gradientDirection(uint hash) {
    switch (int(hash) & 3) { // look at the last two bits to pick a gradient direction
    case 0:
        return float2(1.0, 1.0);
    case 1:
        return float2(-1.0, 1.0);
    case 2:
        return float2(1.0, -1.0);
    case 3:
        return float2(-1.0, -1.0);
    }
}

float interpolate(float value1, float value2, float value3, float value4, float2 t) {
    return lerp(lerp(value1, value2, t.x), lerp(value3, value4, t.x), t.y);
}

float2 fade(float2 t) {
    // 6t^5 - 15t^4 + 10t^3
	return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

float perlinNoise(float2 position, uint seed) {
    float2 floorPosition = floor(position);
    float2 fractPosition = position - floorPosition;
    float2 cellCoordinates = float2(floorPosition);
    float value1 = dot(gradientDirection(hash(cellCoordinates, seed)), fractPosition);
    float value2 = dot(gradientDirection(hash((cellCoordinates + float2(1, 0)), seed)), fractPosition - float2(1.0, 0.0));
    float value3 = dot(gradientDirection(hash((cellCoordinates + float2(0, 1)), seed)), fractPosition - float2(0.0, 1.0));
    float value4 = dot(gradientDirection(hash((cellCoordinates + float2(1, 1)), seed)), fractPosition - float2(1.0, 1.0));
    return interpolate(value1, value2, value3, value4, fade(fractPosition));
}

float perlinNoise(float2 position, int frequency, int octaveCount, float persistence, float lacunarity, uint seed) {
    float value = 0.0;
    float amplitude = 1.0;
    float currentFrequency = float(frequency);
    uint currentSeed = seed;
    for (int i = 0; i < octaveCount; i++) {
        currentSeed = hash(currentSeed, 0x0U); // create a new seed for each octave
        value += perlinNoise(position * currentFrequency, currentSeed) * amplitude;
        amplitude *= persistence;
        currentFrequency *= lacunarity;
    }
    return value;
}

float4 get_shaded_waves(
	float2 pixel_grid, float intensity, float3 wave_color
) {
	float checker_board = fmod(pixel_grid.x + pixel_grid.y,2);
	float checker_board_2 = fmod(pixel_grid.x + pixel_grid.y,3);

	if (intensity > 0.995) return float4(0.5, 0.5, 0.5, 1);
	else if (intensity > 0.93) return float4(wave_color,1);
	else if (intensity > 0.8 && checker_board_2) return float4(wave_color,1);
	else if (intensity > 0.5 && checker_board) return float4(wave_color,1);
	else if (intensity > 0.3 && !checker_board) return float4(wave_color*0.4,1);
	else if (intensity > 0.1 && !checker_board_2) return float4(wave_color*0.4,1);
	else return 0;
}

void ocean_float(
	float2 screen_position, float time, float number_of_waves,
	float wave_length, float wave_width, float wave_chaoticness,
	float scroll_speed, float3 background_color, float3 wave_color,
	out float3 out_color
) {
	float ratio = 16.0/9.0;
	screen_position.x = 1 - screen_position.x;
	screen_position.x *= ratio;
    screen_position *= 2;
	screen_position.x += + scroll_speed/number_of_waves * time;
	screen_position *= 150;
	screen_position = floor(screen_position);
	float2 pixel_grid = screen_position;
	screen_position /= 150;
	float x = screen_position.x * number_of_waves;

	uint seed = 0x578437adU;
	float noise_frequency = 1;
	float2 noise_coord = float2(x, screen_position.y * number_of_waves);
	noise_coord.x += time * 0.1;
	float perlin_noise = perlinNoise(noise_coord, noise_frequency, 3, 0.5, 2.0, seed);
	noise_coord.x -= time * 0.3;
	float perlin_noise_3 = perlinNoise(noise_coord*0.6, seed);
	noise_coord.x += time * 0.2;
	float perlin_noise_2 = perlinNoise(noise_coord*0.4, seed);
	noise_coord.x += time * scroll_speed / 5;
	noise_coord.x /= 10;
	float perlin_noise_4 = perlinNoise(noise_coord, 4, 3, 0.5, 2.0, seed);
	
	float wave_distance = 0.1;
	float o = 1 - wave_distance;
	float number_vertical_waves = 1/wave_width;
	float y = screen_position.y;
	float wave_mask = (sin((y+perlin_noise_3*0.35)*number_vertical_waves * 2*PI) + o) / (1+o);
	wave_mask = max(0,wave_mask);
	float wave_offset = wave_mask * 0.5;

	float intensity = frac(x + wave_offset + perlin_noise_2*0.4 + perlin_noise*wave_chaoticness);
	intensity = 1 - intensity;
	float wave_id = ceil(x + wave_offset + perlin_noise*wave_chaoticness);

	intensity = smoothstep(1-wave_length, 1, intensity);
	intensity = lerp(0, intensity, smoothstep(0,0.5,wave_mask));
	intensity = clamp(intensity, 0, 1);

	out_color = intensity;
	out_color.gb *= fmod(wave_id,2);

	if (perlin_noise_4 > 0.5) out_color = background_color*0.4;
	else if (perlin_noise_4 > 0) out_color = background_color*0.6;
	else out_color = background_color;

	float4 shading = get_shaded_waves(pixel_grid, intensity, wave_color);

	out_color = lerp(out_color, shading, shading.a);
}

void wave_float(
	float2 uv, float progress, float wave_width, float pixels, float3 wave_color,
	float wave_sdf,
	out float3 out_color, out float out_alpha
) {
	uv *= pixels;
	uv = round(uv);
	float2 pixel_grid = uv;
	uv /= pixels;

	uint seed = 0x578437adU;
	float perlin_noise = perlinNoise(uv, 3, 3, 0.5, 2.0, seed) * 0.5 + 0.5;

	uv -= 0.5;

	float2 direction = float2(0,0) - uv;
	float distance = length(direction) * 2;

	distance = wave_sdf;

	//out_color = distance;
	//out_alpha = 1;
	//return;

	distance += perlin_noise * 0.25;

	float wave_distance = distance - progress;
	float wave = smoothstep(-wave_width, 0, wave_distance);
	if (wave >= 1) wave = 0;

	wave *= smoothstep(1, 0.75, progress);
	float4 wave_shading = get_shaded_waves(pixel_grid, wave, wave_color);

	out_color = wave_shading.rgb;
	out_alpha = wave_shading.a;
}
