using Xamarin.Forms;
using Xamarin.Forms.Vonage;
using Xamarin.Forms.Vonage.iOS.Service;
using Xamarin.Forms.Vonage.iOS;
using UIKit;
using Foundation;

[assembly: ExportRenderer(typeof(VonagePublisherView), typeof(VonagePublisherViewRenderer))]
namespace Xamarin.Forms.Vonage.iOS
{
    [Preserve(AllMembers = true)]
    public class VonagePublisherViewRenderer : VonageViewRenderer
    {
        public static void Preserve() { }

        protected override UIView GetNativeView() => PlatformVonageService.Instance.PublisherKit?.View;

        protected override void SubscribeResetControl() => PlatformVonageService.Instance.PublisherUpdated += ResetControl;

        protected override void UnsubscribeResetControl() => PlatformVonageService.Instance.PublisherUpdated -= ResetControl;
    }
}
