using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SongTag : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text txtSongKind;
    [SerializeField] private Transform triangleTran;

    [SerializeField] private SongTitle titlePrefab;

    private bool isDropdownOpened;

    private List<SongTitle> generatedObjs = new();


    /// <summary>
    /// 初期設定
    /// </summary>
    /// <param name="songTag"></param>
    public void Setup(Tag songTag, Manager manager)
    {
        txtSongKind.text = songTag.ToString();

        button.OnClickAsObservable()
            .ThrottleFirst(System.TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ => OnClick(songTag, manager))
            .AddTo(this);
    }

    /// <summary>
    /// ボタンを押した際の処理
    /// </summary>
    private void OnClick(Tag songTag, Manager manager)
    {
        // 三角形の回転を制御
        triangleTran.localRotation = isDropdownOpened ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, -180);

        // 曲の追加・削除
        if (!isDropdownOpened)
        {
            // 自身が親の何番目の子か調べる
            var index = transform.GetSiblingIndex();

            // 自身と同じタグを持つ曲を下に生成・追加
            DataBaseManager.instance.songDataSO.songDataList
                .Where(data => data.tag == songTag).ToList()
                .ForEach(data =>
                {
                    var titleObj = Instantiate(titlePrefab, manager.SongsTran);
                    titleObj.transform.SetSiblingIndex(index + 1);
                    titleObj.Setup(data, manager);

                    generatedObjs.Add(titleObj);
                });

            isDropdownOpened = true;
        }
        else
        {
            // 自身と同じタグを持つ曲オブジェクトを破棄
            generatedObjs
                .Where(obj => obj.SongData.tag == songTag).ToList()
                .ForEach(obj =>
                {
                    generatedObjs.Remove(obj);
                    Destroy(obj.gameObject);
                });

            isDropdownOpened = false;
        }
    }
}
