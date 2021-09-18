using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

public class SMR_SpawnRandom : UdonSharpBehaviour
{
    [Header("-----------------------------")]
    [Header("General Parameters")]
    [Header("-----------------------------")]
    [SerializeField] private GameObject animCharacter;

    [Space(10)]
    [Header("-----------------------------")]
    [Header("Random Spawn")]
    [Header("-----------------------------")]

    [Tooltip("Please Keep Under 1000")]
    [SerializeField] private int SpawnNum = 10;

    [Tooltip("(-)  ~ (+) Value")]
    [SerializeField] private float RandomX = 5.0f;

    [Tooltip("(-)  ~ (+) Value")]
    [SerializeField] private float RandomY = 0.0f;

    [Tooltip("(-)  ~ (+) Value")]
    [SerializeField] private float RandomZ = 5.0f;

    [Tooltip("(-) ~ (+) Yaw Angle [deg]")]
    [SerializeField] private float RandomRotation = 0.0f;

    [Space(10)]

    [Tooltip("(x) Min ~ (y) Max")]
    [SerializeField] private Vector2 RandomScale = new Vector2(0.8f, 1.2f);

    [Space(10)]
    [SerializeField] private string[] animationNames;

    private GameObject[] characters = new GameObject[1000];


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < SpawnNum; i++)
        {

            characters[i] = GenerateCharacter();

            if (i > characters.Length) return;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GenerateCharacter()
    {
        GameObject go = VRCInstantiate(animCharacter);
        go.SetActive(true);

        float scale = UnityEngine.Random.Range(RandomScale.x, RandomScale.y);
        go.transform.localScale = new Vector3(scale, scale, scale);

        float angle_yaw = UnityEngine.Random.Range(-RandomRotation, RandomRotation);
        Quaternion quat_yaw = Quaternion.Euler(new Vector3(0.0f, angle_yaw, 0.0f));

        Vector3 targetPos = new Vector3(
            UnityEngine.Random.Range(-RandomX, RandomX),
            UnityEngine.Random.Range(-RandomY, RandomY),
            UnityEngine.Random.Range(-RandomZ, RandomZ)
        );

        targetPos += this.transform.position;

        go.transform.SetPositionAndRotation(targetPos, quat_yaw);

        go.transform.SetParent(this.transform);

        var idx = UnityEngine.Random.Range(0, animationNames.Length);
        Animator animator = go.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animationNames[idx]);
        }

        //var idx = UnityEngine.Random.Range(0, animationFrameInfoList.FrameInfo.Length);
        //Vector4 frameInfo = animationFrameInfoList.FrameInfo[idx];

        //MaterialPropertyBlock props = new MaterialPropertyBlock();
        //// props.SetColor("_Color", new Color(Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f)));
        //props.SetFloat("_OffsetSeconds", UnityEngine.Random.Range(0.0f, frameInfo[1] - 1));
        //props.SetFloat("_StartFrame", frameInfo[0]);
        //props.SetFloat("_FrameCount", frameInfo[1]);

        //// Repeat
        //props.SetFloat("_ROOT_MOTION", (ApplyRootMotion) ? 1.0f : 0.0f);
        //props.SetFloat("_RepeatStartFrame", frameInfo[2]);
        //props.SetFloat("_RepeatNum", Mathf.Min(frameInfo[3], RepeatNum));

        //MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
        //foreach (var meshRenderer in meshRenderers)
        //{
        //    meshRenderer.SetPropertyBlock(props);
        //}

        //MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();

        return go;
    }


}
