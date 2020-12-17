using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableSlideBody : DrawableSentakkiLanedHitObject
    {
        public new SlideBody HitObject => (SlideBody)base.HitObject;

        public override bool RemoveWhenNotAlive => false;

        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset / 2;

        public Container<DrawableSlideNode> SlideNodes;

        public SlideVisual Slidepath;
        public StarPiece SlideStar;

        private float starProg;
        public float StarProgress
        {
            get => starProg;
            set
            {
                starProg = value;
                SlideStar.Position = Slidepath.Path.PositionAt(value);
                SlideStar.Rotation = Slidepath.Path.PositionAt(value - .001f).GetDegreesFromPosition(Slidepath.Path.PositionAt(value + .001f));
            }
        }

        public DrawableSlideBody() : this(null) { }
        public DrawableSlideBody(SlideBody hitObject)
            : base(hitObject) { }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Rotation = -22.5f;
            AddRangeInternal(new Drawable[]
            {
                Slidepath = new SlideVisual
                {
                    Alpha = 0,
                },
                new Container{
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = SlideStar = new StarPiece
                    {
                        Alpha = 0,
                        Scale = Vector2.Zero,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Position = SentakkiExtensions.GetCircularPosition(296.5f,22.5f),
                        RelativeSizeAxes  = Axes.None,
                        Size = new Vector2(75),
                    }
                },
                SlideNodes = new Container<DrawableSlideNode>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            });

            AccentColour.BindValueChanged(c => Colour = c.NewValue);
            OnNewResult += queueProgressUpdate;
            OnRevertResult += queueProgressUpdate;
        }

        private double progress;
        private double maxSlideDistance = 120;
        private double tickProgress = 0.001;
        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;
        protected override void OnApply()
        {
            base.OnApply();
            Slidepath.Path = HitObject.SlideInfo.SlidePath.Path;
            updatePathProgress();

            AccentColour.BindTo(ParentHitObject.AccentColour);
            progress = 0;
            pendingProgressUpdate = true;
        }

        protected override void OnFree()
        {
            base.OnFree();
            AccentColour.UnbindFrom(ParentHitObject.AccentColour);
        }

        // We want to ensure that the correct progress is visually shown on screen
        // I don't think that OnRevert of HitObjects is ordered properly
        // So just to make sure, when multiple OnReverts are called, we just queue for a forced update on the visuals
        // This makes sure that we always have the right visuals shown.
        private bool pendingProgressUpdate;

        private void queueProgressUpdate(DrawableHitObject hitObject, JudgementResult result)
        {
            pendingProgressUpdate = true;
        }

        protected override void Update()
        {
            base.Update();

            if ( Auto )
            {
                progress = ( Clock.CurrentTime - HitObject.StartTime - HitObject.ShootDelay ) / (HitObject.Duration - HitObject.ShootDelay );
                pendingProgressUpdate = true;
            }
            else
            {
                while ( progress < 1 && ( checkForTouchInput() || checkForMouseInput() ) )
                {
                    progress += tickProgress;
                    pendingProgressUpdate = true;
                }
            }

            if (pendingProgressUpdate)
                updatePathProgress();
        }

        private bool checkForMouseInput ()
            => ( SentakkiActionInputManager.CurrentState.Mouse.Position - Slidepath.ToScreenSpace( Slidepath.Path.PositionAt(progress) ) ).Length <= maxSlideDistance && SentakkiActionInputManager.PressedActions.Any();

        private bool checkForTouchInput ()
        {
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            foreach ( var t in touchInput.ActiveSources )
                if ( ( touchInput.GetTouchPosition(t).Value - Slidepath.ToScreenSpace( Slidepath.Path.PositionAt(progress) ) ).Length <= maxSlideDistance )
                    return true;

            return false;
        }

        // Used to hide and show segments accurately
        private void updatePathProgress()
        {
            Slidepath.Progress = (float)progress;

            pendingProgressUpdate = false;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            Slidepath.FadeInFromZero(AdjustedAnimationDuration / 2);
            using (BeginAbsoluteSequence(HitObject.StartTime - 50, true))
            {
                SlideStar.FadeInFromZero(100).ScaleTo(1, 100);
                this.Delay(100 + HitObject.ShootDelay).TransformTo(nameof(StarProgress), 1f, (HitObject as IHasDuration).Duration - 50 - HitObject.ShootDelay);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Player completed all nodes, we consider this user triggered
            if (SlideNodes.All(node => node.Result.HasResult))
                userTriggered = true;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                {
                    // Miss the last node to ensure that all of them have results
                    SlideNodes.Last().ForcefullyMiss();
                    if (SlideNodes.Count(node => !node.Result.IsHit) <= 2 && SlideNodes.Count > 2)
                        ApplyResult(r => r.Type = HitResult.Meh);
                    else
                        ApplyResult(r => r.Type = r.Judgement.MinResult);
                }

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                result = HitResult.Meh;

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_miss = 400 /* time_fade_miss = 400 */;
            switch (state)
            {
                case ArmedState.Hit:
                    SlideStar.FadeOut();
                    break;
                case ArmedState.Miss:
                    this.FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint).FadeOut(time_fade_miss).Expire();
                    break;
            }
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case SlideBody.SlideNode node:
                    return new DrawableSlideNode(node)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoBindable = { BindTarget = AutoBindable },
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            switch (hitObject)
            {
                case DrawableSlideNode node:
                    SlideNodes.Add(node);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            SlideNodes.Clear(false);
        }
    }
}
