using IPA;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using HarmonyLib;
using BSLedShow.Lighting;
using BSLedShow.Utils;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using BS_Utils.Utilities;
using IPA.Config.Stores;
using BSLedShow.Configuration;

namespace BSLedShow
{

    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public enum ConnectionStatus
        {
            Failed = 0,
            Connected = 1,
            Connecting = 2
        }

        // TODO: If using Harmony, uncomment and change YourGitHub to the name of your GitHub account, or use the form "com.company.project.product"
        //       You must also add a reference to the Harmony assembly in the Libs folder.

        public const string HarmonyId = "com.github.ByadminPresents.BSLedShow";
        internal static readonly Harmony harmony = new Harmony(HarmonyId);

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static BSLedShowController PluginController { get { return BSLedShowController.Instance; } }
        public float time = 0;
        public float bpmTime = 0;
        public float currentBPM = 0;
        public float baseBPM = 0;
        public int boostColorsOffset = 0;
        public float[][] envColors;
        public LightGroupsProvider lightGroupsProvider;
        public string environmentName;

        public IReadonlyBeatmapData beatmapData;
        public BeatmapCallbacksController CallbackController;
        public List<BeatmapDataCallbackWrapper> callbackData = new List<BeatmapDataCallbackWrapper>();

        public LightsProcessor LightsProcessor;

        public Task lightsProcessTask = null;
        public CancellationTokenSource lightsProcessTaskCTS;

        public Stopwatch beatMapStartLEDEffectWaiterSW = new Stopwatch();

        public delegate void ConnectionStatusChangedHandler(ConnectionStatus status);
        public event ConnectionStatusChangedHandler OnConnectionStatusChanged;

        private ConnectionStatus _LEDControllerConnectionStatus;
        public ConnectionStatus LEDControllerConnectionStatus
        {
            get { return _LEDControllerConnectionStatus; }
            set
            {
                _LEDControllerConnectionStatus = value;
                OnConnectionStatusChanged?.Invoke(_LEDControllerConnectionStatus);
            }
        }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public Plugin(IPALogger logger)
        {
            Instance = this;
            Plugin.Log = logger;
            Plugin.Log?.Debug("Logger initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config

        [Init]
        public void InitWithConfig(IPA.Config.Config conf)
        {
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            Plugin.Log?.Debug("Config loaded");
        }

        #endregion


        #region Disableable

        /// <summary>
        /// Called when the plugin is enabled (including when the game starts if the plugin is enabled).
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            new GameObject("BSLedShowController").AddComponent<BSLedShowController>();
            ApplyHarmonyPatches();

            LightsProcessor = new LightsProcessor();
            BSEvents.menuSceneLoaded += RunIdleLEDEffect;
        }

        public void RunIdleLEDEffect()
        {
            Task.Run(() =>
            {
                if (lightsProcessTask != null)
                {
                    lightsProcessTaskCTS.Cancel();
                    lightsProcessTask.Wait();
                    lightsProcessTask = null;
                }
                LEDControllerConnectionStatus = ConnectionStatus.Connecting;
                LightsProcessor.RunLEDEffect(PluginConfig.Instance.mainMenuLEDEffectId, new uint[] { 0 });
                LEDControllerConnectionStatus = ConnectionStatus.Connected;
            });
        }

        /// <summary>
        /// Called when the plugin is disabled and on Beat Saber quit. It is important to clean up any Harmony patches, GameObjects, and Monobehaviours here.
        /// The game should be left in a state as if the plugin was never started.
        /// Methods marked [OnDisable] must return void or Task.
        /// </summary>
        [OnDisable]
        public void OnDisable()
        {
            if (PluginController != null)
                GameObject.Destroy(PluginController);
            RemoveHarmonyPatches();
            BSEvents.menuSceneLoaded -= RunIdleLEDEffect;
        }

        /*
        /// <summary>
        /// Called when the plugin is disabled and on Beat Saber quit.
        /// Return Task for when the plugin needs to do some long-running, asynchronous work to disable.
        /// [OnDisable] methods that return Task are called after all [OnDisable] methods that return void.
        /// </summary>
        [OnDisable]
        public async Task OnDisableAsync()
        {
            await LongRunningUnloadTask().ConfigureAwait(false);
        }
        */
        #endregion

        // Uncomment the methods in this section if using Harmony
        #region Harmony

        /// <summary>
        /// Attempts to apply all the Harmony patches in this assembly.
        /// </summary>
        internal static void ApplyHarmonyPatches()
        {
            try
            {
                Plugin.Log?.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error applying Harmony patches: " + ex.Message);
                Plugin.Log?.Debug(ex);
            }
        }

        /// <summary>
        /// Attempts to remove all the Harmony patches that used our HarmonyId.
        /// </summary>
        internal static void RemoveHarmonyPatches()
        {
            try
            {
                // Removes all patches with this HarmonyId
                harmony.UnpatchAll(HarmonyId);
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error removing Harmony patches: " + ex.Message);
                Plugin.Log?.Debug(ex);
            }
        }

        #endregion
    }
}
