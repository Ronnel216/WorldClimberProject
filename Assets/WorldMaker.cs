using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaker : MonoBehaviour {

    public enum LandShapeChipId
    {
        Undefind = -1,  // 未定義　このままはダメ
        Air,            // 何もない
        Inside,         // 地形内部
        Wall,           // 壁　掴めない
        Grippable,      // 掴み位置
        Ground         // 足場
       
    }

    public class MyMap<Type> 
    {
        Type[] map = null;
        int xSize = 0;

        public MyMap(int x, int y, Type init)
        {
            xSize = x;
            map = new Type[x + (xSize * y)];

            for (var i = 0; i < map.Length; i++)
                Set(i, init);
        }

        public int GetIndex(int x, int y)
        {
            return x + (xSize * y);
        }

        public Type Get(int i)
        {
            return map[i];
        }

        public Type Get(int x, int y)
        {
            return map[GetIndex(x, y)];
        }

        public Type Set(int i, Type value)
        {
            return map[i] = value;
        }

        public Type Set(int x, int y, Type value)
        {
            return map[GetIndex(x, y)] = value;
        }

        public int Length
        {
            get { return map.Length; }
        }

    }

    public class LandShapeMap : MyMap<LandShapeChipId>
    {
        List<int> undefind = null;

        public LandShapeMap(int x, int y, LandShapeChipId init)
            : base(x, y, init)
        {
            undefind = new List<int>(Length);
            for (int i = 0; i < Length; i++)
                undefind[i] = i;
        }

        public List<int> GetUndefindList()
        {
            return undefind;
        }

        public bool Remove(int i)
        {
            return undefind.Remove(i);
        }
        public bool Remove(int x, int y)
        {
            return undefind.Remove(GetIndex(x, y));
        }

    }



    // サイズ設定
    // 地形の大きさの管理
    [System.Serializable]
    public class SizeControler
    {
        [SerializeField, Range(1, int.MaxValue)]
        int max = 1;
        [SerializeField, Range(1, int.MaxValue)]
        int min = 1;

        public int Max { get { return this.max; } }
        public int Min { get { return this.min; } }

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
        [SerializeField, Range(1, int.MaxValue)]
        int max = 1;
        [SerializeField, Range(1, int.MaxValue)]
        int min = 1;

        public int Max { get { return this.max; } }
        public int Min { get { return this.min; } }


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
        [SerializeField, Range(0f, 1f)]
        float consistency;        

        public float Consistency { get { return this.consistency; } }

        public void SetInsideCell(float x, float y, ref LandShapeChipId[,] map, Vector2Int[] search)
        {
            // 0,1,2,3 それぞれ 左上右下            
            var index = Random.Range(0, search.Length);

            Debug.Assert(map[search[index].x, search[index].y] == LandShapeChipId.Undefind, "既に定義済み");
            map[search[index].x, search[index].y] = LandShapeChipId.Inside;
        }

    }

    [SerializeField]
    SizeControler sizeControler;

    [SerializeField]
    Independence independence;

    [SerializeField]
    Complexity complexity;

    [SerializeField]
    Vector3Int fieldHalfSize = new Vector3Int(5, 5, 5);

    [SerializeField, Range(1f, int.MaxValue)]
    int cellSize = 1;

    LandShapeChipId[] mapArray = null;

    private void Awake()
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // 前提条件
        Debug.Assert(fieldHalfSize.x % cellSize == 0);
        Debug.Assert(fieldHalfSize.y % cellSize == 0);
        Debug.Assert(fieldHalfSize.z % cellSize == 0);

        // 初期化
        var map = new LandShapeMap(fieldHalfSize.x / cellSize, fieldHalfSize.z / cellSize, LandShapeChipId.Undefind);



    }

    private void Update()
    {

    }
}
