using System;
using CEOWars.Core;
using CEOWars.UI;
using UnityEngine;

namespace CEOWars.Game
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }
        public GameState State { get; private set; }

        public event Action StateChanged;
        public event Action<string> ToastRequested;
        public event Action VisualsChanged;

        private float saveTimer;
        private float tickAccumulator;
        private bool initialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            State = SaveService.Load();
            ApplyOfflineIncome();
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;

            tickAccumulator += Time.unscaledDeltaTime;
            saveTimer += Time.unscaledDeltaTime;

            if (tickAccumulator >= 0.25f)
            {
                var seconds = tickAccumulator;
                tickAccumulator = 0;
                AddRevenue(State.RevenuePerSecond * seconds, false);
            }

            if (saveTimer >= 15f)
            {
                saveTimer = 0;
                SaveService.Save(State);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveService.Save(State);
        }

        private void OnApplicationQuit() => SaveService.Save(State);

        public void SetCompanyName(string name)
        {
            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.Length < 2) trimmed = "New Venture";
            State.companyName = trimmed.Length > 24 ? trimmed.Substring(0, 24) : trimmed;
            State.onboardingComplete = true;
            SaveService.Save(State);
            StateChanged?.Invoke();
        }

        public bool HireEmployee()
        {
            var cost = State.HireCost;
            if (!SpendCash(cost, "Not enough cash to hire.")) return false;
            State.employees++;
            AddXp(18);
            ToastRequested?.Invoke($"New employee hired for {Money(cost)}");
            VisualsChanged?.Invoke();
            StateChanged?.Invoke();
            return true;
        }

        public bool DevelopProduct()
        {
            var cost = State.ProductCost;
            if (!SpendCash(cost, "Not enough cash for R&D.")) return false;
            State.products++;
            AddXp(60);
            ToastRequested?.Invoke("Product launched! Recurring revenue increased.");
            StateChanged?.Invoke();
            return true;
        }

        public bool UpgradeOffice()
        {
            var cost = State.OfficeUpgradeCost;
            if (!SpendCash(cost, "Not enough cash for this office upgrade.")) return false;
            State.officeLevel++;
            AddXp(120);
            ToastRequested?.Invoke($"Office upgraded to level {State.officeLevel}.");
            VisualsChanged?.Invoke();
            StateChanged?.Invoke();
            return true;
        }

        public bool BuyExecutiveBoost()
        {
            const int cost = 20;
            if (State.gems < cost)
            {
                ToastRequested?.Invoke("You need 20 gems for the Executive Boost.");
                return false;
            }

            State.gems -= cost;
            var start = State.HasActiveBoost ? new DateTime(State.boostEndsUtcTicks, DateTimeKind.Utc) : DateTime.UtcNow;
            State.boostEndsUtcTicks = start.AddMinutes(10).Ticks;
            ToastRequested?.Invoke("Executive Boost active: x2 revenue for 10 minutes.");
            StateChanged?.Invoke();
            return true;
        }

        public bool BuyInstantCash()
        {
            const int cost = 50;
            if (State.gems < cost)
            {
                ToastRequested?.Invoke("You need 50 gems for Instant Cash.");
                return false;
            }

            State.gems -= cost;
            var payout = Math.Max(15000d, State.RevenuePerSecond * 3600d);
            State.cash += payout;
            ToastRequested?.Invoke($"Investor cash: +{Money(payout)}");
            StateChanged?.Invoke();
            return true;
        }

        public void GrantFounderPackForTesting()
        {
            State.gems += 250;
            State.cash += 50000;
            ToastRequested?.Invoke("DEV Founder Pack granted.");
            StateChanged?.Invoke();
        }

        public void ResetGame()
        {
            SaveService.Delete();
            State = new GameState();
            SaveService.Save(State);
            ToastRequested?.Invoke("Company reset.");
            VisualsChanged?.Invoke();
            StateChanged?.Invoke();
        }

        private bool SpendCash(double amount, string error)
        {
            if (State.cash + 0.001d < amount)
            {
                ToastRequested?.Invoke(error);
                return false;
            }
            State.cash -= amount;
            return true;
        }

        private void AddRevenue(double amount, bool notify)
        {
            if (amount <= 0) return;
            State.cash += amount;
            State.lifetimeRevenue += amount;
            State.xp += amount * 0.0025d;
            HandleLevelUps();
            StateChanged?.Invoke();
            if (notify) ToastRequested?.Invoke($"+{Money(amount)}");
        }

        private void AddXp(double amount)
        {
            State.xp += amount;
            HandleLevelUps();
        }

        private void HandleLevelUps()
        {
            while (State.xp >= State.XpForNextLevel)
            {
                State.xp -= State.XpForNextLevel;
                State.level++;
                State.gems += 10;
                ToastRequested?.Invoke($"CEO level {State.level}! +10 gems");
            }
        }

        private void ApplyOfflineIncome()
        {
            if (State.lastSavedUtcTicks <= 0) return;

            var last = new DateTime(State.lastSavedUtcTicks, DateTimeKind.Utc);
            var elapsed = DateTime.UtcNow - last;
            var cappedSeconds = Math.Min(Math.Max(0, elapsed.TotalSeconds), 8 * 60 * 60);
            if (cappedSeconds < 60) return;

            var earned = State.BaseRevenuePerSecond * cappedSeconds * 0.75d;
            State.cash += earned;
            State.lifetimeRevenue += earned;
            ToastRequestedLate($"While you were away: +{Money(earned)}");
        }

        private void ToastRequestedLate(string message)
        {
            StartCoroutine(LateToast(message));
        }

        private System.Collections.IEnumerator LateToast(string message)
        {
            yield return null;
            yield return null;
            ToastRequested?.Invoke(message);
            StateChanged?.Invoke();
        }

        public static string Money(double value)
        {
            if (value >= 1_000_000_000) return "$" + (value / 1_000_000_000d).ToString("0.##") + "B";
            if (value >= 1_000_000) return "$" + (value / 1_000_000d).ToString("0.##") + "M";
            if (value >= 1_000) return "$" + (value / 1_000d).ToString("0.##") + "K";
            return "$" + value.ToString("0");
        }
    }
}
