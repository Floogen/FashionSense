using FashionSense.Framework.Interfaces;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionSense.Framework.Managers
{
    class ApiManager
    {
        private IMonitor _monitor;
        private IContentPatcherApi _contentPatcherApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        internal bool HookIntoContentPatcher(IModHelper helper)
        {
            _contentPatcherApi = helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");

            if (_contentPatcherApi is null)
            {
                _monitor.Log("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
            return true;
        }

        public IContentPatcherApi GetContentPatcherApi()
        {
            return _contentPatcherApi;
        }
    }
}
