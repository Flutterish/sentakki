﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Scoring;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Sentakki.Beatmaps;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Mods;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Sentakki.Scoring;
using osu.Game.Rulesets.Sentakki.Statistics;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Sentakki.Edit;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Rulesets.Sentakki
{
    public class SentakkiRuleset : Ruleset
    {
        public override string Description => "sentakki";
        public override string PlayingVerb => "Washing laundry";

        public override ScoreProcessor CreateScoreProcessor() => new SentakkiScoreProcessor();

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableSentakkiRuleset(this, beatmap, mods);

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new SentakkiBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) =>
            new SentakkiDifficultyCalculator(this, beatmap);

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new SentakkiReplayFrame();

        public override HitObjectComposer CreateHitObjectComposer() => new SentakkiHitObjectComposer(this);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new MultiMod(new SentakkiModHalfTime(), new SentakkiModDaycore()),
                        new SentakkiModNoFail(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new SentakkiModHardRock(),
                        new MultiMod(new SentakkiModSuddenDeath(), new SentakkiModPerfect()),
                        new MultiMod(new SentakkiModDoubleTime(), new SentakkiModNightcore()),
                        new MultiMod(new SentakkiModHidden(), new SentakkiModFadeIn()),
                    };

                case ModType.Automation:
                    return new Mod[]
                    {
                        new SentakkiModAutoplay(),
                        new SentakkiModRelax()
                    };

                case ModType.Fun:
                    return new Mod[]
                    {
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new SentakkiModSpin(),
                        new SentakkiModExperimental(),
                    };

                default:
                    return Array.Empty<Mod>();
            }
        }

        public override string ShortName => "Sentakki";

        public override RulesetSettingsSubsection CreateSettings() => new SentakkiSettingsSubsection(this);

        public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new SentakkiRulesetConfigManager(settings, RulesetInfo);

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, SentakkiAction.Button1),
            new KeyBinding(InputKey.X, SentakkiAction.Button2),
            new KeyBinding(InputKey.MouseLeft, SentakkiAction.Button1),
            new KeyBinding(InputKey.MouseRight, SentakkiAction.Button2),
        };

        public override StatisticRow[] CreateStatisticsForScore(ScoreInfo score, IBeatmap playableBeatmap) => new[]
        {
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem("Timing Distribution", new HitEventTimingDistributionGraph(score.HitEvents)
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 250
                    })
                }
            },
            new StatisticRow
            {
                Columns = new[]
                {
                    new StatisticItem("Judgement Distribution", new JudgementChart(score.HitEvents)
                    {
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(1, 250)
                    }),
                }
            }
        };

        public override Drawable CreateIcon() => new Sprite
        {
            Texture = new TextureStore(new TextureLoaderStore(CreateResourceStore()), false).Get("Textures/Icon2"),
        };
    }
}
