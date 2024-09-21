namespace UnityBundleReader;

public static class Progress
{
    public static readonly IProgress<int> Default = new Progress<int>();
    static int _preValue;

    public static void Reset()
    {
        _preValue = 0;
        Default.Report(0);
    }

    public static void Report(int current, int total)
    {
        int value = (int)(current * 100f / total);
        Report(value);
    }

    static void Report(int value)
    {
        if (value > _preValue)
        {
            _preValue = value;
            Default.Report(value);
        }
    }
}
