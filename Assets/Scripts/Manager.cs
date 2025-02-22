using System.Collections.Generic;
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
    [SerializeField] private Button btnShuffle;

    [SerializeField] private Image imgPlayStopButton;

    [SerializeField] private Sprite playSprite;
    public Sprite PlaySprite => playSprite;
    [SerializeField] private Sprite stopSprite;

    [SerializeField] private CanvasGroup repeatSlashGroup;


    void Start()
    {
        // 再生・停止ボタンの画像の制御
        videoPlayer
            .ObserveEveryValueChanged(vp => vp.isPlaying)
            .Subscribe(value => imgPlayStopButton.sprite = value ? playSprite : stopSprite)
            .AddTo(this);

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
                // 動画の再生・一時停止
                if (videoPlayer.isPlaying) videoPlayer.Pause();
                else videoPlayer.Play();
            })
            .AddTo(this);

        btnReplay.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // 最初から再生
                videoPlayer.time = 0f;
                videoPlayer.Play();
            })
            .AddTo(this);

        btnRepeat.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // ループ再生の設定
                videoPlayer.isLooping = !videoPlayer.isLooping;
                repeatSlashGroup.alpha = videoPlayer.isLooping ? 0 : 1;

                Debug.Log("動きました①");
            })
            .AddTo(this);

        btnShuffle.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            //.Subscribe(_ => ShufflePlay())
            .Subscribe(_ =>
            {
                ShufflePlay();
                Debug.Log("動きました");
            })
            .AddTo(this);
    }

    /// <summary>
    /// ランダム再生
    /// </summary>
    private void ShufflePlay()
    {
        // TODO ループボタン、リプレイボタン等他のボタンの制御。一旦今のままでどうなるかも確認

        int playNo = 0;

        // 全曲をランダムに並び替えて、プレイリストを作成
        List<SongDataSO.SongData> playList = new();
        playList = DataBaseManager.instance.songDataSO.songDataList.OrderBy(_ => Random.value).ToList();
        foreach (var songData in playList)
        {
            Debug.Log(songData.video.name);
        }

        Observable.Merge
            (
                Observable.Return(false), // 最初の一回は強制的にfalseを流す
                videoPlayer.ObserveEveryValueChanged(vp => vp.isPlaying)  // 2回目以降は、isPlayingの変化に従う
            )
            //.Where(value => false)  // 間違い、以下と同義ではない。この場合、すべての値が止められる(value => trueではすべての値が流れる)
            .Where(value => !value)
            .Subscribe(value =>
            {
                Debug.Log("次の曲を再生します");

                // 次の曲を再生
                videoPlayer.clip = playList[playNo].video;
                videoPlayer.Play();

                playNo++;
            })
            .AddTo(this);
    }


    /* 実装 */
    // ランダム再生(ループどうするか。ランダム中も1曲のみループされてしまうのか確認。強制的にisLoopingをfalseにして、ボタンを押せなくする？)
    // (次の曲・前の曲を再生するボタン)

    /* ランダム再生での問題点 */
    // SongTitleを押した際、選択した曲ではなく、プレイリストの次の曲へ移行してしまう
    // 停止ボタンを押した際にもプレイリストの次の曲へ移ってしまう(現在の曲の再生・停止が行われない)
    // プレイリスト1番目の曲が再生されない。
}
