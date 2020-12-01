using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogMonitor.Database
{
    public class DbDevice
    {
        [PrimaryKey, Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("mac"), NotNull]
        public string DeviceMac { get; set; }

        [Column("ip")]
        public string DeviceIp { get; set; }

        [Column("connected"), NotNull]
        public bool Connected { get; set; }

        [Column("lastupdated")]
        public DateTime LastUpdated { get; set; }
    }
}
