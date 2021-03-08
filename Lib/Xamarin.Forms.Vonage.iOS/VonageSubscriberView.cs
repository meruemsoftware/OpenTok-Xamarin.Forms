using Xamarin.Forms;
using Xamarin.Forms.Vonage;
using Xamarin.Forms.Vonage.iOS.Service;
using Xamarin.Forms.Vonage.iOS;
using UIKit;
using Foundation;
using System.ComponentModel;
using System.Linq;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Vonage.VonageSubscriberView), typeof(Xamarin.Forms.Vonage.iOS.VonageSubscriberView))]
namespace Xamarin.Forms.Vonage.iOS
{
    [Preserve(AllMembers = true)]
    public class VonageSubscriberView : VonageViewRenderer
    {
        public static void Preserve() { }

        protected Vonage.VonageSubscriberView OpenTokSubscriberView => VonageView as Vonage.VonageSubscriberView;

        protected override UIView GetNativeView()
        {
            var streamId = OpenTokSubscriberView?.StreamId;
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
            switch(e.PropertyName)
            {
                case nameof(OpenTokSubscriberView.StreamId):
                    ResetControl();
                    break;
            }
        }
    }
}
