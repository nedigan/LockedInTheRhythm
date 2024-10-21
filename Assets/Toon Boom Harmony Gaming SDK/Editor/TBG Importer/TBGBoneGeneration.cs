#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.U2D;
using UnityEngine.Rendering;
using UnityEngine.U2D.Animation;
using UnityEngine.Profiling;
using ToonBoom.TBGRenderer;

namespace ToonBoom.TBGImporter
{
    public class TBGBoneGenerator
    {
        public TBGXmlData data;
        public IEnumerable<InstantiatedNode> instantiatedNodes;
        public ILookup<string, Dictionary<string, Sprite>> spriteSheetToSprites;
        public int DiscretizationStep;
        public enum WeightAgainst
        {
            Parent,
            None,
            Child,
        }
        public void GenerateRestInfo(
            out ILookup<string, TransformRestInfo> nodeToRestOffsetInfoResult,
            out Dictionary<string, BoneRestInfo> nodeToBoneRestInfoResult,
            out List<InstantiatedBone> allInstantiatedBones,
            out Dictionary<string, NodeInstance> finalBoneEnds,
            out Dictionary<string, Dictionary<Sprite, TBGStore.DeformedSpriteData>> nodeToDeformedSprites)
        {
            // Assuming we can find everything we need in first animation / skeleton TODO less naive?
            var animation = data.animations.First().Value;
            var skeleton = data.skeletons.First();

            // Generate bones from deform data.
            var nodeToAttrLinks = animation.attrlinks.ToLookup(attrlink => attrlink.node, attrlink => attrlink);
            var idToNode = skeleton.nodes.ToDictionary(entry => entry.id, entry => entry);
            var nodeToIds = skeleton.nodes.ToLookup(entry => entry.name, entry => entry.id);
            var inToOut = skeleton.links.ToLookup(entry => entry.nodeIn, entry => entry.nodeOut);
            var outToIn = skeleton.links.ToLookup(entry => entry.nodeOut, entry => entry.nodeIn);

            // Per bone ---
            //      Figure out the rest transform and position existing transform to that rest position
            //      Search downward to find root (explore first parent until non-bone found)
            //      Dictates rest offset and rotation
            //      This is outputed for reference in animation translations -- all non-bones reference parent bone's rest for offset position/rotation
            var nodeToBoneRestInfo = new Dictionary<string, BoneRestInfo>();
            var nodeToInverseTransformRestInfo = new Dictionary<int, TransformRestInfo>();
            var nodeToBindPose = new Dictionary<int, Matrix4x4>();
            var nodeIsParentBone = new HashSet<string>();
            var nodeToBoneInfo = new Dictionary<int, BoneAdditionalInfo>();
            foreach (var boneNode in skeleton.nodes
                .Where(node => node.tag == "bone"))
            {
                var parentNode = outToIn[boneNode.id].First();
                bool isFirstBoneInDeform = true;
                var deformRootNode = parentNode;
                {
                    while (idToNode[deformRootNode].tag == "bone")
                    {
                        isFirstBoneInDeform = false;
                        deformRootNode = outToIn[deformRootNode].First();
                    }
                }
                var attrlinks = nodeToAttrLinks[boneNode.name].ToLookup(attrlink => attrlink.attr, attrlink => attrlink);
                var restAngle = (float)attrlinks["rest.rotation.z"].FirstOrDefault().value;
                var restRotation = Quaternion.AngleAxis(
                    Mathf.Rad2Deg * restAngle,
                    Vector3.forward);
                var restOffset = new Vector3(
                        (float)attrlinks["rest.offset.x"].FirstOrDefault().value,
                        (float)attrlinks["rest.offset.y"].FirstOrDefault().value,
                        0.0f)
                    + (isFirstBoneInDeform
                        ? Vector3.zero
                        : Vector3.right * nodeToBoneInfo[parentNode].length);
                var restLength = (float)attrlinks["rest.length"].FirstOrDefault().value;
                var restRadius = (float)attrlinks["rest.radius"].FirstOrDefault().value;
                var inverseParentBindPose = isFirstBoneInDeform ? Matrix4x4.identity : Matrix4x4.Inverse(nodeToBindPose[parentNode]);
                var relativeToRootParent = new
                {
                    position = inverseParentBindPose.MultiplyPoint(restOffset),
                    rotation = restRotation * inverseParentBindPose.rotation,
                };

                // Populate data relevant to output / next iteration
                nodeToBoneInfo[boneNode.id] = new BoneAdditionalInfo
                {
                    length = restLength,
                    nodeID = boneNode.id,
                    radius = restRadius,
                    position = restOffset,
                    rotation = restRotation,
                };
                nodeToBindPose[boneNode.id] = Matrix4x4.Rotate(Quaternion.Inverse(relativeToRootParent.rotation)) * Matrix4x4.Translate(-relativeToRootParent.position);
                nodeToBoneRestInfo[boneNode.name] = new BoneRestInfo
                {
                    parentNode = parentNode,
                    restLength = restLength,
                    restRadius = restRadius,
                };
                nodeToInverseTransformRestInfo[boneNode.id] = new TransformRestInfo
                {
                    restRootPosition = relativeToRootParent.position,
                    restRootAngle = relativeToRootParent.rotation.eulerAngles.z * Mathf.Deg2Rad,
                };
                nodeIsParentBone.Add(idToNode[parentNode].name);
            }

            // Per drawing ---
            //      Search downward collecting all bones that are parents, stop chain when finding a kinematic output, this is this bone's deform collection
            //      Generate Unity Bone data per-bone
            //      Assign bone data to sprite for drawing
            //      Add SpriteSkin behaviour to existing sprite node transform
            var nodeToDrws = data.drawingAnimations
                .SelectMany(drawingAnimation => drawingAnimation.Value.drawings
                    .SelectMany(drawing => drawing.Value
                        .Select(drw => new { drw = drw.name, node = drawing.Key })))
                .ToLookup(entry => entry.node, entry => entry.drw);
            var nodeToInstantiated = instantiatedNodes.ToLookup(entry => entry.id, entry => entry.gameObject);
            var deformedReadNodes = skeleton.nodes
                .Where(node => node.tag == "read")
                .Select(readNode =>
                {
                    var affectingBoneNodes = new List<int>();
                    var parentNodes = outToIn[readNode.id];
                    var candidateNodes = new Queue<int>(parentNodes);
                    while (candidateNodes.Count > 0)
                    {
                        var candidateNode = candidateNodes.Dequeue();
                        if (candidateNode < 0)
                            continue;
                        var tag = idToNode[candidateNode].tag;
                        if (tag == "bone" && !affectingBoneNodes.Contains(candidateNode))
                            affectingBoneNodes.Add(candidateNode);
                        if (tag != "kinematic")
                        {
                            foreach (var parentNode in outToIn[candidateNode])
                                candidateNodes.Enqueue(parentNode);
                        }
                    }
                    affectingBoneNodes.Reverse();
                    return new { readNode, affectingBoneNodes };
                })
                .Where(entry => entry.affectingBoneNodes.Count > 0)
                .ToList();


            allInstantiatedBones = new List<InstantiatedBone>();
            nodeToDeformedSprites = new Dictionary<string, Dictionary<Sprite, TBGStore.DeformedSpriteData>>();
            foreach (var entry in deformedReadNodes)
            {
                // Create new transforms under each affecting bone for bones for this specific SpriteSkin
                var instantiatedBones = entry.affectingBoneNodes
                    .Select(node =>
                    {
                        var transform = nodeToInstantiated[node].First().transform;
                        var childTransform = new GameObject($"{transform.name}_{entry.readNode.name}").transform;
                        childTransform.parent = transform;
                        childTransform.localPosition = Vector3.zero;
                        childTransform.localScale = Vector3.one;
                        childTransform.localRotation = Quaternion.identity;
                        return new InstantiatedBone
                        {
                            readName = entry.readNode.name,
                            boneName = idToNode[node].name,
                            gameObject = childTransform.gameObject,
                        };
                    })
                    .ToList();
                allInstantiatedBones.AddRange(instantiatedBones);
                var rootInstantiatedBone = instantiatedBones.First();
                // Hack - Set spriteSkin properties without needing to be 'internal'.
                foreach (var spriteSkin in nodeToInstantiated[entry.readNode.id]
                    .Select(go => go?
                        .GetComponentInChildren<SpriteRenderer>()?.gameObject?
                        .AddComponent<SpriteSkin>()))
                {
                    if (spriteSkin == null)
                        continue;
                    var serializedObject = new SerializedObject(spriteSkin);
                    serializedObject.FindProperty("m_RootBone").objectReferenceValue = rootInstantiatedBone.gameObject.transform;
                    var boneTransforms = serializedObject.FindProperty("m_BoneTransforms");
                    boneTransforms.ClearArray();
                    for (int i = 0; i < instantiatedBones.Count; i++)
                    {
                        boneTransforms.InsertArrayElementAtIndex(i);
                        boneTransforms.GetArrayElementAtIndex(i).objectReferenceValue = instantiatedBones[i].gameObject.transform;
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                // Setup each sprite belonging to this node
                if (!nodeToDeformedSprites.TryGetValue(entry.readNode.name, out var deformedSprites))
                {
                    deformedSprites = new Dictionary<Sprite, TBGStore.DeformedSpriteData>();
                    nodeToDeformedSprites.Add(entry.readNode.name, deformedSprites);
                }
                var drws = nodeToDrws[entry.readNode.name];
                foreach (var drw in drws)
                {
                    foreach (var spriteSheetEntry in spriteSheetToSprites)
                    {
                        if (!spriteSheetEntry.First().TryGetValue(drw, out var sprite))
                            continue;
                        var length = entry.affectingBoneNodes.Count;
                        var bones = new SpriteBone[length];
                        var bindPoses = new Matrix4x4[length];
                        var boneRadiuses = new float[length];
                        var nodeToDeformIndex = entry.affectingBoneNodes
                            .Select((nodeID, deformID) => new { nodeID, deformID })
                            .ToDictionary(entry => entry.nodeID, entry => entry.deformID);
                        Profiler.BeginSample("Generate bone index to children lookup");
                        var boneIndexToChildren = entry.affectingBoneNodes
                            .Select((nodeIndex, boneIndex) =>
                            {
                                var parentNodeID = outToIn[nodeIndex].First();
                                var parentDeformID = nodeToDeformIndex.TryGetValue(parentNodeID, out var result) ? result : -1;
                                var boneInfo = nodeToBoneInfo[nodeIndex];
                                var nodeInfo = idToNode[nodeIndex];
                                bones[boneIndex] = new SpriteBone
                                {
                                    parentId = parentDeformID,
                                    name = nodeInfo.name,
                                    length = boneInfo.length,
                                    position = boneInfo.position,
                                    rotation = boneInfo.rotation,
                                };
                                bindPoses[boneIndex] = nodeToBindPose[nodeIndex];
                                boneRadiuses[boneIndex] = boneInfo.radius;
                                return new
                                {
                                    deformID = nodeToDeformIndex[nodeIndex],
                                    parentDeformID = parentDeformID,
                                };
                            })
                            .ToLookup(entry => entry.parentDeformID, entry => entry.deformID);
                        Profiler.EndSample();
                        deformedSprites[sprite] = AssignBoneDataToSprite(sprite, bones, bindPoses, boneRadiuses, boneIndexToChildren);
                    }
                }
            }

            Profiler.BeginSample("Generate finalBoneEnds");
            finalBoneEnds = nodeToBoneRestInfo
                .Where(entry => !nodeIsParentBone.Contains(entry.Key))
                .Select(entry =>
                {
                    var finalBoneEnd = new GameObject($"{entry.Key}_End");
                    finalBoneEnd.transform.parent = nodeToInstantiated[nodeToIds[entry.Key].First()].First().transform;
                    return new { entry.Key, Value = new NodeInstance { name = finalBoneEnd.name, transform = finalBoneEnd.transform } };
                })
                .ToDictionary(entry => entry.Key, entry => entry.Value);
            Profiler.EndSample();

            Profiler.BeginSample("Generate nodeToRestOffsetInfoResult");
            nodeToRestOffsetInfoResult = nodeToInverseTransformRestInfo
                .SelectMany(entry =>
                {
                    var children = new List<int>();
                    foreach (var child in inToOut[entry.Key])
                    {
                        if (idToNode[child].tag == "composite")
                            children.AddRange(inToOut[child]);
                        else
                            children.Add(child);
                    }
                    var nodeName = idToNode[entry.Key].name;
                    var boneRestLength = nodeToBoneRestInfo[nodeName].restLength;
                    var restRotation = Quaternion.AngleAxis(
                        Mathf.Rad2Deg * entry.Value.restRootAngle,
                        Vector3.forward);
                    var childOffsetRestInfo = new TransformRestInfo
                    {
                        restRootAngle = -entry.Value.restRootAngle,
                        restRootPosition = -(Vector2)(Quaternion.Inverse(restRotation) * entry.Value.restRootPosition)
                            - new Vector2(boneRestLength, 0),
                    };
                    return children.Select(child => new { child, restInfo = childOffsetRestInfo });
                })
                .Where(entry => !nodeToBoneInfo.ContainsKey(entry.child))
                .ToLookup(entry => idToNode[entry.child].name, entry => entry.restInfo);
            Profiler.EndSample();

            nodeToBoneRestInfoResult = nodeToBoneRestInfo;
        }

        private struct ClosestBone
        {
            public int index;
            public float distance;
        }
        private struct VertexBoneCalc
        {
            public float distance;
            public float flatDistance;
            public WeightAgainst weightAgainst;
        }

        public TBGStore.DeformedSpriteData AssignBoneDataToSprite(Sprite sprite, SpriteBone[] bones, Matrix4x4[] bindPoses, float[] boneRadiuses, ILookup<int, int> boneIndexToChildren)
        {
            // Calculate mesh bounds from sprite vertices
            Bounds meshBounds;
            {
                var previousVertices = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position).ToList();
                meshBounds = new Bounds(previousVertices[0], Vector3.zero);
                for (var i = 1; i < previousVertices.Count; i++)
                {
                    meshBounds.Encapsulate(previousVertices[i]);
                }
            }

            // Tesselate mesh and calculate per-bone weighting
            var width = DiscretizationStep;
            var widthPlus1 = width + 1;
            var height = DiscretizationStep;
            var heightPlus1 = height + 1;
            var quadTriangles = new int[] { 0, 1, 2, 2, 1, 3 };
            Profiler.BeginSample("Generate indices");
            var indices = new ushort[width * height * 6];
            var quadIndices = new ushort[4];
            for (var y = 0; y < height; y++)
            {
                var yTimesWidthPlus1 = y * widthPlus1;
                var yTimesWidth = y * width;
                for (var x = 0; x < width; x++)
                {
                    quadIndices[0] = (ushort)(yTimesWidthPlus1 + x);
                    quadIndices[1] = (ushort)(yTimesWidthPlus1 + (x + 1));
                    quadIndices[2] = (ushort)((y + 1) * widthPlus1 + x);
                    quadIndices[3] = (ushort)((y + 1) * widthPlus1 + (x + 1));
                    for (var i = 0; i < 6; i++)
                        indices[yTimesWidth * 6 + x * 6 + i] = quadIndices[quadTriangles[i]];
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("Generate vertices");
            var vertices = new Vector3[widthPlus1 * heightPlus1];
            for (var y = 0; y < heightPlus1; y++)
            {
                var yTimesWidthPlus1 = y * widthPlus1;
                var yTimesWidth = y * width;
                for (var x = 0; x < widthPlus1; x++)
                {
                    vertices[yTimesWidthPlus1 + x] = new Vector3(
                        x / (float)width * meshBounds.size.x + meshBounds.min.x,
                        (1.0f - y / (float)height) * meshBounds.size.y + meshBounds.min.y,
                        0.0f);
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("Generate weights");
            var boneCalcs = new VertexBoneCalc[bindPoses.Length];
            var weights = new BoneWeight[vertices.Length];
            for (var weightsIndex = 0; weightsIndex < vertices.Length; weightsIndex++)
            {
                var worldPosition = vertices[weightsIndex];
                for (var index = 0; index < bindPoses.Length; index++)
                {
                    var bindPose = bindPoses[index];
                    var bone = bones[index];
                    var radius = index == 0 ? 0.0f : boneRadiuses[index];
                    int childIndex = -1;
                    for (var i = 0; i < bones.Length; i++)
                    {
                        if (bones[i].parentId == index)
                        {
                            childIndex = i;
                            break;
                        }
                    }
                    var childRadius = childIndex < 0 ? 0.0f : boneRadiuses[childIndex];
                    var placeOnShrunkenBone = bindPose.MultiplyPoint(worldPosition) - Vector3.right * radius;
                    var shrunkenLength = bone.length - radius - childRadius;
                    if (placeOnShrunkenBone.x < 0)
                    {
                        boneCalcs[index] = new VertexBoneCalc
                        {
                            distance = placeOnShrunkenBone.magnitude,
                            flatDistance = -placeOnShrunkenBone.x,
                            weightAgainst = WeightAgainst.Parent
                        };
                    }
                    else if (placeOnShrunkenBone.x < shrunkenLength)
                        boneCalcs[index] = new VertexBoneCalc
                        {
                            distance = Math.Abs(placeOnShrunkenBone.y),
                            flatDistance = 0.0f,
                            weightAgainst = WeightAgainst.None,
                        };
                    else
                        boneCalcs[index] = new VertexBoneCalc
                        {
                            distance = (placeOnShrunkenBone - Vector3.right * shrunkenLength).magnitude,
                            flatDistance = placeOnShrunkenBone.x - shrunkenLength,
                            weightAgainst = WeightAgainst.Child,
                        };
                }
                var closestBone = new ClosestBone { index = -1, distance = float.MaxValue };
                for (var calcsIndex = 0; calcsIndex < boneCalcs.Length; calcsIndex++)
                {
                    if (boneCalcs[calcsIndex].distance < closestBone.distance)
                        closestBone = new ClosestBone { index = calcsIndex, distance = boneCalcs[calcsIndex].distance };
                }
                var closestChildBone = new ClosestBone { index = -1, distance = float.MaxValue };
                foreach (var childBoneIndex in boneIndexToChildren[closestBone.index])
                {
                    if (boneCalcs[childBoneIndex].distance < closestChildBone.distance)
                        closestChildBone = new ClosestBone { index = childBoneIndex, distance = boneCalcs[childBoneIndex].distance };
                }

                if (boneCalcs[closestBone.index].weightAgainst == WeightAgainst.None)
                {
                    weights[weightsIndex] = new BoneWeight
                    {
                        boneIndex0 = closestBone.index,
                        weight0 = 1,
                    };
                    continue;
                }
                var otherBoneIndex = boneCalcs[closestBone.index].weightAgainst == WeightAgainst.Parent
                    ? bones[closestBone.index].parentId
                    : closestChildBone.index;
                if (otherBoneIndex < 0)
                {
                    weights[weightsIndex] = new BoneWeight
                    {
                        boneIndex0 = closestBone.index,
                        weight0 = 1,
                    };
                    continue;
                }
                var weight0 = Mathf.Pow(boneCalcs[closestBone.index].flatDistance, 2);
                var weight1 = Mathf.Pow(boneCalcs[otherBoneIndex].flatDistance, 2);
                var combinedDistance = weight0 + weight1;
                weights[weightsIndex] = new BoneWeight
                {
                    boneIndex0 = closestBone.index,
                    weight0 = weight1 / combinedDistance,
                    boneIndex1 = otherBoneIndex,
                    weight1 = weight0 / combinedDistance,
                };
            }

            return new()
            {
                bones = bones,
                bindPoses = bindPoses,
                indices = indices,
                vertices = vertices,
                weights = weights,
            };
        }
    }

}

#endif
