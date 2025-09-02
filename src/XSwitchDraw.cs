using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace XControls.Maui
{
    public class XSwitchDraw : GraphicsView
    {
        // Starea switch-ului
        private bool _isOn = false;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    AnimateKnob(value ? 1f : 0f);
                    Toggled?.Invoke(this, _isOn);
                }
            }
        }

        // Eveniment toggled
        public event EventHandler<bool> Toggled;

        // Poziția bulinei (0 = stânga, 1 = dreapta)
        private float _knobPos = 0f;
        private float _knobPosAtPanStart = 0f;

        public XSwitchDraw()
        {
            HeightRequest = 40;
            WidthRequest = 100;

            Drawable = new SwitchDrawable(() => _knobPos);

            // Tap
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => IsOn = !IsOn;
            GestureRecognizers.Add(tap);

            // Pan
            var pan = new PanGestureRecognizer { TouchPoints = 1 };
            pan.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(pan);

            SizeChanged += (s, e) =>
            {
                _knobPos = IsOn ? 1f : 0f;
                Invalidate();
            };
        }

        private void AnimateKnob(float targetPos)
        {
            var anim = new Animation(v =>
            {
                _knobPos = (float)Math.Clamp(v, 0f, 1f);
                Invalidate();
            }, _knobPos, targetPos);

            anim.Commit(this, "SwitchAnim", length: 200, easing: Easing.CubicInOut);
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _knobPosAtPanStart = _knobPos;
                    this.AbortAnimation("SwitchAnim");
                    break;

                case GestureStatus.Running:
                    float totalWidth = (float)(Width > 0 ? Width : WidthRequest);
                    float height = (float)(Height > 0 ? Height : HeightRequest);
                    if (totalWidth <= 0 || height <= 0) return;

                    float knobDiameter = height - 6f;
                    float availableMovement = totalWidth - knobDiameter - 6f;

                    float deltaProportion = (float)e.TotalX / availableMovement;
                    _knobPos = Math.Clamp(_knobPosAtPanStart + deltaProportion, 0f, 1f);
                    Invalidate();
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    // Forțăm knobPos între 0 și 1
                    _knobPos = Math.Clamp(_knobPos, 0f, 1f);

                    // Determinăm poziția finală
                    float targetPos = _knobPos >= 0.5f ? 1f : 0f;
                    bool shouldBeOn = targetPos == 1f;

                    // Animăm întotdeauna către targetPos
                    var anim = new Animation(v =>
                    {
                        _knobPos = (float)v;
                        Invalidate(); // redesenăm
                    }, _knobPos, targetPos);

                    anim.Commit(this, "SwitchAnim", length: 200, easing: Easing.CubicInOut);

                    // Actualizează starea
                    if (_isOn != shouldBeOn)
                    {
                        _isOn = shouldBeOn;
                        Toggled?.Invoke(this, _isOn);
                    }
                    break;

            }
        }

        private class SwitchDrawable : IDrawable
        {
            private readonly Func<float> _getKnobPos;

            public SwitchDrawable(Func<float> getKnobPos) => _getKnobPos = getKnobPos;

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                float width = dirtyRect.Width;
                float height = dirtyRect.Height;
                if (width <= 0 || height <= 0) return;

                float radius = height / 2f;
                float pos = _getKnobPos();

                // fundal gradient simplu
                Color offColor = Colors.LightGray;
                Color onColor = Color.FromArgb("#4CD964");
                Color bgColor = LerpColor(offColor, onColor, pos);

                canvas.FillColor = bgColor;
                canvas.FillRoundedRectangle(0, 0, width, height, radius);

                // bulina
                float circleDiameter = height - 6f;
                float minX = 3f;
                float maxX = width - circleDiameter - 3f;
                float circleX = minX + (maxX - minX) * pos;
                float circleY = 3f;

                canvas.SaveState();
                canvas.SetShadow(new SizeF(0, 2), 6, Colors.Black.WithAlpha(0.25f));

                canvas.FillColor = Colors.White;
                canvas.FillEllipse(circleX, circleY, circleDiameter, circleDiameter);

                canvas.RestoreState();

                canvas.StrokeColor = Colors.Black.WithAlpha(0.06f);
                canvas.StrokeSize = 1;
                canvas.DrawEllipse(circleX, circleY, circleDiameter, circleDiameter);
            }

            private static Color LerpColor(Color a, Color b, float t)
            {
                t = Math.Clamp(t, 0f, 1f);
                return Color.FromRgba(
                    a.Red + (b.Red - a.Red) * t,
                    a.Green + (b.Green - a.Green) * t,
                    a.Blue + (b.Blue - a.Blue) * t,
                    a.Alpha + (b.Alpha - a.Alpha) * t
                );
            }
        }
    }
}

 