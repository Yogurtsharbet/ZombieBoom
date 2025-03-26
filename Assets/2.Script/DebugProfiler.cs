//#if UNITY_EDITOR
//using System.Collections;
//using TMPro;
//using Unity.Profiling;
//using UnityEngine;
//using UnityEngine.UI;

//public class DebugProfiler : MonoBehaviour {
//    [SerializeField] private TMP_Text Debug_DropCount;
//    [SerializeField] private TMP_Text Debug_FPS;
//    [SerializeField] private TMP_Text Debug_Mass;

//    private BlockManager blockManager;

//    private string cachedFormat = "F3";
//    private WaitForSeconds waitSec = new WaitForSeconds(0.5f);

//    private float fps;
//    private ProfilerRecorder drawCallsRecorder;
//    private ProfilerRecorder verticesRecorder;
//    private ProfilerRecorder systemMemoryRecorder;
//    private ProfilerRecorder mainThreadTimeRecorder;

//    private int dropCount;

//    public bool DebugMode;

//    private void Awake() {
//        blockManager = FindAnyObjectByType<BlockManager>();

//        dropCount = 0;
//    }

//    private void Start() {
//        if (DebugMode)
//        {
//            mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
//            systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
//            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
//            verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
//            StartCoroutine(Debugging());
//        }
//        else
//        {
//            OnDisable();
//        }

//    }

//    private void OnEnable() {
//        Debug_DropCount.enabled = true ;
//        Debug_FPS.enabled = true;
//        Debug_Mass.enabled = true;
//    }

//    private void OnDisable() {
//        Debug_DropCount.enabled = false;
//        Debug_FPS.enabled = false;
//        Debug_Mass.enabled = false;
//    }

//    private IEnumerator Debugging() {
//        yield return new WaitForSeconds(0.1f);
//        var drawCall = drawCallsRecorder.LastValue;
//        var vertices = verticesRecorder.LastValue;

//        if (Debug_Mass != null && Debug_FPS != null) {
//            while (true) {
//                fps = 1f / Time.deltaTime;

//                drawCall = drawCallsRecorder.LastValue != 0 ? drawCallsRecorder.LastValue : drawCall;
//                vertices = verticesRecorder.LastValue != 0 ? verticesRecorder.LastValue : vertices;

//                Debug_FPS.text = $"" +
//                    $"Frame : {Mathf.RoundToInt(fps)} ( {GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1} ms )\n" +
//                    $"DrawCalls : {drawCall}\n" +
//                    $"Vertices : {vertices}\n" +
//                    $"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB\n";
//                Debug_Mass.text = $"Mass : {blockManager.TotalBlockMass.ToString(cachedFormat)}";

//                yield return waitSec;
//            }
//        }
//    }

//    private static double GetRecorderFrameAverage(ProfilerRecorder recorder) {
//        var samplesCount = recorder.Capacity;
//        if (samplesCount == 0)
//            return 0;

//        double r = 0;
//        unsafe {
//            var samples = stackalloc ProfilerRecorderSample[samplesCount];
//            recorder.CopyTo(samples, samplesCount);
//            for (var i = 0; i < samplesCount; ++i)
//                r += samples[i].Value;
//            r /= samplesCount;
//        }

//        return r;
//    }

//    public void OnDrop() {
//        dropCount++;
//        Debug_DropCount.text = $"Drop : {dropCount}";
//    }
//}
//#endif