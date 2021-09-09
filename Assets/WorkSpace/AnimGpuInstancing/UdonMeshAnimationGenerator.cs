#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDKBase;
using UdonSharp;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;


public class UdonMeshAnimationGenerator
{
    private const int boneMatrixRowCount = 3;
    private const int targetFrameRate = 30;
    private const int MaxLoop = 0;

    [MenuItem("UdonMeshANimationGenerator/Generate")]
    private static void Generate()
    {
        var targetObject = Selection.activeGameObject;
        if (targetObject == null)
        {
            EditorUtility.DisplayDialog("Warning", "Wrong Object Type", "OK");
            return;
        }

        SkinnedMeshRenderer[] skinnedMeshRenderers = targetObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (!skinnedMeshRenderers.Any() || skinnedMeshRenderers.Count() != 1)
        {
            EditorUtility.DisplayDialog("Warning", "Only Support 1 Skinned Mesh Renderer", "OK");
            return;
        }

        Animator animator = targetObject.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            EditorUtility.DisplayDialog("Warning", "Selected Game Object Does Not Have Animator", "OK");
            return;
        }


        Shader animShader = Shader.Find("AnimationGpuInstancing/Unlit");
        if (animShader == null)
        {
            EditorUtility.DisplayDialog("Warning", "Wrong Shader Type", "OK");
            return;
        }

        //UdonBehaviour[] udonBehaviours = targetObject.GetComponentsInChildren<UdonBehaviour>();
        //if (!udonBehaviours.Any() || udonBehaviours.Count() != 1)
        //{
        //    EditorUtility.DisplayDialog("Warning", "Only Support 1 Udon Behaviour", "OK");
        //    return;
        //}

        GameObject goUdon = targetObject.transform.Find("AnimationFrameInfoList").gameObject;
        if (goUdon == null)
        {
            EditorUtility.DisplayDialog("Warning", "Wrong GameObject(Udon) ", "OK");
            return;
        }

        UdonBehaviour udonBehaviour = goUdon.GetComponent<UdonBehaviour>();
        if (udonBehaviour == null)
        {
            EditorUtility.DisplayDialog("Warning", "No UdonBehaviour ", "OK");
            return;
        }
        IUdonVariableTable publicVariables = udonBehaviour.publicVariables;


