using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Contracts.Services
{
    public interface IEquipmentService
    {
        event Action? OnConnectedChanged;

        bool IsConnected { get; set; }

        bool Connect(bool reconnect = false);

        object GetData();

    }
}
