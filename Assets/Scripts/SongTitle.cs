using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SongTitle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text txtSongTitle;

    private SongDataSO.SongData songData;
    public SongDataSO.SongData SongData => songData;


    public void Setup(SongDataSO.SongData data, Manager manager)
    {
        songData = data;

        txtSongTitle.text = data.name;

        button.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ => OnClick(data, manager))
            .AddTo(this);
    }

    private void OnClick(SongDataSO.SongData data, Manager manager)
    {
        manager.VideoPlayer.clip = data.video;
        manager.VideoPlayer.Play();
        manager.ImgPlayStopButton.sprite = manager.PlaySprite;
    }
}
