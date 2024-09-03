using HarmonyLib;
using BSLedShow.Lighting;
using System.Threading;
using System.Threading.Tasks;
using System;
using BSLedShow.Utils;
using UnityEngine;
using UnityEngine.UI;
using Heck;
using IPA.Utilities.Async;
using System.Diagnostics;
using CustomJSONData.CustomBeatmap;
using UnityEngine.SceneManagement;
using BS_Utils.Utilities;

namespace BSLedShow.Patch
{
    [HarmonyPatch(typeof(BeatmapCallbacksController), "ManualUpdate")] //BeatmapObjectCallbackController
    public class LightingEventsSubscriber
    {
        public static bool isInjected = false;
        [HarmonyAfter(new string[] { "com.aeroluna.BeatSaber.CustomJSONData" })]
        private static void Postfix(BeatmapCallbacksController __instance, IReadonlyBeatmapData ____beatmapData, float ____songTime)
        {
            Plugin.Instance.bpmTime += (____songTime - Plugin.Instance.time) * (Plugin.Instance.currentBPM / 60);
            Plugin.Instance.time = ____songTime;

            if (isInjected)
            {
                return;
            }
            isInjected = true;

            Plugin.Instance.currentBPM = Plugin.Instance.baseBPM;

            Plugin.Instance.beatmapData = ____beatmapData;
            Plugin.Instance.CallbackController = __instance;

            Plugin.Instance.callbackData.Add(Plugin.Instance.CallbackController.AddBeatmapCallback<ColorBoostBeatmapEventData>(CallbacksHandler.HandleBeatmapBoostLightEventCallback));
            Plugin.Instance.callbackData.Add(Plugin.Instance.CallbackController.AddBeatmapCallback<BasicBeatmapEventData>(CallbacksHandler.HandleBeatmapLightEventCallback));
            Plugin.Instance.callbackData.Add(Plugin.Instance.CallbackController.AddBeatmapCallback<BPMChangeBeatmapEventData>(CallbacksHandler.HandleBeatmapBPMChangingCallback));
        }
    }


    [HarmonyPatch(typeof(GameScenesManager), "ScenesTransitionCoroutine")] //BeatmapObjectCallbackController
    public class BeatMapScenePatch
    {
        private static void Prefix(ScenesTransitionSetupDataSO newScenesTransitionSetupData, ref float minDuration)
        {
            if (newScenesTransitionSetupData?.GetType() == typeof(StandardLevelScenesTransitionSetupDataSO) && Configuration.PluginConfig.Instance.isBeatMapStartLEDEffectEnabled && Configuration.PluginConfig.Instance.beatMapStartLEDEffectWaitForMillis && Plugin.Instance.beatMapStartLEDEffectWaiterSW.ElapsedMilliseconds < Configuration.PluginConfig.Instance.beatMapStartLEDEffectDelayMillis)
            {
                Plugin.Instance.beatMapStartLEDEffectWaiterSW.Stop();
                minDuration += (Configuration.PluginConfig.Instance.beatMapStartLEDEffectDelayMillis - Plugin.Instance.beatMapStartLEDEffectWaiterSW.ElapsedMilliseconds) / 1000f;
            }
            else if (newScenesTransitionSetupData?.GetType() == typeof(MenuScenesTransitionSetupDataSO))
            {
                Plugin.Instance.RunIdleLEDEffect();
            }
        }
    }

    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
    internal class NewLevelStarted
    {
        private static void Postfix(IPreviewBeatmapLevel previewBeatmapLevel, ColorScheme overrideColorScheme, IDifficultyBeatmap difficultyBeatmap)
        {
            var pluginInstance = Plugin.Instance;
            
            if (pluginInstance.lightsProcessTask != null)
            {
                pluginInstance.lightsProcessTaskCTS.Cancel();
                pluginInstance.lightsProcessTask.Wait();
                pluginInstance.lightsProcessTask = null;
            }

            if (pluginInstance.LightsProcessor != null)
            {
                pluginInstance.LightsProcessor = null;
            }

            pluginInstance.LightsProcessor = new LightsProcessor();

            pluginInstance.time = 0;
            pluginInstance.bpmTime = 0;
            pluginInstance.baseBPM = previewBeatmapLevel.beatsPerMinute;
            pluginInstance.boostColorsOffset = 0;

            var environmentInfo = BeatmapEnvironmentHelper.GetEnvironmentInfo(difficultyBeatmap);

            pluginInstance.environmentName = environmentInfo.serializedName;

            ColorScheme colorScheme = overrideColorScheme;

            if (colorScheme == null)
            {
                colorScheme = new ColorScheme(environmentInfo.colorScheme);
            }

            pluginInstance.envColors = new float[6][] {
                    new float[] { colorScheme.environmentColor0.r, colorScheme.environmentColor0.g, colorScheme.environmentColor0.b, colorScheme.environmentColor0.a },
                    new float[] { colorScheme.environmentColor1.r, colorScheme.environmentColor1.g, colorScheme.environmentColor1.b, colorScheme.environmentColor1.a },
                    new float[] { colorScheme.environmentColorW.r, colorScheme.environmentColorW.g, colorScheme.environmentColorW.b, colorScheme.environmentColorW.a },
                    new float[] { colorScheme.environmentColor0Boost.r, colorScheme.environmentColor0Boost.g, colorScheme.environmentColor0Boost.b, colorScheme.environmentColor0Boost.a },
                    new float[] { colorScheme.environmentColor1Boost.r, colorScheme.environmentColor1Boost.g, colorScheme.environmentColor1Boost.b, colorScheme.environmentColor1Boost.a },
                    new float[] { colorScheme.environmentColorWBoost.r, colorScheme.environmentColorWBoost.g, colorScheme.environmentColorWBoost.b, colorScheme.environmentColorWBoost.a }
                };

            foreach (var callbackWrapper in pluginInstance.callbackData)
            {
                pluginInstance.CallbackController?.RemoveBeatmapCallback(callbackWrapper);
            }
            LightingEventsSubscriber.isInjected = false;

            pluginInstance.lightsProcessTaskCTS = new CancellationTokenSource();
            var ctToken = pluginInstance.lightsProcessTaskCTS.Token;

            if (Configuration.PluginConfig.Instance.isBeatMapStartLEDEffectEnabled)
            {
                pluginInstance.LightsProcessor.RunLEDEffect(Configuration.PluginConfig.Instance.beatMapStartLEDEffectId, new uint[] { 1 });
                pluginInstance.beatMapStartLEDEffectWaiterSW.Restart();
            }

            pluginInstance.lightsProcessTask = Task.Run(() =>
            {
                var localCTToken = ctToken;
                var sw = new Stopwatch();
                sw.Start();

                while (!localCTToken.IsCancellationRequested)
                {
                    pluginInstance.LightsProcessor.ProcessLights();

                    Thread.Sleep((int)Math.Max((float)(1000.0 / Configuration.PluginConfig.Instance.ledUpdateSpeed - sw.ElapsedMilliseconds), 0));
                    sw.Restart();
                }
                sw.Stop();
                sw = null;
            });
        }
    }
}
