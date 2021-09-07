﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SpawnMarching : UdonSharpBehaviour
{
    [SerializeField] private GameObject animCharacter;
    [SerializeField] private AnimationFrameInfoList frameInfoList;
    [SerializeField] private int NumSpawn;


    GameObject[] characters = new GameObject[1000];
    private float elapsedTime = 0.0f;


    void Start()
    {

        for (int i=0; i<NumSpawn; ++i)
        {
            characters[i] = GenerateCharacter(i);
        }
        
    }

    private void Update()
    {
        var player = Networking.LocalPlayer;

        if (player == null) return;
        var OriginHead = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);


    }


    public GameObject GenerateCharacter(int n)
    {
        GameObject go = VRCInstantiate(animCharacter);
        go.SetActive(true);

        float posX = (float) n * 0.8f;

        go.transform.position = new Vector3(posX, 0.0f, 0.0f);

        go.transform.SetParent(this.transform);

        var idx = Random.Range(0, frameInfoList.FrameInfo.Length);
        Vector4 frameInfo = frameInfoList.FrameInfo[idx];

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        // props.SetColor("_Color", new Color(Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f), Random.Range(0.9f, 1.0f)));
        props.SetFloat("_OffsetSeconds", Random.Range(0.0f, frameInfo[2] - frameInfo[3] - 2));

        props.SetFloat("_StartFrame", frameInfo[0]);
        props.SetFloat("_EndFrame", frameInfo[1] - frameInfo[3] - 1);
        props.SetFloat("_FrameCount", frameInfo[2] - frameInfo[3] - 1);

        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.SetPropertyBlock(props);

        return go;
    }
}