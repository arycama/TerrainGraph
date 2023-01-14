using UnityEngine;
using UnityEngine.Rendering;
using NodeGraph;

[NodeMenuItem("Combine/Lerp")]
public partial class LerpNode : TerrainInputNode
{
    [Input] private RenderTargetIdentifier inputA;
    [Input] private RenderTargetIdentifier inputB;
    [Input] private RenderTargetIdentifier inputT;

    public override float Min => Mathf.Min(GetConnectionMin("inputA"), GetConnectionMin("inputB"));
    public override float Max => Mathf.Max(GetConnectionMax("inputA"), GetConnectionMax("inputB"));

    protected override void Generate(TerrainGraph graph, CommandBuffer command)
    {
        if (!NodeIsConnected("inputA") || !NodeIsConnected("inputB") || !NodeIsConnected("inputT"))
            return;

        var computeShader = Resources.Load<ComputeShader>("LerpNode");
        command.SetComputeTextureParam(computeShader, 0, "InputA", inputA);
        command.SetComputeTextureParam(computeShader, 0, "InputB", inputB);
        command.SetComputeTextureParam(computeShader, 0, "InputT", inputT);
        command.SetComputeTextureParam(computeShader, 0, "Result", result);
        command.DispatchNormalized(computeShader, 0, graph.Resolution, graph.Resolution, 1);
    }
}