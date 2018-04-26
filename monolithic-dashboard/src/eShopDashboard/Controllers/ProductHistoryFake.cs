using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Controllers
{
    public class ProductHistoryFake
    {
        public static string Value(int productId)
        {
            if (productId == 153)
            {
                return @"
[
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 1,
    ""units"": 50,
    ""avg"": 25,
    ""count"": 2,
    ""max"": 25,
    ""min"": 25,
    ""prev"": null,
    ""next"": 100
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 2,
    ""units"": 100,
    ""avg"": 33,
    ""count"": 3,
    ""max"": 50,
    ""min"": 25,
    ""prev"": 50,
    ""next"": 135
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 3,
    ""units"": 135,
    ""avg"": 27,
    ""count"": 5,
    ""max"": 35,
    ""min"": 25,
    ""prev"": 100,
    ""next"": 50
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 4,
    ""units"": 50,
    ""avg"": 25,
    ""count"": 2,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 135,
    ""next"": 50
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 5,
    ""units"": 50,
    ""avg"": 25,
    ""count"": 2,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 50,
    ""next"": 50
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 6,
    ""units"": 50,
    ""avg"": 25,
    ""count"": 2,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 50,
    ""next"": 50
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 8,
    ""units"": 50,
    ""avg"": 25,
    ""count"": 2,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 50,
    ""next"": 75
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 9,
    ""units"": 75,
    ""avg"": 25,
    ""count"": 3,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 50,
    ""next"": 25
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 10,
    ""units"": 25,
    ""avg"": 25,
    ""count"": 1,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 75,
    ""next"": 50
  },
  {
    ""productId"": 153,
    ""year"": 2017,
    ""month"": 11,
    ""units"": 50,
    ""avg"": 25,
    ""count"": 2,
    ""max"": 25,
    ""min"": 25,
    ""prev"": 25,
    ""next"": null
  }
]";
            }
            else
            {
                return @"
[
  {
    ""productId"": 109,
    ""year"": 2016,
    ""month"": 12,
    ""units"": 65,
    ""avg"": 16,
    ""count"": 4,
    ""max"": 32,
    ""min"": 1,
    ""prev"": null,
    ""next"": 32
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 1,
    ""units"": 32,
    ""avg"": 32,
    ""count"": 1,
    ""max"": 32,
    ""min"": 32,
    ""prev"": 65,
    ""next"": 16
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 2,
    ""units"": 16,
    ""avg"": 16,
    ""count"": 1,
    ""max"": 16,
    ""min"": 16,
    ""prev"": 32,
    ""next"": 49
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 3,
    ""units"": 49,
    ""avg"": 16,
    ""count"": 3,
    ""max"": 32,
    ""min"": 1,
    ""prev"": 16,
    ""next"": 35
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 4,
    ""units"": 35,
    ""avg"": 11,
    ""count"": 3,
    ""max"": 16,
    ""min"": 3,
    ""prev"": 49,
    ""next"": 96
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 5,
    ""units"": 96,
    ""avg"": 24,
    ""count"": 4,
    ""max"": 32,
    ""min"": 16,
    ""prev"": 35,
    ""next"": 96
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 6,
    ""units"": 96,
    ""avg"": 19,
    ""count"": 5,
    ""max"": 32,
    ""min"": 16,
    ""prev"": 96,
    ""next"": 53
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 7,
    ""units"": 53,
    ""avg"": 13,
    ""count"": 4,
    ""max"": 16,
    ""min"": 5,
    ""prev"": 96,
    ""next"": 498
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 8,
    ""units"": 498,
    ""avg"": 99,
    ""count"": 5,
    ""max"": 384,
    ""min"": 2,
    ""prev"": 53,
    ""next"": 79
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 9,
    ""units"": 79,
    ""avg"": 9,
    ""count"": 8,
    ""max"": 16,
    ""min"": 1,
    ""prev"": 498,
    ""next"": 112
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 10,
    ""units"": 112,
    ""avg"": 12,
    ""count"": 9,
    ""max"": 48,
    ""min"": 1,
    ""prev"": 79,
    ""next"": 101
  },
  {
    ""productId"": 109,
    ""year"": 2017,
    ""month"": 11,
    ""units"": 101,
    ""avg"": 14,
    ""count"": 7,
    ""max"": 32,
    ""min"": 2,
    ""prev"": 112,
    ""next"": null
  }
]";
            }
        }
    }
}
