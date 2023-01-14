using System;
using NodeGraph;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    [NodeMenuItem("Combine/Math")]
    public partial class MathNode : TerrainInputNode
    {
        [SerializeField] private MathOperation mathOperation = MathOperation.Add;

        [Input] private RenderTargetIdentifier inputA;
        [Input] private RenderTargetIdentifier inputB;

        public override float Min
        {
            get
            {
                var nodeA = GetConnectedNode("inputA") as TerrainInputNode;
                var nodeB = GetConnectedNode("inputB") as TerrainInputNode;

                if (nodeA == null || nodeB == null)
                {
                    return base.Min;
                }

                switch (mathOperation)
                {
                    case MathOperation.Add:
                        return nodeA.Min + nodeB.Min;
                    case MathOperation.Subtract:
                        return nodeA.Min - nodeB.Max;
                    case MathOperation.Multiply:
                        return nodeA.Min * nodeB.Min;
                    case MathOperation.Divide:
                        return nodeA.Min / nodeB.Min;
                    case MathOperation.Modulo:
                        return nodeA.Min % nodeB.Min;
                    case MathOperation.Pow:
                        return (float)Math.Pow(nodeA.Min, nodeB.Min);
                    case MathOperation.Min:
                        return Math.Min(nodeA.Min, nodeB.Min);
                    case MathOperation.Max:
                        return Math.Min(nodeA.Min, nodeB.Min);
                    default:
                        throw new NotImplementedException(mathOperation.ToString());
                }
            }
        }

        public override float Max
        {
            get
            {
                var nodeA = GetConnectedNode("inputA") as TerrainInputNode;
                var nodeB = GetConnectedNode("inputB") as TerrainInputNode;

                if (nodeA == null || nodeB == null)
                {
                    return base.Min;
                }

                switch (mathOperation)
                {
                    case MathOperation.Add:
                        return nodeA.Max + nodeB.Max;
                    case MathOperation.Subtract:
                        return nodeA.Max - nodeB.Max;
                    case MathOperation.Multiply:
                        return nodeA.Max * nodeB.Max;
                    case MathOperation.Divide:
                        return nodeA.Max / nodeB.Max;
                    case MathOperation.Modulo:
                        return nodeA.Max % nodeB.Max;
                    case MathOperation.Pow:
                        return (float)Math.Pow(nodeA.Max, nodeB.Max);
                    case MathOperation.Min:
                        return Math.Max(nodeA.Max, nodeB.Max);
                    case MathOperation.Max:
                        return Math.Max(nodeA.Max, nodeB.Max);
                    default:
                        throw new NotImplementedException(mathOperation.ToString());
                }
            }
        }

        protected override void Generate(TerrainGraph graph, CommandBuffer command)
        {
            if (!NodeIsConnected("inputA") || !NodeIsConnected("inputB"))
                return;

            var computeShader = Resources.Load<ComputeShader>("MathNode");
            var kernelIndex = (int)mathOperation;

            command.SetComputeTextureParam(computeShader, kernelIndex, "InputA", inputA);
            command.SetComputeTextureParam(computeShader, kernelIndex, "InputB", inputB);
            command.SetComputeTextureParam(computeShader, kernelIndex, "Result", result);
            command.DispatchNormalized(computeShader, kernelIndex, graph.Resolution, graph.Resolution, 1);
        }
    }
}