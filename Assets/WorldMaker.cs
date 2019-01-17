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

        public MyMap(int xSize, int ySize, Type init)
        {
            this.xSize = xSize;

            map = new Type[xSize + ((xSize - 1) * ySize)];
            for (var i = 0; i < map.Length; i++)
                Set(i, init);
        }

        public Vector2Int Size
        {
            get { return new Vector2Int(xSize, map.Length / xSize); }
        }
       
        public void Each(System.Action<Type, int, int, int> func)
        {
            int x = 0, y = 0;
            for (var i = 0; i < Length; i++)
            {
                GetIndex(i, out x, out y);
                func(Get(i), x, y, i);
            }
        }

        public void EachPerFrame(System.Action<Type, int, int, int> func, int i)
        {
            if (i >= Length) return;
            int x = 0, y = 0;
            GetIndex(i, out x, out y);
            func(Get(i), x, y, i);
        }

        public int GetIndex(int x, int y)
        {
            return x + (xSize * y);
        }

        public void GetIndex(int i, out int x, out int y)
        {
            x = i % xSize;
            y = i / xSize;
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
                undefind.Add( i );

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
        [SerializeField, Range(1, 100)]
        int max = 1;
        [SerializeField, Range(1, 100)]
        int min = 1;

        public int Max { get { return this.max; } }
        public int Min { get { return this.min; } }

        public int GetSize(float x, float y)
        {
            var noise = Noise.PerlinNoise(x, y);
            Debug.Log("min" + min);
            Debug.Log("max" + max);
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

        public void SetInsideCell(float x, float y, ref LandShapeMap map, ref List<Vector2Int> search)
        {
            var index = Random.Range(0, search.Count);

            Debug.Assert(map.Get(search[index].x, search[index].y) == LandShapeChipId.Undefind, "既に定義済み");
            map.Set(search[index].x, search[index].y, LandShapeChipId.Inside);

            // 探索範囲の追加
            GetAroundCell(search[index].x, search[index].y, map, ref search);

            search.RemoveAt(index);            
        }

        void GetAroundCell(int x, int y, LandShapeMap map, ref List<Vector2Int> list)
        {
            var around = new List<Vector2Int>();
            // 周辺のセルの取得
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= 1; j++)
                    around.Add(new Vector2Int(x + i, y + j));

            // 中心のセルの削除
            around.RemoveAt(4);

            // 範囲外のセルの削除
            around.RemoveAll((Vector2Int pos) => {
                return
                pos.x < 0 ||
                pos.x > map.Size.x ||
                pos.y < 0 ||
                pos.y > map.Size.y;
            });

            foreach (var item in around)
            {
                Debug.Log(item);
            }


            // リストへの追加
            var cnt = list.Count;
            foreach (var v in around)
            {
                for (var i = 0; i < cnt; i++)
                {
                    if (list[i] == v) continue;
                    if (map.Get(v.x, v.y) != LandShapeChipId.Undefind) continue;

                    Debug.Log("add");
                    list.Add(v);
                }
            }          

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

    bool isFinished = false;

    LandShapeMap map;
    GameObject obj;
    private void Awake()
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // 前提条件
        Debug.Assert(fieldHalfSize.x % cellSize == 0);
        Debug.Assert(fieldHalfSize.y % cellSize == 0);
        Debug.Assert(fieldHalfSize.z % cellSize == 0);

        // 初期化
        Vector3Int numCell = new Vector3Int
        (
            2 * fieldHalfSize.x / cellSize,
            2 * fieldHalfSize.y / cellSize,
            2 * fieldHalfSize.z / cellSize
        );

        map = new LandShapeMap(numCell.x / 2, numCell.z / 2, LandShapeChipId.Undefind);

        // とりあえず中心から配置
        var size = sizeControler.GetSize(0, 0.2f);
        var count = 0;

        var searchCell = new List<Vector2Int>();

        for (var i = 0; i < 4; i++)
            searchCell.Add(new Vector2Int(i % 2, i / 2));

        Debug.Log(size);

        while (count < size)
        {
            if (searchCell.Count == 0) break;
            complexity.SetInsideCell(0f, 0f, ref map, ref searchCell);
            count++;
            if (count > 10000000) break;
        }

    }
    int index = 0;

    private void Update()
    {
        map.EachPerFrame((LandShapeChipId id, int x, int y, int i) =>
        {
            if (id == LandShapeChipId.Inside)
                GameObject.Instantiate(obj, new Vector3((float)x, 0f, (float)y), Quaternion.identity);
        }, index++);
        
    }
}
