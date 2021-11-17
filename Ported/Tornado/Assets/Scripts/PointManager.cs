﻿#define DEBUG_DOTS
//#define SYNCED_SIMULATION
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class PointManager : MonoBehaviour
{
    const float spacing = 2f;
    const int instancesPerBatch = 1023;

    public Mesh barMesh;
    public Material barMaterial;

    [Range(0, 999)] public int buildingCount;
    [Range(0, 10000)] public int groundDetailCount;
    [Range(0f, 1f)] public float breakResistance;
    [Range(0f, 1f)] public float damping;
    [Range(0f, 1f)] public float friction;

    Tornado[] tornados;
    MeshRenderer ground;
    internal Matrix4x4[][] matrices;
    MaterialPropertyBlock[] matProps;
    float3 groundExtents;
    int maxTornadoHeight;

    internal void Awake()
    {
        Time.timeScale = 0f;

        ground = GetComponent<MeshRenderer>();
        tornados = Resources.FindObjectsOfTypeAll<Tornado>();

        groundExtents = ground.bounds.extents;
        maxTornadoHeight = Mathf.FloorToInt(tornados.Max(t => t.height) * .9f / spacing);
    }

    internal void Start()
    {
        #if DEBUG_DOTS
        using (new DebugTimer("Generate buildings", 500d))
        #endif
        { 
            Generate();
            Time.timeScale = 1f;
        }
    }

    #if SYNCED_SIMULATION
    int frameCount = 1;
    double totalTimeMs = 0;
    internal void FixedUpdate()
    {
        #if DEBUG_DOTS
        using (var t = new DebugTimer($"Simulate ({frameCount} frames, avg. {totalTimeMs / frameCount:F1} ms)", frameCount++ % 300 == 0 ? 8d : double.MaxValue))
        #endif
        {
            Simulate();
            totalTimeMs += t.timeMs;
        }
    }
    #endif

    internal void Update()
    {
        #if DEBUG_DOTS
        using (var t = new DebugTimer($"Rendering {matrices.Length} batches", 3d))
        #endif
        {
            for (int i = 0; i < matrices.Length; i++)
                Graphics.DrawMeshInstanced(barMesh, 0, barMaterial, matrices[i], matrices[i].Length, matProps[i]);
        }
    }

    internal Entity CreatePoint(EntityManager em, in float3 pos, in bool anchored, in int neighborCount = 0)
    {
        var point = em.CreateEntity();
        em.AddComponentData(point, new Point(pos) { neighborCount = neighborCount });
        em.AddComponentData(point, new PointDamping(1f - damping, friction));
        if (anchored)
            em.AddComponent<AnchoredPoint>(point);
        else
            em.AddComponent<DynamicPoint>(point);
        return point;
    }

    void Generate()
    {
        var random = new Unity.Mathematics.Random(1234);
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create buildings
        var groundLimits = groundExtents;
        var buildingGroundLimits = groundLimits * 0.9f;

        for (int i = 0; i < buildingCount; i++)
        {
            int height = random.NextInt(4, maxTornadoHeight);
            var pos = random.NextFloat3(-buildingGroundLimits, buildingGroundLimits);
            for (int j = 0; j < height; j++)
            {
                var anchored = j == 0;
                var jspace = j * spacing;

                CreatePoint(em, new float3(pos.x + spacing, jspace, pos.z - spacing), anchored);
                CreatePoint(em, new float3(pos.x - spacing, jspace, pos.z - spacing), anchored);
                CreatePoint(em, new float3(pos.x + 0f, jspace, pos.z + spacing), anchored);
            }
        }

        // Create ground details
        for (int i = 0; i < groundDetailCount; i++)
        {
            var pos = random.NextFloat3(-groundLimits, groundLimits);

            CreatePoint(em, new float3(pos.x + random.NextFloat(-.2f, -.1f), pos.y + random.NextFloat(0f, 3f), pos.z + random.NextFloat(.1f, .2f)), false);
            CreatePoint(em, new float3(pos.x + random.NextFloat(.2f, .1f), pos.y + random.NextFloat(0f, .2f), pos.z + random.NextFloat(-.1f, -.2f)), random.NextFloat() < .1f);
        }

        // Create beams
        int beamCount = 0;
        int matrixBatchIndex = 0;
        var matricesList = new List<List<Matrix4x4>> { new List<Matrix4x4>() };
        using (var pointQuery = em.CreateEntityQuery(typeof(Point)))
        using (var points = pointQuery.ToEntityArray(Allocator.TempJob))
        {
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = i + 1; j < points.Length; j++)
                {
                    var a = em.GetComponentData<Point>(points[i]);
                    var b = em.GetComponentData<Point>(points[j]);
                    var delta = (b.pos - a.pos);
                    var length = math.length(delta);
                    if (length >= 5f || length <= .2f)
                        continue;

                    ++a.neighborCount;
                    ++b.neighborCount;

                    em.SetComponentData(points[i], a);
                    em.SetComponentData(points[j], b);

                    var beam = new Beam(points[i], points[j], random.NextFloat(.25f, .35f), length, matrixBatchIndex, matricesList[matrixBatchIndex].Count);
                    var barLength = math.length(delta);
                    var norm = delta / barLength;

                    var pos = (a.pos + b.pos) * .5f;
                    var rot = Quaternion.LookRotation(delta);
                    var scale = new Vector3(beam.thickness, beam.thickness, barLength);
                    var beamMatrix = Matrix4x4.TRS(pos, rot, scale);

                    matricesList[matrixBatchIndex].Add(beamMatrix);
                    if (matricesList[matrixBatchIndex].Count == instancesPerBatch)
                    {
                        matrixBatchIndex++;
                        matricesList.Add(new List<Matrix4x4>());
                    }

                    var beamEntity = em.CreateEntity();
                    em.AddComponent<Beam>(beamEntity);
                    em.SetComponentData(beamEntity, beam);

                    beamCount++;
                }
            }
        }

        int pointCount = 0;
        using (var pointQuery = em.CreateEntityQuery(typeof(Point)))
        using (var points = pointQuery.ToEntityArray(Allocator.TempJob))
        {
            for (int i = 0; i < points.Length; i++)
            {
                var point = em.GetComponentData<Point>(points[i]);
                if (point.neighborCount == 0)
                    em.DestroyEntity(points[i]);
                else
                    pointCount++;
            }
        }

        matrices = new Matrix4x4[matricesList.Count][];
        for (int i = 0; i < matrices.Length; i++)
            matrices[i] = matricesList[i].ToArray();

        matProps = new MaterialPropertyBlock[beamCount];
        using (var bq = em.CreateEntityQuery(typeof(Beam)))
        using (var beams = bq.ToEntityArray(Allocator.TempJob))
        {
            var colors = new Vector4[instancesPerBatch];
            for (int i = 0; i < beamCount; i++)
            {
                var beamData = em.GetComponentData<Beam>(beams[i]);
                var point1 = em.GetComponentData<Point>(beamData.point1);
                var point2 = em.GetComponentData<Point>(beamData.point2);
                var delta = point2.pos - point1.pos;

                float upDot = math.acos(math.abs(math.dot(math.up(), math.normalize(delta)))) / math.PI;
                var color = Color.white * upDot * random.NextFloat(.7f, 1f);

                colors[i % instancesPerBatch] = color;
                if ((i + 1) % instancesPerBatch == 0 || i == beamCount - 1)
                {
                    var block = new MaterialPropertyBlock();
                    block.SetVectorArray("_Color", colors);
                    matProps[i / instancesPerBatch] = block;
                }
            }
        }

        #if DEBUG_DOTS
        Debug.Log($"Generated {buildingCount} buildings and {groundDetailCount} " +
            $"ground details for a total {pointCount} points and {beamCount} beams");
        #endif
    }

    #if SYNCED_SIMULATION
    void Simulate()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var activeTornados = tornados.Where(t => t.simulate).ToArray();
        var random = new Unity.Mathematics.Random(1234);

        float invDamping = 1f - damping;
        using (var pointQuery = em.CreateEntityQuery(typeof(DynamicPoint), typeof(Point)))
        using (var points = pointQuery.ToEntityArray(Allocator.TempJob))
        {
            for (int i = 0; i < points.Length; i++)
            {
                var point = em.GetComponentData<Point>(points[i]);
                var start = point.pos;
                point.old.y += .01f;

                foreach (var tornado in activeTornados)
                {
                    // tornado force
                    float tdx = tornado.x + Tornado.Sway(point.pos.y) - point.pos.x;
                    float tdz = tornado.y - point.pos.z;
                    float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
                    tdx /= tornadoDist;
                    tdz /= tornadoDist;
                    if (tornadoDist < tornado.maxForceDist)
                    {
                        float force = (1f - tornadoDist / tornado.maxForceDist);
                        float yFader = Mathf.Clamp01(1f - point.pos.y / tornado.height);
                        force *= tornado.fader * tornado.force * random.NextFloat(-.3f, 1.3f);
                        float forceY = tornado.upForce;
                        point.old.y -= forceY * force;
                        float forceX = -tdz + tdx * tornado.inwardForce * yFader;
                        float forceZ = tdx + tdz * tornado.inwardForce * yFader;
                        point.old.x -= forceX * force;
                        point.old.z -= forceZ * force;

                        // TODO: ecb create affected beam
                    }
                }

                point.pos += (point.pos - point.old) * invDamping;
                point.old = start;

                if (point.pos.y < 0f)
                {
                    point.pos.y = 0f;
                    point.old.y = -point.old.y;
                    point.old.x += (point.pos.x - point.pos.x) * friction;
                    point.old.z += (point.pos.z - point.pos.z) * friction;
                }

                // Write back point data
                em.SetComponentData(points[i], point);
            }
        }

        using (var bq = em.CreateEntityQuery(typeof(Beam)))
        using (var beams = bq.ToEntityArray(Allocator.TempJob))
        {
            for (int i = 0; i < beams.Length; ++i)
            {
                var beam = em.GetComponentData<Beam>(beams[i]);
                var point1 = em.GetComponentData<Point>(beam.point1);
                var point2 = em.GetComponentData<Point>(beam.point2);

                var point1Anchored = em.HasComponent<AnchoredPoint>(beam.point1);
                var point2Anchored = em.HasComponent<AnchoredPoint>(beam.point2);

                var delta = point2.pos - point1.pos; // TODO: precompute delta and length
                var dist = math.length(delta);
                var extraDist = dist - beam.length;
                var norm = delta / dist;
                var push = norm * extraDist * .5f;
                var breaks = Mathf.Abs(extraDist) > breakResistance;
                var writeBackPoint1 = false;
                var writeBackPoint2 = false;
                var writeBackBeam = false;

                if (!point1Anchored && !point2Anchored)
                {
                    point1.pos += push;
                    point2.pos -= push;
                    writeBackPoint1 = writeBackPoint2 = true;
                }
                else if (point1Anchored)
                {
                    point2.pos -= push * 2f;
                    writeBackPoint2 = true;
                }
                else if (point2Anchored)
                {
                    point1.pos += push * 2f;
                    writeBackPoint1 = true;
                }

                var matrix = matrices[beam.m1i][beam.m2i];
                var translate = (point1.pos + point2.pos) * .5f;
                if (norm.x * beam.norm.x + norm.y * beam.norm.y + norm.z * beam.norm.z < .99f)
                {
                    // bar has rotated: expensive full-matrix computation
                    matrix = Matrix4x4.TRS(translate, Quaternion.LookRotation(delta), new Vector3(beam.thickness, beam.thickness, dist));
                    beam.norm = norm;
                }
                else
                {
                    // bar hasn't rotated: only update the position elements
                    matrix.m03 = translate.x;
                    matrix.m13 = translate.y;
                    matrix.m23 = translate.z;
                }

                matrices[beam.m1i][beam.m2i] = matrix;

                if (breaks)
                {
                    if (point2.neighborCount > 1)
                    {
                        point2.neighborCount--;
                        writeBackPoint2 = true;

                        beam.point2 = CreatePoint(em, point2.pos, false, 1);
                        writeBackBeam = true;
                    }
                    else if (point1.neighborCount > 1)
                    {
                        point1.neighborCount--;
                        writeBackPoint1 = true;

                        beam.point1 = CreatePoint(em, point1.pos, false, 1);
                        writeBackBeam = true;
                    }
                }

                if (writeBackPoint1) em.SetComponentData(beam.point1, point1);
                if (writeBackPoint2) em.SetComponentData(beam.point2, point2);
                if (writeBackBeam) em.SetComponentData(beams[i], beam);
            }
        }
    }
    #endif
}
