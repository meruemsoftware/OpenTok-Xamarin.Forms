﻿using System;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms.Vonage.Service;
using AVFoundation;
using OpenTok;
using Foundation;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Xamarin.Forms.Vonage.iOS.Service
{
    [Preserve(AllMembers = true)]
    public sealed class PlatformVonageService : BaseVonageService
    {
        public event Action PublisherUpdated;
        public event Action SubscriberUpdated;

        private readonly object _sessionLocker = new object();
        private readonly ObservableCollection<string> _subscriberStreamIds = new ObservableCollection<string>();
        private readonly Collection<OTSubscriber> _subscribers = new Collection<OTSubscriber>();
        private static readonly Dictionary<CameraResolution, OTCameraCaptureResolution> cameraResolutions = new Dictionary<CameraResolution, OTCameraCaptureResolution>()
        {
            { CameraResolution.Low, OTCameraCaptureResolution.Low },
            { CameraResolution.Medium, OTCameraCaptureResolution.Medium },
            { CameraResolution.High, OTCameraCaptureResolution.High }
        };
        private PlatformVonageService()
        {
            _subscriberStreamIds.CollectionChanged += OnSubscriberStreamIdsCollectionChanged;
            PropertyChanged += OnPropertyChanged;
            StreamIdCollection = new ReadOnlyObservableCollection<string>(_subscriberStreamIds);
            Subscribers = new ReadOnlyCollection<OTSubscriber>(_subscribers);
        }

        public static PlatformVonageService Instance => CrossVonage.Current as PlatformVonageService;

        public override ReadOnlyObservableCollection<string> StreamIdCollection { get; }
        public ReadOnlyCollection<OTSubscriber> Subscribers { get; }
        public OTSession Session { get; private set; }
        public OTPublisher PublisherKit { get; private set; }

        public static void Init()
        {
            VonagePublisherViewRenderer.Preserve();
            VonageSubscriberView.Preserve();
            CrossVonage.Init(() => new PlatformVonageService());
        }

        public override bool TryStartSession()
        {
            lock (_sessionLocker)
            {
                if (!CheckPermissions() ||
                    string.IsNullOrWhiteSpace(ApiKey) ||
                    string.IsNullOrWhiteSpace(SessionId) ||
                    string.IsNullOrWhiteSpace(UserToken))
                {
                    return false;
                }

                EndSession();
                IsSessionStarted = true;

                Session = new OTSession(ApiKey, SessionId, null);
                Session.ConnectionDestroyed += OnConnectionDestroyed;
                Session.DidConnect += OnDidConnect;
                Session.StreamCreated += OnStreamCreated;
                Session.StreamDestroyed += OnStreamDestroyed;
                Session.DidFailWithError += OnError;
                Session.ReceivedSignalType += OnSignalReceived;
                Session.Init();
                Session.ConnectWithToken(UserToken, out OTError error);
                using (error)
                {
                    return error == null;
                }
            }
        }

        public override void EndSession()
        {
            lock (_sessionLocker)
            {
                try
                {
                    if (Session == null)
                    {
                        return;
                    }

                    foreach (var subscriberKit in _subscribers)
                    {
                        ClearSubscriber(subscriberKit);
                    }
                    _subscribers.Clear();
                    _subscriberStreamIds.Clear();

                    ClearPublisher();

                    RaisePublisherUpdated()
                        .RaiseSubscriberUpdated();

                    if (Session != null)
                    {
                        using (Session)
                        {
                            Session.ConnectionDestroyed -= OnConnectionDestroyed;
                            Session.DidConnect -= OnDidConnect;
                            Session.StreamCreated -= OnStreamCreated;
                            Session.StreamDestroyed -= OnStreamDestroyed;
                            Session.DidFailWithError -= OnError;
                            Session.ReceivedSignalType -= OnSignalReceived;
                            Session.Disconnect();
                        }
                        Session = null;
                    }
                }
                finally
                {
                    IsSessionStarted = false;
                    IsPublishingStarted = false;
                }
            }
        }

        public override bool CheckPermissions() => true;

        public override Task<bool> SendMessageAsync(string signalType, string message)
        {
            Session.SignalWithType(signalType, message, null, out OTError error);
            using (error)
            {
                return Task.FromResult(error == null);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsVideoPublishingEnabled):
                    UpdatePublisherProperty(p => p.PublishVideo = IsVideoPublishingEnabled);
                    return;
                case nameof(IsAudioPublishingEnabled):
                    UpdatePublisherProperty(p => p.PublishAudio = IsAudioPublishingEnabled);
                    return;
                case nameof(PublisherVideoType):
                    OnDidConnect(this, EventArgs.Empty);
                    return;
                case nameof(IsVideoSubscriptionEnabled):
                    UpdateSubscriberProperty(s => s.SubscribeToVideo = IsVideoSubscriptionEnabled);
                    return;
                case nameof(IsAudioSubscriptionEnabled):
                    UpdateSubscriberProperty(s => s.SubscribeToAudio = IsAudioSubscriptionEnabled);
                    return;
            }
        }

        private void UpdatePublisherProperty(Action<OTPublisher> updateAction)
        {
            if (PublisherKit == null)
            {
                return;
            }
            updateAction?.Invoke(PublisherKit);
        }

        private void UpdateSubscriberProperty(Action<OTSubscriber> updateAction)
        {
            foreach (var subscriberKit in _subscribers)
            {
                updateAction?.Invoke(subscriberKit);
            }
        }

        public override void CycleCamera()
        {
            if (PublisherKit == null)
            {
                return;
            }

            PublisherKit.CameraPosition = PublisherKit.CameraPosition == AVCaptureDevicePosition.Front
                ? AVCaptureDevicePosition.Back
                : AVCaptureDevicePosition.Front;
        }

        private void OnConnectionDestroyed(object sender, OTSessionDelegateConnectionEventArgs e)
            => RaiseSubscriberUpdated();

        private void OnDidConnect(object sender, EventArgs e)
        {
            if (Session == null)
            {
                return;
            }

            ClearPublisher();

            PublisherKit = new OTPublisher(null, new OTPublisherSettings
            {
                Name = PublisherName,
                CameraFrameRate = OTCameraCaptureFrameRate.OTCameraCaptureFrameRate15FPS,
                CameraResolution = cameraResolutions[PublisherCameraResolution],
                VideoTrack = Permissions.HasFlag(VonagePermission.Camera),
                AudioTrack = Permissions.HasFlag(VonagePermission.RecordAudio)
            })
            {
                PublishVideo = IsVideoPublishingEnabled,
                PublishAudio = IsAudioPublishingEnabled,
                AudioFallbackEnabled = PublisherVideoType == VonagePublisherVideoType.Camera,
                VideoType = PublisherVideoType == VonagePublisherVideoType.Camera
                    ? OTPublisherKitVideoType.Camera
                    : OTPublisherKitVideoType.Screen
            };
            PublisherKit.StreamCreated += OnPublisherStreamCreated;
            Session.Publish(PublisherKit);
            RaisePublisherUpdated();
        }

        private void OnStreamCreated(object sender, OTSessionDelegateStreamEventArgs e)
        {
            if (Session == null)
            {
                return;
            }

            DestroyStream(e.Stream?.StreamId);

            var subscriberKit = new OTSubscriber(e.Stream, null)
            {
                SubscribeToVideo = IsVideoSubscriptionEnabled,
                SubscribeToAudio = IsAudioSubscriptionEnabled
            };
            subscriberKit.DidConnectToStream += OnSubscriberConnected;
            subscriberKit.DidDisconnectFromStream += OnSubscriberDisconnected;
            subscriberKit.VideoDataReceived += OnSubscriberVideoDataReceived;
            subscriberKit.VideoEnabled += OnSubscriberVideoEnabled;
            subscriberKit.VideoDisabled += OnSubscriberVideoDisabled;

            Session.Subscribe(subscriberKit);
            var streamId = e.Stream.StreamId;
            _subscribers.Add(subscriberKit);
            _subscriberStreamIds.Add(streamId);
        }

        private void OnStreamDestroyed(object sender, OTSessionDelegateStreamEventArgs e)
            => DestroyStream(e.Stream?.StreamId);

        private void OnError(object sender, OTSessionDelegateErrorEventArgs e)
        {
            RaiseError(e.Error?.Code.ToString(CultureInfo.CurrentUICulture));
            EndSession();
        }

        private void OnSubscriberVideoDisabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
            => RaiseSubscriberUpdated();

        private void OnSubscriberVideoDataReceived(object sender, EventArgs e)
            => RaiseSubscriberUpdated();

        private void OnSubscriberVideoEnabled(object sender, OTSubscriberKitDelegateVideoEventReasonEventArgs e)
            => RaiseSubscriberUpdated();

        private void OnSubscriberConnected(object sender, EventArgs e)
            => RaisePublisherUpdated().RaiseSubscriberUpdated();

        private void OnSubscriberDisconnected(object sender, EventArgs e)
            => RaisePublisherUpdated().RaiseSubscriberUpdated();

        private void DestroyStream(string streamId)
        {
            var subscriberKit = _subscribers.FirstOrDefault(x => x.Stream?.StreamId == streamId);
            if (subscriberKit != null)
            {
                ClearSubscriber(subscriberKit);
                _subscribers.Remove(subscriberKit);
            }
            _subscriberStreamIds.Remove(streamId);
            RaiseSubscriberUpdated();
        }

        private PlatformVonageService RaiseSubscriberUpdated()
        {
            SubscriberUpdated?.Invoke();
            return this;
        }

        private PlatformVonageService RaisePublisherUpdated()
        {
            PublisherUpdated?.Invoke();
            return this;
        }

        private void OnPublisherStreamCreated(object sender, OTPublisherDelegateStreamEventArgs e)
            => IsPublishingStarted = true;

        private void OnSignalReceived(object sender, OTSessionDelegateSignalEventArgs e)
        {
            if (!(IgnoreSentMessages && e.Connection.ConnectionId == Session.Connection.ConnectionId))
            {
                RaiseMessageReceived(e.StringData);
            }
        }

        private void ClearSubscriber(OTSubscriber subscriberKit)
        {
            using (subscriberKit)
            {
                subscriberKit.SubscribeToAudio = false;
                subscriberKit.SubscribeToVideo = false;
                subscriberKit.DidConnectToStream -= OnSubscriberConnected;
                subscriberKit.DidDisconnectFromStream -= OnSubscriberDisconnected;
                subscriberKit.VideoDataReceived -= OnSubscriberVideoDataReceived;
                subscriberKit.VideoEnabled -= OnSubscriberVideoEnabled;
                subscriberKit.VideoDisabled -= OnSubscriberVideoDisabled;
                Session.Unsubscribe(subscriberKit);
            }
        }

        private void ClearPublisher()
        {
            if (PublisherKit == null)
            {
                return;
            }

            using (PublisherKit)
                {
                    PublisherKit.PublishAudio = false;
                    PublisherKit.PublishVideo = false;
                    PublisherKit.StreamCreated -= OnPublisherStreamCreated;
                    Session.Unpublish(PublisherKit);
                }
                PublisherKit = null;
        }
    }
}