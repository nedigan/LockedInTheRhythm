#if ENABLE_UNITY_2D_ANIMATION && ENABLE_UNITY_COLLECTIONS

using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.Linq;
using System.Collections.Generic;

namespace ToonBoom.TBGImporter
{
    public struct RenderState
    {
        public GameObject instance;
        public PreviewRenderUtility previewRenderUtility;

        public bool Initialize(Object target)
        {
            if (previewRenderUtility != null)
                return false;
            var assetPath = AssetDatabase.GetAssetPath(target);
            var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (gameObject == null)
                return false;
            previewRenderUtility = new PreviewRenderUtility();
            if (instance == null)
                instance = previewRenderUtility.InstantiatePrefabInScene(gameObject);
            previewRenderUtility.AddSingleGO(instance);
            var camera = previewRenderUtility.camera;
            camera.orthographic = true;
            var renderers = instance.GetComponentsInChildren<SpriteRenderer>();
            var bounds = new Bounds();
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            camera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;
            camera.transform.position = bounds.center - Vector3.forward * 4;
            EditorUtility.SetCameraAnimateMaterials(previewRenderUtility.camera, true);
            return true;
        }
        public readonly Texture Render(Rect rect, GUIStyle background)
        {
            previewRenderUtility.BeginPreview(rect, background);
            previewRenderUtility.camera.Render();
            return previewRenderUtility.EndPreview();
        }
        public readonly Texture2D RenderStatic(Rect rect)
        {
            previewRenderUtility.BeginStaticPreview(rect);
            previewRenderUtility.camera.Render();
            return previewRenderUtility.EndStaticPreview();
        }
        public void Destroy()
        {
            if (previewRenderUtility != null)
            {
                previewRenderUtility.Cleanup();
                previewRenderUtility = null;
            }
            if (instance == null)
                Object.DestroyImmediate(instance);
        }
    }
    [ExecuteInEditMode]
    [CustomEditor(typeof(TBGImporter), true)]
    public class TBGImporterEditor : ScriptedImporterEditor
    {
        public override bool HasPreviewGUI()
        {
            return true;
        }
        protected override bool useAssetDrawPreview => false;
        private static GUIContent[] timeIcons = new GUIContent[2];
        private static bool shouldAnimate = true;
        private static RenderState renderState;

        private EditorCools.Editor.ButtonsDrawer _buttonsDrawer;

        public override void OnEnable()
        {
            base.OnEnable();
            _buttonsDrawer = new EditorCools.Editor.ButtonsDrawer(target);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Tools header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Utiliites", EditorStyles.boldLabel);
            _buttonsDrawer.DrawButtons(targets);
            ApplyRevertGUI();
        }

        public override void OnPreviewSettings()
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
                return;

            if (timeIcons[0] == null)
            {
                timeIcons[0] = EditorGUIUtility.TrIconContent("PlayButton");
                timeIcons[1] = EditorGUIUtility.TrIconContent("PauseButton");

                if (bool.TryParse(EditorPrefs.GetString("HarmonyPreviewAnimating"), out bool animate))
                {
                    shouldAnimate = animate;
                }
            }

            GUIStyle toolbarButton = (GUIStyle)"toolbarbutton";
            if (GUILayout.Button(timeIcons[shouldAnimate ? 1 : 0], toolbarButton))
            {
                shouldAnimate = !shouldAnimate;
                EditorPrefs.SetString("HarmonyPreviewAnimating", shouldAnimate.ToString());
            }
        }
        public override bool RequiresConstantRepaint()
        {
            return shouldAnimate;
        }
        private float lastUpdateTime = 0;
        private float currentAnimationTime = 0;
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture)
            {
                if (Event.current.type != EventType.Repaint)
                    return;
                EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, 40f), "Material preview \nnot available");
                return;
            }

            // Reinit.
            renderState.Initialize(target);

            // Every frame.
            if (renderState.instance != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(target);
                var animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                if (shouldAnimate)
                {
                    var time = (float)EditorApplication.timeSinceStartup;
                    currentAnimationTime = Mathf.Repeat(currentAnimationTime + time - lastUpdateTime, animationClip.length);
                    lastUpdateTime = time;
                }
                animationClip.SampleAnimation(renderState.instance, currentAnimationTime);
                var texture = renderState.Render(rect, background);
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, false);
            }
        }
        public override void OnDisable()
        {
            renderState.Destroy();
            base.OnDisable();
        }
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            // BUG - this is never called from Unity
            // Debug.Log("RenderStaticPreview");

            // var pinkTex = new Texture2D(1, 1);
            // pinkTex.SetPixel(0, 0, Color.magenta);
            // return pinkTex; // If this is returned, the preview is a magenta square.

            renderState.Initialize(target);
            return renderState.RenderStatic(new Rect(0, 0, width, height));
        }
    }
}

#endif