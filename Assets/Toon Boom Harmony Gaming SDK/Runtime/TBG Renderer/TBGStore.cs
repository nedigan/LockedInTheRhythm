#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace ToonBoom.TBGRenderer
{
    /// <summary> Data unique to each Harmony exported project, does not need to be duplicated per-behavior </summary>
    public class TBGStore : ScriptableObject
    {
        [Serializable]
        public struct PaletteInfo
        {
            public string PaletteName;
            public Sprite[] Sprites;
        }
        [Serializable]
        public struct ResolutionInfo
        {
            public string ResolutionName;
            public PaletteInfo[] Palettes;
        }
        [Serializable]
        public struct SkinGroupInfo
        {
            public string GroupName;
            public string[] SkinNames;
        }
        public SkinGroupInfo[] SkinGroups;
        [Serializable]
        public struct MetadataEntry
        {
            public string Node;
            public string Name;
            public string Value;
        }
        public Material Material;
        public bool[] CutterToInverse;
        public ushort[] CutterToCutteeReadIndex;
        public ushort[] CutterToMatteReadIndex;
        public ResolutionInfo[] Resolutions;
        [Serializable]
        public class DeformedSpriteData
        {
            public UnityEngine.U2D.SpriteBone[] bones;
            public Matrix4x4[] bindPoses;
            public ushort[] indices;
            public Vector3[] vertices;
            public BoneWeight[] weights;
        }
        [Serializable]
        public struct DeformedSpriteEntry
        {
            public Sprite Original;
            public DeformedSpriteData DeformData;
        }
        [Serializable]
        public struct ReadToDeformedSpriteEntry
        {
            public int ReadIndex;
            public DeformedSpriteEntry[] DeformedSprites;
        }
        public string[] SpriteNames;
        public MetadataEntry[] Metadata;
        public ReadToDeformedSpriteEntry[] ReadToDeformedSpriteEntries;

        public Dictionary<Sprite, int> SpriteToIndex = new();
        public Dictionary<int, Dictionary<Sprite, Sprite>> ReadToDeformedSprite = new();

        public void OnEnable()
        {
            if (Resolutions != null)
            {
                SpriteToIndex.Clear();
                for (int i = 0; i < Resolutions.Length; i++)
                {
                    var resolution = Resolutions[i];
                    for (int j = 0; j < resolution.Palettes.Length; j++)
                    {
                        var palette = resolution.Palettes[j];
                        for (int k = 0; k < palette.Sprites.Length; k++)
                        {
                            SpriteToIndex.Add(palette.Sprites[k], k);
                        }
                    }
                }
            }
            if (ReadToDeformedSpriteEntries != null)
            {
                ReadToDeformedSprite.Clear();
                for (int i = 0; i < ReadToDeformedSpriteEntries.Length; i++)
                {
                    var readToDeformedSpriteEntry = ReadToDeformedSpriteEntries[i];
                    var deformedSpriteLookup = new Dictionary<Sprite, Sprite>();
                    for (int j = 0; j < readToDeformedSpriteEntry.DeformedSprites.Length; j++)
                    {
                        var deformedSpriteEntry = readToDeformedSpriteEntry.DeformedSprites[j];
                        var original = deformedSpriteEntry.Original;
                        var deformData = deformedSpriteEntry.DeformData;

                        Profiler.BeginSample("Create new sprite");
                        var sprite = Sprite.Create(original.texture,
                            rect: original.rect,
                            pivot: original.pivot / original.rect.size,
                            // Same settings as from the original Sprite creation in TBGImporter.cs
                            pixelsPerUnit: original.pixelsPerUnit,
                            extrude: 0,
                            meshType: SpriteMeshType.FullRect,
                            border: Vector4.zero,
                            generateFallbackPhysicsShape: false);

                        Profiler.EndSample();

                        Profiler.BeginSample("Set sprite data");
                        sprite.SetBones(deformData.bones);
                        sprite.SetBindPoses(new NativeArray<Matrix4x4>(deformData.bindPoses, Allocator.Temp));
                        sprite.name = $"{original.name}-{readToDeformedSpriteEntry.ReadIndex}";
                        sprite.SetIndices(new NativeArray<ushort>(deformData.indices, Allocator.Temp));
                        sprite.SetVertexCount(deformData.vertices.Length);
                        sprite.SetVertexAttribute<Vector3>(VertexAttribute.Position, new NativeArray<Vector3>(deformData.vertices, Allocator.Temp));
                        sprite.SetVertexAttribute<BoneWeight>(VertexAttribute.BlendWeight, new NativeArray<BoneWeight>(deformData.weights, Allocator.Temp));
                        Profiler.EndSample();

                        deformedSpriteLookup.Add(deformedSpriteEntry.Original, sprite);
                    }
                    ReadToDeformedSprite.Add(readToDeformedSpriteEntry.ReadIndex, deformedSpriteLookup);
                }
            }
        }
    }
}

#endif