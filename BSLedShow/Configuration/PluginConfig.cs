using System;
using System.Runtime.CompilerServices;
using BSLedShow.Utils;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using Newtonsoft.Json;
using System.IO;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BSLedShow.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        [SerializedName("LEDGroupBindingsPath")]
        public virtual string LEDGroupBindingsPath { get; set; } = "";

        [SerializedName("IpAddressOfLEDController")]
        public virtual string LEDControllerIpAddress { get; set; } = "192.168.0.1";

        [SerializedName("PortOfLEDController")]
        public virtual int LEDControllerPort { get; set; } = 1234;

        [SerializedName("LEDUpdateSpeedHz")]
        public virtual int ledUpdateSpeed { get; set; } = 60;

        [SerializedName("MainMenuLEDEffectID")]
        public virtual byte mainMenuLEDEffectId { get; set; } = 254;

        [SerializedName("EnableBeatMapStartLEDEffect")]
        public virtual bool isBeatMapStartLEDEffectEnabled { get; set; } = true;

        [SerializedName("BeatMapStartLEDEffectID")]
        public virtual byte beatMapStartLEDEffectId { get; set; } = 255;

        [SerializedName("EnableDelayBeforeMapStarts")]
        public virtual bool beatMapStartLEDEffectWaitForMillis { get; set; } = true;

        [SerializedName("DelayBeforeMapStartsInMillis")]
        public virtual int beatMapStartLEDEffectDelayMillis { get; set; } = 3500;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            if (ledUpdateSpeed <= 0)
            {
                ledUpdateSpeed = 1;
            }
            if (ledUpdateSpeed > 250)
            {
                ledUpdateSpeed = 250;
            }

            try
            {
                var json = File.ReadAllText(LEDGroupBindingsPath);
                Plugin.Instance.lightGroupsProvider = JsonConvert.DeserializeObject<LightGroupsProvider>(json);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Plugin.Log?.Error($"LEDGroupBindings config file not found");
                    Plugin.Instance.lightGroupsProvider = new LightGroupsProvider();
                }
                else
                {
                    Plugin.Log?.Error($"Error occured during LEDGroupBindings config file load: {ex}");
                }
            }

            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}
