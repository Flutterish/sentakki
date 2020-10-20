using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public class DrawableTouch : DrawableSentakkiHitObject
    {
        // IsHovered is used
        public override bool HandlePositionalInput => true;

        protected override double InitialLifetimeOffset => base.InitialLifetimeOffset + HitObject.HitWindows.WindowFor(HitResult.Meh) * 2 * GameplaySpeed;

        private readonly TouchBlob blob1;
        private readonly TouchBlob blob2;
        private readonly TouchBlob blob3;
        private readonly TouchBlob blob4;

        private readonly TouchFlashPiece flash;
        private readonly ExplodePiece explode;

        private readonly DotPiece dot;

        private SentakkiInputManager sentakkiActionInputManager;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= GetContainingInputManager() as SentakkiInputManager;

        public DrawableTouch(Touch hitObject) : base(hitObject)
        {
            AccentColour.BindTo(HitObject.ColourBindable);
            AccentColour.BindValueChanged(c => Colour = c.NewValue, true);
            Size = new Vector2(300);
            Position = hitObject.Position;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Alpha = 0;
            Scale = Vector2.Zero;

            AddRangeInternal(new Drawable[]{
                blob1 = new TouchBlob{
                    Position = new Vector2(40, 0)
                },
                blob2 = new TouchBlob{
                    Position = new Vector2(-40, 0)
                },
                blob3 = new TouchBlob{
                    Position = new Vector2(0, 40)
                },
                blob4 = new TouchBlob{
                    Position = new Vector2(0, -40)
                },
                dot = new DotPiece(),
                flash = new TouchFlashPiece{
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(80)
                },
                explode = new ExplodePiece{
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(80)
                },
            });

            trackedKeys.BindValueChanged(x =>
            {
                if (AllJudged)
                    return;

                UpdateResult(true);
            });
        }

        private BindableInt trackedKeys = new BindableInt(0);

        protected override void Update()
        {
            Position = HitObject.Position;
            base.Update();
            int count = 0;
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            if (touchInput.ActiveSources.Any())
                count = touchInput.ActiveSources.Where(x => ReceivePositionalInputAt(touchInput.GetTouchPosition(x) ?? new Vector2(float.MinValue))).Count();
            else if (IsHovered)
                count = SentakkiActionInputManager.PressedActions.Where(x => x < SentakkiAction.Key1).Count();

            trackedKeys.Value = count;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SentakkiRulesetConfigManager sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double fadeIn = AdjustedAnimationDuration;
            double moveTo = HitObject.HitWindows.WindowFor(HitResult.Meh) * 2 * GameplaySpeed;

            this.FadeIn(fadeIn, Easing.OutSine).ScaleTo(1, fadeIn, Easing.OutSine);

            using (BeginDelayedSequence(fadeIn, true))
            {
                blob1.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint);
                blob2.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint).Then().FadeOut();
                blob3.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint).Then().FadeOut();
                blob4.MoveTo(new Vector2(0), moveTo, Easing.InQuint).ScaleTo(1, moveTo, Easing.InQuint).Then().FadeOut();
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered || Auto)
            {
                if (Auto && timeOffset > 0)
                    ApplyResult(r => r.Type = HitResult.Great);

                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);

            if (result == HitResult.None)
                return;

            if (timeOffset < 0)
            {
                if (result <= HitResult.Miss) return;

                if (result < HitResult.Great) result = HitResult.Great;
            }

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);
            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    const double flash_in = 40;
                    const double flash_out = 100;

                    flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(flash_out);

                    dot.Delay(flash_in).FadeOut();

                    explode.FadeIn(flash_in);
                    this.ScaleTo(1.5f, 400, Easing.OutQuad);

                    this.Delay(time_fade_hit).FadeOut().Expire();

                    break;

                case ArmedState.Miss:
                    this.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .FadeOut(time_fade_miss);
                    break;
            }
        }
    }
}
