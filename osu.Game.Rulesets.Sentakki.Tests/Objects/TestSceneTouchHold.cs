﻿using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Sentakki.Tests.Objects
{
    [TestFixture]
    public class TestSceneTouchHold : OsuTestScene
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneTouchHold()
        {
            base.Content.Add(content = new SentakkiInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Single", () => testSingle());
            AddStep("Hit Single", () => testSingle(true));
            AddUntilStep("Wait for object despawn", () => !Children.Any(h => (h is DrawableSentakkiHitObject) && (h as DrawableSentakkiHitObject).AllJudged == false));
        }

        private void testSingle(bool auto = false)
        {
            var circle = new TouchHold
            {
                StartTime = Time.Current + 1000,
                Duration = 5000,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            Add(new DrawableTouchHold(circle)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = depthIndex++,
                Auto = auto
            });
        }
    }
}
