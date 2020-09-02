using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Judgements;
using System;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class SlideBody : SentakkiHitObject, IHasDuration
    {
        public static readonly float SLIDE_CHEVRON_DISTANCE = 25;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration
        {
            get => SlideInfo.Duration;
            set => SlideInfo.Duration = value;
        }

        public SentakkiSlideInfo SlideInfo { get; set; }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            var distance = SlideInfo.SlidePath.Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / SlideBody.SLIDE_CHEVRON_DISTANCE);
            double chevronInterval = 1.0 / chevrons;

            for (int i = 5; i < chevrons - 2; i += 5)
            {
                var progress = i * chevronInterval;
                AddNested(new SlideNode
                {
                    Progress = (float)progress
                });
            }

            AddNested(new SlideNode
            {
                StartTime = EndTime,
                Progress = 1
            });
        }

        protected override HitWindows CreateHitWindows() => new SentakkiSlideHitWindows();
        public override Judgement CreateJudgement() => new SentakkiJudgement();

        public class SlideNode : SentakkiHitObject
        {
            public virtual float Progress { get; set; }

            public bool IsTailNote => Progress == 1;
            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
            public override Judgement CreateJudgement() => new IgnoreJudgement();
        }
    }
}
