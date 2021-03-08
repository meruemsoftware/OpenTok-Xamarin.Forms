using Xamarin.Forms;
using Xamarin.Forms.Vonage;
using Xamarin.Forms.Vonage.Android;
using Android.Content;
using AView = Android.Views.View;
using Xamarin.Forms.Vonage.Android.Service;
using Android.Runtime;
using System.ComponentModel;
using System.Linq;
using SystemIntPtr = System.IntPtr;
using AndroidRuntimeJniHandleOwnership = Android.Runtime.JniHandleOwnership;
using Plugin.CurrentActivity;

[assembly: ExportRenderer(typeof(VonageSubscriberView), typeof(VonageSubscriberViewRenderer))]
namespace Xamarin.Forms.Vonage.Android
{
    [Preserve(AllMembers = true)]
    public class VonageSubscriberViewRenderer : VonageViewRenderer
    {
        public VonageSubscriberViewRenderer(Context context) : base(context)
        {
        }

#pragma warning disable
        public VonageSubscriberViewRenderer(SystemIntPtr p0, AndroidRuntimeJniHandleOwnership p1) : this(CrossCurrentActivity.Current.Activity)
        {
        }
#pragma warning restore

        public static void Preserve() { }

        protected VonageSubscriberView VonageSubscriberView => VonageView as VonageSubscriberView;

        protected override AView GetNativeView()
        {
            var streamId = VonageSubscriberView?.StreamId;
            var subscribers = PlatformVonageService.Instance.Subscribers;
            return (streamId != null
                ? subscribers.FirstOrDefault(x => x.Stream?.StreamId == streamId)
                : subscribers.FirstOrDefault())?.View;
        }

        protected override void SubscribeResetControl() => PlatformVonageService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformVonageService.Instance.SubscriberUpdated -= ResetControl;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            switch (e.PropertyName)
            {
                case nameof(VonageSubscriberView.StreamId):
                    ResetControl();
                    break;
            }
        }
    }
}