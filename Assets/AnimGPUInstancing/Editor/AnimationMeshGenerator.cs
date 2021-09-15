#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.Rendering;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

public class AnimationMeshGenerator : EditorWindow
{
    // Const Parameters 
    private const int BoneMatrixRowCount = 3;
    private const int TargetFrameRate = 30;


    // User Parameters 
    private const int MaxRepeat = 20;


    // Components 
    private GameObject targetObject;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private Animator animator;
    private AnimationClip[] clips;
    private Shader animShader;
    private GameObject goUdon;
    private UdonBehaviour udonBehaviour;
    private string saveName = "AnimatedMesh";
    private string savePath;

    private int shaderIdx;

    // Scroll 
    private Vector2 _scrollPosition = Vector2.zero;

    [MenuItem("Window/Extension Tools/Animation Mesh Generator")]
    static void Open()
    {
        GetWindow<AnimationMeshGenerator>();
    }

    private void OnGUI()
    {
        GUILayout.Label("", EditorStyles.boldLabel);

        /* *** Get Components **** */
        Color defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Prefab (Project)", EditorStyles.whiteLabel);

            }
            GUI.backgroundColor = defaultColor;

            EditorGUI.indentLevel++;
            targetObject = EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true) as GameObject;


            if (targetObject == null) return;


            string udonName = "AnimationFrameInfoList";
            var transUdon = targetObject.transform.Find(udonName);
            if (transUdon == null)
            {
                EditorGUILayout.HelpBox($"Game Object (Udon) Not Found: {udonName}", MessageType.Error);
                return;
            }
            goUdon = transUdon.gameObject;

            udonBehaviour = goUdon.GetComponent<UdonBehaviour>();
            if (udonBehaviour == null)
            {
                EditorGUILayout.HelpBox($"Udon Behaviour Not Found: ", MessageType.Error);
                return;
            }

            EditorGUI.indentLevel--;
        }

        /* *** Scroll Start *** */
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Label("", EditorStyles.boldLabel);
        /* *** *** *** */

        /* *** Skinned Mesh Renderer **** */
        defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Skinned Mesh Renderer", EditorStyles.whiteLabel);

            }

            EditorGUI.indentLevel++;


            skinnedMeshRenderers = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers == null)
            {
                EditorGUILayout.HelpBox("Skinned Mesh Renderer Not Found", MessageType.Error);
                return;
            }

            // Show Components 
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                EditorGUILayout.LabelField($"{skinnedMeshRenderers[i].name}");
            }
            GUI.backgroundColor = defaultColor;


            EditorGUI.indentLevel--;
        }

        GUILayout.Label("", EditorStyles.boldLabel);
        /* *** *** *** */

        /* *** Animations **** */
        defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Animation Clips", EditorStyles.whiteLabel);

            }

            EditorGUI.indentLevel++;

            Animator[] animators = targetObject.GetComponentsInChildren<Animator>();
            if (!animators.Any() || animators.Count() != 1)
            {
                EditorGUILayout.HelpBox("Only Support 1 Animator", MessageType.Error);
                return;
            }
            Debug.Log(animators.First());
            animator = animators.First();

            if (animator.runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox("Runtime Animator Contoroller Not Found", MessageType.Error);
                return;

            }

            clips = animator.runtimeAnimatorController.animationClips;
            if (!clips.Any())
            {
                EditorGUILayout.HelpBox("Animation Clips Not Found", MessageType.Warning);
                return;
            }

            // Show Components 
            for (int i = 0; i < clips.Length; i++)
            {
                EditorGUILayout.LabelField($"{clips[i].name}");
            }
            GUI.backgroundColor = defaultColor;


            EditorGUI.indentLevel--;
        }

        GUILayout.Label("", EditorStyles.boldLabel);
        /* *** *** *** */

        /* *** Shader **** */
        defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;


            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Shader", EditorStyles.whiteLabel);

            }

            EditorGUI.indentLevel++;

            ShaderInfo[] shaders = ShaderUtil.GetAllShaderInfo();
            string[] shaderNames = shaders.Select(path => path.name).Where(name => name.Contains("AnimationGpuInstancing")).ToArray();
            if (!shaderNames.Any()) return;

            shaderIdx = EditorGUILayout.Popup(shaderIdx, shaderNames);

            animShader = Shader.Find(shaderNames[shaderIdx]);
            if (animShader == null)
            {
                EditorGUILayout.HelpBox($"Shader Not Found ", MessageType.Error);
                return;
            }



            GUI.backgroundColor = defaultColor;



            EditorGUI.indentLevel--;
        }

        GUILayout.Label("", EditorStyles.boldLabel);
        /* *** *** *** */

        /* *** Path **** */
        defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Save Path", EditorStyles.whiteLabel);

            }

            EditorGUI.indentLevel++;

            //PrefabUtility.GetPrefabParent(gameObject);

            var selectionPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(targetObject)));
            if (selectionPath == null || selectionPath == "")
            {
                EditorGUILayout.HelpBox($"Select 1 Prefab in Project ", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Prefab Path", selectionPath);


            saveName = EditorGUILayout.TextField("Save Name", saveName);
            savePath = $"{selectionPath}/{saveName}";


            EditorGUI.indentLevel--;
        }

        GUILayout.Label("", EditorStyles.boldLabel);

        /* *** *** *** */

        /* *** Scroll End *** */
        EditorGUILayout.EndScrollView();


        /* *** Button **** */
        using (new GUILayout.HorizontalScope(GUI.skin.box))
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Convert"))
            {
                Convert();
            }
            GUI.backgroundColor = defaultColor;
        }
        /* *** *** *** */
    }

    void Convert()
    {
        // Prefab Init
        GameObject animObject = new GameObject();
        Vector4[] animationInfo = new Vector4[clips.Length];

        // Create Path : Main, Tex, Mat, Mesh
        Directory.CreateDirectory(savePath);

        string savePathTex = $"{savePath}/Textures";
        Directory.CreateDirectory(savePathTex);

        string savePathMat = $"{savePath}/Materials";
        Directory.CreateDirectory(savePathMat);

        string savePathMesh = $"{savePath}/Meshs";
        Directory.CreateDirectory(savePathMesh);

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {

            int pixelCountPerFrame = BoneMatrixRowCount * skinnedMeshRenderer.bones.Length; // [TODO] Different SMR has different bone size ?? 

            // Mesh Renderer 
            Mesh animMesh = SkinnedMesh2BoneWeightedMesh(skinnedMeshRenderer);

            // Animation Texture 
            Texture animTexture = GenerateAnimationTexture(targetObject, clips.ToList(), skinnedMeshRenderer, pixelCountPerFrame, ref animationInfo);
            Texture animRepeatTexture = GenerateAnimationRepeatTexture(targetObject, clips.ToList(), skinnedMeshRenderer, pixelCountPerFrame, ref animationInfo, ref animMesh);


            // Material 
            Material[] animMaterials = GenerateMaterials(targetObject, skinnedMeshRenderer, animTexture, animRepeatTexture, animShader, clips, pixelCountPerFrame);

            // Save Each Asset 
            AssetDatabase.CreateAsset(animMesh, string.Format($"{savePathMesh}/AnimMesh_{skinnedMeshRenderer.name}.asset"));
            AssetDatabase.CreateAsset(animTexture, string.Format($"{savePathTex}/AnimTex_{skinnedMeshRenderer.name}.asset"));
            AssetDatabase.CreateAsset(animRepeatTexture, string.Format($"{savePathTex}/AnimRepeatTex_{skinnedMeshRenderer.name}.asset"));

            foreach(var material in animMaterials)
            {  
                AssetDatabase.CreateAsset(material, string.Format($"{savePathMat}/AnimMat_{skinnedMeshRenderer.name}_{material.name}.asset"));
            }

            // Udon Variable 
            IUdonVariableTable publicVariables = udonBehaviour.publicVariables;
            SetUdonVariable(publicVariables, "FrameInfo", animationInfo, typeof(AnimationFrameInfoList));

            // Save to GameObject
            GenerateAnimObject(ref animObject, targetObject, animMesh, animMaterials);
        }

        // Add Udon Behaviour 
        AddUdonBehaviour(ref animObject, goUdon);

        // Save to Prefab
        PrefabUtility.SaveAsPrefabAsset(animObject, $"{savePath}/{targetObject.name}_Anim.prefab");
        UnityEngine.Object.DestroyImmediate(animObject);

        AssetDatabase.SaveAssets();
    }

    private static Mesh SkinnedMesh2BoneWeightedMesh(SkinnedMeshRenderer smr)
    {

        var mesh = UnityEngine.Object.Instantiate(smr.sharedMesh);

        BoneWeight[] boneInfo = smr.sharedMesh.boneWeights;
        List<Vector4> boneIndexes = boneInfo.Select(x => new Vector4(x.boneIndex0, x.boneIndex1, x.boneIndex2, x.boneIndex3)).ToList();
        List<Vector4> boneWeights = boneInfo.Select(x => new Vector4(x.weight0, x.weight1, x.weight2, x.weight3)).ToList();

        mesh.SetUVs(2, boneIndexes);
        mesh.SetUVs(3, boneWeights);

        return mesh;

    }

    private static Texture GenerateAnimationTexture(GameObject go, List<AnimationClip> clips, SkinnedMeshRenderer smr, int pixelCountPerFrame, ref Vector4[] animationInfo)
    {
        Vector2 texBoundary = CalculateTextureBoundary(clips, smr.bones.Count());

        Texture2D texture = new Texture2D((int)texBoundary.x, (int)texBoundary.y, TextureFormat.RGBAHalf, false, true);
        Color[] pixels = texture.GetPixels();
        int pixelIndex = 0;
        int currentClipFrame = 0;


        // Default Pose 
        foreach (Matrix4x4 boneMatrix in smr.bones.Select((b, idx) => b.localToWorldMatrix * smr.sharedMesh.bindposes[idx]))
        {
            pixels[pixelIndex++] = new Color(boneMatrix.m00, boneMatrix.m01, boneMatrix.m02, boneMatrix.m03);
            pixels[pixelIndex++] = new Color(boneMatrix.m10, boneMatrix.m11, boneMatrix.m12, boneMatrix.m13);
            pixels[pixelIndex++] = new Color(boneMatrix.m20, boneMatrix.m21, boneMatrix.m22, boneMatrix.m23);
        }


        //  Save Animation 
        for (int i = 0; i < clips.Count; i++)
        {
            AnimationClip clip = clips[i];

            int totalFrame = (int)(clip.length * TargetFrameRate);
            foreach (int frame in Enumerable.Range(0, totalFrame))
            {
                clip.SampleAnimation(go, (float)frame / TargetFrameRate); // sampling 

                foreach (Matrix4x4 boneMatrix in smr.bones.Select((b, idx) => b.localToWorldMatrix * smr.sharedMesh.bindposes[idx]))
                {

                    pixels[pixelIndex++] = new Color(boneMatrix.m00, boneMatrix.m01, boneMatrix.m02, boneMatrix.m03);
                    pixels[pixelIndex++] = new Color(boneMatrix.m10, boneMatrix.m11, boneMatrix.m12, boneMatrix.m13);
                    pixels[pixelIndex++] = new Color(boneMatrix.m20, boneMatrix.m21, boneMatrix.m22, boneMatrix.m23);


                }

            }

          
            int frameCount = totalFrame;
            int startFrame = currentClipFrame + 1;
            int endFrame = startFrame + frameCount - 1;
            animationInfo[i].x = startFrame;
            animationInfo[i].y = frameCount;
            currentClipFrame = endFrame;

        }


        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return texture;
    }

    private static Texture GenerateAnimationRepeatTexture(GameObject go, List<AnimationClip> clips, SkinnedMeshRenderer smr, int pixelCountPerFrame, ref Vector4[] animationInfo, ref Mesh mesh)
    {
        Vector2 texBoundary = CalculateRepeatTextureBoundary(clips, MaxRepeat);

        Texture2D texture = new Texture2D((int)texBoundary.x, (int)texBoundary.y, TextureFormat.RGBAHalf, false, true);
        Color[] pixels = texture.GetPixels();
        int pixelIndex = 0;
        int currentClipFrame = 0;

        Bounds bounds = mesh.bounds;
        Vector3 rootRelMin = bounds.min;
        Vector3 rootRelMax = bounds.max;

        // Get Initial Root 
        Vector3 rootPosInitial = ToVector3(smr.bones[0].localToWorldMatrix.GetColumn(3));

        // save root matrix 
        Matrix4x4 boneMatrixRoot = Matrix4x4.TRS(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, smr.bones[0].localToWorldMatrix.lossyScale);
        boneMatrixRoot = Matrix4x4.identity;
        pixels[pixelIndex++] = new Color(boneMatrixRoot.m00, boneMatrixRoot.m01, boneMatrixRoot.m02, boneMatrixRoot.m03);
        pixels[pixelIndex++] = new Color(boneMatrixRoot.m10, boneMatrixRoot.m11, boneMatrixRoot.m12, boneMatrixRoot.m13);
        pixels[pixelIndex++] = new Color(boneMatrixRoot.m20, boneMatrixRoot.m21, boneMatrixRoot.m22, boneMatrixRoot.m23);


        //  Save Animation 
        for (int i = 0; i < clips.Count; i++)
        {
            AnimationClip clip = clips[i];

            int totalFrame = (int)(clip.length * TargetFrameRate);

            // get final root TRS
            boneMatrixRoot = Matrix4x4.identity;
            clip.SampleAnimation(go, (float)totalFrame / TargetFrameRate); // sampling
            var boneMatrixRootFinal = smr.bones[0].localToWorldMatrix;
            var pos = ToVector3(boneMatrixRootFinal.GetColumn(3));
            pos.y = 0.0f;
            var rot = boneMatrixRootFinal.rotation;
            var eul_yaw = rot.eulerAngles.y;
            var rot_yaw = Quaternion.Euler(0.0f, eul_yaw, 0.0f);
            boneMatrixRootFinal = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));

            // save each root TRS 
            for (int loop = 0; loop < MaxRepeat; loop++)
            {

                boneMatrixRoot = boneMatrixRootFinal * boneMatrixRoot;

                pixels[pixelIndex++] = new Color(boneMatrixRoot.m00, boneMatrixRoot.m01, boneMatrixRoot.m02, boneMatrixRoot.m03);
                pixels[pixelIndex++] = new Color(boneMatrixRoot.m10, boneMatrixRoot.m11, boneMatrixRoot.m12, boneMatrixRoot.m13);
                pixels[pixelIndex++] = new Color(boneMatrixRoot.m20, boneMatrixRoot.m21, boneMatrixRoot.m22, boneMatrixRoot.m23);

                // Get Min Max Bounds
                Vector3 rootPosFinal = ToVector3(boneMatrixRoot.GetColumn(3)) - rootPosInitial;
                rootRelMax = Vector3.Max(rootRelMax, rootPosFinal);
                rootRelMin = Vector3.Min(rootRelMin, rootPosFinal);
            }

            int frameCount = MaxRepeat;
            int startFrame = currentClipFrame + 1;
            int endFrame = startFrame + frameCount - 1;
            animationInfo[i].z = startFrame;
            animationInfo[i].w = frameCount;
            currentClipFrame = endFrame;

        }


        // Set new Bounds 
        bounds.SetMinMax(rootRelMin, rootRelMax);
        mesh.bounds = bounds;

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return texture;
    }

    private static Vector2 CalculateTextureBoundary(IEnumerable<AnimationClip> clips, int boneLength)
    {
        int boneMatrixCount = BoneMatrixRowCount * boneLength;

        int totalPixels = clips.Aggregate(boneMatrixCount, (pixels, currentClip) => pixels + boneMatrixCount * (int)(currentClip.length * TargetFrameRate));

        int texWidth = 1;
        int texHeight = 1;

        while (texWidth * texHeight < totalPixels)
        {
            if (texWidth <= texHeight) texWidth *= 2;
            else texHeight *= 2;
        }

        return new Vector2(texWidth, texHeight);

    }

    private static Vector2 CalculateRepeatTextureBoundary(IEnumerable<AnimationClip> clips, int maxRepeat)
    {
        int totalPiexes = 1 + clips.Count() * BoneMatrixRowCount * MaxRepeat;
        int texWidth = 1;
        int texHeight = 1;

        while (texWidth * texHeight < totalPiexes)
        {
            if (texWidth <= texHeight) texWidth *= 2;
            else texHeight *= 2;
        }

        return new Vector2(texWidth, texHeight);

    }

    private static Material[] GenerateMaterials(GameObject go, SkinnedMeshRenderer smr, Texture tex, Texture texRepeat, Shader shader, IEnumerable<AnimationClip> clips, int pixelCountPerFrame)
    {
        Material[] materials = new Material[smr.sharedMaterials.Length];

        for (int i=0; i<smr.sharedMaterials.Length; i++)
        {
            materials[i] = UnityEngine.Object.Instantiate(smr.sharedMaterials[i]);
            materials[i].name = smr.sharedMaterials[i].name;
            materials[i].shader = shader;
            materials[i].SetTexture("_AnimTex", tex);
            materials[i].SetFloat("_PixelCountPerFrame", pixelCountPerFrame);
            materials[i].enableInstancing = true;

            materials[i].SetTexture("_AnimRepeatTex", texRepeat);
            materials[i].SetFloat("_RepeatMax", MaxRepeat);

        }

        return materials;
    }

    private static void GenerateAnimObject(ref GameObject parentObject, GameObject go, Mesh mesh, Material[] materials)
    {
        GameObject animObject = new GameObject();
        animObject.name = mesh.name;

        MeshFilter mf = animObject.AddComponent<MeshFilter>();


        mf.mesh = mesh;

        MeshRenderer mr = animObject.AddComponent<MeshRenderer>();
        mr.sharedMaterials = materials;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.lightProbeUsage = LightProbeUsage.Off;


        animObject.transform.SetParent(parentObject.transform);
    }

    private static void AddUdonBehaviour(ref GameObject animObject, GameObject goUdon)
    {

        Component[] components = goUdon.GetComponents<Component>();
        foreach (Component component in components)
        {

            if (component is UdonBehaviour)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(animObject);
            }

        }

    }

    public static Vector3 ToVector3(Vector4 parent)
    {
        return new Vector3(parent.x, parent.y, parent.z);
    }

    private static void SetUdonVariable(IUdonVariableTable publicVariables, string name, object value, Type declaredType)
    {
        if (!publicVariables.TrySetVariableValue(name, value))
        {
            Debug.Log("Failed Set Variable: " + name);
            if (!publicVariables.TryAddVariable(CreateUdonVariable(name, value, declaredType)))
            {
                Debug.LogError("Faleid to set public variable");
            }

            return;
        }
    }

    private static IUdonVariable CreateUdonVariable(string symbolName, object value, Type declaredType)
    {
        Type udonVariableType = typeof(UdonVariable<>).MakeGenericType(declaredType);
        return (IUdonVariable)Activator.CreateInstance(udonVariableType, symbolName, value);

    }
}

#endif