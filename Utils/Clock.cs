﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;

namespace Eclipse1807.BlishHUD.FishingBuddy.Utils
{
    // Based on https://github.com/manlaan/BlishHud-Clock/
    class Clock : Container
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(Clock));

        // TODO this time should be countdown til next
        //private CountdownTimer _countdownTimer;
        //public TimeSpan TimeTilNextPhase { get { return _countdownTimer.TimeLeft; } set { _countdownTimer.SetTime(value); this._countdownTimer.Start(); } }
        private string _timePhase = "";
        public string TimePhase
        {
            get { return this._timePhase; }
            set
            {
                if (!Equals(this.TimePhase, value))
                {
                    Logger.Debug($"Time of day changed {this.TimePhase} -> {value}");
                    OnTimeOfDayChanged(new ValueChangedEventArgs<string>(this.TimePhase, value));
                }
            }
        }
        // TODO default to false & properly show label BLOCKED on time til next phase
        public bool HideLabel = true;
        public bool Drag = false;
        // TODO deal with resizing label/font on resize based on time panel size
        public ContentService.FontSize Font_Size = ContentService.FontSize.Size14;
        public VerticalAlignment LabelAlign = VerticalAlignment.Top;

        private static BitmapFont _font;
        private Point _dragStart = Point.Zero;
        private bool _dragging;

        internal ClickThroughImage _dawn;
        internal ClickThroughImage _day;
        internal ClickThroughImage _dusk;
        internal ClickThroughImage _night;
        internal ClickThroughImage _currentTime;

        public event EventHandler<ValueChangedEventArgs<string>> TimeOfDayChanged;

        public Clock()
        {
            this.Location = new Point(50, 50);
            this.Size = new Point(0, 0);
            this.Visible = true;
            this.Padding = Thickness.Zero;
            //this._countdownTimer = new CountdownTimer();
            //this._countdownTimer.Start();
            // Problem, not fully up to date
            //this._countdownTimer.TimeChanged += () => _currentTime.BasicTooltipText = $"{TimePhase}\n{_countdownTimer.TimeLeftStr}";

            this._dawn = new ClickThroughImage
            {
                Parent = this,
                Texture = FishingBuddyModule._imgDawn,
                Size = new Point(FishingBuddyModule._timeOfDayImgSize.Value),
                Location = new Point(0),
                Opacity = 1.0f,
                BasicTooltipText = "Dawn",
                Visible = this.TimePhase == "Dawn",
                Capture = Drag
            };
            Resized += delegate { this._dawn.Size = new Point(this.Size.X); };

            this._day = new ClickThroughImage
            {
                Parent = this,
                Texture = FishingBuddyModule._imgDay,
                Size = new Point(FishingBuddyModule._timeOfDayImgSize.Value),
                Location = new Point(0),
                Opacity = 1.0f,
                BasicTooltipText = "Day",
                Visible = this.TimePhase == "Day",
                Capture = Drag
            };
            Resized += delegate { this._day.Size = new Point(this.Size.X); };

            this._dusk = new ClickThroughImage
            {
                Parent = this,
                Texture = FishingBuddyModule._imgDusk,
                Size = new Point(FishingBuddyModule._timeOfDayImgSize.Value),
                Location = new Point(0),
                Opacity = 1.0f,
                BasicTooltipText = "Dusk",
                Visible = this.TimePhase == "Dusk",
                Capture = Drag
            };
            Resized += delegate { this._dusk.Size = new Point(this.Size.X); };

            this._night = new ClickThroughImage
            {
                Parent = this,
                Texture = FishingBuddyModule._imgNight,
                Size = new Point(FishingBuddyModule._timeOfDayImgSize.Value),
                Location = new Point(0),
                Opacity = 1.0f,
                BasicTooltipText = "Night",
                Visible = this.TimePhase == "Night",
                Capture = Drag
            };
            Resized += delegate { this._night.Size = new Point(this.Size.X); };
            this._currentTime = this._day;
        }

        protected override CaptureType CapturesInput() => this.Drag ? CaptureType.Mouse : CaptureType.Filter;

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (this.Drag)
            {
                this._dragging = true;
                this._dragStart = Input.Mouse.Position;
            }
            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            if (this.Drag)
            {
                this._dragging = false;
                FishingBuddyModule._timeOfDayPanelLoc.Value = this.Location;
            }
            base.OnLeftMouseButtonReleased(e);
        }

        // TODO in bounds should probably add/subtract panel position
        private Boolean IsPointInBounds(Point point)
        {
            Point windowSize = GameService.Graphics.SpriteScreen.Size;

            return point.X > 0 &&
                    point.Y > 0 &&
                    point.X < windowSize.X &&
                    point.Y < windowSize.Y;
        }

        //TODO fix mouse, see: https://discord.com/channels/531175899588984842/534492173362528287/962805066673299457
        public override void UpdateContainer(GameTime gameTime)
        {
            if (this._dragging)
            {
                this._dawn.Capture = this.Drag;
                this._day.Capture = this.Drag;
                this._dusk.Capture = this.Drag;
                this._night.Capture = this.Drag;
                if (this.IsPointInBounds(Input.Mouse.Position))
                {
                    Point nOffset = Input.Mouse.Position - this._dragStart;
                    this.Location += nOffset;
                }
                else
                {
                    this._dragging = false;
                    FishingBuddyModule._timeOfDayPanelLoc.Value = this.Location;
                }
                this._dragStart = Input.Mouse.Position;
            }
            else
            {
                this._dawn.Capture = this.Drag;
                this._day.Capture = this.Drag;
                this._dusk.Capture = this.Drag;
                this._night.Capture = this.Drag;
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!this.HideLabel)
            {
                _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, this.Font_Size, ContentService.FontStyle.Regular);

                //TODO recalc size if needed taking > of size vs label size
                //TODO resize so that time shows above or below img
                //TODO this isn't working... also not working for bottom
                //this.Size = new Point(
                //    Math.Max((int)_font.MeasureString(TimeTilNextPhase.TimeLeftStr).Width, this.Size.X),
                //    (int)_font.MeasureString(TimeTilNextPhase.TimeLeftStr).Height + this.Size.Y
                //    );

                //spriteBatch.DrawStringOnCtrl(this,
                //    _countdownTimer.IsRunning ? _countdownTimer.TimeLeftStr : "",
                //    _font,
                //    new Rectangle(0, 0, this.Size.X, this.Size.Y),
                //    Color.White,
                //    false,
                //    true,
                //    1,
                //    HorizontalAlignment.Center,
                //    LabelAlign
                //    );
            }
        }

        protected virtual void OnTimeOfDayChanged(ValueChangedEventArgs<string> e)
        {
            this._timePhase = e.NewValue;
            switch (this.TimePhase)
            {
                case "Dawn":
                    this._currentTime.Visible = false;
                    this._currentTime = this._dawn;
                    this._currentTime.Visible = true;
                    break;
                case "Day":
                    this._currentTime.Visible = false;
                    this._currentTime = this._day;
                    this._currentTime.Visible = true;
                    break;
                case "Dusk":
                    this._currentTime.Visible = false;
                    this._currentTime = this._dusk;
                    this._currentTime.Visible = true;
                    break;
                case "Night":
                    this._currentTime.Visible = false;
                    this._currentTime = this._night;
                    this._currentTime.Visible = true;
                    break;
            }
            TimeOfDayChanged?.Invoke(this, e);
        }
    }
}
