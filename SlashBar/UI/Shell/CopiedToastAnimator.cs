using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SlashBar.UI.Shell;

public static class CopiedToastAnimator {

    public static void Show(FrameworkElement toast, TranslateTransform slideTransform) {
        toast.BeginAnimation(UIElement.OpacityProperty, null);
        slideTransform.BeginAnimation(TranslateTransform.YProperty, null);

        var easeOut = new QuadraticEase { EasingMode = EasingMode.EaseOut };
        var easeIn = new QuadraticEase { EasingMode = EasingMode.EaseIn };

        var opacity = new DoubleAnimationUsingKeyFrames();
        opacity.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        opacity.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160))) {
            EasingFunction = easeOut
        });
        opacity.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
        opacity.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1220))) {
            EasingFunction = easeIn
        });

        var slide = new DoubleAnimationUsingKeyFrames();
        slide.KeyFrames.Add(new EasingDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        slide.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))) {
            EasingFunction = easeOut
        });
        slide.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000))));
        slide.KeyFrames.Add(new EasingDoubleKeyFrame(6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1220))) {
            EasingFunction = easeIn
        });

        toast.BeginAnimation(UIElement.OpacityProperty, opacity);
        slideTransform.BeginAnimation(TranslateTransform.YProperty, slide);
    }
}
