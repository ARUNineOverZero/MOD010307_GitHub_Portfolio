using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

// Shows numbers from Unity's Profiler on screen, averaged over the last 60 frames,
// so you can write down a steady "before" and "after" for each change.
public class PerfReadout : MonoBehaviour
{
    [SerializeField] private Text readout;
    [SerializeField] private float refreshSeconds = 0.5f;

    private ProfilerRecorder mainThread, playerLoop, scripts, garbage, drawCalls, setPassCalls;
    private float nextRefresh;

    private void OnEnable()
    {
        mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 60);
        playerLoop = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "PlayerLoop", 60);
        scripts = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "BehaviourUpdate", 60);
        garbage = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 60);
        drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 60);
        setPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 60);
    }

    private void OnDisable()
    {
        mainThread.Dispose();
        playerLoop.Dispose();
        scripts.Dispose();
        garbage.Dispose();
        drawCalls.Dispose();
        setPassCalls.Dispose();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + refreshSeconds;

        double frameMs = Average(mainThread) / 1e6;   // the recorder counts nanoseconds
        // In the Editor the frame includes the Editor's own work too. "Game" (PlayerLoop) is only the game.
        readout.text = $"Frame: {frameMs:0.0} ms ({1000 / frameMs:0} fps)\n" +
                       $"Game: {Average(playerLoop) / 1e6:0.0} ms\n" +
                       $"Scripts: {Average(scripts) / 1e6:0.00} ms\n" +
                       $"Garbage: {Average(garbage) / 1024:0.0} KB per frame\n" +
                       $"Draw calls: {Average(drawCalls):0}   SetPass: {Average(setPassCalls):0}";
    }

    private static double Average(ProfilerRecorder recorder)
    {
        if (!recorder.Valid || recorder.Count == 0) return 0;
        double total = 0;
        for (int i = 0; i < recorder.Count; i++) total += recorder.GetSample(i).Value;
        return total / recorder.Count;
    }
}
