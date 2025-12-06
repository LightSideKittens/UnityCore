using UnityEngine;
using UnityEditor;


    public class UniText_SDFShaderGUI : UniText_BaseShaderGUI
    {
        static ShaderFeature s_OutlineFeature, s_UnderlayFeature;

        static bool s_Face = true, s_Outline = true, s_Underlay = true;

        static UniText_SDFShaderGUI()
        {
            s_OutlineFeature = new ShaderFeature()
            {
                undoLabel = "Outline",
                keywords = new[] { "OUTLINE_ON" }
            };

            s_UnderlayFeature = new ShaderFeature()
            {
                undoLabel = "Underlay",
                keywords = new[] { "UNDERLAY_ON", "UNDERLAY_INNER" },
                label = new GUIContent("Underlay Type"),
                keywordLabels = new[]
                {
                    new GUIContent("None"), new GUIContent("Normal"), new GUIContent("Inner")
                }
            };
        }

        protected override void DoGUI()
        {
            s_Face = BeginPanel("Face", s_Face);
            if (s_Face)
                DoFacePanel();
            EndPanel();

            s_Outline = BeginPanel("Outline", s_OutlineFeature, s_Outline);
            if (s_Outline)
                DoOutlinePanel();
            EndPanel();

            if (m_Material.HasProperty("_UnderlayColor"))
            {
                s_Underlay = BeginPanel("Underlay", s_UnderlayFeature, s_Underlay);
                if (s_Underlay)
                    DoUnderlayPanel();
                EndPanel();
            }

            s_DebugExtended = BeginPanel("Debug Settings", s_DebugExtended);
            if (s_DebugExtended)
                DoDebugPanel();
            EndPanel();

            EditorGUILayout.Space();
        }

        void DoFacePanel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_FaceColor", "Color");

            if (m_Material.HasProperty("_FaceDilate"))
                DoSlider("_FaceDilate", "Dilate");

            if (m_Material.HasProperty("_OutlineSoftness"))
                DoSlider("_OutlineSoftness", "Softness");

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoOutlinePanel()
        {
            EditorGUI.indentLevel += 1;
            DoColor("_OutlineColor", "Color");
            DoSlider("_OutlineWidth", "Thickness");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoUnderlayPanel()
        {
            EditorGUI.indentLevel += 1;
            s_UnderlayFeature.DoPopup(m_Editor, m_Material);
            DoColor("_UnderlayColor", "Color");
            DoSlider("_UnderlayOffsetX", "Offset X");
            DoSlider("_UnderlayOffsetY", "Offset Y");
            DoSlider("_UnderlayDilate", "Dilate");
            DoSlider("_UnderlaySoftness", "Softness");
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }

        void DoDebugPanel()
        {
            EditorGUI.indentLevel += 1;
            DoTexture2D("_MainTex", "Font Atlas");
            DoFloat("_GradientScale", "Gradient Scale");
            DoFloat("_TextureWidth", "Texture Width");
            DoFloat("_TextureHeight", "Texture Height");
            EditorGUILayout.Space();
            DoFloat("_ScaleX", "Scale X");
            DoFloat("_ScaleY", "Scale Y");

            if (m_Material.HasProperty("_Sharpness"))
                DoSlider("_Sharpness", "Sharpness");

            if (m_Material.HasProperty("_PerspectiveFilter"))
                DoSlider("_PerspectiveFilter", "Perspective Filter");

            EditorGUILayout.Space();
            DoFloat("_VertexOffsetX", "Offset X");
            DoFloat("_VertexOffsetY", "Offset Y");

            if (m_Material.HasProperty("_MaskSoftnessX"))
            {
                EditorGUILayout.Space();
                DoFloat("_MaskSoftnessX", "Softness X");
                DoFloat("_MaskSoftnessY", "Softness Y");
                DoVector("_ClipRect", "Clip Rect", s_LbrtVectorLabels);
            }

            if (m_Material.HasProperty("_Stencil"))
            {
                EditorGUILayout.Space();
                DoFloat("_Stencil", "Stencil ID");
                DoFloat("_StencilComp", "Stencil Comp");
            }

            if (m_Material.HasProperty("_CullMode"))
            {
                EditorGUILayout.Space();
                DoPopup("_CullMode", "Cull Mode", s_CullingTypeLabels);
            }

            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(true);
            DoFloat("_ScaleRatioA", "Scale Ratio A");
            DoFloat("_ScaleRatioB", "Scale Ratio B");
            DoFloat("_ScaleRatioC", "Scale Ratio C");
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();
        }
    }

