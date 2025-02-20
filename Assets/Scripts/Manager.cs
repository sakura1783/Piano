using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Manager : MonoBehaviour
{
    [SerializeField] private Transform songsTran;
    public Transform SongsTran => songsTran;

    [SerializeField] private SongTag tagPrefab;
    [SerializeField] private SongTitle titlePrefab;

    [SerializeField] private VideoPlayer videoPlayer;
    public VideoPlayer VideoPlayer => videoPlayer;

    [SerializeField] private Button btnPlay_Stop;
    [SerializeField] private Button btnReplay;
    [SerializeField] private Button btnRepeat;

    [SerializeField] private Image imgPlayStopButton;
    public Image ImgPlayStopButton => imgPlayStopButton;

    [SerializeField] private Sprite playSprite;
    public Sprite PlaySprite => playSprite;
    [SerializeField] private Sprite stopSprite;

    [SerializeField] private CanvasGroup repeatSlashGroup;


    void Start()
    {
        // Tagオブジェクトの生成
        foreach (Tag tag in System.Enum.GetValues(typeof(Tag)))
        {
            if (tag != Tag.None)
            {
                var tagObj = Instantiate(tagPrefab, songsTran);
                tagObj.Setup(tag, this);
            }
        }

        // Titleオブジェクトの生成
        DataBaseManager.instance.songDataSO.songDataList
            .Where(data => data.tag == Tag.None).ToList()
            .ForEach(data => 
            {
                var titleObj = Instantiate(titlePrefab, songsTran);
                titleObj.Setup(data, this);
            });

        // 各ボタンの処理
        btnPlay_Stop.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // ボタンの画像の設定と、動画の再生・一時停止
                if (videoPlayer.isPlaying)
                {
                    videoPlayer.Pause();
                    imgPlayStopButton.sprite = stopSprite;
                }
                else
                {
                    videoPlayer.Play();
                    imgPlayStopButton.sprite = playSprite;
                }
            })
            .AddTo(this);

        btnReplay.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // 最初から再生
                videoPlayer.time = 0f;
                videoPlayer.Play();
                imgPlayStopButton.sprite = playSprite;
            })
            .AddTo(this);

        btnRepeat.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // ループ再生の設定
                videoPlayer.isLooping = !videoPlayer.isLooping;
                repeatSlashGroup.alpha = videoPlayer.isLooping ? 0 : 1;
            })
            .AddTo(this);
    }


    /* 実装 */
    // ランダム再生(ループどうするか。ランダム中も1曲のみループされてしまうのか確認。強制的にisLoopingをfalseにして、ボタンを押せなくする？)
    // (次の曲・前の曲を再生するボタン)
}
