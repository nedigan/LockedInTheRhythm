#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.Profiling;

namespace ToonBoom.TBGImporter
{
    public struct RegisteredCurvesKey
    {
        public string TransformPath;
        public string PropertyName;
    }
    public struct RegisteredCurve
    {
        public TBGCurveGenerator.BezierCurve curve;
        public BlendFunction blendFunction;
    }
    public class TBGClipBuilderSettings
    {
        public bool Stepped;
        public string Name;
        public float Framerate;
        public StageSettings Stage;
        public SkeletonSettings Skeleton;
        public GameObject RootGameObject;
        public ILookup<string, int> NodeNameToIDs;
        public ILookup<int, GameObject> NodeToInstantiated;
        public AnimationSettings Animation;
        public Dictionary<int, string> NodeIDToName;
        public ILookup<int, int> OutToIn;
        public DrawingAnimationSettings DrawingAnimation;
        public Dictionary<RegisteredCurvesKey, List<RegisteredCurve>> RegisteredCurvesLookup = new Dictionary<RegisteredCurvesKey, List<RegisteredCurve>>();
        public Dictionary<string, ObjectReferenceKeyframe[]> RegisteredSpriteCurvesLookup = new Dictionary<string, ObjectReferenceKeyframe[]>();
        public int animationLength
        {
            get
            {
                if (Stage.play.markerLength != null)
                {
                    return (int)Stage.play.markerLength;
                }
                try
                {
                    // Infer marker length from DrawingAnimation data
                    Stage.play.markerLength = Math.Max(0, DrawingAnimation.drawings
                        .Select(drawing => drawing.Value
                            .Select(drw => drw.frame + drw.repeat)
                            .DefaultIfEmpty(0)
                            .Max())
                        .DefaultIfEmpty(0)
                        .Max() - 1);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    Stage.play.markerLength = 0;
                }
                return (int)Stage.play.markerLength;
            }
        }

        private AnimationClip _clip;
        public AnimationClip clip
        {
            get
            {
                if (_clip != null)
                    return _clip;
                _clip = new AnimationClip()
                {
                    name = Name,
                    frameRate = Framerate,
                };
                AnimationUtility.SetAnimationClipSettings(_clip, new AnimationClipSettings
                {
                    loopTime = true,
                });
                return _clip;
            }
            set
            {
                _clip = value;
            }
        }
    }

    public class TBGClipBuilder
    {
        public TBGClipBuilderSettings Settings;
        public Dictionary<string, string> AttributeToProperty;
        public HashSet<string> AttributesToSplit3D;
        public Dictionary<string, NodeToValueMap> AttributeToNodeToValueMap;
        public Dictionary<string, AdvancedNodeMapping[]> AttributeToAdvancedNodeMappings;

        public delegate OffsetRetriever NodeToOffsetRetriever(string node);

