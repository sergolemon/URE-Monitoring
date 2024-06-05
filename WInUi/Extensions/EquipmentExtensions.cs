using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using URE.Core.Models.Equipment;
using URE.Services.Equipment;
using URE.Contracts.Services;
using URE.Core.Contracts.Repositories;
using URE.Services.Equipment;

namespace URE.Extensions
{
    public class EquipmentCollection: Dictionary<EquipmentType, IEquipmentService> 
    {
        public void Connect(bool reconnect = false)
        {
            foreach (var equip in this)
            {
                equip.Value.Connect(reconnect);
            }
        }

        public void Connect(EquipmentType type, bool reconnect = false)
        {
            if(TryGetValue(type, out var equip))
            {
                equip.Connect(reconnect);
            }
        }

        public bool IsConnected(EquipmentType type)
        {
            bool connected = false;

            if (TryGetValue(type, out var equip))
            {
                connected = equip.IsConnected;
            }

            return connected;
        }

        public IEquipmentService GetByType(EquipmentType type)
        {
            if (TryGetValue(type, out var equip))
            {
                return equip;
            }

            return default(IEquipmentService);
        }
    }

    public static class EquipmentExtensions
    {
        public static IServiceCollection AddEquipment(this IServiceCollection services)
        {
            services.AddSingleton<DetectorsService>();
            services.AddSingleton<GpsService>();
            services.AddSingleton<MeteoService>();

            services.AddSingleton<EquipmentCollection>(sp =>
            {
                return new EquipmentCollection
                {
                    { EquipmentType.GM0 , sp.GetService<DetectorsService>() },
                    { EquipmentType.GPS , sp.GetService<GpsService>() },
                    { EquipmentType.METEO, sp.GetService<MeteoService>() }
                };
            });

            return services;
        }
    }
}
