namespace Xamarin.Forms.Vonage
{
    public sealed class VonageSubscriberView : VonageView
    {
        public static readonly BindableProperty StreamIdProperty = BindableProperty.Create(nameof(StreamId), typeof(string), typeof(VonageSubscriberView), null);

        public string StreamId
        {
            get => GetValue(StreamIdProperty) as string;
            set => SetValue(StreamIdProperty, value);
        }
    }
}