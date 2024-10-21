#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToonBoom.TBGImporter
{
    public struct SpriteSheetSettings
    {
        public string name;
        public string filename;
        public string resolution;
        public int width;
        public int height;
        public List<SpriteSettings> sprites;
    }
    public struct SpriteSettings
    {
        public string filename;
        public int[] rect;
        public double scaleX;
        public double scaleY;
        public double offsetX;
        public double offsetY;
        public string name;
    }
    public struct SkeletonSettings
    {
        public string name;
        public List<NodeSettings> nodes;
        public List<LinkSettings> links;
    }
    public struct NodeSettings
    {
        public string tag;
        public int id;
        public string name;
        public bool? visible;
    }
    public struct LinkSettings
    {
        public int nodeIn;
        public int nodeOut;
        public int? port;
    }
    public struct DrawingChannelInfo
    {
        public int channelID;
        public Transform transform;
        public string propertyName;
    }
    public struct DrawingAnimationSettings
    {
        public string name;
        public string spritesheet;
        public Dictionary<string, List<DrwSettings>> drawings;
    }
    public struct DrawingSettings
    {
        public string node;
        public List<DrwSettings> drws;
    }
    public struct DrwSettings
    {
        public int skinId;
        public string name;
        public int frame;
        public int repeat;
    }
    public struct AnimationSettings
    {
        public string name;
        public List<AttrLinkSettings> attrlinks;
        public ILookup<string, TimedValueSettings> timedvalues;
    }
    public struct AttrLinkSettings
    {
        public string node;
        public string attr;
        public string timedvalue;
        public double value;
    }

    public struct TimedValueSettings
    {
        public string tag;
        public string name;
        public List<TimedValuePoint> points;
    }
    public struct TimedValuePoint
    {
        public double x;
        public double y;
        public double? z;
        public double? lx;
        public double? ly;
        public double? rx;
        public double? ry;
        public int? lockedInTime;
        public bool? constSeg;
        public int? start;
    }
    public struct StageSettings
    {
        public string name;
        public List<SkinSettings> skins;
        public List<GroupSettings> groups;
        public List<Metadata> metadata;
        public List<StageNodeSettings> nodes;
        public PlaySettings play;
        public SoundSettings sound;
    }
    public struct SkinSettings
    {
        public int skinId;
        public string name;
    }
    public struct Metadata
    {
        public string name;
        public string value;
        public string node;
    }
    public struct GroupSettings
    {
        public int groupId;
        public string name;
    }
    public struct StageNodeSettings
    {
        public int drwId;
        public string name;
        public int groupId;
        public List<int> skinIds;
    }
    public struct PlaySettings
    {
        public string name;
        public string skeleton;
        public string animation;
        public string drawingAnimation;
        public int framerate;
        public int? markerLength;
    }
    public struct SoundSettings
    {
        public string name;
        public int time;
    }
    public struct NodeOrderInfo : IComparable<NodeOrderInfo>
    {
        const double QUANTUM = 1.0 / (1024.0 * 8);
        public int zIndex; public double zOffset;
        public int nodeID;
        private int Transform(double value)
        {
            return (int)Math.Round(value / QUANTUM);
        }
        public int CompareTo(NodeOrderInfo other)
        {
            var first = (int)Transform(zOffset);
            var second = (int)Transform(other.zOffset);
            return first == second
                ? zIndex - other.zIndex
                : first < second
                    ? 1
                    : -1;
        }
    }
    public class InstantiatedNode
    {
        public int id;
        public string name;
        public GameObject gameObject;
    }
    public class InstantiatedBone
    {
        public string readName;
        public string boneName;
        public GameObject gameObject;
    }
    public class SynthesizedCurve
    {
        public string relativePath;
        public Type type;
        public string propertyName;
        public AnimationCurve animationCurve;
    }
    public class BoneAdditionalInfo
    {
        public float length;
        public float radius;
        public int nodeID;
        public Vector2 position;
        public Quaternion rotation;
    }
    public class SkewTransforms
    {
        public Transform skewBase;
        public Transform skewCounter;
    }
    public class CutterEntry
    {
        public SpriteRenderer cuttee;
        public SpriteRenderer matte;
        public bool inverse;
    }
    public struct TransformRestInfo
    {
        public Vector2 restRootPosition;
        public float restRootAngle;
        public override string ToString()
        {
            return $"{restRootPosition}, {restRootAngle}";
        }
    }
    public struct BoneRestInfo
    {
        public int parentNode;
        public float restLength;
        public float restRadius;
        public override string ToString()
        {
            return $"{restLength}, {restRadius}";
        }
    }
    public struct NodeInstance
    {
        public string name;
        public Transform transform;
    }
    public delegate float OffsetRetriever(float time);
    public delegate double ValueMap(double inputValue);
    public delegate ValueMap NodeToValueMap(string nodeID);
    public delegate float BlendFunction(float a, float b);
    public delegate IEnumerable<Transform> NodeToTransform(string node);
    public delegate IEnumerable<NodeInstance> NodeToNameTransform(string node);
    public delegate List<string> NodeMap(string node);
    public class AdvancedNodeMapping
    {
        public NodeToNameTransform nodeToInstance;
        public Dictionary<String, NodeValueTransform> propertyToNodeValueTransform;
    }
    public class NodeValueTransform
    {
        public NodeToValueMap nodeToValueMap;
        public BlendFunction blendFunction;
    }
    delegate AdvancedNodeMapping AttributeToAdvancedNodeMapping(string attribute);
}

#endif