public class DeterministicRng
{
    private System.Random rng;

    public DeterministicRng(int seed)
    {
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
}