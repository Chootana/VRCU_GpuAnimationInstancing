
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnimationFrameInfo : UdonSharpBehaviour
{
    public string Name;
    public int StartFrame;
    public int EndFrame;
    public int FrameCount;

    private void Start()
    {
        
    }
    public void Initialize(string name, int startFrame, int endFrame, int frameCount)
    {
        Name = name;
        StartFrame = startFrame;
        EndFrame = endFrame;
        FrameCount = frameCount;
    }
}
