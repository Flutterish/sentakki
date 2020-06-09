using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using System.Diagnostics;
using System.Threading;

namespace osu.Game.Rulesets.Sentakki.Beatmaps
{
    public class SentakkiPatternGenerator
    {
        private readonly Random rng;
        public SentakkiPatternGenerator(IBeatmap beatmap)
        {
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            int seed = ((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20) + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            rng = new Random(seed);
        }


        //The patterns will generate the note path to be used based on the current offset
        // argument list is (offset, diff)
        private List<Func<int>> patternlist => new List<Func<int>>{
            //Stream pattern, path difference determined by offset2
            ()=> {
                offset+=offset2;
                return offset%8;
            },
        };
        private int currentPattern = 0;
        private int offset = 0;
        private int offset2 = 0;

        public void CreateNewPattern()
        {
            currentPattern = rng.Next(0, patternlist.Count); // Pick a pattern
            offset = rng.Next(0, 8); // Give it a random offset for variety
            offset2 = rng.Next(0, 7); // Give it a random offset for variety
        }
        public SentakkiHitObject GenerateNewNote(HitObject original)
        {
            int notePath = patternlist[currentPattern].Invoke();
            switch (original)
            {
                default:
                    return new Tap
                    {
                        NoteColor = Color4.Orange,
                        Angle = notePath.GetAngleFromPath(),
                        Samples = original.Samples,
                        StartTime = original.StartTime,
                        EndPosition = SentakkiExtensions.GetPosition(SentakkiPlayfield.INTERSECTDISTANCE, notePath),
                        Position = SentakkiExtensions.GetPosition(SentakkiPlayfield.NOTESTARTDISTANCE, notePath),
                    };
            }

        }
    }
}