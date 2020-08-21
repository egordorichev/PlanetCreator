using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlanetGenerator {
	private static Vector3[] directions = {
		Vector3.left,
		Vector3.back,
		Vector3.right,
		Vector3.forward
	};
	
	private static void Normalize(Vector3[] vertices, Vector3[] normals) {
		for (var i = 0; i < vertices.Length; i++) {
			normals[i] = vertices[i] = vertices[i].normalized;
		}
	}

	private static void CreateUV(Vector3[] vertices, Vector2[] uv) {
		var length = vertices.Length;
		var prevX = 1f;
		
		for (var i = 0; i < length; i++) {
			var v = vertices[i];

			if (v.x == prevX) {
				uv[i - 1].x = 1f;
			}

			prevX = v.x;
			var x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI);

			if (x < 0) {
				x++;
			}
			
			uv[i] = new Vector2(x, Mathf.Asin(v.y) / Mathf.PI + 0.5f);
		}

		uv[length - 4].x = uv[0].x = 0.125f;
		uv[length - 3].x = uv[1].x = 0.375f;
		uv[length - 2].x = uv[2].x = 0.625f;
		uv[length - 1].x = uv[3].x = 0.875f;
	}

	private static int CreateVertexLine(Vector3 from, Vector3 to, int steps, int v, Vector3[] vertices) {
		for (var i = 1; i <= steps; i++) {
			vertices[v++] = Vector3.Lerp(from, to, (float) i / steps);
		}

		return v;
	}

	private static int CreateLowerStrip(int steps, int vTop, int vBottom, int t, int[] triangles) {
		for (var i = 1; i < steps; i++) {
			triangles[t++] = vBottom;
			triangles[t++] = vTop - 1;
			triangles[t++] = vTop;
			
			triangles[t++] = vBottom++;
			triangles[t++] = vTop++;
			triangles[t++] = vBottom;
		}
		
		triangles[t++] = vBottom;
		triangles[t++] = vTop - 1;
		triangles[t++] = vTop;
		
		return t;
	}

	private static int CreateUpperStrip(int steps, int vTop, int vBottom, int t, int[] triangles) {
		triangles[t++] = vBottom;
		triangles[t++] = vTop - 1;
		triangles[t++] = ++vBottom;
		
		for (var i = 1; i <= steps; i++) {
			triangles[t++] = vTop - 1;
			triangles[t++] = vTop;
			triangles[t++] = vBottom;
			
			triangles[t++] = vBottom;
			triangles[t++] = vTop++;
			triangles[t++] = ++vBottom;
		}

		return t;
	}

	private static void CreateOctahedron(Vector3[] vertices, int[] triangles, int resolution) {
		var v = 0;
		var vBottom = 0;
		var t = 0;

		for (var i = 0; i < 4; i++) {
			vertices[v++] = Vector3.down;
		}
		
		for (var i = 1; i <= resolution; i++) {
			var progress = (float) i / resolution;
			var to = vertices[v++] = Vector3.Lerp(Vector3.down, Vector3.forward, progress);
			Vector3 from;

			for (var d = 0; d < 4; d++) {
				from = to;
				
				to = Vector3.Lerp(Vector3.down, directions[d], progress);
				t = CreateLowerStrip(i, v, vBottom, t, triangles);
				v = CreateVertexLine(from, to, i, v, vertices);

				vBottom += i > 1 ? (i - 1) : 1;
			}

			vBottom = v - 1 - i * 4;
		}
		
		for (var i = resolution - 1; i >= 1; i--) {
			var progress = (float) i / resolution;
			var to = vertices[v++] = Vector3.Lerp(Vector3.up, Vector3.forward, progress);
			Vector3 from;

			for (var d = 0; d < 4; d++) {
				from = to;
				
				to = Vector3.Lerp(Vector3.up, directions[d], progress);
				t = CreateUpperStrip(i, v, vBottom, t, triangles);
				v = CreateVertexLine(from, to, i, v, vertices);

				vBottom += i + 1;
			}

			vBottom = v - 1 - i * 4;
		}

		for (var i = 0; i < 4; i++) {
			triangles[t++] = vBottom;
			triangles[t++] = v;
			triangles[t++] = ++vBottom;
			
			vertices[v++] = Vector3.up;
		}
	}

	private static float Sin(float a) {
		return Mathf.Sin(a * (float) Math.PI * 2);
	}

	private static float Cos(float a) {
		return Mathf.Cos(a * (float) Math.PI * 2);
	}

	private static float AdjustedNoise(float textureSize, float tx, float ty, float scale) {
		return Noise(tx / textureSize, (ty / textureSize + 0.5f) * 0.5f, scale);
	}

	private static float Noise(float tx, float ty, float scale) {
		var scl = Cos(ty);
		return OctavedNoise(scl * Sin(tx) * scale, -Sin(ty) * scale, scl * Cos(tx) * scale);
	}

	public static float FrequencyMultiplier = 2.5f;
	public static float AmplitudeMultiplier = 0.4f;

	private static float seed;
	
	private static float OctavedNoise(float x, float y, float z) {
		var noise = 0f;
		var frequency = 1f;
		var amplitude = 1f;
		
		for (var i = 0; i < 4; i++) {
			noise += Perlin.Noise(x * frequency, y * frequency, z * frequency) * amplitude;
			frequency *= FrequencyMultiplier;
			amplitude *= AmplitudeMultiplier;
		}

		return Mathf.Clamp(noise, -1f, 1f);
	}
	
	private static float[,] GenerateHeightMap(float textureSize, float sampleScale, float noiseScale) {
		var map = new float[(int) textureSize, (int) textureSize];

		for (var x = 0; x < textureSize; x++) {
			for (var y = 0; y < textureSize; y++) {
				map[x, y] = Mathf.Clamp((AdjustedNoise(textureSize, x, y, sampleScale) * 0.5f + 0.5f) * noiseScale, 0, 1);
			}
		}

		return map;
	}
	
	public static float SeaLevel = 0.5f;
	public static float BeachLevel = 0.05f;
	public static float SeaHumiditySpread = 0.34f;

	private static float[,] GenerateHumidityMap(int textureSize, float[,] heightMap) {
		var map = new float[textureSize, textureSize];

		for (var x = 0; x < textureSize; x++) {
			for (var y = 0; y < textureSize; y++) {
				var height = heightMap[x, y];
				var value = 0f;

				if (height <= SeaLevel) {
					value = 1f;
				} else if (height <= SeaLevel + SeaHumiditySpread) {
					value = 0.5f; // Mathf.Lerp(1f, 0f, height / SeaLevel);
				}
				
				map[x, y] = value;
			}
		}

		return map;
	}
	
	private static float[,] GenerateTemperatureMap(int textureSize, float[,] heightMap, float noiseScale) {
		var map = new float[textureSize, textureSize];

		for (var x = 0; x < textureSize; x++) {
			for (var y = 0; y < textureSize; y++) {
				var value = Mathf.Sin((float) y / textureSize * (float) Math.PI);

				value += (heightMap[x, y] - 1f) * 2f * noiseScale;
				map[x, y] = Mathf.Clamp(value, 0, 1);
			}
		}

		return map;
	}

	private static Color[] biomeColors = new Color[(int) Biome.Total];
	private static Color[] secondBiomeColors = new Color[(int) Biome.Total];

	static PlanetGenerator() {
		biomeColors[(int) Biome.DeepOcean] = new Color(0, 57 / 255f, 109 / 255f);
		biomeColors[(int) Biome.Ocean] = new Color(0, 105 / 255f, 170 / 255f);
		biomeColors[(int) Biome.Beach] = new Color(246 / 255f, 202 / 255f, 159 / 255f);
		biomeColors[(int) Biome.Forest] = new Color(51 / 255f, 152 / 255f, 75 / 255f);
		biomeColors[(int) Biome.Desert] = new Color(1f, 200 / 255f, 37 / 255f);
		biomeColors[(int) Biome.Snow] = new Color(1f, 1f, 1f);
		biomeColors[(int) Biome.FrozenOcean] = new Color(0, 152 / 255f, 220 / 255f);
		biomeColors[(int) Biome.Mountains] = new Color(101 / 255f, 115 / 255f, 146 / 255f);
		
		secondBiomeColors[(int) Biome.DeepOcean] = new Color(25 / 255f, 26 / 255f, 50 / 255f);
		secondBiomeColors[(int) Biome.Ocean] = new Color(0, 57 / 255f, 109 / 255f);
		secondBiomeColors[(int) Biome.Beach] = new Color(191 / 255f, 111 / 255f, 74 / 255f);
		secondBiomeColors[(int) Biome.Forest] = new Color(12 / 255f, 46 / 255f, 68 / 255f);
		secondBiomeColors[(int) Biome.Desert] = new Color(237 / 255f, 118 / 255f, 20 / 255f);
		secondBiomeColors[(int) Biome.Snow] = new Color(0.8f, 0.8f, 0.9f);
		secondBiomeColors[(int) Biome.FrozenOcean] = new Color(0, 130 / 255f, 180 / 255f);
		secondBiomeColors[(int) Biome.Mountains] = new Color(42 / 255f, 47 / 255f, 78 / 255f);
	}

	private static Biome DecideBiome(float height, float humidity, float temperature) {
		if (height <= SeaLevel + 0.01f) {
			/*if (temperature < 0.4f) {
				return Biome.FrozenOcean;
			}*/

			if (height <= SeaLevel * 0.75f) {
				return Biome.DeepOcean;
			}

			return Biome.Ocean;
		}

		if (temperature < 0.3f) {
			return Biome.Snow;
		}
		
		if (height <= SeaLevel + BeachLevel) {
			return Biome.Beach;
		}

		if (height >= SeaLevel + BeachLevel + 0.2) {
			if (height >= SeaLevel + BeachLevel + 0.4) {
				return Biome.Snow;
			}

			return Biome.Mountains;
		}

		if (humidity > 0.1f && temperature < 0.85f) {
			return Biome.Forest;
		}

		return Biome.Desert;
	}
	
	private static Texture2D CreateMoonTexture(float textureSize, float[,] heightMap) {
		var texture = new Texture2D((int) textureSize, (int) textureSize, TextureFormat.RGB24, false);

		for (var y = 0; y < textureSize; y++) {
			for (var x = 0; x < textureSize; x++) {
				var v = heightMap[x, y];
				texture.SetPixel(x, y, new Color(v, v, v));
			}
		}

		texture.Apply();
		texture.name = "Moon Surface";
		
		return texture;
	}

	private static Texture2D CombineMaps(float textureSize, float[,] heightMap, float[,] humidityMap, float[,] temperatureMap) {
		var texture = new Texture2D((int) textureSize, (int) textureSize, TextureFormat.RGB24, false);

		for (var y = 0; y < textureSize; y++) {
			for (var x = 0; x < textureSize; x++) {
				var height = heightMap[x, y];
				var humidity = humidityMap[x, y];
				var temperature = temperatureMap[x, y];

				var biome = DecideBiome(height, humidity, temperature);

				if (biome == Biome.Ocean || biome == Biome.DeepOcean) {
					heightMap[x, y] = SeaLevel;
				} else if (biome == Biome.Forest) {
					heightMap[x, y] += 0.05f;
				} else if (biome == Biome.Desert) {
					heightMap[x, y] = SeaLevel + 0.1f + (AdjustedNoise(textureSize, x, y, 10) * 0.5f + 0.5f) * 0.1f;
				} else if (biome == Biome.Mountains) {
					var v = heightMap[x, y];

					if (v < 1) {
						heightMap[x, y] = v / v;
					} else {
						heightMap[x, y] = v * v;
					}
				}

				var color = biomeColors[(int) biome];
				
				texture.SetPixel(x, y, Color.Lerp(color, secondBiomeColors[(int) biome], (AdjustedNoise(textureSize, x + (int) biome * 10000, y, 5) * 0.5f + 0.5f)));
			}
		}

		texture.Apply();
		texture.name = "Not So Surface";

		return texture;
	}

	public static Mesh Generate(Material material, int textureSize, float radius, int divisions, float sampleScale, float noiseScale, bool moon, float amplitude, float frequency) {
		if (divisions > 6) {
			divisions = 6;
		}

		AmplitudeMultiplier = amplitude;
		FrequencyMultiplier = frequency;
		
		seed = Random.Range(-12321321, 123213213);

		var resolution = 1 << divisions;
		var vertices = new Vector3[(resolution + 1) * (resolution + 1) * 4 - (resolution * 2 - 1) * 3];
		var verticesCount = vertices.Length;
		var triangles = new int[(1 << (divisions * 2 + 3)) * 3];

		CreateOctahedron(vertices, triangles, resolution);
		
		var normals = new Vector3[verticesCount];
		Normalize(vertices, normals);
		
		var uv = new Vector2[verticesCount];
		CreateUV(vertices, uv);

		var heightMap = GenerateHeightMap(textureSize, sampleScale, 1f);
		Texture2D texture;

		if (moon) {
			texture = CreateMoonTexture(textureSize, heightMap);
		} else {
			var humidityMap = GenerateHumidityMap(textureSize, heightMap);
			var temperatureMap = GenerateTemperatureMap(textureSize, heightMap, noiseScale);
			texture = CombineMaps(textureSize, heightMap, humidityMap, temperatureMap);
		}

		material.SetTexture("_MainTex", texture);

		for (var i = 0; i < verticesCount; i++) {
			var vertex = vertices[i];
			var u = uv[i];

			vertices[i] += vertex.normalized * (heightMap[Mathf.Clamp((int) (u.x * textureSize), 0, textureSize - 1), Mathf.Clamp((int) (u.y * textureSize), 0, textureSize - 1)] * noiseScale);
		}
		
		if (Math.Abs(radius - 1f) > 0.01f) {
			for (var i = 0; i < verticesCount; i++) {
				vertices[i] *= radius;
			}
		}

		var mesh = new Mesh();

		mesh.name = "RUN FOOL";
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;

		mesh.RecalculateNormals();
		
		return mesh;
	}
}