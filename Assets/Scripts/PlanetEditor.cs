using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetController))]
public class PlanetEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		var planet = (PlanetController) target;

		PlanetGenerator.SeaLevel = EditorGUILayout.FloatField("Sea Level", PlanetGenerator.SeaLevel);
		PlanetGenerator.BeachLevel = EditorGUILayout.FloatField("Beach Level", PlanetGenerator.BeachLevel);
		PlanetGenerator.SeaHumiditySpread = EditorGUILayout.FloatField("Sea Humidity Spread", PlanetGenerator.SeaHumiditySpread);

		if (GUILayout.Button("Generate")) {
			planet.Generate();
		}
	}
}