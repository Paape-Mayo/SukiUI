using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Skia;
using Avalonia.Styling;
using SkiaSharp;

namespace SukiUI.Utilities.Effects
{
    internal class EffectBackgroundDraw : EffectDrawBase
    {
        public static readonly object EnableTransitions = new(), DisableTransitions = new();
        internal static readonly object RefreshTheme = new(), ReleaseSoftwareFrame = new();
        
        internal bool TransitionsEnabled { get; set; }
        internal double TransitionTime { get; set; }

        private float TransitionSeconds => (float)CompositionNow.TotalSeconds;

        private SukiEffect? _oldEffect;
        private float _transitionStartTime;
        private float _transitionEndTime;
        private SKImage? _softwareFrame;
        private (int Width, int Height, SukiEffect? Effect, ThemeVariant Variant, SukiUI.Models.SukiColorTheme Theme) _softwareKey;

        public EffectBackgroundDraw() : base(false)
        {
            
        }
        
        protected override void EffectChanged(SukiEffect? oldValue, SukiEffect? newValue)
        {
            if (!TransitionsEnabled) return;
            if (oldValue is null || Equals(oldValue, newValue)) return;
            _oldEffect = oldValue;
            _transitionStartTime = TransitionSeconds;
            _transitionEndTime = TransitionSeconds + (float)Math.Max(0, TransitionTime);
        }

        public override void OnMessage(object message)
        {
            base.OnMessage(message);
            if (message == EnableTransitions) TransitionsEnabled = true;
            else if (message == DisableTransitions) TransitionsEnabled = false;
            if (message is double time) TransitionTime = time;
            if (message == ReleaseSoftwareFrame)
            {
                _softwareFrame?.Dispose();
                _softwareFrame = null;
                return;
            }
            // Property and theme changes must repaint even when animation is disabled.
            Invalidate();
        }

        protected override void Render(SKCanvas canvas, SKRect rect)
        {
            if (Effect is not null)
            {
                using var paint = new SKPaint();
                using var shader = EffectWithUniforms();
                paint.Shader = shader;
                canvas.DrawRect(rect, paint);
            }
            if (_oldEffect is not null)
            {
                using var paint = new SKPaint();
                // TODO: Investigate how to blend the shaders better - currently the only problem with this system.
                // Blend modes effect the transition quite heavily, only these 3 seem to work in any reasonable way.
                // paint.BlendMode = SKBlendMode.ColorBurn; // - Okay
                // paint.BlendMode = SKBlendMode.Overlay; // - Not Great
                paint.BlendMode = SKBlendMode.Darken; // - Best
                var lerped = InverseLerp(_transitionStartTime, _transitionEndTime, TransitionSeconds);
                using var shader = EffectWithUniforms(_oldEffect, (float)(1 - lerped));
                paint.Shader = shader;
                if (lerped < 1)
                {
                    canvas.DrawRect(rect, paint);
                    if(!AnimationEnabled) Invalidate();
                }
                else
                    _oldEffect = null;
            }
        }

        protected override void RenderSoftware(SKCanvas canvas, SKRect rect)
        {
            // An explicit request for the inexpensive, flat fallback is still respected.
            // Draw within this visual: Clear would erase siblings outside a floating host.
            if (ForceSoftwareRendering || Effect is null)
            {
                using var paint = new SKPaint { Color = ActiveTheme.BackgroundFor(ActiveVariant).ToSKColor() };
                canvas.DrawRect(rect, paint);
                return;
            }

            // Skia runtime shaders also work on raster surfaces. Cache a static gradient
            // so pointer movement and wiring updates do not rerun it for every frame.
            int width = Math.Max(1, (int)Math.Ceiling(rect.Width));
            int height = Math.Max(1, (int)Math.Ceiling(rect.Height));
            var key = (width, height, Effect, ActiveVariant, ActiveTheme);
            if (_softwareFrame is null || _softwareKey != key || AnimationEnabled || _oldEffect is not null)
            {
                using var raster = SKSurface.Create(new SKImageInfo(width, height));
                if (raster is null) return;
                raster.Canvas.Clear(SKColors.Transparent);
                Render(raster.Canvas, rect);
                _softwareFrame?.Dispose();
                _softwareFrame = raster.Snapshot();
                _softwareKey = key;
            }
            canvas.DrawImage(_softwareFrame, rect);
        }

        public override void Dispose()
        {
            _softwareFrame?.Dispose();
            _softwareFrame = null;
            base.Dispose();
        }

        private static double InverseLerp(double start, double end, double value) =>
            Math.Max(0, Math.Min(1, (value - start) / (end - start)));
    }
}
