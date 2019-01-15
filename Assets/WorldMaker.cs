using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaker : MonoBehaviour {

    // サイズ設定
    // 地形の大きさの管理
    [System.Serializable]
    public class SizeControler
    {
        [Range(1, int.MaxValue)]
        public int max = 1;
        [Range(1, int.MaxValue)]
        public int min = 1;
        
        public int GetSize(float x, float y)
        {
            var noise = Noise.PerlinNoise(x, y);
            int result = (int)Noise.Range(min, max, noise);
            return result;
        }
    }

    // 独立性
    // 地形が他の地形とどのくらい離れるべきなのか
    [System.Serializable]
    public class Independence
    {
        [Range(1, int.MaxValue)]
        public int max = 1;
        [Range(1, int.MaxValue)]
        public int min = 1;

        public int Check(Vector2Int a, Vector2Int b)
        {
            var distanceLimit = (int)Noise.Range(min, max, Noise.PerlinNoise(a.x, a.y));
            var temp = (int)Vector2Int.Distance(a, b);
            return temp - distanceLimit;
        }
    }

    // 複雑性
    // グリッドの中心からの面積をどう広げるか
    [System.Serializable]
    public class Complexity
    {

    }

    private void Awake()
    {
    }

    private void Update()
    {

    }
}
