using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public enum Tag
{
    None,

    クラシック,
    ジブリ,
    魔法少女まどかマギカ,
    メメントモリ,
    NieR_Automata,
    ゼルダの伝説,
}

[CreateAssetMenu(fileName = "SongDataSO", menuName = "Create SongDataSO")]
public class SongDataSO : ScriptableObject
{
    public List<SongData> songDataList = new();


    [System.Serializable]
    public class SongData
    {
        public Tag tag;
        public string name;
        public VideoClip video;
    }
}
