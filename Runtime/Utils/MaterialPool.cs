using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain_Graph
{
    public static class MaterialPool
    {
        private static readonly Dictionary<string, Material> cache = new();

        public static Material Get(string shader)
        {
            if (!cache.TryGetValue(shader, out var material))
            {
                var shaderFile = Shader.Find(shader);
                if (shaderFile == null)
                {
                    throw new Exception($"Shader {shader} could not be found, check that it exists in your project and does not have compile errors.");
                }

                material = new Material(shaderFile) { hideFlags = HideFlags.DontSave };
                cache.Add(shader, material);
            }

            return material;
        }
    }
}