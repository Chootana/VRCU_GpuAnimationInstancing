using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;


public class ExampleShaderInspector : ShaderGUI
{
    #region MaterialProperties 
    MaterialProperty mainTex;
    MaterialProperty color;
    MaterialProperty animTex;
    MaterialProperty startFrame;
    MaterialProperty frameCount;
    MaterialProperty offsetSeconds;
    MaterialProperty pixelCountPerFrame;

    MaterialProperty isRootMotion;
    MaterialProperty animRepeatTex;
    MaterialProperty repeatMax;
    MaterialProperty repeatNum;

    MaterialProperty isPause;
    MaterialProperty isLighting;
    MaterialProperty bumpMap;
    #endregion

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        // show all 
        //base.OnGUI(materialEditor, properties);

        Material material = materialEditor.target as Material;

        mainTex = FindProperty("_MainTex", props);
        color = FindProperty("_Color", props);
        animTex = FindProperty("_AnimTex", props);
        startFrame = FindProperty("_StartFrame", props);
        frameCount = FindProperty("_FrameCount", props);
        offsetSeconds = FindProperty("_OffsetSeconds", props);
        pixelCountPerFrame = FindProperty("_PixelCountPerFrame", props);

        isRootMotion = FindProperty("_ROOT_MOTION", props);
        animRepeatTex = FindProperty("_AnimRepeatTex", props);
        repeatMax = FindProperty("_RepeatMax", props);
        repeatNum = FindProperty("_RepeatNum", props);

        isPause = FindProperty("_PAUSE", props);
        isLighting = FindProperty("_LIGHTING", props);

        bumpMap = FindProperty("_BumpMap", props);


        // materialEditor.ShaderProperty(mainTexProps, mainTexProps.displayName);
        materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture"), mainTex, color);
        using (new EditorGUI.IndentLevelScope())
        {
            materialEditor.TextureScaleOffsetProperty(mainTex);
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();

        /* *** Animation *** */
        materialEditor.TexturePropertySingleLine(new GUIContent("Animation Texture"), animTex);
        materialEditor.ShaderProperty(startFrame, startFrame.displayName);
        materialEditor.ShaderProperty(frameCount, frameCount.displayName);
        materialEditor.ShaderProperty(offsetSeconds, offsetSeconds.displayName);
        materialEditor.ShaderProperty(pixelCountPerFrame, pixelCountPerFrame.displayName);

        EditorGUILayout.Space();

        /* *** Repeat *** */
        materialEditor.ShaderProperty(isRootMotion, isRootMotion.displayName);
        materialEditor.TexturePropertySingleLine(new GUIContent("Animation Repeat Texture"), animRepeatTex);
        materialEditor.ShaderProperty(repeatMax, repeatMax.displayName);
        materialEditor.ShaderProperty(repeatNum, repeatNum.displayName);

        EditorGUILayout.Space();

        /* *** DEBUG *** */
        materialEditor.ShaderProperty(isPause, isPause.displayName);

        /* *** Lighting *** */
        materialEditor.ShaderProperty(isLighting, isLighting.displayName);
        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), bumpMap);
        using (new EditorGUI.IndentLevelScope())
        {
            materialEditor.TextureScaleOffsetProperty(bumpMap);
            EditorGUILayout.Space();
        }


    }
}
