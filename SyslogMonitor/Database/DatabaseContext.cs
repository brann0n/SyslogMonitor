using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SyslogMonitor.ConnectionManager;

namespace SyslogMonitor.Database
{
    public sealed class DatabaseContext
    {
        private static readonly Lazy<SQLiteAsyncConnection> lazyInitilizer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection("./SyslogDB.db3", SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
        });

        private static SQLiteAsyncConnection Database => lazyInitilizer.Value;
        private static bool IsInitialized = false;

        /// <summary>
        /// This database class is scrambled together from a variety of different online sources, all code included was intentional and has purpose.
        /// </summary>
        public DatabaseContext()
        {
            InitAsync().FireAndForget(false);
        }

        /// <summary>
        /// Make sure all tables exist and create them if they dont.
        /// </summary>
        /// <returns></returns>
        public async Task InitAsync()
        {
            if (!IsInitialized)
            {
                if (!Database.TableMappings.Any(n => n.MappedType.Name == typeof(DbDevice).Name))
                {
                    await Database.CreateTableAsync<DbDevice>(CreateFlags.None).ConfigureAwait(false);
                }

                IsInitialized = true;
            }
        }

        public async Task AddDeviceAsync(DeviceInfo device)
        {
            if (await Database.Table<DbDevice>().Where(n => n.Id == device.DeviceId).CountAsync() < 1)
            {
                await Database.InsertAsync(new DbDevice() {Id = device.DeviceId, DeviceMac = device.DeviceMac, Connected = device.DeviceConnected, DeviceIp = device.DeviceIp, Name = device.DeviceName, LastUpdated = device.LastUpdate});
            }
        }

        public async Task UpdateDeviceAsync(DeviceInfo device)
        {
            var parsingDevice = new DbDevice() { Id = device.DeviceId, DeviceMac = device.DeviceMac, Connected = device.DeviceConnected, DeviceIp = device.DeviceIp, Name = device.DeviceName, LastUpdated = device.LastUpdate };
            if(await Database.Table<DbDevice>().Where(n => n.Id  == parsingDevice.Id).CountAsync() == 1)
            {
                await Database.UpdateAsync(parsingDevice);
            }
            else
            {
                await Database.InsertAsync(parsingDevice);
            }
        }

        public async Task<List<DeviceInfo>> GetDevicesAsync()
        {
            List<DbDevice> list = await Database.Table<DbDevice>().ToListAsync();
            return list.Select<DbDevice, DeviceInfo>(x => x).ToList();
        }
        
    }
}