        var selectionPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetObject));
        SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshRenderers.First();
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        Vector4[] animationInfo = new Vector4[clips.Length];
        int pixelCountPerFrame = boneMatrixRowCount * skinnedMeshRenderer.bones.Length;

        Directory.CreateDirectory(Path.Combine(selectionPath, "AnimatedMesh"));

        // Mesh Renderer 
        Mesh animMesh = SkinnedMesh2BoneWeightedMesh(skinnedMeshRenderer);


        // Animation Texture
        Texture animTexture = GenerateAnimationTexture(targetObject, clips.ToList(), skinnedMeshRenderer, pixelCountPerFrame, ref animationInfo, ref animMesh);


        SetUdonVariable(publicVariables, "FrameInfo", animationInfo, typeof(AnimationFrameInfoList));


        // Material 
        Material animMaterial = GenerateMaterial(targetObject, skinnedMeshRenderer, animTexture, animShader, clips, pixelCountPerFrame);

        // Save Each Asset Before Generating Prefab 
        AssetDatabase.CreateAsset(animMesh, string.Format($"{selectionPath}/AnimatedMesh/{targetObject.name}_AnimMesh.asset"));
        AssetDatabase.CreateAsset(animTexture, string.Format($"{selectionPath}/AnimatedMesh/{targetObject.name}_AnimTex.asset"));
        AssetDatabase.CreateAsset(animMaterial, string.Format($"{selectionPath}/AnimatedMesh/{targetObject.name}_AnimMat.asset"));

        // Prefab
        GameObject animObject = GenerateAnimObject(targetObject, goUdon, animMesh, animMaterial);
        PrefabUtility.SaveAsPrefabAsset(animObject, $"{selectionPath}/AnimatedMesh/{targetObject.name}_Anim.prefab");


        UnityEngine.Object.DestroyImmediate(animObject);
        AssetDatabase.SaveAssets();

    }

    private static Texture GenerateAnimationTexture(GameObject go, List<AnimationClip> clips, SkinnedMeshRenderer smr, int pixelCountPerFrame, ref Vector4[] animationInfo, ref Mesh mesh)
    {
        Vector2 texBoundary = CalculateTextureBoundary(clips, smr.bones.Count());

        Texture2D texture = new Texture2D((int)texBoundary.x, (int)texBoundary.y, TextureFormat.RGBAHalf, false, true);
        Color[] pixels = texture.GetPixels();
        int pixelIndex = 0;
        int currentClipFrame = 0;

        // smr.SetBlendShapeWeight(1, 100); // [TODO] We can modify this for generate various shape Character

        Bounds bounds = mesh.bounds;
        Vector3 rootRelMin = bounds.min;
        Vector3 rootRelMax = bounds.max;

        // Get Initial Root 
        Vector3 rootPosInitial = ToVector3(smr.bones[0].localToWorldMatrix.GetColumn(3));

        // Default Pose 
        foreach (Matrix4x4 boneMatrix in smr.bones.Select((b, idx) => b.localToWorldMatrix * smr.sharedMesh.bindposes[idx]))
        {
            pixels[pixelIndex++] = new Color(boneMatrix.m00, boneMatrix.m01, boneMatrix.m02, boneMatrix.m03);
            pixels[pixelIndex++] = new Color(boneMatrix.m10, boneMatrix.m11, boneMatrix.m12, boneMatrix.m13);
            pixels[pixelIndex++] = new Color(boneMatrix.m20, boneMatrix.m21, boneMatrix.m22, boneMatrix.m23);
        }


        //  Save Animation 
        for (int i=0; i < clips.Count; i++)
        {
            AnimationClip clip = clips[i];

            int totalFrame = (int)(clip.length * targetFrameRate);
            foreach (int frame in Enumerable.Range(0, totalFrame))
            {
                clip.SampleAnimation(go, (float)frame / targetFrameRate); // sampling 

                foreach (Matrix4x4 boneMatrix in smr.bones.Select((b, idx) => b.localToWorldMatrix * smr.sharedMesh.bindposes[idx]))
                {

                    pixels[pixelIndex++] = new Color(boneMatrix.m00, boneMatrix.m01, boneMatrix.m02, boneMatrix.m03);
                    pixels[pixelIndex++] = new Color(boneMatrix.m10, boneMatrix.m11, boneMatrix.m12, boneMatrix.m13);
                    pixels[pixelIndex++] = new Color(boneMatrix.m20, boneMatrix.m21, boneMatrix.m22, boneMatrix.m23);


                }

            }

            // save root matrix 
            Matrix4x4 boneMatrixRoot = Matrix4x4.identity;
            pixels[pixelIndex++] = new Color(boneMatrixRoot.m00, boneMatrixRoot.m01, boneMatrixRoot.m02, boneMatrixRoot.m03);
            pixels[pixelIndex++] = new Color(boneMatrixRoot.m10, boneMatrixRoot.m11, boneMatrixRoot.m12, boneMatrixRoot.m13);
            pixels[pixelIndex++] = new Color(boneMatrixRoot.m20, boneMatrixRoot.m21, boneMatrixRoot.m22, boneMatrixRoot.m23);

            // get final root TRS 
            //clip.SampleAnimation(go, (float)totalFrame / targetFrameRate); // sampling
            //var boneMatrixRootFinal = smr.bones[0].localToWorldMatrix;
            //var pos = ToVector3(boneMatrixRootFinal.GetColumn(3));
            //pos.y = 0.0f;
            //var rot = boneMatrixRootFinal.rotation;
            //var eul_yaw = rot.eulerAngles.y;
            //var rot_yaw = Quaternion.Euler(0.0f, eul_yaw, 0.0f);
            //boneMatrixRootFinal = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));

            //// save each root TRS 
            //for (int loop = 0; loop < MaxLoop; loop++)
            //{

            //    boneMatrixRoot = boneMatrixRootFinal * boneMatrixRoot;

            //    pixels[pixelIndex++] = new Color(boneMatrixRoot.m00, boneMatrixRoot.m01, boneMatrixRoot.m02, boneMatrixRoot.m03);
            //    pixels[pixelIndex++] = new Color(boneMatrixRoot.m10, boneMatrixRoot.m11, boneMatrixRoot.m12, boneMatrixRoot.m13);
            //    pixels[pixelIndex++] = new Color(boneMatrixRoot.m20, boneMatrixRoot.m21, boneMatrixRoot.m22, boneMatrixRoot.m23);


            //    // Get Min Max Bounds
            //    Vector3 rootPosFinal = ToVector3(boneMatrixRoot.GetColumn(3)) - rootPosInitial;
            //    rootRelMax = Vector3.Max(rootRelMax, rootPosFinal);
            //    rootRelMin = Vector3.Min(rootRelMin, rootPosFinal);


            //}


            int frameCount = totalFrame;
            // frameCount += 1 + MaxLoop;
            int startFrame = currentClipFrame + 1;

            int endFrame = startFrame + frameCount - 1;
            animationInfo[i].Set(startFrame, endFrame, frameCount, MaxLoop);
            currentClipFrame = endFrame + 1;

        }


        // Set new Bounds 
        bounds.SetMinMax(rootRelMin, rootRelMax);
        mesh.bounds = bounds;

        Debug.Log($"1 bounds : {bounds} mesh.bounds: {mesh.bounds}");

        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        return texture;
    }

    public static Vector3 ToVector3(Vector4 parent)
    {
        return new Vector3(parent.x, parent.y, parent.z);
    }

    private static Vector2 CalculateTextureBoundary(IEnumerable<AnimationClip> clips, int boneLength)
    {
        int boneMatrixCount = boneMatrixRowCount * boneLength;

        int totalPixels = clips.Aggregate(boneMatrixCount, (pixels, currentClip) => pixels + boneMatrixCount * (int)(currentClip.length * targetFrameRate));

        // [TODO] 
        totalPixels += 1 + MaxLoop;

        int texWidth = 1;
        int texHeight = 1;

        while (texWidth * texHeight < totalPixels)
        {
            if (texWidth <= texHeight) texWidth *= 2;
            else texHeight *= 2;
        }

        return new Vector2(texWidth, texHeight);

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

    private static Material GenerateMaterial(GameObject go, SkinnedMeshRenderer smr, Texture tex, Shader shader, IEnumerable<AnimationClip> clips, int pixelCountPerFrame)
    {
        Material material = UnityEngine.Object.Instantiate(smr.sharedMaterial);
        material.shader = shader;
        material.SetTexture("_AnimTex", tex);
        material.SetInt("_PixelCountPerFrame", pixelCountPerFrame);
        material.enableInstancing = true;

        return material;
    }

    private static GameObject GenerateAnimObject(GameObject go, GameObject goUdon, Mesh mesh, Material material)
    {
        GameObject animObject = new GameObject();
        animObject.name = go.name;

        MeshFilter mf = animObject.AddComponent<MeshFilter>();


        mf.mesh = mesh;
        Debug.Log($"Final bounds max {mesh.bounds.max} min {mesh.bounds.min} bounds {mesh.bounds}");

        MeshRenderer mr = animObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.lightProbeUsage = LightProbeUsage.Off;


        Component[] components = goUdon.GetComponents<Component>();
        foreach (Component component in components)
        {
            Debug.Log(component);

            if (component is UdonBehaviour)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(component);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(animObject);
            }

        }

        return animObject;
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