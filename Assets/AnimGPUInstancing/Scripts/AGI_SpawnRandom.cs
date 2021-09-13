
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AGI_SpawnRandom : UdonSharpBehaviour
{
    [Header("-----------------------------")]
    [Header("General Parameters")]
    [Header("-----------------------------")]
    [SerializeField] private GameObject animCharacter;
    [SerializeField] private AnimationFrameInfoList animationFrameInfoList;

    [Space(10)]
    [Header("-----------------------------")]
    [Header("Random Parameters")]
    [Header("-----------------------------")]

    [Tooltip("Please Keep Under 1000")]
    [SerializeField] private int NumSpawn = 10;

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


    private GameObject[] characters = new GameObject[1000];


    void Start()
    {

        for (int i = 0; i < NumSpawn; i++)
        {
            characters[i] = GenerateCharacter();

            if (i > characters.Length) return;
        }


    }

    private void Update()
    {
    }


    public GameObject GenerateCharacter()
    {
        GameObject go = VRCInstantiate(animCharacter);
        go.SetActive(true);

        float scale = Random.Range(RandomScale.x, RandomScale.y);
        go.transform.localScale = new Vector3(scale, scale, scale);

        float angle_yaw = Random.Range(-RandomRotation, RandomRotation);
        Quaternion quat_yaw = Quaternion.Euler(new Vector3(0.0f, angle_yaw, 0.0f));

        Vector3 targetPos = new Vector3(
            Random.Range(- RandomX, RandomX),
            Random.Range(- RandomY, RandomY),
            Random.Range(- RandomZ, RandomZ)
        );

        targetPos += this.transform.position;

        go.transform.SetPositionAndRotation(targetPos, quat_yaw);

        go.transform.SetParent(this.transform);


        var idx = Random.Range(0, animationFrameInfoList.FrameInfo.Length);
        Vector4 frameInfo = animationFrameInfoList.FrameInfo[idx];

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        // props.SetColor("_Color", new Color(Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f)));
        props.SetFloat("_OffsetSeconds", Random.Range(0.0f, frameInfo[2] - 1));
        props.SetFloat("_StartFrame", frameInfo[0]);
        props.SetFloat("_FrameCount", frameInfo[1]);

        // Loop
        props.SetFloat("_LoopStartFrame", frameInfo[2]);
        props.SetFloat("_LoopNum", 10.0f);


        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);

        return go;
    }
}

