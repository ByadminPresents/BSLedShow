using BSLedShow.Utils;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BSLedShow.Lighting
{
    public class LightsProcessor
    {
        public class ExtractedEventData
        {
            public float time { get; set; }
            public float bpmTime { get; set; }
            public float baseBrightness { get; set; }
            private byte[] _calculatedColor;
            public byte[] calculatedColor { get { return _calculatedColor; } set { _calculatedColor = value; } }
            public Effect effectType { get; set; }
            public ColorType colorType { get; set; }
            public Functions gradientEasingType { get; set; }
            public float[] firstColor { get; set; }
            public float[] secondColor { get; set; }
            public float gradientDuration { get; set; }
            public float[] nextSameTypeEventFirstColor { get; set; }
            public float nextSameTypeEventBaseBrightness { get; set; }
            public Effect nextSameTypeEventEffectType { get; set; }
            public float nextSameTypeEventTime { get; set; }
            public bool isActive { get; set; }
            private float[] bufferFloats;

            public ExtractedEventData()
            {
                nextSameTypeEventTime = -1;
                calculatedColor = new byte[3];
                bufferFloats = new float[4];
                isActive = true;
            }

            public void CalculateColor()
            {
                if (this.effectType == Effect.Off)
                {
                    calculatedColor[0] = 0;
                    calculatedColor[1] = 0;
                    calculatedColor[2] = 0;
                    return;
                }

                switch (this.colorType)
                {
                    case ColorType.Default:
                        {
                            switch (this.effectType)
                            {
                                case Effect.OnRight:
                                case Effect.BlinkRight:
                                case Effect.FadeOutRight:
                                case Effect.TransitionRight:
                                    {
                                        MultiplyARGBComponentsByAlpha(Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][0], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][1], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][2], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][3] * this.baseBrightness, ref this.bufferFloats);
                                        break;
                                    }
                                case Effect.WhiteOn:
                                case Effect.WhiteBlink:
                                case Effect.WhiteFadeOut:
                                case Effect.WhiteTransition:
                                    {
                                        MultiplyARGBComponentsByAlpha(Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][0], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][1], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][2], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][3] * this.baseBrightness, ref this.bufferFloats);
                                        break;
                                    }
                                default:
                                    {
                                        MultiplyARGBComponentsByAlpha(Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][0], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][1], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][2], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][3] * this.baseBrightness, ref this.bufferFloats);
                                        break;
                                    }
                            }
                            break;
                        }
                    case ColorType.ChromaColor:
                        {
                            MultiplyARGBComponentsByAlpha(this.firstColor[0], this.firstColor[1], this.firstColor[2], this.firstColor[3] * this.baseBrightness, ref this.bufferFloats);
                            break;
                        }
                    case ColorType.ChromaLightGradient:
                        {
                            LerpARGBColor(this.firstColor[0], this.firstColor[1], this.firstColor[2], this.firstColor[3] * this.baseBrightness, this.secondColor[0], this.secondColor[1], this.secondColor[2], this.secondColor[3] * this.baseBrightness, Easings.Interpolate(Mathf.Min((Plugin.Instance.bpmTime - this.bpmTime) / this.gradientDuration, 1f), this.gradientEasingType), ref this.bufferFloats);
                            MultiplyARGBComponentsByAlpha(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], ref this.bufferFloats);
                            break;
                        }

                }

                switch (this.effectType)
                {
                    case Effect.BlinkLeft:
                    case Effect.BlinkRight:
                    case Effect.WhiteBlink:
                        {
                            if (this.secondColor == null)
                            {
                                this.secondColor = new float[4];
                            }
                            this.secondColor[0] = this.secondColor[1] = this.secondColor[2] = this.secondColor[3] = 0f;
                            this.bufferFloats[3] = 1.39f;
                            MultiplyARGBComponentsByAlpha(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], ref this.bufferFloats);
                            LerpARGBColor(this.bufferFloats, this.secondColor, Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / 0.8f, 1f), Functions.easeOutQuad), ref this.bufferFloats);
                            break;
                        }
                    case Effect.FadeOutLeft:
                    case Effect.FadeOutRight:
                    case Effect.WhiteFadeOut:
                        {
                            if (this.secondColor == null)
                            {
                                this.secondColor = new float[4];
                            }
                            this.secondColor[0] = this.secondColor[1] = this.secondColor[2] = this.secondColor[3] = 0f;
                            LerpARGBColor(this.bufferFloats, this.secondColor, Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / 0.9f, 1f), Functions.easeOutCubic), ref this.bufferFloats);
                            break;
                        }
                    case Effect.OnLeft:
                    case Effect.OnRight:
                    case Effect.TransitionLeft:
                    case Effect.TransitionRight:
                    case Effect.WhiteTransition:
                        {
                            if (this.nextSameTypeEventTime == -1)
                            {
                                break;
                            }
                            if (this.nextSameTypeEventEffectType == Effect.TransitionRight || this.nextSameTypeEventEffectType == Effect.TransitionLeft || this.nextSameTypeEventEffectType == Effect.WhiteTransition)
                            {

                                if (this.nextSameTypeEventFirstColor == null)
                                {
                                    if (this.nextSameTypeEventEffectType == Effect.TransitionRight)
                                    {
                                        LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][0], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][1], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][2], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][3] * nextSameTypeEventBaseBrightness, Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear), ref this.bufferFloats);
                                    }
                                    else if (this.nextSameTypeEventEffectType == Effect.WhiteTransition)
                                    {
                                        LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][0], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][1], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][2], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][3] * nextSameTypeEventBaseBrightness, Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear), ref this.bufferFloats);
                                    }
                                    else
                                    {
                                        LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][0], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][1], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][2], Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][3] * nextSameTypeEventBaseBrightness, Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear), ref this.bufferFloats);
                                    }
                                }
                                else
                                {
                                    LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], this.nextSameTypeEventFirstColor[0], this.nextSameTypeEventFirstColor[1], this.nextSameTypeEventFirstColor[2], this.nextSameTypeEventFirstColor[3] * nextSameTypeEventBaseBrightness, Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear), ref this.bufferFloats);
                                }
                            }
                            break;
                        }
                }
                GetRGBFromFloatARGBComponents(this.bufferFloats[0], this.bufferFloats[1], this.bufferFloats[2], this.bufferFloats[3], ref this._calculatedColor);
                return;
            }
        }


        private const int BoostColorsEventTypeValue = 5;

        public enum Effect
        {
            Off = 0,
            OnRight = 1,
            OnLeft = 5,
            BlinkRight = 2,
            BlinkLeft = 6,
            FadeOutRight = 3,
            FadeOutLeft = 7,
            TransitionRight = 4,
            TransitionLeft = 8,
            WhiteOn = 9,
            WhiteBlink = 10,
            WhiteFadeOut = 11,
            WhiteTransition = 12,
        }

        public enum ColorType
        {
            Default = 0,
            ChromaColor = 1,
            ChromaLightGradient = 2
        }

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
            TCPPacketSender.AppendByteArray(effectId, effectParams, ref effectByteArray, ref effectBitPosition);
            TCPPacketSender.SendBytes(effectByteArray, (int)Math.Ceiling(effectBitPosition / 8.0));
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
                        TCPPacketSender.SendBytes(byteArray, (int)Math.Ceiling(bitPosition / 8.0));
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"error: {ex}");
            }
        }

        private static void LerpARGBColor(float[] a, float[] b, float t, ref float[] result)
        {
            result[0] = a[0] + (b[0] - a[0]) * t;
            result[1] = a[1] + (b[1] - a[1]) * t;
            result[2] = a[2] + (b[2] - a[2]) * t;
            result[3] = a[3] + (b[3] - a[3]) * t;
        }

        private static void LerpARGBColor(float r1, float g1, float b1, float a1, float r2, float g2, float b2, float a2, float t, ref float[] result)
        {
            result[0] = r1 + (r2 - r1) * t;
            result[1] = g1 + (g2 - g1) * t;
            result[2] = b1 + (b2 - b1) * t;
            result[3] = a1 + (a2 - a1) * t;
        }


        private static void GetRGBFromFloatARGBComponents(float r, float g, float b, float a, ref byte[] color)
        {
            r = r * a;
            g = g * a;
            b = b * a;
            float maxComponentValue = 0;
            if (1 > r && 1 > g && 1 > b)
            {
                maxComponentValue = 1;
            }
            else if (r > g && r > b)
            {
                maxComponentValue = r;
            }
            else if (g > r && g > b)
            {
                maxComponentValue = g;
            }
            else
            {
                maxComponentValue = b;
            }
            color[0] = (byte)(r / maxComponentValue * 255);
            color[1] = (byte)(g / maxComponentValue * 255);
            color[2] = (byte)(b / maxComponentValue * 255);
        }

        private static void MultiplyARGBComponentsByAlpha(float r, float g, float b, float a, ref float[] argbComponents)
        {
            argbComponents[0] = (float)Math.Pow(r * a, 2);
            argbComponents[1] = (float)Math.Pow(g * a, 2);
            argbComponents[2] = (float)Math.Pow(b * a, 2);
            float maxComponentValue = 0;
            if (1 > argbComponents[0] && 1 > argbComponents[1] && 1 > argbComponents[2])
            {
                maxComponentValue = 1;
            }
            else if (argbComponents[0] > argbComponents[1] && argbComponents[0] > argbComponents[2])
            {
                maxComponentValue = argbComponents[0];
            }
            else if (argbComponents[1] > argbComponents[0] && argbComponents[1] > argbComponents[2])
            {
                maxComponentValue = argbComponents[1];
            }
            else
            {
                maxComponentValue = argbComponents[2];
            }
            argbComponents[0] = argbComponents[0] / maxComponentValue;
            argbComponents[1] = argbComponents[1] / maxComponentValue;
            argbComponents[2] = argbComponents[2] / maxComponentValue;
            argbComponents[3] = 1f;
        }

        private float[] MultiplyARGBComponentsByAlpha(float[] argbColor)
        {
            double[] argbComponents = new double[3] { Math.Pow(argbColor[0] * argbColor[3], 2), Math.Pow(argbColor[1] * argbColor[3], 2), Math.Pow(argbColor[2] * argbColor[3], 2) };
            double maxComponentValue = Math.Max(argbComponents.Max(), 1d);
            for (int i = 0; i < argbComponents.Length; i++)
            {
                argbComponents[i] = (argbComponents[i] / maxComponentValue);
            }
            return new float[] { (float)argbComponents[0], (float)argbComponents[1], (float)argbComponents[2], 1f };
        }

        private byte[] GetRGBFromFloatRGBComponents(float[] rgbColor)
        {
            double[] argbComponents = new double[3] { Math.Pow(rgbColor[0], 2), Math.Pow(rgbColor[1], 2), Math.Pow(rgbColor[2], 2) };
            double maxComponentValue = Math.Max(argbComponents.Max(), 1d);
            byte[] rgb = new byte[3];
            for (int i = 0; i < argbComponents.Length; i++)
            {
                rgb[i] = (byte)(argbComponents[i] / maxComponentValue * 255);
            }
            return rgb;
        }
    }
}
