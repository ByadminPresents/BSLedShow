using BSLedShow;
using BSLedShow.Utils;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Threading;

public class SharedCoroutineStarter : MonoBehaviour
{
    public static MonoBehaviour instance;

    public DelayedCoroutine delayedCoroutine;

    public static void Init()
    {
        instance = new GameObject().AddComponent<SharedCoroutineStarter>();

    }

    void Update()
    {
        //if (Plugin.LightsProcessor == null)
        //{
        //    return;
        //}
        //if (delayedCoroutine == null)
        //{
        //    delayedCoroutine = new DelayedCoroutine(() => Task.Run(Plugin.LightsProcessor.Test), 0.02f);
        //}
        //StartCoroutine(delayedCoroutine.CallNextFrame());

    }

    void Start()
    {
        //Task.Run(() =>
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();

        //    while (true)
        //    {
        //        Plugin.LightsProcessor.Test();
                
        //        Thread.Sleep((int)Math.Max((float)(20 - sw.ElapsedMilliseconds), 0));
        //        //Plugin.Log?.Error($"sleeping for {sw.ElapsedMilliseconds}");
        //        sw.Restart();
        //    }
        //});
    }

    void Awake()
    {
        //delayedCoroutine = new DelayedCoroutine(() => Plugin.LightsProcessor.Test(), 0.05f);
        
        GameObject.DontDestroyOnLoad(gameObject);
    }

}