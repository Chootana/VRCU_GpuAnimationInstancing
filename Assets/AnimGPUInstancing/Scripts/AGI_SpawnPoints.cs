using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AGI_SpawnPoints : UdonSharpBehaviour
{
    [Header("-----------------------------")]
    [Header("General Parameters")]
    [Header("-----------------------------")]
    [SerializeField] private GameObject animCharacter;
    [SerializeField] private AnimationFrameInfoList animationFrameInfoList;

    [Space(10)]

    [Header("-----------------------------")]
    [Header("Spawn Points")]
    [Header("-----------------------------")]
    [Header("Element 0 -> Animation 0")]
    [Header("Element 1 -> Animation 1 ...")]
    [Header("-----------------------------")]
    [Tooltip("Please Keep Under Animation Size")]
    [SerializeField] private GameObject[] spawnPoints;

    [Space(10)]

    [Header("-----------------------------")]
    [Header("Apply Root Motion for Locomotion")]
    [Header("-----------------------------")]
    [Tooltip("True for Locomotion")]
    [SerializeField] private bool ApplyRootMotion = false;

    [Tooltip("How Many Repeats to Move")]
    [SerializeField] private int RepeatNum = 10;



    GameObject[] characters = new GameObject[1000];

    // Start is called before the first frame update
    void Start()
    {
        if (spawnPoints.Length > animationFrameInfoList.FrameInfo.Length)
        {
            Debug.LogError("Spawn Points Size is Larger than Animations");
            return;
        }

        int cnt = 0;
        for (int i = 0; i < spawnPoints.Length; i++)
        {

            foreach (Transform point in spawnPoints[i].transform)
            {
                characters[cnt++] = GenerateCharacter(point, i);

                if (cnt > 1000) return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GenerateCharacter(Transform point, int animID)
    {
        GameObject go = VRCInstantiate(animCharacter);
        go.SetActive(true);


        go.transform.localScale = point.localScale;
        go.transform.SetPositionAndRotation(point.position, point.rotation);

        go.transform.SetParent(this.transform);

        Vector4 frameInfo = animationFrameInfoList.FrameInfo[animID];

        // var randomColor = new Color(Random.Range(0.00f, 1.00f), Random.Range(0.00f, 1.00f), Random.Range(0.00f, 1.00f), 1);

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        // props.SetColor("_Color", randomColor);
        props.SetFloat("_OffsetSeconds", UnityEngine.Random.Range(0.0f, frameInfo[1] - 1));
        props.SetFloat("_StartFrame", frameInfo[0]);
        props.SetFloat("_FrameCount", frameInfo[1]);

        // Repeat
        props.SetFloat("_ROOT_MOTION", (ApplyRootMotion) ? 1.0f : 0.0f);
        props.SetFloat("_RepeatStartFrame", frameInfo[2]);
        props.SetFloat("_RepeatNum", Mathf.Min(frameInfo[3], RepeatNum));

        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);

        return go;
    }
}
