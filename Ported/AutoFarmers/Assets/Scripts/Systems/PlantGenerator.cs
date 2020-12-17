﻿using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantGenerator : SystemBase
{
	/*
	//public Random random;
	public static Dictionary<int, Mesh> meshLookup;
	protected override void OnCreate()
    {
		base.OnCreate();
		//random = new Random(1234);
    }*/

    protected override void OnUpdate()
    {
		var commonSettings = GetSingleton<CommonSettings>();
		Entities
            .ForEach((Entity entity, in Plant plant) =>
            {
				//var linearIndex = plant.Position.x + plant.Position.y * commonSettings.GridSize.x;
				//var Mesh = GenerateMesh(linearIndex);
			}).WithoutBurst().Run();
    }
	/*
	Mesh GenerateMesh(int seed)
	{
		Random.State oldRandState = Random.state;
		Random.InitState(seed);

		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Color> colors = new List<Color>();
		List<Vector2> uv = new List<Vector2>();

		Color color1 = Random.ColorHSV(0f, 1f, .5f, .8f, .25f, .9f);
		Color color2 = Random.ColorHSV(0f, 1f, .5f, .8f, .25f, .9f);

		float height = Random.Range(.4f, 1.4f);

		float angle = Random.value * Mathf.PI * 2f;
		float armLength1 = Random.value * .4f + .1f;
		float armLength2 = Random.value * .4f + .1f;
		float armRaise1 = Random.value * .3f;
		float armRaise2 = Random.value * .6f - .3f;
		float armWidth1 = Random.value * .5f + .2f;
		float armWidth2 = Random.value * .5f + .2f;
		float armJitter1 = Random.value * .3f;
		float armJitter2 = Random.value * .3f;
		float stalkWaveStr = Random.value * .5f;
		float stalkWaveFreq = Random.Range(.25f, 1f);
		float stalkWaveOffset = Random.value * Mathf.PI * 2f;

		int triCount = Random.Range(15, 35);

		for (int i = 0; i < triCount; i++)
		{
			// front face
			triangles.Add(vertices.Count);
			triangles.Add(vertices.Count + 1);
			triangles.Add(vertices.Count + 2);

			// back face
			triangles.Add(vertices.Count + 1);
			triangles.Add(vertices.Count);
			triangles.Add(vertices.Count + 2);

			float t = i / (triCount - 1f);
			float armLength = Mathf.Lerp(armLength1, armLength2, t);
			float armRaise = Mathf.Lerp(armRaise1, armRaise2, t);
			float armWidth = Mathf.Lerp(armWidth1, armWidth2, t);
			float armJitter = Mathf.Lerp(armJitter1, armJitter2, t);
			float stalkWave = Mathf.Sin(t * stalkWaveFreq * 2f * Mathf.PI + stalkWaveOffset) * stalkWaveStr;

			float y = t * height;
			vertices.Add(new Vector3(stalkWave, y, 0f));
			Vector3 armPos = new Vector3(stalkWave + Mathf.Cos(angle) * armLength, y + armRaise, Mathf.Sin(angle) * armLength);
			vertices.Add(armPos + Random.insideUnitSphere * armJitter);
			armPos = new Vector3(stalkWave + Mathf.Cos(angle + armWidth) * armLength, y + armRaise, Mathf.Sin(angle + armWidth) * armLength);
			vertices.Add(armPos + Random.insideUnitSphere * armJitter);

			colors.Add(color1);
			colors.Add(color2);
			colors.Add(color2);
			uv.Add(Vector2.zero);
			uv.Add(Vector2.right);
			uv.Add(Vector2.right);

			// golden angle in radians
			angle += 2.4f;
		}

		Mesh outputMesh = new Mesh();
		outputMesh.name = "Generated Plant (" + seed + ")";

		outputMesh.SetVertices(vertices);
		outputMesh.SetColors(colors);
		outputMesh.SetTriangles(triangles, 0);
		outputMesh.RecalculateNormals();

		meshLookup.Add(seed, outputMesh);

		//Farm.RegisterSeed(seed);
		Random.state = oldRandState;
		return outputMesh;
	}
	*/
}