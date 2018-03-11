using System.Diagnostics;
using System.Collections.Generic;

public class CustomProfiler
{
    public class ProfilerData
    {
        public int captures;
        public long totalTime;

        public long averagePerCapture { get { return totalTime / captures; } }
    }

    public string ID;
    private Stopwatch m_Stopwatch;

    public long TicksPerSecond { get { return Stopwatch.Frequency; } }

    private static Dictionary<string, ProfilerData> m_Data = new Dictionary<string, ProfilerData>();

    public CustomProfiler(string id = "_", bool runNow = false)
    {
        ID = id;

        if (runNow)
        {
            Start();
        }
    }

    public void Start()
    {
        m_Stopwatch = new Stopwatch();
        m_Stopwatch.Start();
    }

    public void Stop()
    {
        m_Stopwatch.Stop();

        ProfilerData data;
        if (!m_Data.TryGetValue(ID, out data))
        {
            data = new ProfilerData();
            m_Data.Add(ID, data);
        }

        ++data.captures;
        data.totalTime += m_Stopwatch.ElapsedTicks;
    }
}
