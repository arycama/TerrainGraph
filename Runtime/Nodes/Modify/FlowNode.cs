using System;
using UnityEngine;
using UnityEngine.Rendering;
using NodeGraph;

[NodeMenuItem("Modify/Flow")]
public partial class FlowNode : TerrainInputNode
{
    [SerializeField, Min(0)] private float amount = 0.0001f;
    [SerializeField, Range(0f, 1f)] private float time = 0.2f;
    [SerializeField, Range(1, 512)] private int iterations = 5;

    [Input] private RenderTargetIdentifier input;

    [Output] private RenderTargetIdentifier waterMap;
    [Output] private RenderTargetIdentifier velocity;

    private int waterMapId, velocityId, outflowId;

    public override void Initialize()
    {
        base.Initialize();

        waterMapId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_WaterMap");
        velocityId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_Velocity");
        outflowId = Shader.PropertyToID($"{GetType()}_{GetInstanceID()}_Outflow");

        waterMap = waterMapId;
        velocity = velocityId;
    }

    protected override void Generate(TerrainGraph graph, CommandBuffer command)
    {
        if (!NodeIsConnected("input"))
            return;

        var waterDescriptor = new RenderTextureDescriptor(graph.Resolution, graph.Resolution, RenderTextureFormat.RFloat)
        {
            enableRandomWrite = true,
        };

        var outflowDescriptor = new RenderTextureDescriptor(graph.Resolution, graph.Resolution, RenderTextureFormat.ARGBHalf)
        {
            enableRandomWrite = true,
        };

        var velocityDescriptor = new RenderTextureDescriptor(graph.Resolution, graph.Resolution, RenderTextureFormat.RGHalf)
        {
            enableRandomWrite = true,
        };

        command.GetTemporaryRT(waterMapId,waterDescriptor);
        command.GetTemporaryRT(outflowId, outflowDescriptor);
        command.GetTemporaryRT(velocityId, velocityDescriptor);

        var computeShader = Resources.Load<ComputeShader>("Modify/FlowNode");
        command.SetComputeVectorParam(computeShader, "_Scale", graph.ActiveTerrain.terrainData.heightmapScale);
        command.SetComputeIntParam(computeShader, "_Size", graph.Resolution);
        command.SetComputeFloatParam(computeShader, "Time", time);
        command.SetComputeFloatParam(computeShader, "_Height", amount);

        for (var i = 0; i < iterations; i++)
        {
            command.SetComputeFloatParam(computeShader, "_First", i == 0 ? 1f : 0f);

            command.SetComputeTextureParam(computeShader, 0, "Input", input);
            command.SetComputeTextureParam(computeShader, 0, "WaterMap", waterMap);
            command.SetComputeTextureParam(computeShader, 0, "OutFlow", outflowId);
            command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);

            command.SetComputeTextureParam(computeShader, 1, "Input", input);
            command.SetComputeTextureParam(computeShader, 1, "WaterMap", waterMap);
            command.SetComputeTextureParam(computeShader, 1, "OutFlow", outflowId);
            command.DispatchNormalized(computeShader, 1, graph.Resolution, graph.Resolution, 1);
        }

        command.SetComputeTextureParam(computeShader, 2, "OutFlow", outflowId);
        command.SetComputeTextureParam(computeShader, 2, "_Velocity", velocity);
        command.SetComputeTextureParam(computeShader, 2, "Result", result);
        command.DispatchNormalized(computeShader, 2, graph.Resolution, graph.Resolution, 1);

        command.ReleaseTemporaryRT(outflowId);
    }

    public override void OnFinishProcess(TerrainGraph graph, CommandBuffer command)
    {
        base.OnFinishProcess(graph, command);

        command.ReleaseTemporaryRT(waterMapId);
        command.ReleaseTemporaryRT(velocityId);
    }
}