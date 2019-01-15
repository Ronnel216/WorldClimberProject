using UnityEngine;

public class Noise
{

    private static int randomTableSize = 256;
    private static Vector4[] randomTable;
    private static int[] permX;
    private static int[] permY;
    private static int[] permZ;
    private static int[] permW;

    static Noise()
    {
        randomTable = new Vector4[randomTableSize];
        for (int i = 0; i < randomTableSize; i++)
        {
            randomTable[i] = new Vector4(Random.value, Random.value, Random.value, Random.value) * 2.0f - Vector4.one;
        }

        permX = new int[randomTableSize];
        permY = new int[randomTableSize];
        permZ = new int[randomTableSize];
        permW = new int[randomTableSize];
        for (int i = 0; i < randomTableSize; i++)
        {
            permX[i] = i;
            permY[i] = i;
            permZ[i] = i;
            permW[i] = i;
        }
        shuffle(permX);
        shuffle(permY);
        shuffle(permZ);
        shuffle(permW);

    }

    private static void shuffle(int[] array)
    {
        int len = array.Length;
        for (int i = 0; i < len; i++)
        {
            int j = Mathf.FloorToInt(Random.value * len);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;

        }
    }

    private static float GetGradient(int x)
    {
        return randomTable[permX[x % randomTableSize]].x;
    }

    private static Vector2 GetGradient(int x, int y)
    {
        return randomTable[permY[(y + permX[x % randomTableSize]) % randomTableSize]];
    }

    private static Vector3 GetGradient(int x, int y, int z)
    {
        return randomTable[permZ[(z + permY[(y + permX[x % randomTableSize]) % randomTableSize]) % randomTableSize]];
    }

    private static Vector4 GetGradient(int x, int y, int z, int w)
    {
        return randomTable[permW[(w + permZ[(z + permY[(y + permX[x % randomTableSize]) % randomTableSize]) % randomTableSize]) % randomTableSize]];
    }

    public static float Range(float min, float max, float noise)
    {
        return ((max - min) * noise + min);
    }

    public static float PerlinNoise(float x)
    {
        x = Mathf.Abs(x);
        int xi = (int)x;
        float f = x - xi;

        float g0 = GetGradient(xi);
        float g1 = GetGradient(xi + 1);

        float p0 = f;
        float p1 = f - 1f;

        float v0 = g0 * p0;
        float v1 = g1 * p1;

        return Mathf.SmoothStep(v0, v1, f);
    }

    public static float PerlinNoise(float x, float y)
    {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        int xi = (int)x;
        int yi = (int)y;
        float xf = x - xi;
        float yf = y - yi;
        Vector2 f = new Vector2(xf, yf);

        Vector2 g00 = GetGradient(xi, yi);
        Vector2 g10 = GetGradient(xi + 1, yi);
        Vector2 g01 = GetGradient(xi, yi + 1);
        Vector2 g11 = GetGradient(xi + 1, yi + 1);

        Vector2 p00 = f - new Vector2(0, 0);
        Vector2 p10 = f - new Vector2(1, 0);
        Vector2 p01 = f - new Vector2(0, 1);
        Vector2 p11 = f - new Vector2(1, 1);

        float v00 = Vector2.Dot(g00, p00);
        float v10 = Vector2.Dot(g10, p10);
        float v01 = Vector2.Dot(g01, p01);
        float v11 = Vector2.Dot(g11, p11);

        return Mathf.SmoothStep(Mathf.SmoothStep(v00, v10, xf), Mathf.SmoothStep(v01, v11, xf), yf);
    }

    public static float PerlinNoise(float x, float y, float z)
    {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        z = Mathf.Abs(z);
        int xi = (int)x;
        int yi = (int)y;
        int zi = (int)z;
        float xf = x - xi;
        float yf = y - yi;
        float zf = z - zi;
        Vector3 f = new Vector3(xf, yf, zf);

        Vector3 g000 = GetGradient(xi, yi, zi);
        Vector3 g100 = GetGradient(xi + 1, yi, zi);
        Vector3 g010 = GetGradient(xi, yi + 1, zi);
        Vector3 g110 = GetGradient(xi + 1, yi + 1, zi);
        Vector3 g001 = GetGradient(xi, yi, zi + 1);
        Vector3 g101 = GetGradient(xi + 1, yi, zi + 1);
        Vector3 g011 = GetGradient(xi, yi + 1, zi + 1);
        Vector3 g111 = GetGradient(xi + 1, yi + 1, zi + 1);

        Vector3 p000 = f - new Vector3(0, 0, 0);
        Vector3 p100 = f - new Vector3(1, 0, 0);
        Vector3 p010 = f - new Vector3(0, 1, 0);
        Vector3 p110 = f - new Vector3(1, 1, 0);
        Vector3 p001 = f - new Vector3(0, 0, 1);
        Vector3 p101 = f - new Vector3(1, 0, 1);
        Vector3 p011 = f - new Vector3(0, 1, 1);
        Vector3 p111 = f - new Vector3(1, 1, 1);

        float v000 = Vector3.Dot(g000, p000);
        float v100 = Vector3.Dot(g100, p100);
        float v010 = Vector3.Dot(g010, p010);
        float v110 = Vector3.Dot(g110, p110);
        float v001 = Vector3.Dot(g001, p001);
        float v101 = Vector3.Dot(g101, p101);
        float v011 = Vector3.Dot(g011, p011);
        float v111 = Vector3.Dot(g111, p111);

        return Mathf.SmoothStep(
            Mathf.SmoothStep(Mathf.SmoothStep(v000, v100, xf), Mathf.SmoothStep(v010, v110, xf), yf),
            Mathf.SmoothStep(Mathf.SmoothStep(v001, v101, xf), Mathf.SmoothStep(v011, v111, xf), yf),
            zf);
    }

