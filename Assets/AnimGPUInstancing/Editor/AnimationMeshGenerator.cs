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
    private const int MaxLoop = 20;


    // Components 
    private GameObject targetObject;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Animator animator;
    private AnimationClip[] clips;
    private Shader animShader;
    private GameObject goUdon;
    private UdonBehaviour udonBehaviour;
    private string saveName = "AnimatedMesh";
    private string savePath;

    private int shaderIdx;

    [MenuItem("Window/Extension Tools/Animation Mesh Generator")]
    static void Open()
    {
        GetWindow<AnimationMeshGenerator>();
    }

    private void OnGUI()
    {
        GUILayout.Label("", EditorStyles.boldLabel);

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


            // Get Components
            if (targetObject == null) return;


            SkinnedMeshRenderer[] skinnedMeshRenderers = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (!skinnedMeshRenderers.Any() || skinnedMeshRenderers.Count() != 1)
            {
                EditorGUILayout.HelpBox("Only Support 1 Skinned Mesh Renderer", MessageType.Error);
                return;
            }
            skinnedMeshRenderer = skinnedMeshRenderers.First();

            Animator[] animators = targetObject.GetComponentsInChildren<Animator>();
            if (!animators.Any() || animators.Count() != 1)
            {
                EditorGUILayout.HelpBox("Only Support 1 Animator", MessageType.Error);
                return;
            }
            animator = animators.First();

            clips = animator.runtimeAnimatorController.animationClips;
            if (!clips.Any())
            {
                EditorGUILayout.HelpBox("No Animation", MessageType.Warning);
            }


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

        GUILayout.Label("", EditorStyles.boldLabel);

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

        defaultColor = GUI.backgroundColor;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.gray;
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Animations", EditorStyles.whiteLabel);

            }

            EditorGUI.indentLevel++;

            // Show Components 
            for (int i = 0; i < clips.Length; i++)
            {
                EditorGUILayout.LabelField($"{clips[i].name}");
            }
            GUI.backgroundColor = defaultColor;



            EditorGUI.indentLevel--;
        }

        GUILayout.Label("", EditorStyles.boldLabel);

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

        using (new GUILayout.HorizontalScope(GUI.skin.box))
        {
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Convert"))
            {
                Convert();
            }
            GUI.backgroundColor = defaultColor;

        }
    }

    void Convert()
    {
        Vector4[] animationInfo = new Vector4[clips.Length];
        int pixelCountPerFrame = BoneMatrixRowCount * skinnedMeshRenderer.bones.Length;

        Debug.Log(savePath);
        Directory.CreateDirectory(savePath);

        // Mesh Renderer 
        Mesh animMesh = SkinnedMesh2BoneWeightedMesh(skinnedMeshRenderer);

        // Animation Texture 
        Texture animTexture = GenerateAnimationTexture(targetObject, clips.ToList(), skinnedMeshRenderer, pixelCountPerFrame, ref animationInfo);
        Texture animLoopTexture = GenerateAnimationLoopTexture(targetObject, clips.ToList(), skinnedMeshRenderer, pixelCountPerFrame, ref animationInfo, ref animMesh);


        // Udon Variable 
        IUdonVariableTable publicVariables = udonBehaviour.publicVariables;
        SetUdonVariable(publicVariables, "FrameInfo", animationInfo, typeof(AnimationFrameInfoList));

        // Material 
        Material animMaterial = GenerateMaterial(targetObject, skinnedMeshRenderer, animTexture, animLoopTexture, animShader, clips, pixelCountPerFrame);

        // Save Each Asset 
        AssetDatabase.CreateAsset(animMesh, string.Format($"{savePath}/{targetObject.name}_AnimMesh.asset"));
        AssetDatabase.CreateAsset(animTexture, string.Format($"{savePath}/{targetObject.name}_AnimTex.asset"));
        AssetDatabase.CreateAsset(animLoopTexture, string.Format($"{savePath}/{targetObject.name}_AnimLoopTex.asset"));
        AssetDatabase.CreateAsset(animMaterial, string.Format($"{savePath}/{targetObject.name}_AnimMat.asset"));

        // Prefab 
        GameObject animObject = GenerateAnimObject(targetObject, goUdon, animMesh, animMaterial);
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

    private static Texture GenerateAnimationLoopTexture(GameObject go, List<AnimationClip> clips, SkinnedMeshRenderer smr, int pixelCountPerFrame, ref Vector4[] animationInfo, ref Mesh mesh)
    {
        Vector2 texBoundary = CalculateLoopTextureBoundary(clips, MaxLoop);

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
            for (int loop = 0; loop < MaxLoop; loop++)
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

            int frameCount = MaxLoop;
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

    private static Vector2 CalculateLoopTextureBoundary(IEnumerable<AnimationClip> clips, int maxLoop)
    {
        int totalPiexes = 1 + clips.Count() * BoneMatrixRowCount * MaxLoop;
        int texWidth = 1;
        int texHeight = 1;

        while (texWidth * texHeight < totalPiexes)
        {
            if (texWidth <= texHeight) texWidth *= 2;
            else texHeight *= 2;
        }

        return new Vector2(texWidth, texHeight);

    }

    private static Material GenerateMaterial(GameObject go, SkinnedMeshRenderer smr, Texture tex, Texture texLoop, Shader shader, IEnumerable<AnimationClip> clips, int pixelCountPerFrame)
    {
        Material material = UnityEngine.Object.Instantiate(smr.sharedMaterial);
        material.shader = shader;
        material.SetTexture("_AnimTex", tex);
        material.SetFloat("_PixelCountPerFrame", pixelCountPerFrame);
        material.enableInstancing = true;

        if (shader.name.Contains("Loop"))
        {
            material.SetTexture("_AnimLoopTex", texLoop);
            material.SetFloat("_LoopMax", MaxLoop);
        }

        return material;
    }


    private static GameObject GenerateAnimObject(GameObject go, GameObject goUdon, Mesh mesh, Material material)
    {
        GameObject animObject = new GameObject();
        animObject.name = go.name;

        MeshFilter mf = animObject.AddComponent<MeshFilter>();


        mf.mesh = mesh;

        MeshRenderer mr = animObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.lightProbeUsage = LightProbeUsage.Off;


        Component[] components = goUdon.GetComponents<Component>();
        foreach (Component component in components)
        {

            if (component is UdonBehaviour)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(animObject);
            }

        }

        return animObject;
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