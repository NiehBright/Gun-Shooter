using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class EliteCase
    {
        public List<MeshPair> pairs;
        public List<SimpleMeshPair> simplePairs;

        public void SetElite()
        {
            pairs?.ForEach((pair) => pair.renderer.sharedMesh = pair.eliteMesh);
            simplePairs?.ForEach((pair) => pair.filter.mesh = pair.eliteMesh);
        }

        public void SetRegular()
        {
            pairs?.ForEach((pair) => pair.renderer.sharedMesh = pair.simpleMesh);
            simplePairs?.ForEach((pair) => pair.filter.mesh = pair.simpleMesh);
        }

        public void Validate()
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                if (pairs[i].renderer == null || pairs[i].simpleMesh == null || pairs[i].eliteMesh == null)
                {
                    Debug.LogError("[Enemy Behavior] Elite enemy case is not properly configured. Please check if all references are assigned on enemy script field Elite Case.");
                    pairs.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < simplePairs.Count; i++)
            {
                if (simplePairs[i].filter == null || simplePairs[i].simpleMesh == null || simplePairs[i].eliteMesh == null)
                {
                    Debug.LogError("[Enemy Behavior] Elite enemy case is not properly configured. Please check if all references are assigned on enemy script field Elite Case.");
                    simplePairs.RemoveAt(i);
                    i--;
                }
            }
        }

        [System.Serializable]
        public struct MeshPair
        {
            public SkinnedMeshRenderer renderer;
            public Mesh simpleMesh;
            public Mesh eliteMesh;
        }

        [System.Serializable]
        public struct SimpleMeshPair
        {
            public MeshFilter filter;
            public Mesh simpleMesh;
            public Mesh eliteMesh;
        }
    }
}