        public ValueMap GetValueMap(string nodeName, string attribute, string property, AdvancedNodeMapping advancedNodeMapping)
        {
            return AttributeToNodeToValueMap != null && AttributeToNodeToValueMap.TryGetValue(attribute, out var nodeToValueMap)
                ? nodeToValueMap(nodeName)
                : advancedNodeMapping != null && advancedNodeMapping.propertyToNodeValueTransform != null
                    ? advancedNodeMapping.propertyToNodeValueTransform[property].nodeToValueMap(nodeName)
                    : value => value;
        }
        public TBGClipBuilder WithTimedValueCurves()
        {
            Profiler.BeginSample("WithTimedValueCurves");
            var curveGenerator = new TBGCurveGenerator
            {
                AnimationLength = Settings.animationLength,
                Stepped = Settings.Stepped,
            };
            foreach (var attrlink in Settings.Animation.attrlinks)
            {
                Profiler.BeginSample("Retrieve Curve Inputs");

                List<TBGCurveGenerator.CurveInput> curveInputs;
                if (AttributesToSplit3D.Contains(attrlink.attr))
                {
                    curveInputs = new string[] { "x", "y", "z" }
                        .Select(subAttribute =>
                        {
                            var attribute = $"{attrlink.attr}.{subAttribute}";
                            return new TBGCurveGenerator.CurveInput
                            {
                                info = $"{attrlink.node}.{attribute}",
                                node = attrlink.node,
                                attribute = attribute,
                                timedValuePoints = Settings.Animation.timedvalues[attrlink.timedvalue]
                                    .First().points
                                    .Select(point =>
                                    {
                                        var time = point.lockedInTime != null
                                            ? (int)point.lockedInTime
                                            : point.start != null
                                            ? (int)point.start : 0;
                                        return subAttribute switch
                                        {
                                            "x" => new TimedValuePoint
                                            {
                                                x = time,
                                                lx = time,
                                                rx = time,
                                                y = point.x,
                                                ly = point.x,
                                                ry = point.x,
                                                constSeg = true,
                                            },
                                            "y" => new TimedValuePoint
                                            {
                                                x = time,
                                                lx = time,
                                                rx = time,
                                                y = point.y,
                                                ly = point.y,
                                                ry = point.y,
                                                constSeg = true,
                                            },
                                            _ => new TimedValuePoint
                                            {
                                                x = time,
                                                lx = time,
                                                rx = time,
                                                y = point.z ?? 0.0,
                                                ly = point.z ?? 0.0,
                                                ry = point.z ?? 0.0,
                                                constSeg = true,
                                            },
                                        };
                                    })
                                    .ToList(),
                            };
                        })
                        .ToList();
                }
                else
                {
                    curveInputs = new List<TBGCurveGenerator.CurveInput> { new TBGCurveGenerator.CurveInput {
                        info = $"{attrlink.node}.{attrlink.attr}",
                        node = attrlink.node,
                        attribute = attrlink.attr,
                        value = (float)attrlink.value,
                        timedValuePoints = attrlink.timedvalue == null
                            ? null
                            : Settings.Animation.timedvalues[attrlink.timedvalue].First().points,
                    } };
                }
                Profiler.EndSample();

                foreach (var curveInput in curveInputs)
                {
                    if (AttributeToAdvancedNodeMappings == null
                        || !AttributeToAdvancedNodeMappings.TryGetValue(curveInput.attribute, out var advancedNodeMappings)
                        || advancedNodeMappings.Length == 0)
                    {
                        advancedNodeMappings = new AdvancedNodeMapping[] { null };
                    }

                    foreach (var advancedNodeMapping in advancedNodeMappings)
                    {
                        var properties = advancedNodeMapping != null && advancedNodeMapping.propertyToNodeValueTransform != null
                            ? (IEnumerable<string>)advancedNodeMapping.propertyToNodeValueTransform.Keys
                            : AttributeToProperty != null && AttributeToProperty.TryGetValue(attrlink.attr, out var propertyResult)
                                ? new string[] { propertyResult }
                                : new string[] { };
                        foreach (var property in properties)
                        {
                            Profiler.BeginSample("Retrieve Instantiated");

                            IEnumerable<NodeInstance> instantiated;
                            if (!(advancedNodeMapping != null
                                && advancedNodeMapping.nodeToInstance != null
                                && (instantiated = advancedNodeMapping.nodeToInstance(curveInput.node)).Any()))
                            {
                                instantiated = Settings.NodeNameToIDs[curveInput.node]
                                    .SelectMany(nodeID => Settings.NodeToInstantiated[nodeID]
                                        .Select(instance => new NodeInstance { name = Settings.NodeIDToName[nodeID], transform = instance.transform }));
                            }

                            Profiler.EndSample();

                            foreach (var instantiatedEntry in instantiated)
                            {
                                var curve = curveGenerator.FromTimedValues(curveInput, GetValueMap(instantiatedEntry.name, curveInput.attribute, property, advancedNodeMapping));

                                var blendFunction = advancedNodeMapping?.propertyToNodeValueTransform?[property]?.blendFunction;
                                var transformPath = AnimationUtility.CalculateTransformPath(instantiatedEntry.transform, Settings.RootGameObject.transform);
                                RegisterCurve(transformPath, property, curve, blendFunction);
                            }
                        }
                    }
                }
            }
            Profiler.EndSample();
            return this;
        }

        private void RegisterCurve(string transformPath, string property, TBGCurveGenerator.BezierCurve curve, BlendFunction blendFunction)
        {
            if (!Settings.RegisteredCurvesLookup.TryGetValue(new RegisteredCurvesKey { TransformPath = transformPath, PropertyName = property }, out var existingCurve))
            {
                existingCurve = new List<RegisteredCurve>();
                Settings.RegisteredCurvesLookup.Add(new RegisteredCurvesKey { TransformPath = transformPath, PropertyName = property }, existingCurve);
            }
            existingCurve.Add(new RegisteredCurve { curve = curve, blendFunction = blendFunction });
        }

        public static float GetTotalValueFromBlendedKeyframes(List<RegisteredCurve> registeredCurves, int frame)
        {
            var totalValue = 0.0f;
            for (var i = 0; i < registeredCurves.Count; i++)
            {
                var registeredCurve = registeredCurves[i];
                var value = (float)registeredCurve.curve.GetValue(frame);
                if (i == 0)
                    totalValue = value;
                else
                    totalValue = registeredCurve.blendFunction != null ? registeredCurve.blendFunction(totalValue, value) : value;
            }
            return totalValue;
        }

