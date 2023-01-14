﻿using System;
using UnityEngine.Rendering;

namespace TerrainGraph
{
    public readonly struct CommandBufferProfilerScope : IDisposable
    {
        private readonly CommandBuffer commandBuffer;
        private readonly string name;

        public CommandBufferProfilerScope(CommandBuffer commandBuffer, string name)
        {
            this.commandBuffer = commandBuffer;
            this.name = name;
            commandBuffer.BeginSample(name);
        }

        void IDisposable.Dispose()
        {
            commandBuffer.EndSample(name);
        }
    }
}