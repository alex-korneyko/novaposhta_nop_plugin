﻿using System.Linq;
using Nop.Plugin.Shipping.NovaPoshta.Data;
using Nop.Plugin.Shipping.NovaPoshta.Domain;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Shipping.NovaPoshta.Services
{
    public class NovaPoshtaUpdateScheduledTask : IScheduleTask
    {
        private readonly INovaPoshtaRepository<NovaPoshtaSettlement> _settlementsRepository;
        private readonly INovaPoshtaRepository<NovaPoshtaWarehouse> _warehousesRepository;
        private readonly INovaPoshtaApiService _novaPoshtaApiService;
        private readonly ILogger _logger;

        public NovaPoshtaUpdateScheduledTask(
            INovaPoshtaRepository<NovaPoshtaSettlement> settlementsRepository,
            INovaPoshtaRepository<NovaPoshtaWarehouse> warehousesRepository,
            INovaPoshtaApiService novaPoshtaApiService,
            ILogger logger
        )
        {
            _settlementsRepository = settlementsRepository;
            _warehousesRepository = warehousesRepository;
            _novaPoshtaApiService = novaPoshtaApiService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            await _logger.InformationAsync("Start Nova Poshta plugin data update scheduled task");
            
            await UpdateSettlements();
            await UpdateWarehouses();
            
            await _logger.InformationAsync("End Nova Poshta plugin data update scheduled task");
        }

        private async Task UpdateSettlements()
        {
            var allSettlements = await _novaPoshtaApiService.GetAllSettlements();
            
            await _logger.InformationAsync($"Received from API-server {allSettlements.Count} settlement objects");

            if (allSettlements.Any())
            {
                var deleted = await _settlementsRepository.DeleteAsync(_ => true);
                await _logger.InformationAsync($"Deleted from database {deleted} settlement entities");
                
                await _settlementsRepository.InsertAsync(allSettlements);
            }
        }

        private async Task UpdateWarehouses()
        {
            var allWarehouses = await _novaPoshtaApiService.GetAllWarehouses();
            
            await _logger.InformationAsync($"Received from API-server {allWarehouses.Count} warehouse objects");

            if (allWarehouses.Any())
            {
                var deleted = await _warehousesRepository.DeleteAsync(_ => true);
                await _logger.InformationAsync($"Deleted from database {deleted} warehouse entities");
                
                await _warehousesRepository.InsertAsync(allWarehouses);
            }
        }
    }
}