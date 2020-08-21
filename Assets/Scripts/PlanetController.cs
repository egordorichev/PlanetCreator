using System;
using UnityEngine;

public class PlanetController : MonoBehaviour {
	public int SubDivisions = 0;
	public float Radius = 1f;
	public float RotationSpeed = 10f;
	public float NoiseScale = 1f;
	public float SampleScale = 1f;
	public int TextureSize = 1024;
	public float OrbitSpeed = 10f;
	public float FrequencyMultiplier = 2.5f;
	public float AmplitudeMultiplier = 0.4f;

	public bool Moon;

	private float angle;
	private float distance;

	public void Generate() {
		GetComponent<MeshFilter>().mesh = PlanetGenerator.Generate(GetComponent<MeshRenderer>().material, TextureSize, Radius, SubDivisions, SampleScale, NoiseScale, Moon, AmplitudeMultiplier, FrequencyMultiplier);
	}

	public void Start() {
		Generate();
		
		angle = Mathf.Atan2(transform.position.z, transform.position.x);
		distance = (Vector3.zero - transform.position).magnitude;
	}

	public void Update() {
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + RotationSpeed * Time.deltaTime, 0);

		if (Moon) {
			angle += Time.deltaTime * OrbitSpeed;
			transform.position = new Vector3(Mathf.Cos(angle * (float) Math.PI * 2 / 360f) * distance, 0, Mathf.Sin(angle * (float) Math.PI * 2 / 360f) * distance);
		}
	}
	
	/*public void OnDrawGizmosSelected() {
		Gizmos.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh, transform.position);
	}*/
}