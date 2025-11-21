using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salahly.DSL.Interfaces.Payments
{
    public interface IPaymentStrategyFactory
    {
        IPaymentStrategy GetStrategy(string method);
    }
}
