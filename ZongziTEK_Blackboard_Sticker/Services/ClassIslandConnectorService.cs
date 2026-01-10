using dotnetCampus.Ipc.CompilerServices.GeneratedProxies;
using dotnetCampus.Ipc.IpcRouteds.DirectRouteds;
using dotnetCampus.Ipc.Pipes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZongziTEK_Blackboard_Sticker.Interfaces;
using ZongziTEK_Blackboard_Sticker.Shared.IPC;

namespace ZongziTEK_Blackboard_Sticker.Services
{
    public class ClassIslandConnectorService : IManagedService
    {
        public bool IsTimetableSyncEnabled => _isTimetableSyncEnabled;
        public List<Lesson> TimetableShared => _timetableShared;

        private IpcProvider? _ipcProvider;
        private PeerProxy? _peerProxy;
        private IConnectService? _connectService;
        private JsonIpcDirectRoutedProvider? _ipcDirectRoutedProvider;

        private bool _isTimetableSyncEnabled;
        private List<Lesson> _timetableShared = new();

        private void RegisterNotificationHandlers()
        {
            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.TimetableUpdated",
                OnClassIslandTimetableUpdated);
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 订阅 ClassIsland 课程表变化事件");

            _ipcDirectRoutedProvider!.AddNotifyHandler(
                "ZongziTEK_Blackboard_Sticker_Connector.IsTimetableSyncEnabledChanged",
                OnIsTimetableSyncEnabledChanged);
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 订阅 IsTimetableSyncEnabledChanged 事件");
        }

        public async Task StartAsync(CancellationToken _)
        {
            _ipcProvider = new IpcProvider("ZongziTEK_Blackboard_Sticker");
            _ipcDirectRoutedProvider = new JsonIpcDirectRoutedProvider(_ipcProvider);

            // add notify handler
            RegisterNotificationHandlers();

            // connect
            _ipcDirectRoutedProvider.StartServer();
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 启动 IPC 服务器");

            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 开始连接 ClassIsland 插件");
            _peerProxy = await _ipcProvider.GetAndConnectToPeerAsync("ZongziTEK_Blackboard_Sticker_Connector");
            _connectService = _ipcProvider.CreateIpcProxy<IConnectService>(_peerProxy);
            Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} 连接到 ClassIsland 成功");

            // get initial value
            _isTimetableSyncEnabled = await _connectService.GetIsTimetableSyncEnabled();

            // call methods once
            UpdateMainWindowTimetable();
        }

        public async Task StopAsync(CancellationToken _)
        {
            if (_ipcDirectRoutedProvider != null)
            {
                _ipcDirectRoutedProvider.IpcProvider.Dispose();
                _ipcDirectRoutedProvider = null;
            }
        }

        private async void OnClassIslandTimetableUpdated()
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} ClassIsland 课程表变化");
            _timetableShared = await _connectService.GetCurrentTimetable();

            UpdateMainWindowTimetable();
        }

        private async void OnIsTimetableSyncEnabledChanged()
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} IsTimetableSyncEnabled 变化");
            _isTimetableSyncEnabled = await _connectService.GetIsTimetableSyncEnabled();

            UpdateMainWindowTimetable();
        }

        private void UpdateMainWindowTimetable()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = App.Current.MainWindow as MainWindow;
                mainWindow.LoadTimetableOrCurriculum();
                Console.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} 由 ClassIsland Connector 更新正在显示的课程表");
            });
        }
    }
}
