
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnAnimCharacter : UdonSharpBehaviour
{
    [SerializeField] private GameObject animCharacter;
    [SerializeField] private AnimationFrameInfoList animationFrameInfoList;
    [SerializeField] private int NumSpawn;

    [SerializeField] private bool RandomSpawn = true;

    [SerializeField] private GameObject[] spawnPoints;


    GameObject[] characters = new GameObject[1000];

    [SerializeField] private Transform targetTransform;


    void Start()
    {
        if (spawnPoints.Length > animationFrameInfoList.FrameInfo.Length)
        {
            Debug.LogError("Spawn Points Size is Larger than Animations");
            return;
        }

        if (RandomSpawn)
        {

        }
        
        int cnt = 0;
        for (int i = 0; i<spawnPoints.Length; i++)
        {
            Transform[] points = spawnPoints[i].GetComponentsInChildren<Transform>();

            foreach (var point in points)
            {
                characters[cnt++] = GenerateCharacter(point, i);

                if (cnt > 1000) return;
            }
        }
        
    }

    private void Update()
    {
    }


    public GameObject GenerateCharacter(Transform point, int animID)
    {
        GameObject go = VRCInstantiate(animCharacter);
        go.SetActive(true);

        float scale = Random.Range(0.8f, 1.1f);
        go.transform.localScale = new Vector3(scale, scale, scale);

        float posX = Random.Range(-6.0f, 6.0f);
        float posZ = Random.Range(-6.0f, 3.0f);

        go.transform.SetPositionAndRotation(point.position, point.rotation);
        go.transform.SetParent(this.transform);

        //var idx = Random.Range(0, animationFrameInfoList.FrameInfo.Length);
        Vector4 frameInfo = animationFrameInfoList.FrameInfo[animID];

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