        public TBGClipBuilder ApplyRegisteredCurvesToClip()
        {

            Profiler.BeginSample("ApplyRegisteredSpriteCurvesToClip");
            foreach (var addedCurves in Settings.RegisteredSpriteCurvesLookup)
            {
                var binding = new EditorCurveBinding
                {
                    path = addedCurves.Key,
                    type = typeof(SpriteRenderer),
                    propertyName = "m_Sprite"
                };
                AnimationUtility.SetObjectReferenceCurve(Settings.clip, binding, addedCurves.Value);
            }
            var previousReferenceCurves = AnimationUtility.GetObjectReferenceCurveBindings(Settings.clip);
            AnimationUtility.SetObjectReferenceCurves(
                Settings.clip,
                previousReferenceCurves
                    .Concat(Settings.RegisteredSpriteCurvesLookup
                        .Select(entry => new EditorCurveBinding
                        {
                            path = entry.Key,
                            type = typeof(SpriteRenderer),
                            propertyName = "m_Sprite"
                        }))
                    .ToArray(),
                previousReferenceCurves
                    .Select(binding => AnimationUtility.GetObjectReferenceCurve(Settings.clip, binding))
                    .Concat(Settings.RegisteredSpriteCurvesLookup.Select(entry => entry.Value))
                    .ToArray());
            Profiler.EndSample();

            Profiler.BeginSample("ApplyRegisteredCurvesToClip");
            foreach (var addedCurves in Settings.RegisteredCurvesLookup)
            {
                var propertyName = addedCurves.Key.PropertyName;
                var transformPath = addedCurves.Key.TransformPath;
                var registeredCurves = addedCurves.Value;
                var keyframes = new Keyframe[Settings.animationLength];
                for (var frame = 0; frame < Settings.animationLength; frame++)
                {
                    keyframes[frame] = new Keyframe(frame / Settings.Framerate, GetTotalValueFromBlendedKeyframes(registeredCurves, frame));
                }
                var rleKeyframes = Settings.Stepped
                    ? keyframes
                        .Where((keyframe, index) => index == 0 || keyframe.value != keyframes[index - 1].value)
                        .ToArray()
                    :
                    keyframes
                        .Where((keyframe, index) => index == 0 || keyframe.value != keyframes[index - 1].value
                            || index == keyframes.Length - 1 || keyframe.value != keyframes[index + 1].value)
                        .ToArray();
                var animationCurve = new AnimationCurve(rleKeyframes);
                var tangentMode = Settings.Stepped
                    ? AnimationUtility.TangentMode.Constant
                    : AnimationUtility.TangentMode.Linear;
                var keyframeCount = animationCurve.length;
                for (int i = 0; i < keyframeCount; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(animationCurve, i, tangentMode);
                    AnimationUtility.SetKeyRightTangentMode(animationCurve, i, tangentMode);
                }
                Settings.clip.SetCurve(transformPath, typeof(Transform), propertyName, animationCurve);
            }
            Profiler.EndSample();

            return this;
        }

