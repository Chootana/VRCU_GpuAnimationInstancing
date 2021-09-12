
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
    [SerializeField] private int NumSpawn;

    [Tooltip("(x) Min ~ (y) Max")]
    [SerializeField] private Vector2 RandomX = new Vector2(-5.0f, 5.0f);

    [Tooltip("(x) Min ~ (y) Max")]
    [SerializeField] private Vector2 RandomY = new Vector2(0.0f, 0.0f);

    [Tooltip("(x) Min ~ (y) Max")]
    [SerializeField] private Vector2 RandomZ = new Vector2(-5.0f, 5.0f);

    [Tooltip("(x) Min ~ (y) Max")]
    [SerializeField] private Vector2 RandomScale = new Vector2(0.8f, 1.2f);

    [Space(10)]
    [Tooltip("(-) ~ (+) Yaw Angle [deg]")]
    [SerializeField] private float RandomRotation = 0.0f;

    GameObject[] characters = new GameObject[1000];



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
            Random.Range(RandomX.x, RandomX.y),
            Random.Range(RandomY.x, RandomY.y),
            Random.Range(RandomZ.x, RandomZ.y)
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

        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);

        return go;
    }
}

