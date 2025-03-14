using Heck.Animation;
using UnityEngine;

namespace BSLedShow.Lighting
{
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

    public class ExtractedEventData
    {
        public float time { get; set; }
        public float bpmTime { get; set; }
        public float baseBrightness { get; set; }
        private byte[] _calculatedColor;

        public byte[] calculatedColor
        {
            get { return _calculatedColor; }
            set { _calculatedColor = value; }
        }

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
                            ColorMathMethods.MultiplyARGBComponentsByAlpha(
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][0],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][1],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][2],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][3] *
                                this.baseBrightness, ref this.bufferFloats);
                            break;
                        }
                        case Effect.WhiteOn:
                        case Effect.WhiteBlink:
                        case Effect.WhiteFadeOut:
                        case Effect.WhiteTransition:
                        {
                            ColorMathMethods.MultiplyARGBComponentsByAlpha(
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][0],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][1],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][2],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][3] *
                                this.baseBrightness, ref this.bufferFloats);
                            break;
                        }
                        default:
                        {
                            ColorMathMethods.MultiplyARGBComponentsByAlpha(
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][0],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][1],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][2],
                                Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][3] * this.baseBrightness,
                                ref this.bufferFloats);
                            break;
                        }
                    }

                    break;
                }
                case ColorType.ChromaColor:
                {
                    ColorMathMethods.MultiplyARGBComponentsByAlpha(this.firstColor[0], this.firstColor[1],
                        this.firstColor[2], this.firstColor[3] * this.baseBrightness, ref this.bufferFloats);
                    break;
                }
                case ColorType.ChromaLightGradient:
                {
                    ColorMathMethods.LerpARGBColor(this.firstColor[0], this.firstColor[1], this.firstColor[2],
                        this.firstColor[3] * this.baseBrightness, this.secondColor[0], this.secondColor[1],
                        this.secondColor[2], this.secondColor[3] * this.baseBrightness,
                        Easings.Interpolate(
                            Mathf.Min((Plugin.Instance.bpmTime - this.bpmTime) / this.gradientDuration, 1f),
                            this.gradientEasingType), ref this.bufferFloats);
                    ColorMathMethods.MultiplyARGBComponentsByAlpha(this.bufferFloats[0], this.bufferFloats[1],
                        this.bufferFloats[2], this.bufferFloats[3], ref this.bufferFloats);
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
                    ColorMathMethods.MultiplyARGBComponentsByAlpha(this.bufferFloats[0], this.bufferFloats[1],
                        this.bufferFloats[2], this.bufferFloats[3], ref this.bufferFloats);
                    ColorMathMethods.LerpARGBColor(this.bufferFloats, this.secondColor,
                        Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / 0.8f, 1f),
                            Functions.easeOutQuad), ref this.bufferFloats);
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
                    ColorMathMethods.LerpARGBColor(this.bufferFloats, this.secondColor,
                        Easings.Interpolate(Mathf.Min((Plugin.Instance.time - this.time) / 0.9f, 1f),
                            Functions.easeOutCubic), ref this.bufferFloats);
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

                    if (this.nextSameTypeEventEffectType == Effect.TransitionRight ||
                        this.nextSameTypeEventEffectType == Effect.TransitionLeft ||
                        this.nextSameTypeEventEffectType == Effect.WhiteTransition)
                    {
                        if (this.nextSameTypeEventFirstColor == null)
                        {
                            if (this.nextSameTypeEventEffectType == Effect.TransitionRight)
                            {
                                ColorMathMethods.LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1],
                                    this.bufferFloats[2], this.bufferFloats[3],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][0],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][1],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][2],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 1][3] *
                                    nextSameTypeEventBaseBrightness,
                                    Easings.Interpolate(
                                        Mathf.Min(
                                            (Plugin.Instance.time - this.time) /
                                            (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear),
                                    ref this.bufferFloats);
                            }
                            else if (this.nextSameTypeEventEffectType == Effect.WhiteTransition)
                            {
                                ColorMathMethods.LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1],
                                    this.bufferFloats[2], this.bufferFloats[3],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][0],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][1],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][2],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset + 2][3] *
                                    nextSameTypeEventBaseBrightness,
                                    Easings.Interpolate(
                                        Mathf.Min(
                                            (Plugin.Instance.time - this.time) /
                                            (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear),
                                    ref this.bufferFloats);
                            }
                            else
                            {
                                ColorMathMethods.LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1],
                                    this.bufferFloats[2], this.bufferFloats[3],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][0],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][1],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][2],
                                    Plugin.Instance.envColors[Plugin.Instance.boostColorsOffset][3] *
                                    nextSameTypeEventBaseBrightness,
                                    Easings.Interpolate(
                                        Mathf.Min(
                                            (Plugin.Instance.time - this.time) /
                                            (this.nextSameTypeEventTime - this.time), 1f), Functions.easeLinear),
                                    ref this.bufferFloats);
                            }
                        }
                        else
                        {
                            ColorMathMethods.LerpARGBColor(this.bufferFloats[0], this.bufferFloats[1],
                                this.bufferFloats[2], this.bufferFloats[3], this.nextSameTypeEventFirstColor[0],
                                this.nextSameTypeEventFirstColor[1], this.nextSameTypeEventFirstColor[2],
                                this.nextSameTypeEventFirstColor[3] * nextSameTypeEventBaseBrightness,
                                Easings.Interpolate(
                                    Mathf.Min(
                                        (Plugin.Instance.time - this.time) / (this.nextSameTypeEventTime - this.time),
                                        1f), Functions.easeLinear), ref this.bufferFloats);
                        }
                    }

                    break;
                }
            }

            ColorMathMethods.GetRGBFromFloatARGBComponents(this.bufferFloats[0], this.bufferFloats[1],
                this.bufferFloats[2], this.bufferFloats[3], ref this._calculatedColor);
            return;
        }
    }
}