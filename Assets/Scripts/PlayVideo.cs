using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayVideo : MonoBehaviour
{
    void Start()
    {
        UnityEngine.Video.VideoPlayer player = 
            GetComponent<UnityEngine.Video.VideoPlayer>();

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "snake_gameplay.mp4");
        player.url = filePath;

        player.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
        player.isLooping = true;
        player.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
