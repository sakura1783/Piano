using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    private List<SongDataSO.SongData> playList = new();
    private int playNo;
    private bool repeatPlayList;

    private bool isShuffleEventEnabled;


    void Start()
    {
        // 再生・停止ボタンの画像の制御
        videoPlayer
            .ObserveEveryValueChanged(vp => vp.isPlaying)
            .Subscribe(value => imgPlayStopButton.sprite = value ? playSprite : stopSprite)
            .AddTo(this);

        // Tagオブジェクトの生成
        foreach (Tag tag in Enum.GetValues(typeof(Tag)))
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
            .ThrottleFirst(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // 動画の再生・一時停止
                if (videoPlayer.isPlaying) videoPlayer.Pause();
                else videoPlayer.Play();
            })
            .AddTo(this);

        btnReplay.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // 最初から再生
                videoPlayer.time = 0f;
                videoPlayer.Play();
            })
            .AddTo(this);

        btnRepeat.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                // ループ再生の設定
                if (isShuffleEventEnabled)  // ランダム再生中
                {
                    repeatPlayList = !repeatPlayList;  // フラグの切り替えのみ
                    repeatSlashGroup.alpha = repeatPlayList ? 0 : 1;
                }
                else  // ランダム再生中でない場合
                {
                    videoPlayer.isLooping = !videoPlayer.isLooping;  // ひたすらに一曲をループ
                    repeatSlashGroup.alpha = videoPlayer.isLooping ? 0 : 1;
                }
            })
            .AddTo(this);

        btnShuffle.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ => ShufflePlay())
            .AddTo(this);
    }

    /// <summary>
    /// ランダム再生
    /// </summary>
    private void ShufflePlay()
    {
        isShuffleEventEnabled = true;
        
        videoPlayer.loopPointReached -= OnVideoEnd;  // 前回登録したイベントを解除

        // ランダム再生でのループは、プレイリスト全体を繰り返す(ランダム再生↔︎通常再生での不具合をなくす)
        repeatPlayList = videoPlayer.isLooping;
        videoPlayer.isLooping = false;

        playNo = 0;

        // 全曲をランダムに並び替えて、プレイリストを作成
        playList = DataBaseManager.instance.songDataSO.songDataList.OrderBy(_ => UnityEngine.Random.value).ToList();
        foreach (var songData in playList) Debug.Log(songData.video.name);

        //最初の曲だけ、曲が最後まで再生されるのを待たずに流す
        videoPlayer.clip = playList[playNo].video;
        videoPlayer.time = 0;  // (偶然同じ動画を上記のClipに設定したとき用)
        videoPlayer.Play();
        playNo++;

        // 動画が最後まで再生された際に駆動するイベントを登録
        videoPlayer.loopPointReached += OnVideoEnd;  // loopPointReachedは、動画が最後まで再生された際に呼ばれる。
        
        // subscription = Observable.Merge
        //     (
        //         Observable.Return(true), // 最初の一回だけ強制的にSubscribe内の処理を動かす
        //         //videoPlayer.ObserveEveryValueChanged(vp => vp.isPlaying) // 2回目以降は、isPlayingの変化に従う
        //         videoPlayer.ObserveEveryValueChanged(vp => vp.time >= vp.length)  // 曲が最後まで再生されたら
        //     )
        //     .ThrottleFirst(TimeSpan.FromSeconds(0.1f))
        //     //.StartWith(true)  // MergeとObservable.Return()を使わないのであれば、こちらでも同じことが実装可能
        //     //.Where(value => false)  // 間違い、以下と同義ではない。この場合、すべての値が止められる(value => trueではすべての値が流れる)
        //     //.Where(value => !value)
        //     .Subscribe(_ =>
        //     {
        //         Debug.Log("次の曲を再生します");

        //         // 次の曲を再生
        //         videoPlayer.clip = playList[playNo].video;
        //         videoPlayer.Play();

        //         playNo++;
        //     });
    }

    void OnVideoEnd(VideoPlayer vp)  // loopPointReachedにVideoPlayerの情報を引数に取らないと、シグネチャが合わずエラーとなる。
    {
        // 次の曲が存在せず、ループしない場合はランダム再生をやめる
        if (playNo >= playList.Count && !repeatPlayList) DisableShufflePlay();

        if (!isShuffleEventEnabled) return;

        // 次の曲が存在せず、ループさせる場合はプレイリストの最初の曲から再生し、プレイリストをループさせる
        if (playNo >= playList.Count) playNo = 0;

        // 次の曲を再生
        vp.clip = playList[playNo].video;
        vp.Play();

        playNo++;

        Debug.Log("次の曲を再生します");
    }

    /// <summary>
    /// ランダム再生をやめる
    /// </summary>
    public void DisableShufflePlay()
    {
        isShuffleEventEnabled = false;

        // isLoopingを再設定(ランダム再生↔︎通常再生での不具合をなくす)
        videoPlayer.isLooping = repeatPlayList;
    }

    /// <summary>
    /// ランダム再生でのVideoPlayer.isPlayingの購読を破棄
    /// </summary>
    // public void DisposeShuffleSubscription()
    // {
    //     if (subscription != null)  subscription.Dispose();
    //     subscription = null;
    // }


    /* TODO 実装 */
    // 次の曲を再生するボタン(ランダム再生の時、需要が結構あるかも)
}
