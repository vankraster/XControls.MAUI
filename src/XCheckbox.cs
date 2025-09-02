namespace XControls.Maui
{
    public class XCheckbox : GraphicsView
    {
        private readonly CheckBoxDrawable _drawable;

        public static readonly BindableProperty IsCheckedProperty =
            BindableProperty.Create(
                nameof(IsChecked),
                typeof(bool),
                typeof(XCheckbox),
                false,
                propertyChanged: OnIsCheckedChanged);

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public XCheckbox()
        {
            HeightRequest = 40;
            WidthRequest = 40;

            _drawable = new CheckBoxDrawable();
            Drawable = _drawable;

            var tap = new TapGestureRecognizer();
            tap.Tapped += OnTapped;
            GestureRecognizers.Add(tap);
        }

        private async void OnTapped(object sender, EventArgs e)
        {
            IsChecked = !IsChecked;

            // rulează un mic efect de puls la click
            for (int i = 0; i <= 10; i++)
            {
                _drawable.ClickAnimationProgress = 1f - i / 10f;
                Invalidate();
                await Task.Delay(15);
            }
        }

        private static void OnIsCheckedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var checkbox = (XCheckbox)bindable;
            checkbox._drawable.IsChecked = (bool)newValue;
            checkbox.Invalidate();
        }

        private class CheckBoxDrawable : IDrawable
        {
            public bool IsChecked { get; set; }
            public float ClickAnimationProgress { get; set; }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                // fundal alb
                canvas.FillColor = Colors.White;
                canvas.FillRectangle(dirtyRect);

                // border cu colțuri rotunjite
                if (IsChecked)
                    canvas.StrokeColor = Color.FromArgb("#4CD964");
                else
                    canvas.StrokeColor = Colors.Gray;
                canvas.StrokeSize = 2;
                float cornerRadius = 6f;
                canvas.DrawRoundedRectangle(5, 5, dirtyRect.Width - 10, dirtyRect.Height - 10, cornerRadius);


                // bifa verde
                if (IsChecked)
                {
                    canvas.StrokeColor = Color.FromArgb("#4CD964");
                    canvas.StrokeSize = 4;
                    canvas.StrokeLineCap = LineCap.Round; // face capetele rotunjite

                    var path = new PathF();
                    path.MoveTo(12, 22);  // punctul de start
                    path.LineTo(18, 28);  // colțul de jos
                    path.LineTo(30, 12);  // capătul de sus

                    canvas.DrawPath(path);
                }

                // highlight la click
                if (ClickAnimationProgress > 0)
                {
                    if (IsChecked)
                        canvas.FillColor = Color.FromArgb("#4CD964").WithAlpha(ClickAnimationProgress);
                    else
                        canvas.FillColor = Colors.Gray.WithAlpha(ClickAnimationProgress);
                    canvas.FillRoundedRectangle(5, 5, dirtyRect.Width - 10, dirtyRect.Height - 10, cornerRadius);
                }
            }
        }
    }
}
