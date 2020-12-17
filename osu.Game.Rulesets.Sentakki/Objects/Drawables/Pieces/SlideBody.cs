using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces
{
    public class SlideVisual : CompositeDrawable
    {
        // This will be proxied, so a must.
        public override bool RemoveWhenNotAlive => false;
        private SliderPath path;

        public SliderPath Path
        {
            get => path;
            set
            {
                if (path == value)
                    return;
                path = value;
                updateVisuals();
            }
        }

        private readonly Container<SlideSegment> segments;
        private readonly DrawablePool<SlideSegment> segmentPool;
        private readonly DrawablePool<SlideChevron> chevronPool;

        public SlideVisual()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AddRangeInternal(new Drawable[]{
                segmentPool = new DrawablePool<SlideSegment>(15),
                chevronPool = new DrawablePool<SlideChevron>(74),
                segments = new Container<SlideSegment>(),
            });
        }

        private double chevronInterval;
        private void updateVisuals()
        {
            foreach (var segment in segments)
                segment.ClearChevrons();
            segments.Clear(false);

            var distance = Path.Distance;
            int chevrons = (int)Math.Ceiling(distance / SlideBody.SLIDE_CHEVRON_DISTANCE);
            chevronInterval = 1.0 / chevrons;

            float? prevAngle = null;
            SlideSegment currentSegment = segmentPool.Get();

            // We add the chevrons starting from the last, so that earlier ones remain on top
            for (double i = chevrons - 1; i > 0; --i)
            {
                Vector2 prevPos = Path.PositionAt((i - 1) * chevronInterval);
                Vector2 currentPos = Path.PositionAt(i * chevronInterval);

                float angle = prevPos.GetDegreesFromPosition(currentPos);
                bool shouldHide = SentakkiExtensions.GetDeltaAngle(prevAngle ?? angle, angle) >= 89;
                prevAngle = angle;

                currentSegment.Add(chevronPool.Get().With(c =>
                {
                    c.Position = currentPos;
                    c.Rotation = angle;
                    c.Alpha = shouldHide ? 0 : 1;
                }));

                if (i % 5 == 0 && chevrons - 1 - i > 2)
                {
                    segments.Add(currentSegment);
                    currentSegment = segmentPool.Get();
                }
            }

            segments.Add(currentSegment);

            foreach ( var (a,b) in segments.Zip( (Parent as DrawableSlideBody).SlideNodes.Reverse(), (a,b) => (a,b) ) )
            {
                a.Apply( b, ( Parent as DrawableSlideBody ).SlideNodes );
            }
        }

        protected override void Update ()
        {
            base.Update();
            foreach ( var segment in segments )
            {
                segment.UpdateProgress();
            }
        }

        private class SlideSegment : PoolableDrawable
        {
            private DrawableSlideNode node;
            private DrawableSlideNode nextNode => nodes.FirstOrDefault( x => x.HitObject.StartTime > node.HitObject.StartTime );
            private DrawableSlideNode previousNode => nodes.LastOrDefault( x => x.HitObject.StartTime < node.HitObject.StartTime );
            private float duration => (float)(node.HitObject.StartTime - ( previousNode ?? node ).HitObject.StartTime);
            private IEnumerable<DrawableSlideNode> nodes;
            public void Apply ( DrawableSlideNode node, IEnumerable<DrawableSlideNode> nodes )
            {
                this.node = node;
                this.nodes = nodes;
            }

            public void ClearChevrons() => ClearInternal(false);
            public void Add(Drawable drawable) => AddInternal(drawable);
            public int ChevronCount => InternalChildren.Count;
            public void UpdateProgress ()
            {
                float chevronsPassed;
                if ( node.IsHit ) chevronsPassed = ChevronCount;
                else if ( ( previousNode ?? node ).IsHit ) chevronsPassed = (float)( ChevronCount * Math.Clamp(( Clock.CurrentTime - ( previousNode ?? node ).HitStateUpdateTime ) / duration, 0, 1) );
                else chevronsPassed = 0;

                var alphaLeft = chevronsPassed;
                foreach ( var chevron in InternalChildren.Reverse() )
                {
                    var given = Math.Min( alphaLeft, 1 );
                    alphaLeft -= given;

                    chevron.Alpha = 1 - given;
                }
            }
        }

        private class SlideChevron : PoolableDrawable
        {
            public SlideChevron()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get("slide")
                });
            }
        }
    }
}
