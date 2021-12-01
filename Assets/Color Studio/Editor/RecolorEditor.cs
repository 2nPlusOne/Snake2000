/* Color Studio by Ramiro Oliva (Kronnect)   /
/  Premium assets for Unity on kronnect.com */


using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace ColorStudio {

    [CustomEditor(typeof(Recolor))]
    public class RecolorEditor : Editor {

        SerializedProperty palette, applyPalette, mode, materialIndex, colorMatch, threshold, showOriginalTexture, colorOperations;
        SerializedProperty enableColorAdjustments, colorAdjustments, lutProp;
        Texture2D originalTexture;
        List<Color> originalColors;
        bool requireRefresh;
        bool pickingFromSceneView;
        bool hasMeshFilter;

        private void OnEnable() {
            palette = serializedObject.FindProperty("palette");
            applyPalette = serializedObject.FindProperty("applyPalette");
            mode = serializedObject.FindProperty("mode");
            materialIndex = serializedObject.FindProperty("materialIndex");
            colorMatch = serializedObject.FindProperty("colorMatch");
            threshold = serializedObject.FindProperty("threshold");
            colorOperations = serializedObject.FindProperty("_colorOperations");
            showOriginalTexture = serializedObject.FindProperty("showOriginalTexture");
            enableColorAdjustments = serializedObject.FindProperty("enableColorAdjustments");
            colorAdjustments = serializedObject.FindProperty("colorAdjustments");
            lutProp = colorAdjustments.FindPropertyRelative("LUT");
            hasMeshFilter = ((Recolor)target).GetComponent<MeshFilter>();
        }

        public void OnSceneGUI() {

            if (!pickingFromSceneView || SceneView.lastActiveSceneView.camera == null) return;

            Event e = Event.current;
            if (e == null) return;

            if (e.type != EventType.MouseDown || e.button != 0) return;

            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = controlID;

            e.Use();
            pickingFromSceneView = false;

            Recolor rc = (Recolor)target;
            MeshCollider tempCollider = null;
            if (rc.gameObject.GetComponent<MeshFilter>() != null && rc.gameObject.GetComponent<Collider>() == null) {
                tempCollider = rc.gameObject.AddComponent<MeshCollider>();
            }

            bool differentObject = true;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                if (hit.collider.gameObject == rc.gameObject) {
                    differentObject = false;
                    Texture2D tex = rc.GetOriginalTexture();
                    tex.EnsureTextureIsReadable();
                    Color[] originalColors = tex.GetPixels();
                    if (tex != null && originalColors != null) {
                        int x = Mathf.Clamp((int)(tex.width * hit.textureCoord.x), 0, tex.width - 1);
                        int y = Mathf.Clamp((int)(tex.height * hit.textureCoord.y), 0, tex.height - 1);
                    int colorIndex = y * tex.width + x;
                    if (colorIndex < originalColors.Length) {
                        Color hitColor = originalColors[colorIndex];
                        // ensure hit color is unique
                        bool repeated = false;
                        for (int k = 0; k < rc.colorOperations.Length; k++) {
                            if (rc.colorOperations[k].color == hitColor) {
                                repeated = true;
                                break;
                            }
                        }
                        if (!repeated) {
                            ColorEntry ce = new ColorEntry { color = hitColor, operation = ColorOperation.Preserve, replaceColor = hitColor };
                            ColorEntry[] cc = rc.colorOperations;
                            System.Array.Resize(ref cc, cc.Length + 1);
                            cc[cc.Length - 1] = ce;
                            rc.colorOperations = cc;
                            EditorUtility.SetDirty(rc);
                                serializedObject.Update();
                                requireRefresh = true;
                            }
                        }
                    }
                }
            }

            if (tempCollider != null) DestroyImmediate(tempCollider);

            if (differentObject) {
                EditorUtility.DisplayDialog("Info", "Please click on the same gameobject with the current Recolor script attached.", "Ok");
            }
        }

        public override void OnInspectorGUI() {


            Recolor rc = (Recolor)target;
            if (rc.GetComponent<Renderer>() == null) {
                EditorGUILayout.HelpBox("Recolor script requires an GameObject with a MeshRenderer or SpriteRenderer component.", MessageType.Warning);
                return;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(this.palette);

            CSPalette palette = (CSPalette)this.palette.objectReferenceValue;
            if (palette != null) {

                if (palette.material == null || palette.material.GetColorArray("_Colors") == null) {
                    palette.UpdateMaterial();
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);

                Rect space = EditorGUILayout.BeginVertical();
                GUILayout.Space(64);
                EditorGUILayout.EndVertical();

                palette.material.SetVector("_CursorPos", Vector3.left);
                EditorGUI.DrawPreviewTexture(space, Texture2D.whiteTexture, palette.material);

                if (GUILayout.Button("Open in Color Studio")) {
                    CSWindow cs = CSWindow.ShowWindow();
                    cs.LoadPalette(palette);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.PropertyField(applyPalette);
            } else {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Color Studio")) {
                    CSWindow.ShowWindow();
                }
                if (GUILayout.Button("Help")) {
                    EditorUtility.DisplayDialog("Quick Help", "This Recolor script changes colors of the gameobject or sprite at runtime.\n\nIf you assign a palette created with Color Studio, Recolor will transform the colors of the original texture to the nearest colors of the palette.\n\nYou can also specify custom color operations, like preserving or replacing individual colors from the original texture.", "Ok");
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.PropertyField(mode, new GUIContent("Recolor Mode"));
            EditorGUILayout.PropertyField(colorMatch);
            EditorGUILayout.PropertyField(threshold, new GUIContent("Color Threshold"));
            EditorGUILayout.PropertyField(materialIndex);

            if (mode.intValue != (int)RecolorMode.MainColorOnly) {

                if (originalTexture == null) {
                    originalTexture = rc.GetOriginalTexture();
                    if (originalTexture != null) {
                        originalTexture = Instantiate<Texture2D>(originalTexture);
                        originalTexture.filterMode = FilterMode.Point;
                    }
                    originalColors = rc.GetOriginalUniqueColors();
                }

                EditorGUILayout.PropertyField(showOriginalTexture);
                if (showOriginalTexture.boolValue) {
                    if (originalTexture != null) {
                        EditorGUILayout.BeginVertical(GUI.skin.box);

                        Rect space = EditorGUILayout.BeginVertical();
                        GUILayout.Space(128);
                        EditorGUILayout.EndVertical();

                        EditorGUI.DrawPreviewTexture(space, originalTexture);
                        EditorGUILayout.EndVertical();

                    }
                }
            }

            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(colorOperations, new GUIContent("Per Color Operations"), true);
            if (EditorGUI.EndChangeCheck()) {
                requireRefresh = true;
                EditorUtility.SetDirty(rc);
                serializedObject.ApplyModifiedProperties();
                rc.Refresh();
                GUIUtility.ExitGUI();
            }

            if (originalColors != null && originalColors.Count < 64 && GUILayout.Button("Add All Texture Colors")) {
                colorOperations.isExpanded = true;
                List<ColorEntry> cc = new List<ColorEntry>();
                if (rc.colorOperations != null) {
                    for (int k = 0; k < rc.colorOperations.Length; k++) {
                        int index = originalColors.IndexOf(rc.colorOperations[k].color);
                        if (index >= 0) {
                            originalColors.RemoveAt(index);
                        }
                        cc.Add(rc.colorOperations[k]);
                    }
                }
                for (int k = 0; k < originalColors.Count; k++) {
                    ColorEntry ce = new ColorEntry { color = originalColors[k], operation = ColorOperation.Preserve, replaceColor = originalColors[k] };
                    cc.Add(ce);
                }
                rc.colorOperations = cc.ToArray();
                EditorUtility.SetDirty(rc);
                serializedObject.Update();
                requireRefresh = true;
            }

            if (hasMeshFilter) {
                if (pickingFromSceneView) {
                    EditorGUILayout.HelpBox("Click on the object in the SceneView to pick the exact texture color", MessageType.Info);
                    if (GUILayout.Button("Cancel picking")) {
                        pickingFromSceneView = false;
                    }
                } else if (GUILayout.Button("Pick Original Color From SceneView")) {
                    pickingFromSceneView = true;
                }
            }

            if (mode.intValue != (int)RecolorMode.MainColorOnly) {
                if (!rc.isSprite && originalTexture != null && GUILayout.Button("Add Main Texture Colors")) {
                    colorOperations.isExpanded = true;
                    List<ColorEntry> cc = new List<ColorEntry>();
                    List<Color> mainColors = rc.GetOriginalTextureMainColors();
                    if (mainColors != null) {
                        if (rc.colorOperations != null) {
                            for (int k = 0; k < rc.colorOperations.Length; k++) {
                                int index = mainColors.IndexOf(rc.colorOperations[k].color);
                                if (index >= 0) {
                                    mainColors.RemoveAt(index);
                                }
                                cc.Add(rc.colorOperations[k]);
                            }
                        }

                        for (int k = 0; k < mainColors.Count; k++) {
                            ColorEntry ce = new ColorEntry { color = mainColors[k], operation = ColorOperation.Preserve, replaceColor = mainColors[k] };
                            cc.Add(ce);
                        }
                        rc.colorOperations = cc.ToArray();
                        EditorUtility.SetDirty(rc);
                        serializedObject.Update();
                        requireRefresh = true;

                    }
                }
                if (!rc.isSprite && mode.intValue == (int)RecolorMode.VertexColors && GUILayout.Button("Add Vertex Colors")) {
                    colorOperations.isExpanded = true;
                    List<ColorEntry> cc = new List<ColorEntry>();
                    List<Color> mainColors = rc.GetOriginalVertexColors();
                    if (rc.colorOperations != null) {
                        for (int k = 0; k < rc.colorOperations.Length; k++) {
                            int index = mainColors.IndexOf(rc.colorOperations[k].color);
                            if (index >= 0) {
                                mainColors.RemoveAt(index);
                            }
                            cc.Add(rc.colorOperations[k]);
                        }
                    }

                    for (int k = 0; k < mainColors.Count; k++) {
                        ColorEntry ce = new ColorEntry { color = mainColors[k], operation = ColorOperation.Preserve, replaceColor = mainColors[k] };
                        cc.Add(ce);
                    }
                    rc.colorOperations = cc.ToArray();
                    EditorUtility.SetDirty(rc);
                    serializedObject.Update();
                    requireRefresh = true;

                }
            }

            // Color adjustments
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(enableColorAdjustments, new GUIContent("Color Correction"), true);
            if (enableColorAdjustments.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(colorAdjustments, true);
                EditorGUI.indentLevel--;
            }

            CheckLUTSettings((Texture2D)lutProp.objectReferenceValue);

            if (rc.enabled) {
                if (GUILayout.Button("Refresh")) {
                    requireRefresh = false;
                    rc.Refresh();
                }
            }

            if (serializedObject.ApplyModifiedProperties() || rc.dirty || requireRefresh) {
                requireRefresh = true;
                rc.dirty = false;
                if (rc.enabled) {
                    if (GUIUtility.hotControl == 0) {
                        requireRefresh = false;
                        rc.Refresh();
                    }
                }
            }
        }

        public void CheckLUTSettings(Texture2D tex) {
            if (Application.isPlaying || tex == null)
                return;
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path))
                return;
            TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null)
                return;
            if (!imp.isReadable || imp.textureType != TextureImporterType.Default || imp.sRGBTexture || imp.mipmapEnabled || imp.textureCompression != TextureImporterCompression.Uncompressed || imp.wrapMode != TextureWrapMode.Clamp || imp.filterMode != FilterMode.Bilinear) {
                EditorGUILayout.HelpBox("Texture has invalid import settings.", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Fix Texture Import Settings", GUILayout.Width(200))) {
                    imp.isReadable = true;
                    imp.textureType = TextureImporterType.Default;
                    imp.sRGBTexture = false;
                    imp.mipmapEnabled = false;
                    imp.textureCompression = TextureImporterCompression.Uncompressed;
                    imp.wrapMode = TextureWrapMode.Clamp;
                    imp.filterMode = FilterMode.Bilinear;
                    imp.anisoLevel = 0;
                    imp.SaveAndReimport();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }


    }
}