    public static float PerlinNoise(float x, float y, float z, float w)
    {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        z = Mathf.Abs(z);
        w = Mathf.Abs(w);
        int xi = (int)x;
        int yi = (int)y;
        int zi = (int)z;
        int wi = (int)w;
        float xf = x - xi;
        float yf = y - yi;
        float zf = z - zi;
        float wf = w - wi;
        Vector4 f = new Vector4(xf, yf, zf, wf);

        Vector4 g0000 = GetGradient(xi, yi, zi, wi);
        Vector4 g1000 = GetGradient(xi + 1, yi, zi, wi);
        Vector4 g0100 = GetGradient(xi, yi + 1, zi, wi);
        Vector4 g1100 = GetGradient(xi + 1, yi + 1, zi, wi);
        Vector4 g0010 = GetGradient(xi, yi, zi + 1, wi);
        Vector4 g1010 = GetGradient(xi + 1, yi, zi + 1, wi);
        Vector4 g0110 = GetGradient(xi, yi + 1, zi + 1, wi);
        Vector4 g1110 = GetGradient(xi + 1, yi + 1, zi + 1, wi);
        Vector4 g0001 = GetGradient(xi, yi, zi, wi + 1);
        Vector4 g1001 = GetGradient(xi + 1, yi, zi, wi + 1);
        Vector4 g0101 = GetGradient(xi, yi + 1, zi, wi + 1);
        Vector4 g1101 = GetGradient(xi + 1, yi + 1, zi, wi + 1);
        Vector4 g0011 = GetGradient(xi, yi, zi + 1, wi + 1);
        Vector4 g1011 = GetGradient(xi + 1, yi, zi + 1, wi + 1);
        Vector4 g0111 = GetGradient(xi, yi + 1, zi + 1, wi + 1);
        Vector4 g1111 = GetGradient(xi + 1, yi + 1, zi + 1, wi + 1);

        Vector4 p0000 = f - new Vector4(0, 0, 0, 0);
        Vector4 p1000 = f - new Vector4(1, 0, 0, 0);
        Vector4 p0100 = f - new Vector4(0, 1, 0, 0);
        Vector4 p1100 = f - new Vector4(1, 1, 0, 0);
        Vector4 p0010 = f - new Vector4(0, 0, 1, 0);
        Vector4 p1010 = f - new Vector4(1, 0, 1, 0);
        Vector4 p0110 = f - new Vector4(0, 1, 1, 0);
        Vector4 p1110 = f - new Vector4(1, 1, 1, 0);
        Vector4 p0001 = f - new Vector4(0, 0, 0, 1);
        Vector4 p1001 = f - new Vector4(1, 0, 0, 1);
        Vector4 p0101 = f - new Vector4(0, 1, 0, 1);
        Vector4 p1101 = f - new Vector4(1, 1, 0, 1);
        Vector4 p0011 = f - new Vector4(0, 0, 1, 1);
        Vector4 p1011 = f - new Vector4(1, 0, 1, 1);
        Vector4 p0111 = f - new Vector4(0, 1, 1, 1);
        Vector4 p1111 = f - new Vector4(1, 1, 1, 1);

        float v0000 = Vector4.Dot(g0000, p0000);
        float v1000 = Vector4.Dot(g1000, p1000);
        float v0100 = Vector4.Dot(g0100, p0100);
        float v1100 = Vector4.Dot(g1100, p1100);
        float v0010 = Vector4.Dot(g0010, p0010);
        float v1010 = Vector4.Dot(g1010, p1010);
        float v0110 = Vector4.Dot(g0110, p0110);
        float v1110 = Vector4.Dot(g1110, p1110);
        float v0001 = Vector4.Dot(g0001, p0001);
        float v1001 = Vector4.Dot(g1001, p1001);
        float v0101 = Vector4.Dot(g0101, p0101);
        float v1101 = Vector4.Dot(g1101, p1101);
        float v0011 = Vector4.Dot(g0011, p0011);
        float v1011 = Vector4.Dot(g1011, p1011);
        float v0111 = Vector4.Dot(g0111, p0111);
        float v1111 = Vector4.Dot(g1111, p1111);

        return
            Mathf.SmoothStep(
                Mathf.SmoothStep(
                    Mathf.SmoothStep(Mathf.SmoothStep(v0000, v1000, xf), Mathf.SmoothStep(v0100, v1100, xf), yf),
                    Mathf.SmoothStep(Mathf.SmoothStep(v0010, v1010, xf), Mathf.SmoothStep(v0110, v1110, xf), yf),
                    zf),
                Mathf.SmoothStep(
                    Mathf.SmoothStep(Mathf.SmoothStep(v0001, v1001, xf), Mathf.SmoothStep(v0101, v1101, xf), yf),
                    Mathf.SmoothStep(Mathf.SmoothStep(v0011, v1011, xf), Mathf.SmoothStep(v0111, v1111, xf), yf),
                    zf),
                wf
            );

    }
}
