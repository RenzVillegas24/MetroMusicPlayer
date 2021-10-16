﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace AnimatedScrollViewer
{
    [TemplatePart(Name = "PART_AniVerticalScrollBar", Type = typeof(ScrollBar))]
    [TemplatePart(Name = "PART_AniHorizontalScrollBar", Type = typeof(ScrollBar))]

    public class AnimatedScrollViewer : ScrollViewer
    {
        #region PART items
        ScrollBar _aniVerticalScrollBar;
        ScrollBar _aniHorizontalScrollBar;

        #endregion

        static AnimatedScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedScrollViewer), new FrameworkPropertyMetadata(typeof(AnimatedScrollViewer)));
        }

        #region ScrollViewer Override Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ScrollBar aniVScroll = base.GetTemplateChild("PART_AniVerticalScrollBar") as ScrollBar;
            if (aniVScroll != null)
            {
                _aniVerticalScrollBar = aniVScroll;
            }
            _aniVerticalScrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(VScrollBar_ValueChanged);

            ScrollBar aniHScroll = base.GetTemplateChild("PART_AniHorizontalScrollBar") as ScrollBar;
            if (aniHScroll != null)
            {
                _aniHorizontalScrollBar = aniHScroll;
            }
            _aniHorizontalScrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(HScrollBar_ValueChanged);

            this.PreviewMouseWheel += new MouseWheelEventHandler(CustomPreviewMouseWheel);
            this.PreviewKeyDown += new KeyEventHandler(AnimatedScrollViewer_PreviewKeyDown);
        }

        void AnimatedScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer)sender;

            if (thisScroller.CanKeyboardScroll)
            {
                Key keyPressed = e.Key;
                double newVerticalPos = thisScroller.TargetVerticalOffset;
                double newHorizontalPos = thisScroller.TargetHorizontalOffset;
                bool isKeyHandled = false;

                //Vertical Key Strokes code
                if (keyPressed == Key.Down)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos + 16.0), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.PageDown)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos + thisScroller.ViewportHeight), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.Up)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos - 16.0), Orientation.Vertical);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.PageUp)
                {
                    newVerticalPos = NormalizeScrollPos(thisScroller, (newVerticalPos - thisScroller.ViewportHeight), Orientation.Vertical);
                    isKeyHandled = true;
                }

                if (newVerticalPos != thisScroller.TargetVerticalOffset)
                {
                    thisScroller.TargetVerticalOffset = newVerticalPos;
                }

                //Horizontal Key Strokes Code

                if (keyPressed == Key.Right)
                {
                    newHorizontalPos = NormalizeScrollPos(thisScroller, (newHorizontalPos + 16), Orientation.Horizontal);
                    isKeyHandled = true;
                }
                else if (keyPressed == Key.Left)
                {
                    newHorizontalPos = NormalizeScrollPos(thisScroller, (newHorizontalPos - 16), Orientation.Horizontal);
                    isKeyHandled = true;
                }

                if (newHorizontalPos != thisScroller.TargetHorizontalOffset)
                {
                    thisScroller.TargetHorizontalOffset = newHorizontalPos;
                }

                e.Handled = isKeyHandled;
            }

        }

        private double NormalizeScrollPos(AnimatedScrollViewer thisScroll, double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > thisScroll.ScrollableHeight)
            {
                returnValue = thisScroll.ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > thisScroll.ScrollableWidth)
            {
                returnValue = thisScroll.ScrollableWidth;
            }

            return returnValue;
        }


        #endregion

        #region Custom Event Handlers

        void CustomPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double mouseWheelChange = (double)e.Delta;

            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer)sender;
            double newVOffset = thisScroller.TargetVerticalOffset - (mouseWheelChange / 3);
            if (newVOffset < 0)
            {
                thisScroller.TargetVerticalOffset = 0;
            }
            else if (newVOffset > thisScroller.ScrollableHeight)
            {
                thisScroller.TargetVerticalOffset = thisScroller.ScrollableHeight;
            }
            else
            {
                thisScroller.TargetVerticalOffset = newVOffset;
            }
            e.Handled = true;
        }

        void VScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnimatedScrollViewer thisScroller = this;
            double oldTargetVOffset = (double)e.OldValue;
            double newTargetVOffset = (double)e.NewValue;

            if (newTargetVOffset != thisScroller.TargetVerticalOffset)
            {
                double deltaVOffset = Math.Round((newTargetVOffset - oldTargetVOffset), 3);

                if (deltaVOffset == 1)
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset + thisScroller.ViewportHeight;

                }
                else if (deltaVOffset == -1)
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset - thisScroller.ViewportHeight;
                }
                else if (deltaVOffset == 0.1)
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset + 16.0;
                }
                else if (deltaVOffset == -0.1)
                {
                    thisScroller.TargetVerticalOffset = oldTargetVOffset - 16.0;
                }
                else
                {
                    thisScroller.TargetVerticalOffset = newTargetVOffset;
                }
            }
        }

        void HScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnimatedScrollViewer thisScroller = this;

            double oldTargetHOffset = (double)e.OldValue;
            double newTargetHOffset = (double)e.NewValue;

            if (newTargetHOffset != thisScroller.TargetHorizontalOffset)
            {

                double deltaVOffset = Math.Round((newTargetHOffset - oldTargetHOffset), 3);

                if (deltaVOffset == 1)
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset + thisScroller.ViewportWidth;

                }
                else if (deltaVOffset == -1)
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset - thisScroller.ViewportWidth;
                }
                else if (deltaVOffset == 0.1)
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset + 16.0;
                }
                else if (deltaVOffset == -0.1)
                {
                    thisScroller.TargetHorizontalOffset = oldTargetHOffset - 16.0;
                }
                else
                {
                    thisScroller.TargetHorizontalOffset = newTargetHOffset;
                }
            }
        }

        #endregion

        #region Custom Dependency Properties

        #region TargetVerticalOffset (DependencyProperty)(double)

        /// <summary>
        /// This is the VerticalOffset that we'd like to animate to
        /// </summary>
        public double TargetVerticalOffset
        {
            get { return (double)GetValue(TargetVerticalOffsetProperty); }
            set { SetValue(TargetVerticalOffsetProperty, value); }
        }
        public static readonly DependencyProperty TargetVerticalOffsetProperty =
            DependencyProperty.Register("TargetVerticalOffset", typeof(double), typeof(AnimatedScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnTargetVerticalOffsetChanged)));

        private static void OnTargetVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer)d;

            if ((double)e.NewValue != thisScroller._aniVerticalScrollBar.Value)
            {
                thisScroller._aniVerticalScrollBar.Value = (double)e.NewValue;
            }

            thisScroller.animateScroller(thisScroller);

        }

        #endregion

        #region TargetHorizontalOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the HorizontalOffset that we'll be animating to
        /// </summary>
        public double TargetHorizontalOffset
        {
            get { return (double)GetValue(TargetHorizontalOffsetProperty); }
            set { SetValue(TargetHorizontalOffsetProperty, value); }
        }
        public static readonly DependencyProperty TargetHorizontalOffsetProperty =
            DependencyProperty.Register("TargetHorizontalOffset", typeof(double), typeof(AnimatedScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnTargetHorizontalOffsetChanged)));

        private static void OnTargetHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisScroller = (AnimatedScrollViewer)d;

            if ((double)e.NewValue != thisScroller._aniHorizontalScrollBar.Value)
            {
                thisScroller._aniHorizontalScrollBar.Value = (double)e.NewValue;
            }

            thisScroller.animateScroller(thisScroller);
        }

        #endregion

        #region HorizontalScrollOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the actual horizontal offset property we're going use as an animation helper
        /// </summary>
        public double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            set { SetValue(HorizontalScrollOffsetProperty, value); }
        }
        public static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(AnimatedScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnHorizontalScrollOffsetChanged)));

        private static void OnHorizontalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisSViewer = (AnimatedScrollViewer)d;
            thisSViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }

        #endregion

        #region VerticalScrollOffset (DependencyProperty) (double)

        /// <summary>
        /// This is the actual VerticalOffset we're going to use as an animation helper
        /// </summary>
        public double VerticalScrollOffset
        {
            get { return (double)GetValue(VerticalScrollOffsetProperty); }
            set { SetValue(VerticalScrollOffsetProperty, value); }
        }
        public static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.Register("VerticalScrollOffset", typeof(double), typeof(AnimatedScrollViewer),
            new PropertyMetadata(0.0, new PropertyChangedCallback(OnVerticalScrollOffsetChanged)));

        private static void OnVerticalScrollOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AnimatedScrollViewer thisSViewer = (AnimatedScrollViewer)d;
            thisSViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        #endregion

        #region AnimationTime (DependencyProperty) (TimeSpan)

        /// <summary>
        /// A property for changing the time it takes to scroll to a new 
        ///     position. 
        /// </summary>
        public TimeSpan ScrollingTime
        {
            get { return (TimeSpan)GetValue(ScrollingTimeProperty); }
            set { SetValue(ScrollingTimeProperty, value); }
        }
        public static readonly DependencyProperty ScrollingTimeProperty =
            DependencyProperty.Register("ScrollingTime", typeof(TimeSpan), typeof(AnimatedScrollViewer),
              new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 350)));

        #endregion

        #region ScrollingSpline (DependencyProperty)

        /// <summary> 
        /// A property to allow users to describe a custom spline for the scrolling
        ///     animation.
        /// </summary>
        public KeySpline ScrollingSpline
        {
            get { return (KeySpline)GetValue(ScrollingSplineProperty); }
            set { SetValue(ScrollingSplineProperty, value); }
        }
        public static readonly DependencyProperty ScrollingSplineProperty =
            DependencyProperty.Register("ScrollingSpline", typeof(KeySpline), typeof(AnimatedScrollViewer),
              new PropertyMetadata(new KeySpline(0, 0, 0, 0.9)));

        #endregion

        #region CanKeyboardScroll (Dependency Property)

        public static readonly DependencyProperty CanKeyboardScrollProperty =
            DependencyProperty.Register("CanKeyboardScroll", typeof(bool), typeof(AnimatedScrollViewer),
                new FrameworkPropertyMetadata((bool)true));

        public bool CanKeyboardScroll
        {
            get { return (bool)GetValue(CanKeyboardScrollProperty); }
            set { SetValue(CanKeyboardScrollProperty, value); }
        }

        #endregion



        #endregion

        #region animateScroller method (Creates and runs animation)
        private void animateScroller(object objectToScroll)
        {
            AnimatedScrollViewer thisScrollViewer = objectToScroll as AnimatedScrollViewer;

            Duration targetTime = new Duration(thisScrollViewer.ScrollingTime);
            KeyTime targetKeyTime = thisScrollViewer.ScrollingTime;
            KeySpline targetKeySpline = thisScrollViewer.ScrollingSpline;

            DoubleAnimationUsingKeyFrames animateHScrollKeyFramed = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames animateVScrollKeyFramed = new DoubleAnimationUsingKeyFrames();

            SplineDoubleKeyFrame HScrollKey1 = new SplineDoubleKeyFrame(thisScrollViewer.TargetHorizontalOffset, targetKeyTime, targetKeySpline);
            SplineDoubleKeyFrame VScrollKey1 = new SplineDoubleKeyFrame(thisScrollViewer.TargetVerticalOffset, targetKeyTime, targetKeySpline);
            animateHScrollKeyFramed.KeyFrames.Add(HScrollKey1);
            animateVScrollKeyFramed.KeyFrames.Add(VScrollKey1);

            thisScrollViewer.BeginAnimation(HorizontalScrollOffsetProperty, animateHScrollKeyFramed);
            thisScrollViewer.BeginAnimation(VerticalScrollOffsetProperty, animateVScrollKeyFramed);

            CommandBindingCollection testCollection = thisScrollViewer.CommandBindings;
            int blah = testCollection.Count;

        }
        #endregion
    }
}
