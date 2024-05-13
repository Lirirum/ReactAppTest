using Microsoft.AspNetCore.SignalR;
using ReactApp1.Server.Services;

namespace ReactApp1.Server
{
    public class CurrencyHub : Hub
    {
        private readonly CurrencyService _currencyService;

        public CurrencyHub(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public async Task SubscribeToCurrencyUpdates(string baseCurrency, string targetCurrency, decimal amount)
        {
            while (true)
            {
                var exchangeRate = await _currencyService.ConvertCurrency(baseCurrency, targetCurrency, amount);
                await Clients.All.SendAsync("ReceiveCurrencyUpdate", exchangeRate);
                await Task.Delay(TimeSpan.FromSeconds(10)); 
            }
        }


    }

}
