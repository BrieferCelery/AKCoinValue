using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AKCoinValue
{
    public class CoinEntity : TableEntity
    {
        public double PriceUsd { get; set; }

        public string Symbol { get; set; }

        public DateTime TimeOfReading { get; set; }
    }
}
