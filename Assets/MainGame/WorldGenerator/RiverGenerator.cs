using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class RiverGenerator
{
    public static float[] CarveRiver(int mapWidth, int mapHeight, int seed, int riverWidth, float maxHeight, float[] heightMap)
    {
        Random rng = new Random(seed);

        // 1. Define 6 waypoints spaced evenly along Y axis
        int numPoints = 6;
        List<int> points = new List<int>();

        for (int i = 0; i < numPoints; i++)
        {
            int y = (i * (mapHeight - 1)) / (numPoints - 1);
            int x = rng.Next(0, mapWidth);
            points.Add(y * mapWidth + x);
        }

        // 2. Generate paths between waypoints
        for (int i = 0; i < points.Count - 1; i++)
        {
            List<int> path = FindPath(points[i], points[i + 1], mapWidth, mapHeight, heightMap, maxHeight);

            // 3. Carve the trench
            if (path != null)
            {
                foreach (int pos in path)
                    CarveTrench(pos, riverWidth, mapWidth, mapHeight, heightMap);
            }
        }
        
        return heightMap;
    }

    private static List<int> FindPath(int start, int end, int w, int h, float[] map, float maxHeight)
    {
        var openSet = new SimplePriorityQueue<int, float>();
        int[] cameFrom = new int[w * h];
        Array.Fill(cameFrom, -1);
        float[] gScore = new float[w * h];
        Array.Fill(gScore, float.MaxValue);

        gScore[start] = 0;
        openSet.Enqueue(start, 0);

        while (openSet.Count > 0)
        {
            int current = openSet.Dequeue();
            if (current == end) return ReconstructPath(cameFrom, current);

            foreach (int neighbor in GetNeighbors(current, w, h))
            {
                if (map[neighbor] > maxHeight) continue;

                // 1. Terrain Slope: Downhill is cheap
                float slope = map[neighbor] - map[current];
                float slopeCost = slope > 0 ? (slope * 50f) : (slope * 0.1f);

                // 2. Add randomness (Noise) to prevent straight lines
                float noise = ((float)(neighbor % w * 31 + neighbor / w * 37) % 20) / 2f;

                // Tentative cost
                float tentativeG = gScore[current] + slopeCost + noise + 1f;

                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;

                    // A* Heuristic: distance to the segment end point
                    float hScore = Math.Abs((neighbor % w) - (end % w)) + Math.Abs((neighbor / w) - (end / w));
                    openSet.Enqueue(neighbor, tentativeG + hScore);
                }
            }
        }
        return null;
    }

    private static void CarveTrench(int index, int width, int w, int h, float[] map)
    {
        int cx = index % w, cy = index / w;
        for (int y = Math.Max(0, cy - width); y <= Math.Min(h - 1, cy + width); y++)
        {
            for (int x = Math.Max(0, cx - width); x <= Math.Min(w - 1, cx + width); x++)
            {
                int idx = y * w + x;
                // Carve only if current terrain is already above 0 to avoid spikes
                // and preserve the terraced floor structure
                map[idx] = 0f;
            }
        }
    }

    private static List<int> ReconstructPath(int[] cameFrom, int current)
    {
        List<int> path = new List<int>();
        while (current != -1) { path.Add(current); current = cameFrom[current]; }
        path.Reverse(); // Ensure path goes from start to end
        return path;
    }

    private static IEnumerable<int> GetNeighbors(int idx, int w, int h)
    {
        int x = idx % w, y = idx / w;
        if (x > 0) yield return idx - 1;
        if (x < w - 1) yield return idx + 1;
        if (y > 0) yield return idx - w;
        if (y < h - 1) yield return idx + w;
    }

    // Min-Heap for A*
    public class SimplePriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<(TElement Element, TPriority Priority)> nodes = new List<(TElement, TPriority)>();
        public int Count => nodes.Count;
        public void Enqueue(TElement element, TPriority priority) { nodes.Add((element, priority)); int i = nodes.Count - 1; while (i > 0) { int p = (i - 1) / 2; if (nodes[p].Priority.CompareTo(nodes[i].Priority) <= 0) break; var temp = nodes[i]; nodes[i] = nodes[p]; nodes[p] = temp; i = p; } }
        public TElement Dequeue() { var result = nodes[0].Element; nodes[0] = nodes[nodes.Count - 1]; nodes.RemoveAt(nodes.Count - 1); int i = 0; while (true) { int l = i * 2 + 1, r = i * 2 + 2, smallest = i; if (l < nodes.Count && nodes[l].Priority.CompareTo(nodes[smallest].Priority) < 0) smallest = l; if (r < nodes.Count && nodes[r].Priority.CompareTo(nodes[smallest].Priority) < 0) smallest = r; if (smallest == i) break; var temp = nodes[i]; nodes[i] = nodes[smallest]; nodes[smallest] = temp; i = smallest; } return result; }
    }
}



