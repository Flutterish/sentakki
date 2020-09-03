using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public class TestSceneSlideNote : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override Ruleset CreateRuleset() => new SentakkiRuleset();

        private int depthIndex;

        public TestSceneSlideNote()
        {
            base.Content.Add(content = new SentakkiInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Single", () => testSingle(2000));
            AddStep("Hit Single", () => testSingle(2000, true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject) && (h as DrawableSentakkiHitObject).AllJudged == false));
        }

        private void testSingle(double duration, bool auto = false)
        {
            var slide = new Slide
            {
                IsBreak = true,
                SlideInfoList = new List<SentakkiSlideInfo>
                {
                    new SentakkiSlideInfo {
                        ID = 25,
                        Duration = 1000,
                    },
                    new SentakkiSlideInfo {
                        ID = 27,
                        Duration = 1500,
                    },
                    new SentakkiSlideInfo {
                        ID = 0,
                        Duration = 2000,
                    }
                },
                StartTime = Time.Current + 1000,
            };

            slide.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            DrawableSlide dSlide;

            Add(dSlide = new DrawableSlide(slide)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });

            foreach (DrawableSentakkiHitObject nested in dSlide.NestedHitObjects)
                nested.Auto = auto;
        }
    }
}