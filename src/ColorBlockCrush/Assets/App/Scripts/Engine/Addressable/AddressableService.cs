using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace App.Scripts.Engine.Addressable
{
    public sealed class AddressableService : IAddressableService, IDisposable
    {
        private readonly Dictionary<string, AsyncOperationHandle> _completedCache = new();
        private readonly Dictionary<string, List<AsyncOperationHandle>> _handles = new();
        
        public async UniTask<T> Load<T>(string address, CancellationToken ct = default) where T : class
        {
            if (_completedCache.TryGetValue(address, out AsyncOperationHandle completedHandle))
                return (T)completedHandle.Result;

            return await RunWithCacheOnComplete(Addressables.LoadAssetAsync<T>(address), address, ct);
        }

        public async UniTask<TAsset> Load<TAsset>(AssetReference assetReference, CancellationToken ct = default) where TAsset : class
        {
            string address = assetReference.AssetGUID;
            
            if (_completedCache.TryGetValue(address, out AsyncOperationHandle completedHandle))
                return (TAsset)completedHandle.Result;

            return await RunWithCacheOnComplete(Addressables.LoadAssetAsync<TAsset>(address), address, ct);
        }

        private async UniTask<T> RunWithCacheOnComplete<T>(AsyncOperationHandle<T> handle, string cacheKey, CancellationToken ct) where T : class
        {
            handle.Completed += completeHandle =>
            {
                if (completeHandle.Status == AsyncOperationStatus.Succeeded)
                    _completedCache[cacheKey] = completeHandle;
            };

            AddHandle(cacheKey, handle);

            return await handle.ToUniTask(cancellationToken: ct);
        }

        private void AddHandle<T>(string key, AsyncOperationHandle<T> handle) where T : class
        {
            if (!_handles.TryGetValue(key, out List<AsyncOperationHandle> resourceHandle))
            {
                resourceHandle = new List<AsyncOperationHandle>();
                _handles[key] = resourceHandle;
            }

            resourceHandle.Add(handle);
        }
        
        public void RemoveAllAssets()
        {
            foreach (AsyncOperationHandle handle in _handles.Values.SelectMany(resourceHandles => resourceHandles))
                Addressables.Release(handle);
      
            _completedCache.Clear();
            _handles.Clear();
        }

        public void RemoveAsset(string key)
        {
            if (_handles.TryGetValue(key, out List<AsyncOperationHandle> resourceHandles))
            {
                foreach (AsyncOperationHandle handle in resourceHandles)
                    Addressables.Release(handle);

                _handles.Remove(key);
            }

            _completedCache.Remove(key);
        }

        public void Dispose()
        {
            RemoveAllAssets();
        }
    }
}