using Xamarin.Forms;
using Xamarin.Forms.Vonage;
using Xamarin.Forms.Vonage.iOS.Service;
using Xamarin.Forms.Vonage.iOS;
using UIKit;
using Foundation;

[assembly: ExportRenderer(typeof(VonagePublisherView), typeof(OpenTokPublisherViewRenderer))]
namespace Xamarin.Forms.Vonage.iOS
{
    [Preserve(AllMembers = true)]
    public class OpenTokPublisherViewRenderer : OpenTokViewRenderer
    {
        public static void Preserve() { }

        protected override UIView GetNativeView() => PlatformOpenTokService.Instance.PublisherKit?.View;

        protected override void SubscribeResetControl() => PlatformOpenTokService.Instance.PublisherUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformOpenTokService.Instance.PublisherUpdated -= ResetControl;
    }
}
