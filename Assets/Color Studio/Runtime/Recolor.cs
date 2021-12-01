using System;
using UnityEngine;
using System.Collections.Generic;

namespace ColorStudio {

    public enum RecolorMode {
        MainColorOnly,
        Texture,
        MainColorAndTexture,
        VertexColors
    }

    [ExecuteInEditMode]
    public class Recolor : MonoBehaviour, IColorOperation {

        [Tooltip("Assign an existing palette created with Color Studio")]
        public CSPalette palette;
        [Tooltip("Apply palette to object colors")]
        public bool applyPalette = true;
        public RecolorMode mode = RecolorMode.Texture;
        [Tooltip("Specify the submesh material to be modified")]
        public int materialIndex;
        [Tooltip("Specify how colors are compared")]
        public ColorMatchMode colorMatch = ColorMatchMode.RGB;
        [Tooltip("Sensibility for custom color operations. 0 means exact color.")]
        [Range(0, 1)]
        public float threshold;
        [SerializeField] ColorEntry[] _colorOperations;
        public ColorEntry[] colorOperations {
            get { return _colorOperations; }
            set { _colorOperations = value; }
        }
        public bool enableColorAdjustments;
        public ColorAdjustments colorAdjustments;


#if UNITY_EDITOR
        public bool showOriginalTexture;
        public bool dirty;
#endif

        [NonSerialized]
        public bool isSprite, isMeshFilter, isSkinnedMesh;

        [SerializeField]
        Material[] backupMats;

        [SerializeField]
        Sprite backupSprite;

        [SerializeField]
        Mesh backupMesh;


        const string CS_NAME_SUFFIX = "_ColorStudio";

        void OnEnable() {
            Refresh();
        }

        void OnDisable() {
            RestoreMaterialsBackup();
        }

        public void Refresh() {

            RestoreMaterialsBackup();
            MakeMaterialsBackup();

            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null) return;
            Material[] mats = renderer.sharedMaterials;
            if (mats == null || materialIndex >= mats.Length) return;
            Material mat = Instantiate(mats[materialIndex]);
            mat.name = mats[materialIndex].name + CS_NAME_SUFFIX;

            CSPalette palette = (applyPalette && this.palette != null) ? this.palette : CSPalette.CreateEmptyPalette();
            switch (mode) {
                case RecolorMode.MainColorOnly:
                    mat.color = palette.GetNearestColor(mat.color, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
                    break;
                case RecolorMode.Texture:
                    if (isSprite) {
                        SpriteRenderer spr = (SpriteRenderer)renderer;
                        UpdateSpriteTexture(palette, spr);
                        return;
                    } else if (mat.mainTexture != null) {
                        mat.mainTexture = palette.GetNearestTexture(mat.mainTexture, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
                    }
                    break;
                case RecolorMode.MainColorAndTexture:
                    mat.color = palette.GetNearestColor(mat.color, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
                    if (isSprite) {
                        SpriteRenderer spr = (SpriteRenderer)renderer;
                        UpdateSpriteTexture(palette, spr);
                    } else if (mat.mainTexture != null) {
                        mat.mainTexture = palette.GetNearestTexture(mat.mainTexture, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
                    }
                    break;
                case RecolorMode.VertexColors:
                    if (isMeshFilter || isSkinnedMesh) {
                        UpdateMeshColors(palette);
                    }
                    break;
            }

            mats[materialIndex] = mat;
            renderer.sharedMaterials = mats;

        }

        void MakeMaterialsBackup() {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterials == null) {
                backupMats = null;
                return;
            }
            if (backupMats == null || backupMats.Length != renderer.sharedMaterials.Length) {
                backupMats = new Material[renderer.sharedMaterials.Length];
            }
            for (int k = 0; k < renderer.sharedMaterials.Length; k++) {
                backupMats[k] = renderer.sharedMaterials[k];
            }
            if (isSprite) {
                backupSprite = ((SpriteRenderer)renderer).sprite;
            } else if (isMeshFilter) {
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf != null) {
                    backupMesh = mf.sharedMesh;
                }
            } else if (isSkinnedMesh) {
                SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
                if (smr != null) {
                    backupMesh = smr.sharedMesh;
                }
            }
        }

        void RestoreMaterialsBackup() {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null) return;

            isSprite = renderer is SpriteRenderer;
            isMeshFilter = renderer is MeshRenderer;
            isSkinnedMesh = renderer is SkinnedMeshRenderer;

            if (backupMats != null && backupMats.Length > 0) {
                Material[] mats = renderer.sharedMaterials;
                if (mats != null) {
                    for (int k = 0; k < mats.Length; k++) {
                        if (mats[k] != null && mats[k].name.Contains(CS_NAME_SUFFIX)) {
                            renderer.sharedMaterials = backupMats;
                            break;
                        }
                    }
                }
            }
            if (isSprite) {
                if (backupSprite != null) {
                    SpriteRenderer spr = ((SpriteRenderer)renderer);
                    if (spr != null && spr.sprite != null && spr.sprite.name.Contains(CS_NAME_SUFFIX)) {
                        spr.sprite = backupSprite;
                    }
                }
            } else if (isMeshFilter) {
                if (backupMesh != null) {
                    MeshFilter mf = GetComponent<MeshFilter>();
                    if (mf != null) {
                        if (mf.sharedMesh != null && mf.sharedMesh.name.Contains(CS_NAME_SUFFIX)) {
                            mf.sharedMesh = backupMesh;
                        }
                    }
                }
            } else if (isSkinnedMesh) {
                if (backupMesh != null) {
                    SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
                    if (smr != null) {
                        if (smr.sharedMesh != null && smr.sharedMesh.name.Contains(CS_NAME_SUFFIX)) {
                            smr.sharedMesh = backupMesh;
                        }
                    }
                }
            }
        }


