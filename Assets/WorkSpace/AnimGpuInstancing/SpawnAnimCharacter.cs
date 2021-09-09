
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnAnimCharacter : UdonSharpBehaviour
{
    [SerializeField] private GameObject animCharacter;
    [SerializeField] private AnimationFrameInfoList frameInfoList;
    [SerializeField] private int NumSpawn;

    [SerializeField] Vector3 targetPosition;

    GameObject[] characters = new GameObject[1000];

    [SerializeField] private Transform targetTransform;

    void Start()
    {

        //for (int i=0; i<NumSpawn; ++i)
        //{
        //    SendCustomEventDelayedSeconds(nameof(Spawn), 0.1f);         
        //}  
        
        for (int i=0; i<NumSpawn; ++i)
        {
            characters[i] = GenerateCharacter();
        }
        
    }

    private void Update()
    {
        var player = Networking.LocalPlayer;

        if (player == null) return;
        var OriginHead = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);



        for (int i=0; i<NumSpawn; ++i)
        {
            var lookPos = OriginHead.position - characters[i].transform.position;
            lookPos.y = 0.0f;
            var rotation = Quaternion.LookRotation(lookPos);
            // Vector3 currentPos = characters[i].transform.position;
            characters[i].transform.rotation = Quaternion.Slerp(characters[i].transform.rotation, rotation, Time.deltaTime * 0.9f);
        }
    }

    public void Spawn()
    {
        GameObject go = VRCInstantiate(animCharacter);

        float posX = Random.Range(-12.0f, 12.0f);
        float posZ = Random.Range(-12.0f, 10.0f);
        go.transform.position = new Vector3(posX, 0.0f, posZ);
        go.transform.LookAt(targetPosition);

        go.transform.SetParent(this.transform);

        var idx = Random.Range(0, frameInfoList.FrameInfo.Length);
        Vector4 frameInfo = frameInfoList.FrameInfo[idx];

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        // props.SetColor("_Color", new Color(Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f)));
        props.SetFloat("_OffsetSeconds", Random.Range(0.0f, frameInfo[2]-1));
        props.SetFloat("_StartFrame", frameInfo[0]);
        props.SetFloat("_EndFrame", frameInfo[1]);
        props.SetFloat("_FrameCount", frameInfo[2]);

        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);
    }

    public GameObject GenerateCharacter()
    {
        GameObject go = VRCInstantiate(animCharacter);
        go.SetActive(true);

        float scale = Random.Range(0.8f, 1.1f);
        go.transform.localScale = new Vector3(scale, scale, scale);

        float posX = Random.Range(-6.0f, 6.0f);
        float posZ = Random.Range(-6.0f, 3.0f);

        go.transform.position = new Vector3(posX, 0.0f, posZ);
        go.transform.LookAt(targetPosition);

        go.transform.SetParent(this.transform);

        var idx = Random.Range(0, frameInfoList.FrameInfo.Length);
        Vector4 frameInfo = frameInfoList.FrameInfo[idx];

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        // props.SetColor("_Color", new Color(Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f)));
        props.SetFloat("_OffsetSeconds", Random.Range(0.0f, frameInfo[2] - 1));
        props.SetFloat("_StartFrame", frameInfo[0]);
        props.SetFloat("_EndFrame", frameInfo[1]);
        props.SetFloat("_FrameCount", frameInfo[2]);

        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);

        return go;
    }
}
