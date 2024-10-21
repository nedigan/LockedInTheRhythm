#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Profiling;

namespace ToonBoom.TBGImporter
{
    public class TBGXmlData
    {
        public int exportVersion;
        public List<SpriteSheetSettings> spriteSheets;
        public List<SkeletonSettings> skeletons;
        public Dictionary<string, DrawingAnimationSettings> drawingAnimations;
        public Dictionary<string, AnimationSettings> animations;
        public ILookup<string, StageSettings> skeletonToStages;
        public TBGXmlData(AssetImportContext ctx)
        {
            Profiler.BeginSample("Unzip File 1");
#nullable enable
            using (var file = new FileStream(ctx.assetPath, FileMode.Open))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
            {
                Profiler.BeginSample("Sprite Sheets");
                XElement? spriteSheetsTag = null;
                var spriteSheetsFile = archive.GetEntry("spriteSheets.xml");
                if (spriteSheetsFile != null)
                {
                    XDocument spriteSheetsXML;
                    using (var spriteSheetReader = new StreamReader(spriteSheetsFile.Open(), Encoding.Default))
                        spriteSheetsXML = XDocument
                            .Parse(spriteSheetReader.ReadToEnd());
                    spriteSheetsTag = spriteSheetsXML
                        .Element("spritesheets");
                }
                spriteSheets = spriteSheetsTag != null
                    ? spriteSheetsTag
                        .Elements("spritesheet")
                        .Select(spriteSheet => new SpriteSheetSettings
                        {
                            name = (string)spriteSheet.Attribute("name"),
                            filename = (string)spriteSheet.Attribute("filename"),
                            resolution = (string)spriteSheet.Attribute("resolution"),
                            width = (int)spriteSheet.Attribute("width"),
                            height = (int)spriteSheet.Attribute("height"),
                            sprites = spriteSheet
                                .Elements("sprite")
                                .Select(sprite => new SpriteSettings
                                {
                                    rect = ((string)sprite.Attribute("rect"))
                                        .Split(',')
                                        .Select(value => int.Parse(value))
                                        .ToArray(),
                                    scaleX = (double)sprite.Attribute("scaleX"),
                                    scaleY = (double)sprite.Attribute("scaleY"),
                                    offsetX = (double)sprite.Attribute("offsetX"),
                                    offsetY = (double)sprite.Attribute("offsetY"),
                                    name = (string)sprite.Attribute("name"),
                                })
                            .OrderBy(sprite => sprite.name)
                            .ToList(),
                        })
                        .ToList()
                    : archive.Entries
                        .Where(entry => Path.GetExtension(entry.Name) == ".sprite")
                        .Select(entry =>
                        {
                            using var spriteReader = new StreamReader(entry.Open(), Encoding.Default);
                            var crop = XDocument.Parse(spriteReader.ReadToEnd())
                                .Element("crop");
                            var pathSplits = entry.FullName.Split('/');
                            return new
                            {
                                spriteSheetName = pathSplits[1],
                                resolution = pathSplits[2],
                                sprite = new SpriteSettings
                                {
                                    name = Path.GetFileNameWithoutExtension(entry.FullName),
                                    filename = string.Join(".", entry.FullName.Split('.').Reverse().Skip(1).Reverse()),
                                    scaleX = (double)crop.Attribute("scaleX"),
                                    scaleY = (double)crop.Attribute("scaleY"),
                                    offsetX = (double)crop.Attribute("pivotX"),
                                    offsetY = (double)crop.Attribute("pivotY"),
                                }
                            };
                        })
                        .ToLookup(entry => entry.resolution, entry => new { entry.sprite, entry.spriteSheetName })
                        .Select(entry => new SpriteSheetSettings
                        {
                            resolution = entry.Key,
                            name = entry.First().spriteSheetName,
                            sprites = entry.Select(entry => entry.sprite).ToList(),
                        })
                        .ToList();
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("Unzip File 2");
            using (var file = new FileStream(ctx.assetPath, FileMode.Open))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
            {

                Profiler.BeginSample("Parse skeleton.xml");
                var skeletonStream = XDocument.Load(archive.GetEntry("skeleton.xml").Open());
                exportVersion = (int)skeletonStream.Element("skeletons").Attribute("version");
                skeletons = skeletonStream
                      .Element("skeletons")
                    .Elements("skeleton")
                    .Select(skeleton => new SkeletonSettings
                    {
                        name = (string)skeleton.Attribute("name"),
                        nodes = skeleton
                            .Element("nodes")
                            .Elements()
                            .Select(element => new NodeSettings
                            {
                                tag = element.Name.ToString(),
                                id = (int)element.Attribute("id"),
                                name = (string)element.Attribute("name"),
                                visible = (bool?)element.Attribute("visible"),
                            })
                            .ToList(),
                        links = skeleton
                            .Element("links")
                            .Elements("link")
                            .Select(link => new LinkSettings
                            {
                                nodeIn = (string)link.Attribute("in") == "Top" ? -1 : (int)link.Attribute("in"),
                                nodeOut = (int)link.Attribute("out"),
                                port = (int?)link.Attribute("port"),
                            })
                            .ToList(),
                    })
                    .ToList();
                Profiler.EndSample();

                Profiler.BeginSample("Parse drawingAnimation.xml");
                drawingAnimations = XDocument.Load(archive.GetEntry("drawingAnimation.xml").Open())
                    .Element("drawingAnimations")
                    .Elements("drawingAnimation")
                    .Select(drawingAnimation => new DrawingAnimationSettings
                    {
                        name = (string)drawingAnimation.Attribute("name"),
                        spritesheet = (string)drawingAnimation.Attribute("spritesheet"),
                        drawings = drawingAnimation
                            .Elements("drawing")
                            .Select(drawing => new DrawingSettings
                            {
                                node = (string)drawing.Attribute("node"),
                                // name = (string)drawing.Attribute("name"), // Same as "node"
                                // drwId = (int)drawing.Attribute("drwId"), // Redundant lookup to "node"
                                drws = drawing
                                    .Elements("drw")
                                    .Select(drw => new DrwSettings
                                    {
                                        skinId = (int?)drw.Attribute("skinId") ?? 0,
                                        name = (string)drw.Attribute("name"),
                                        frame = (int)drw.Attribute("frame"),
                                        repeat = (int?)drw.Attribute("repeat") ?? 1,
                                    })
                                    .ToList(),
                            })
                            .ToDictionary(entry => entry.node, entry => entry.drws),
                    })
                    .ToDictionary(entry => entry.name, entry => entry);
                Profiler.EndSample();

                Profiler.BeginSample("Parse stage.xml");
                skeletonToStages = XDocument.Load(archive.GetEntry("stage.xml").Open())
                    .Element("stages")
                    .Elements("stage")
                    .Select(stage =>
                    {
                        var stageSettings = new StageSettings
                        {
                            name = (string)stage.Attribute("name"),
                            skins = new List<SkinSettings>(),
                            groups = new List<GroupSettings>(),
                            metadata = new List<Metadata>(),
                            nodes = stage
                                .Elements("node")
                                .Select(node => new StageNodeSettings
                                {
                                    drwId = (int)node.Attribute("drwId"),
                                    name = (string)node.Attribute("name"),
                                    groupId = (int)node.Attribute("groupId"),
                                    skinIds = ((string)node.Attribute("skinId"))
                                        .Split(',')
                                        .Where(value => value.Length > 0)
                                        .Select(value => int.Parse(value))
                                        .ToList(),
                                })
                                .ToList(),
                            play = stage
                                .Elements("play")
                                .Select(play => new PlaySettings
                                {
                                    name = (string)play.Attribute("name"),
                                    animation = (string)play.Attribute("animation"),
                                    drawingAnimation = (string)play.Attribute("drawingAnimation"),
                                    skeleton = (string)play.Attribute("skeleton"),
                                    framerate = play.Attributes("framerate")
                                        .Select(element => (int)element)
                                        .DefaultIfEmpty(30)
                                        .First(),
                                    markerLength = (int?)play.Attribute("markerLength")
                                })
                                .First(),
                        };
                        foreach (var element in stage.Elements())
                        {
                            switch (element.Name.ToString())
                            {
                                case "skin":
                                    stageSettings.skins.Add(new SkinSettings
                                    {
                                        skinId = (int)element.Attribute("skinId"),
                                        name = (string)element.Attribute("name"),
                                    });
                                    break;
                                case "group":
                                    stageSettings.groups.Add(new GroupSettings
                                    {
                                        groupId = (int)element.Attribute("groupId"),
                                        name = (string)element.Attribute("name"),
                                    });
                                    break;
                                case "meta":
                                    stageSettings.metadata.Add(new Metadata
                                    {
                                        node = (string)element.Attribute("node"),
                                        name = (string)element.Attribute("name"),
                                        value = (string)element.Attribute("value"),
                                    });
                                    break;
                                case "sound":
                                    stageSettings.sound = stage
                                        .Elements("sound")
                                        .Select(sound => new SoundSettings
                                        {
                                            name = (string)sound.Attribute("name"),
                                            time = (int)sound.Attribute("time"),
                                        })
                                        .First();
                                    break;
                            }
                        }
                        return stageSettings;
                    })
                    .ToLookup(entry => entry.play.skeleton, entry => entry);
                Profiler.EndSample();

                Profiler.BeginSample("Parse animation.xml");
                animations = XDocument.Load(archive.GetEntry("animation.xml").Open())
                    .Element("animations")
                    .Elements("animation")
                    .Select(animation => new AnimationSettings
                    {
                        name = (string)animation.Attribute("name"),
                        attrlinks = animation
                            .Element("attrlinks")
                            .Elements("attrlink")
                            .Select(attrlink => new AttrLinkSettings
                            {
                                node = (string)attrlink.Attribute("node"),
                                attr = (string)attrlink.Attribute("attr"),
                                timedvalue = (string?)attrlink.Attribute("timedvalue") ?? null,
                                value = (double?)attrlink.Attribute("value") ?? 0,
                            })
                            .ToList(),
                        timedvalues = animation
                            .Element("timedvalues")
                            .Elements()
                            .Select(timed => new TimedValueSettings
                            {
                                tag = timed.Name.ToString(),
                                name = (string)timed.Attribute("name"),
                                points = timed
                                    .Elements("pt")
                                    .Select(point =>
                                    {
                                        try
                                        {
                                            return new TimedValuePoint
                                            {
                                                x = (double)point.Attribute("x"),
                                                y = (double)point.Attribute("y"),
                                                z = (double?)point.Attribute("z"),
                                                lx = (double?)point.Attribute("lx"),
                                                ly = (double?)point.Attribute("ly"),
                                                rx = (double?)point.Attribute("rx"),
                                                ry = (double?)point.Attribute("ry"),
                                                lockedInTime = (int?)point.Attribute("lockedInTime"),
                                                constSeg = (bool?)point.Attribute("constSeg") ?? false,
                                                start = (int?)point.Attribute("start"),
                                            };
                                        }
                                        catch (System.Exception e)
                                        {
                                            // Dump the point xml to Debug
                                            Debug.Log(point);
                                            throw;
                                        }
                                    })
                                    .ToList(),
                            })
                            .ToLookup(entry => entry.name, entry => entry),
                    })
                    .ToDictionary(animation => animation.name, animation => animation);
                Profiler.EndSample();

            }
            Profiler.EndSample();
#nullable disable
        }
    }
}

#endif