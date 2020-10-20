using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    public class Slide : SentakkiLanedHitObject, IHasDuration
    {
        public double Duration
        {
            get => SlideInfoList.Any() ? SlideInfoList.Max(s => s.Duration) : 0;
            set => throw new NotSupportedException();
        }

        public double EndTime => StartTime + Duration;

        protected override Color4 DefaultNoteColour => Color4.Aqua;

        public List<SentakkiSlideInfo> SlideInfoList = new List<SentakkiSlideInfo>();

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            AddNested(new Tap
            {
                LaneBindable = { BindTarget = LaneBindable },
                StartTime = StartTime,
                Samples = Samples,
                Break = Break
            });
            createSlideBodies();
        }

        private void createSlideBodies()
        {
            foreach (var SlideInfo in SlideInfoList)
            {
                AddNested(new SlideBody
                {
                    Lane = SlideInfo.SlidePath.EndLane + Lane,
                    StartTime = StartTime,
                    SlideInfo = SlideInfo
                });
            }
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
