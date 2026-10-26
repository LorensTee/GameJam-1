using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(VideoPlayer))]
public class VideoBackgroundUI : MonoBehaviour
{
    private RawImage rawImage;
    private VideoPlayer videoPlayer;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        RenderTexture renderTexture = new RenderTexture((int)source.width, (int)source.height, 24);
        source.targetTexture = renderTexture;
        rawImage.texture = renderTexture;
        source.Play();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }
}