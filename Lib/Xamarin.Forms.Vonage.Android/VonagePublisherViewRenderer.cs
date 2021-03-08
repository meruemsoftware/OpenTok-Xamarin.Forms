using Xamarin.Forms;
using Xamarin.Forms.Vonage;
using Xamarin.Forms.Vonage.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.Vonage.Android.Service;
using Android.Runtime;
using SystemIntPtr = System.IntPtr;
using AndroidRuntimeJniHandleOwnership = Android.Runtime.JniHandleOwnership;
using Plugin.CurrentActivity;

[assembly: ExportRenderer(typeof(VonagePublisherView), typeof(VonagePublisherViewRenderer))]
namespace Xamarin.Forms.Vonage.Android
{
    [Preserve(AllMembers = true)]
    public class VonagePublisherViewRenderer : VonageViewRenderer
    {
        public VonagePublisherViewRenderer(Context context) : base(context)
        {
        }

#pragma warning disable
        public VonagePublisherViewRenderer(SystemIntPtr p0, AndroidRuntimeJniHandleOwnership p1) : this(CrossCurrentActivity.Current.Activity)
        {
        }
#pragma warning restore

        public static void Preserve() { }

        protected override AView GetNativeView() => PlatformVonageService.Instance.PublisherKit?.View;

        protected override void SubscribeResetControl() => PlatformVonageService.Instance.PublisherUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformVonageService.Instance.PublisherUpdated -= ResetControl;
    }
}
