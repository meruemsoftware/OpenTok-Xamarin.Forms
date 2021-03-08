using Xamarin.Forms;
using Xamarin.Forms.Vonage;
using Xamarin.Forms.Vonage.iOS.Service;
using Xamarin.Forms.Vonage.iOS;
using UIKit;
using Foundation;
using System.ComponentModel;
using System.Linq;

[assembly: ExportRenderer(typeof(VonageSubscriberView), typeof(OpenTokSubscriberViewRenderer))]
namespace Xamarin.Forms.Vonage.iOS
{
    [Preserve(AllMembers = true)]
    public class OpenTokSubscriberViewRenderer : OpenTokViewRenderer
    {
        public static void Preserve() { }

        protected VonageSubscriberView OpenTokSubscriberView => OpenTokView as VonageSubscriberView;

        protected override UIView GetNativeView()
        {
            var streamId = OpenTokSubscriberView?.StreamId;
            var subscribers = PlatformOpenTokService.Instance.Subscribers;
            return (streamId != null
                ? subscribers.FirstOrDefault(x => x.Stream?.StreamId == streamId)
                : subscribers.FirstOrDefault())?.View;
        }

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.SubscriberUpdated -= ResetControl;

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            switch(e.PropertyName)
            {
                case nameof(OpenTokSubscriberView.StreamId):
                    ResetControl();
                    break;
            }
        }
    }
}
