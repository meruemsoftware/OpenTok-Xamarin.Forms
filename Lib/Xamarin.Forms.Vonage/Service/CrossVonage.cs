using System;
using System.Threading;

namespace Xamarin.Forms.Vonage.Service
{
    public static class CrossVonage
    {
        private static bool _isInitialized;
        private static Lazy<IVonageService> _implementation;

        static CrossVonage()
        {
        }

        public static void Init(Func<IVonageService> creator)
        {
            if(_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            _implementation = new Lazy<IVonageService>(creator, LazyThreadSafetyMode.PublicationOnly);
        }

        public static IVonageService Current
        {
            get
            {
                var value = _implementation?.Value;
                if (value == null)
                {
                    throw new InvalidOperationException("You must call PlatformVonageService.Init() in platform specific code before using it.");
                }
                return value;
            }
        }
    }
}
