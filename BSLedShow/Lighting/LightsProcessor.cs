using BSLedShow.Utils;
using BSLedShow.Lighting;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BSLedShow.Lighting
{
    public class LightsProcessor
    {
        private const int BoostColorsEventTypeValue = 5;

        private Task dataSendingTask = null;
        private CancellationTokenSource ct = null;

        private byte[] color;
        private float[] argbColor;
        private List<int> ledIndexes;
        public LightsProcessor()
        {
            this.LightEvents = new ConcurrentDictionary<int, ConcurrentDictionary<int, ExtractedEventData>>();
            this.color = new byte[3];
            this.argbColor = new float[4];
            this.ledIndexes = new List<int>(2);
        }

        private ConcurrentDictionary<int, ConcurrentDictionary<int, ExtractedEventData>> LightEvents;
        
        private void InsertEventData(int lightType, CustomBasicBeatmapEventData eventData)
        {
            int lightId = -1;
            float[] firstColor = null, secondColor = null, nextSameTypeEventFirstColor = null;
            Functions easingType = Functions.easeLinear;
            ColorType colorType = ColorType.Default;
            float duration = 0;
            CustomBasicBeatmapEventData nextSameTypeEventData = null;
            int i;
            if (eventData.customData.ContainsKey("_color"))
            {
                colorType = ColorType.ChromaColor;
                firstColor = new float[4] { 0f, 0f, 0f, 1f };
                i = 0;
                foreach (var colorComponent in eventData.customData.Get<List<object>>("_color"))
                {
                    firstColor[i] = Convert.ToSingle(colorComponent);
                    i++;
                }
            }
            else if (eventData.customData.ContainsKey("_lightGradient"))
            {
                colorType = ColorType.ChromaLightGradient;
                var lightGradientObject = (CustomData)eventData.customData.Get<object>("_lightGradient");
                firstColor = new float[4] { 0f, 0f, 0f, 1f };
                secondColor = new float[4] { 0f, 0f, 0f, 1f };
                var firstColorObject = lightGradientObject.Get<List<object>>("_startColor");
                var secondColorObject = lightGradientObject.Get<List<object>>("_endColor");

                for (i = 0; i < firstColorObject.Count; i++)
                {
                    firstColor[i] = Convert.ToSingle(firstColorObject[i]);
                }
                for (i = 0; i < secondColorObject.Count; i++)
                {
                    secondColor[i] = Convert.ToSingle(secondColorObject[i]);
                }

                if (lightGradientObject.ContainsKey("_easing"))
                {
                    easingType = (Functions)Enum.Parse(typeof(Functions), lightGradientObject.Get<string>("_easing"));
                }
                if (lightGradientObject.ContainsKey("_duration"))
                {
                    duration = lightGradientObject.Get<float>("_duration");
                }
                firstColorObject = null;
                secondColorObject = null;
                lightGradientObject = null;
            }

            switch ((Effect)eventData.value)
            {
                case Effect.OnLeft:
                case Effect.OnRight:
                case Effect.TransitionLeft:
                case Effect.TransitionRight:
                case Effect.WhiteTransition:
                    {
                        nextSameTypeEventData = eventData.nextSameTypeEventData as CustomBasicBeatmapEventData;
                        if (nextSameTypeEventData == null || !(((Effect)nextSameTypeEventData.value) == Effect.TransitionRight || ((Effect)nextSameTypeEventData.value) == Effect.TransitionLeft || ((Effect)nextSameTypeEventData.value) == Effect.WhiteTransition))
                        {
                            break;
                        }
                        if (nextSameTypeEventData.customData.ContainsKey("_color"))
                        {
                            nextSameTypeEventFirstColor = new float[4] { 0f, 0f, 0f, 1f };
                            i = 0;
                            foreach (var colorComponent in nextSameTypeEventData.customData.Get<List<object>>("_color"))
                            {
                                firstColor[i] = Convert.ToSingle(colorComponent);
                                i++;
                            }
                        }
                        break;
                    }
            }

            if (eventData.customData.ContainsKey("_lightID"))
            {
                var lightIDObject = eventData.customData.Get<object>("_lightID");

                if (lightIDObject.GetType() == typeof(List<object>))
                {
                    foreach (var id in (lightIDObject as List<object>).Select(x => Convert.ToInt32(x)))
                    {
                        if (!LightEvents[lightType].ContainsKey(id))
                        {
                            LightEvents[lightType].TryAdd(id, new ExtractedEventData());
                        }
                        var _extractedEventData = new ExtractedEventData();
                        _extractedEventData.time = eventData.time;
                        _extractedEventData.bpmTime = Plugin.Instance.bpmTime;
                        _extractedEventData.baseBrightness = eventData.floatValue;
                        _extractedEventData.effectType = (Effect)eventData.value;
                        _extractedEventData.colorType = colorType;
                        _extractedEventData.gradientEasingType = easingType;
                        _extractedEventData.firstColor = firstColor;
                        _extractedEventData.secondColor = secondColor;
                        _extractedEventData.gradientDuration = duration;
                        _extractedEventData.isActive = true;
                        if (nextSameTypeEventData == null)
                        {
                            LightEvents[lightType][id] = _extractedEventData;
                            continue;
                        }
                        _extractedEventData.nextSameTypeEventTime = nextSameTypeEventData.time;
                        _extractedEventData.nextSameTypeEventEffectType = (Effect)nextSameTypeEventData.value;
                        _extractedEventData.nextSameTypeEventFirstColor = nextSameTypeEventFirstColor;
                        _extractedEventData.nextSameTypeEventBaseBrightness = nextSameTypeEventData.floatValue;
                        LightEvents[lightType][id] = _extractedEventData;
                    }
                    lightIDObject = null;

                    return;
                }
                if (lightIDObject.GetType() == typeof(Int64))
                {
                    lightId = Convert.ToInt32(lightIDObject);
                }
            }
            if (lightId == -1)
            {
                foreach (var id in LightEvents[lightType])
                {
                    id.Value.isActive = false;
                }
            }
            if (!LightEvents[lightType].ContainsKey(lightId))
            {
                LightEvents[lightType].TryAdd(lightId, null);
            }
            var extractedEventData = new ExtractedEventData();
            extractedEventData.time = eventData.time;
            extractedEventData.bpmTime = Plugin.Instance.bpmTime;
            extractedEventData.baseBrightness = eventData.floatValue;
            extractedEventData.effectType = (Effect)eventData.value;
            extractedEventData.colorType = colorType;
            extractedEventData.gradientEasingType = easingType;
            extractedEventData.firstColor = firstColor;
            extractedEventData.secondColor = secondColor;
            extractedEventData.gradientDuration = duration;
            extractedEventData.isActive = true;
            if (nextSameTypeEventData == null)
            {
                LightEvents[lightType][lightId] = extractedEventData;
                return;
            }
            extractedEventData.nextSameTypeEventTime = nextSameTypeEventData.time;
            extractedEventData.nextSameTypeEventEffectType = (Effect)nextSameTypeEventData.value;
            extractedEventData.nextSameTypeEventFirstColor = nextSameTypeEventFirstColor;
            extractedEventData.nextSameTypeEventBaseBrightness = nextSameTypeEventData.floatValue;
            LightEvents[lightType][lightId] = extractedEventData;
        }

        public void AddEvent(BasicBeatmapEventData eventData)
        {
            var customEventData = eventData as CustomBasicBeatmapEventData;

            if (!LightEvents.ContainsKey((int)customEventData.basicBeatmapEventType))
            {
                LightEvents.TryAdd((int)customEventData.basicBeatmapEventType, new ConcurrentDictionary<int, ExtractedEventData>());
            }

            InsertEventData((int)eventData.basicBeatmapEventType, customEventData);
        }

        private byte[] effectByteArray = new byte[128];
        private int effectBitPosition = 2;

        public void RunLEDEffect(byte effectId, uint[] effectParams)
        {
            effectByteArray[0] = 64;
            effectBitPosition = 2;
            UDPPacketSender.AppendByteArray(effectId, effectParams, ref effectByteArray, ref effectBitPosition);
            UDPPacketSender.SendBytes(effectByteArray, (int)Math.Ceiling(effectBitPosition / 8.0));
        }

        private byte[] byteArray = new byte[512];
        private int bitPosition = 2;

        public void ProcessLights()
        {
            try
            {
                bitPosition = 2;
                byteArray[0] = 0;

                foreach (var lightTypesDict in LightEvents)
                {
                    foreach (var lightIdData in lightTypesDict.Value)
                    {
                        if (!lightIdData.Value.isActive)
                        {
                            continue;
                        }

                        lightIdData.Value.CalculateColor();
                    }
                }


                foreach (var lightTypesDict in LightEvents)
                {
                    foreach (var lightIdData in lightTypesDict.Value)
                    {
                        if (lightIdData.Key != -1)
                        {
                            continue;
                        }
                        Plugin.Instance.lightGroupsProvider.AppendByteArrayWithLedIndexes(Plugin.Instance.environmentName, lightTypesDict.Key, lightIdData.Key, lightTypesDict.Value, lightIdData.Value.calculatedColor, ref byteArray, ref bitPosition);
                    }

                    foreach (var lightIdData in lightTypesDict.Value)
                    {
                        if (!lightIdData.Value.isActive || lightIdData.Key == -1)
                        {
                            continue;
                        }
                        Plugin.Instance.lightGroupsProvider.AppendByteArrayWithLedIndexes(Plugin.Instance.environmentName, lightTypesDict.Key, lightIdData.Key, lightTypesDict.Value, lightIdData.Value.calculatedColor, ref byteArray, ref bitPosition);
                    }
                }

                if (bitPosition != 2)
                {
                    try
                    {
                        UDPPacketSender.SendBytes(byteArray, (int)Math.Ceiling(bitPosition / 8.0));
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"error: {ex}");
            }
        }

        
    }
}
