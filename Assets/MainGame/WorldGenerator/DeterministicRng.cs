public class DeterministicRng
{
    private System.Random rng;
    private int seed;
    public DeterministicRng(int seed)
    {
        this.seed = seed;
        rng = new System.Random(seed);
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        return rng.Next(minInclusive, maxExclusive);
    }

    public float NextFloat()
    {
        return (float)rng.NextDouble(); // [0,1)
    }

    public float NextFloat(float minInclusive, float maxExclusive)
    {
        return minInclusive + (float)rng.NextDouble() * (maxExclusive - minInclusive);
    }

    public float Hash(int x, int y)
    {
        int h = x * 374761393 + y * 668265263 + seed * 982451653;
        h = (h ^ (h >> 13)) * 1274126177;
        return (h & 0x7fffffff) / (float)int.MaxValue;
    }
}