        void UpdateSpriteTexture(CSPalette palette, SpriteRenderer r) {
            if (r.sprite == null || r.sprite.texture == null) return;
            Sprite sprite = r.sprite;
            Texture2D newTexture = palette.GetNearestTexture(sprite.texture, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
            Sprite newSPrite = Sprite.Create(newTexture, sprite.rect, new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height), sprite.pixelsPerUnit);
            newSPrite.name = sprite.name + CS_NAME_SUFFIX;
            r.sprite = newSPrite;
        }


        void UpdateMeshColors(CSPalette palette) {
            if (isMeshFilter) {
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) return;
                Color[] meshColors = mf.sharedMesh.colors;
                if (meshColors == null) return;
                Mesh mesh = Instantiate(mf.sharedMesh);
                mesh.name = mesh.name + CS_NAME_SUFFIX;
                mesh.colors = palette.GetNearestColors(meshColors, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
                mf.sharedMesh = mesh;
            } else if (isSkinnedMesh) {
                SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
                if (smr == null || smr.sharedMesh == null) return;
                Color[] meshColors = smr.sharedMesh.colors;
                if (meshColors == null) return;
                Mesh mesh = Instantiate(smr.sharedMesh);
                mesh.name = mesh.name + CS_NAME_SUFFIX;
                mesh.colors = palette.GetNearestColors(meshColors, colorMatch, threshold, colorOperations, enableColorAdjustments, colorAdjustments);
                smr.sharedMesh = mesh;
            }
        }


        public Texture2D GetOriginalTexture() {

            if (isSprite) {
                if (backupSprite != null && backupSprite.texture != null) {
                    return backupSprite.texture;
                }
            } else {
                if (backupMats == null) return null;
                if (materialIndex >= backupMats.Length) return null;
                Material mat = backupMats[materialIndex];
                if (mat.mainTexture != null && mat.mainTexture is Texture2D) {
                    return (Texture2D)mat.mainTexture;
                }
            }
            return null;
        }

        public List<Color> GetOriginalUniqueColors() {
            Texture2D tex = GetOriginalTexture();
            if (tex == null) return null;
            Color[] colors = tex.GetPixels();
            return GetUniqueColors(colors);
        }


        public List<Color> GetOriginalTextureMainColors() {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) return null;
            Mesh mesh = mf.sharedMesh;
            if (mesh == null || mesh.uv == null) return null;
            Texture2D tex = GetOriginalTexture();
            if (tex == null) return null;

            HashSet<Color> uniqueColors = new HashSet<Color>();
            Color[] colors = tex.GetPixels();
            int w = tex.width - 1;
            int h = tex.height - 1;
            int tw = w + 1;
            Vector2[] uvs = mesh.uv;
            for (int k = 0; k < uvs.Length; k++) {
                int x = (int)(uvs[k].x * w);
                int y = (int)(uvs[k].y * h);
                int index = y * tw + x;
                if (index >= 0 && index < colors.Length) {
                    if (!uniqueColors.Contains(colors[index])) {
                        uniqueColors.Add(colors[index]);
                    }
                }
            }
            return new List<Color>(uniqueColors);
        }


        public List<Color> GetOriginalVertexColors() {
            Color[] meshColors = null;
            if (isMeshFilter) {
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) return null;
                meshColors = mf.sharedMesh.colors;
            } else if (isSkinnedMesh) {
                SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
                if (smr == null || smr.sharedMesh == null) return null;
                meshColors = smr.sharedMesh.colors;
            }
            if (meshColors == null) return null;
            return GetUniqueColors(meshColors);
        }

        readonly static FastHashSet<Color> match = new FastHashSet<Color>();

        List<Color> GetUniqueColors(Color[] colors) {
            if (colors == null) return null;
            match.Clear();
            int lastHash = -1;
            for (int k = 0; k < colors.Length; k++) {
                int r = (int)(colors[k].r * 255);
                int g = (int)(colors[k].g * 255);
                int b = (int)(colors[k].b * 255);
                int colorHash = (r << 16) + (g << 8) + b;
                if (lastHash != colorHash && !match.ContainsKey(colorHash)) {
                    lastHash = colorHash;
                    match.Add(colorHash, colors[k]);
                }
            }
            return match.Values;

        }

    }

}