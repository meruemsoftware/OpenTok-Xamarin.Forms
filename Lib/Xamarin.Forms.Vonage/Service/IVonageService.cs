using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Xamarin.Forms.Vonage.Service
{
    public interface IVonageService : INotifyPropertyChanged
    {
        event Action<string> Error;

        event Action<string> MessageReceived;

        event NotifyCollectionChangedEventHandler StreamIdCollectionChanged;

        ReadOnlyObservableCollection<string> StreamIdCollection { get; }

        VonagePermission Permissions { get; set; }

        VonagePublisherVideoType PublisherVideoType { get; set; }

        bool IsVideoPublishingEnabled { get; set; }

        bool IsAudioPublishingEnabled { get; set; }

        bool IsVideoSubscriptionEnabled { get; set; }

        bool IsAudioSubscriptionEnabled { get; set; }

        string ApiKey { get; set; }

        string SessionId { get; set; }

        string UserToken { get; set; }

        string PublisherName { get; set; }

        CameraResolution PublisherCameraResolution { get; set; }

        bool IsSessionStarted { get; set; }

        bool IsPublishingStarted { get; set; }

        bool IgnoreSentMessages { get; set; }

        bool CheckPermissions();

        bool TryStartSession();

        void EndSession();

        void CycleCamera();

        Task<bool> SendMessageAsync(string signalType, string message);
    }
}