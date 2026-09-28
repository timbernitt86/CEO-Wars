using System;
using UnityEngine;

namespace CEOWars.Core
{
    [Serializable]
    public class GameState
    {
        public string companyName = "New Venture";
        public bool onboardingComplete = false;
        public double cash = 25000;
        public int gems = 120;
        public int level = 1;
        public double xp = 0;
        public int employees = 1;
        public int products = 0;
        public int officeLevel = 1;
        public double lifetimeRevenue = 0;
        public long lastSavedUtcTicks = 0;
        public long boostEndsUtcTicks = 0;

        public bool HasActiveBoost => boostEndsUtcTicks > DateTime.UtcNow.Ticks;

        public double XpForNextLevel => 100d * Math.Pow(level, 1.35d);

        public double BaseRevenuePerSecond
        {
            get
            {
                var employeeRevenue = employees * 2.5d;
                var productRevenue = products * 16d;
                var officeMultiplier = 1d + ((officeLevel - 1) * 0.20d);
                return (employeeRevenue + productRevenue) * officeMultiplier;
            }
        }

        public double RevenuePerSecond => BaseRevenuePerSecond * (HasActiveBoost ? 2d : 1d);

        public double HireCost => Math.Round(750d * Math.Pow(1.18d, Math.Max(0, employees - 1)));
        public double ProductCost => Math.Round(5000d * Math.Pow(1.42d, products));
        public double OfficeUpgradeCost => Math.Round(15000d * Math.Pow(1.7d, officeLevel - 1));
    }
}