        public TBGClipBuilder WithDrawingAnimationCurves(
            Dictionary<string, Dictionary<int, SpriteRenderer>> nodeToSkinToSpriteRenderer,
            Dictionary<string, Sprite> spriteNameToSprite)
        {
            Profiler.BeginSample("WithDrawingAnimationCurves");
            foreach (var drawing in Settings.DrawingAnimation.drawings)
            {
                var node = drawing.Key;
                if (!nodeToSkinToSpriteRenderer.TryGetValue(node, out var skinToSpriteRenderer))
                    continue;
                foreach (var entry in skinToSpriteRenderer)
                {
                    var skinID = entry.Key;
                    var spriteRenderer = entry.Value;
                    var drws = drawing.Value
                        .Where(drw => drw.skinId == skinID)
                        .ToList();
                    var lastDrawingFrame = drws.LastOrDefault().frame + drws.LastOrDefault().repeat - 1;
                    var emptyKeyframes = drws
                        .Select((drw, index) =>
                        {
                            var lastFrameEnd = index == 0
                                ? -1
                                : drws[index - 1].frame + drws[index - 1].repeat - 1;
                            var currentFrameStart = drw.frame - 1;
                            return new { lastFrameEnd, currentFrameStart };
                        })
                        .Where(entry => entry.lastFrameEnd < entry.currentFrameStart)
                        .Select(entry => new ObjectReferenceKeyframe { time = entry.lastFrameEnd / (float)Settings.Framerate, value = null });
                    var finalKeyframe = lastDrawingFrame == Settings.animationLength
                        ? new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe {
                            time = (lastDrawingFrame - 1) / (float)Settings.Framerate,
                            value = spriteNameToSprite.TryGetValue(drws.LastOrDefault().name, out var sprite) ? sprite : null
                        } }
                        : new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe { time = lastDrawingFrame / (float)Settings.Framerate, value = null } };
                    var visibleKeyframes = drws
                        .Select(drw =>
                        {
                            spriteNameToSprite.TryGetValue(drw.name, out var sprite);
                            return new ObjectReferenceKeyframe { time = (drw.frame - 1) / Settings.clip.frameRate, value = sprite };
                        });
                    var keyframes =
                        emptyKeyframes
                            .Concat(finalKeyframe)
                            .Concat(visibleKeyframes)
                            .OrderBy(keyframe => keyframe.time)
                            .ToArray();
                    var transformPath = AnimationUtility.CalculateTransformPath(spriteRenderer.transform, Settings.RootGameObject.transform);
                    Settings.RegisteredSpriteCurvesLookup.Add(transformPath, keyframes.ToArray());
                }
            }
            Profiler.EndSample();
            return this;
        }

        static HashSet<string> spriteOrderAttributes = new HashSet<string> {
            "position.z",
            "offset.z",
        };

        /** <summary>
        Generate new curves for localPosition.z on every spriteRenderer
        transform to sort rendering based on Harmony rules.
        </summary> */
        public TBGClipBuilder WithSpriteOrderCurves(NodeToTransform nodeToTransform)
        {
            Profiler.BeginSample("WithSpriteOrderCurves");
            var curveGenerator = new TBGCurveGenerator
            {
                AnimationLength = Settings.animationLength,
                Stepped = Settings.Stepped,
            };
            // Collect data for sorting sprites.
            var nodeToZCurve = Settings.Animation.attrlinks
                .Where(attrlink => spriteOrderAttributes.Contains(attrlink.attr))
                .Select(attrlink =>
                {
                    var curve = curveGenerator.FromTimedValues(new TBGCurveGenerator.CurveInput
                    {
                        info = $"{attrlink.node}.position.z",
                        attribute = "position.z",
                        value = (float)attrlink.value,
                        timedValuePoints = attrlink.timedvalue == null
                            ? null
                            : Settings.Animation.timedvalues[attrlink.timedvalue].First().points,
                    },
                        valueMap: value => value);

                    return new { attrlink.node, curve };
                })
                .ToLookup(entry => entry.node, entry => entry.curve);
            var drawingNodes = Settings.Skeleton.nodes
                .Where(node => node.tag == "read")
                .ToList();
            var frameToNodeToZKeyframe = Enumerable
                .Range(0, Settings.animationLength)
                .Select(frame =>
                {
                    var nodeInfo = drawingNodes
                        .Select((node, zIndex) =>
                        {
                            var parentID = Settings.OutToIn[node.id].First();
                            var nodeChain = new List<string>
                            {
                                node.name
                            };
                            while (parentID > -1)
                            {
                                nodeChain.Add(Settings.NodeIDToName[parentID]);
                                parentID = Settings.OutToIn[parentID].First();
                            }
                            return new NodeOrderInfo
                            {
                                nodeID = node.id,
                                zIndex = zIndex,
                                zOffset = nodeChain.Aggregate(0.0, (last, current) =>
                                {
                                    var curves = nodeToZCurve[current];
                                    return last + (!curves.Any()
                                        ? 0
                                        : curves.First().GetValue(frame));
                                }),
                            };
                        })
                        .ToList();
                    nodeInfo.Sort();
                    return nodeInfo
                        .Select((nodeInfo, index) => new
                        {
                            nodeInfo.nodeID,
                            value = index * 0.001f,
                        })
                        .ToDictionary(entry => entry.nodeID, entry => entry.value);
                })
                .ToList();

            // Force sorting order on animation position.z curves.
            foreach (var node in drawingNodes)
            {
                var curve = new TBGCurveGenerator.BezierCurve(frameToNodeToZKeyframe
                    .Select((nodeToZKeyframe, index) => new TBGCurveGenerator.BezierPoint
                    {
                        x = index,
                        y = nodeToZKeyframe[node.id],
                        constSeg = true,
                    }));
                var transforms = nodeToTransform(node.name);
                foreach (var transform in transforms != null && transforms.Any()
                    ? transforms
                    : Settings.NodeToInstantiated[node.id]
                        .Select(instantiated => instantiated.transform)
                        .ToList())
                {
                    var transformPath = AnimationUtility.CalculateTransformPath(transform, Settings.RootGameObject.transform);
                    RegisterCurve(transformPath, "localPosition.z", curve, (a, b) => b);

                    // Update localPosition of node to reflect sorting order in prefab preview window.
                    try
                    {
                        var localPosition = transform.localPosition;
                        var zKeyframe = frameToNodeToZKeyframe.FirstOrDefault()?[node.id] ?? 0;
                        localPosition.z = zKeyframe;
                        transform.localPosition = localPosition;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            Profiler.EndSample();

            return this;
        }
    }
}

#endif
