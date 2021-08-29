
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BearController : UdonSharpBehaviour
{
    [SerializeField] AnimationFrameInfo[] FrameInformations;
    [SerializeField] int NumSpawn;
    public GameObject bearPrefab;

    void Start()
    {
        for (int i=0; i<NumSpawn; ++i)
        {

            SendCustomEventDelayedSeconds(nameof(SpawnBear), 0.5f);
        }
        
    }

    public void SpawnBear()
    {
        GameObject bear = VRCInstantiate(bearPrefab);
        float posX = Random.Range(-15.0f, 15.0f);
        float posZ = Random.Range(-15.0f, 15.0f);
        bear.transform.position = new Vector3(posX, 0.0f, posZ);


        var idx = Random.Range(0, 1);
        var frameInformation = FrameInformations[idx];

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        // props.SetColor("_Color", new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
        props.SetFloat("_OffsetSeconds", Random.Range(0.0f, 10.0f));
        props.SetFloat("_StartFrame", frameInformation.StartFrame);
        props.SetFloat("_EndFrame", frameInformation.EndFrame);
        props.SetFloat("_FrameCount", frameInformation.FrameCount);

        MeshRenderer meshRenderer = bear.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);
    }
}
