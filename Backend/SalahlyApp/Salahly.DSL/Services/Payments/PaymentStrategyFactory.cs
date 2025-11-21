using Microsoft.Extensions.DependencyInjection;
using Salahly.DSL.Interfaces.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Services.Payments
{
    public class PaymentStrategyFactory :IPaymentStrategyFactory 
    {
        private readonly IServiceProvider _provider;

        public PaymentStrategyFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IPaymentStrategy GetStrategy(string method)
            => method switch
            {
                "Card" => _provider.GetRequiredService<PaymobPaymentStrategy>(),
                "Wallet" => _provider.GetRequiredService<PaymobWalletPaymentStrategy>(),
                "Cash" => _provider.GetRequiredService<CashPaymentStrategy>(),
                _ => _provider.GetRequiredService<PaymobPaymentStrategy>()
            };
    }
}
