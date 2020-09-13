﻿using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModHidden : ModHidden
    {
        public override string Description => @"Play with fading notes.";
        public override double ScoreMultiplier => 1.06;

        public override bool HasImplementation => false;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
            {
                d.IsHidden = true;
            }
        }
    }
}
