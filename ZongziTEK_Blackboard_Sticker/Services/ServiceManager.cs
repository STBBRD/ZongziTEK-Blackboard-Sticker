using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Threading;
using System.Threading.Tasks;
using ZongziTEK_Blackboard_Sticker.Interfaces;

namespace ZongziTEK_Blackboard_Sticker.Services
{
    public class ServiceManager
    {
        private readonly Dictionary<Type, IManagedService> _services = new();
        private CancellationTokenSource? _cancellationTokenSource = new();

        #region public methods
        public void RegisterService<T>() where T : IManagedService, new()
        {
            var serviceType = typeof(T);
            if (_services.ContainsKey(serviceType))
            {
                Console.WriteLine($"注册服务，但服务已存在，不再注册。服务名称：{serviceType}");
                return;
            }

            var service = new T();
            _services[serviceType] = service;
            Console.WriteLine($"注册服务。服务名称：{serviceType}");

            if (_cancellationTokenSource != null)
            {
                _ = StartService(service, _cancellationTokenSource.Token);
            }
        }

        public void RemoveService<T>() where T : IManagedService
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                _ = service.StopAsync();
                Console.WriteLine($"停止服务。服务名称：{serviceType}");
                _services.Remove(serviceType);
                Console.WriteLine($"移除服务。服务名称：{serviceType}");
            }
        }

        public async Task RemoveAllServicesAsync()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            foreach (var service in _services.Values)
            {
                await service.StopAsync();
            }

            _services.Clear();
        }

        public T GetService<T>() where T : IManagedService
        {
            var serviceType = typeof(T);

            if (_services.TryGetValue(serviceType, out var service))
            {
                return (T)service;
            }

            return default;
        }
        #endregion

        private async Task StartService(IManagedService service, CancellationToken cancellationToken)
        {
            try
            {
                _cancellationTokenSource = new();
                Console.WriteLine($"启动服务开始，服务名称：{service.GetType()}");
                await service.StartAsync(cancellationToken);
                Console.WriteLine($"服务启动完成。服务名称：{service.GetType()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"服务在启动时崩溃。服务名称：{service.GetType().FullName}，崩溃：\r\n{ex}\r\n--- 崩溃信息末尾 ---");
            }
        }
    }
}
