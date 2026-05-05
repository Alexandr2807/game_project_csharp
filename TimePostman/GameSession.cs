using System;
using System.Collections.Generic;

namespace TimePostman
{
    public class GameSession
    {
        public List<DeliveryOrder> Orders { get; private set; } = new List<DeliveryOrder>();

        public TimePhase CurrentPhase { get; private set; } = TimePhase.Morning;

        public int PhaseTimerMs { get; private set; }
        public int PhaseDurationMs { get; private set; } = 20000;

        public int MatchTimerMs { get; private set; }
        public int MatchDurationMs { get; private set; } = 300000;

        public bool GameWon { get; private set; }
        public bool GameLost { get; private set; }

        public bool TestMode { get; }

        public GameSession(bool testMode)
        {
            TestMode = testMode;

            if (TestMode)
            {
                PhaseDurationMs = 8000;
                MatchDurationMs = 90000;
            }

            Reset();
        }

        public void Reset()
        {
            CurrentPhase = TimePhase.Morning;
            PhaseTimerMs = 0;
            MatchTimerMs = 0;
            GameWon = false;
            GameLost = false;

            Orders.Clear();

            List<string> randomTitles = GetRandomOrderTitles(10);

            Orders.Add(new DeliveryOrder(randomTitles[0], 0, new List<TimePhase> { TimePhase.Morning }));
            Orders.Add(new DeliveryOrder(randomTitles[1], 1, new List<TimePhase> { TimePhase.Day }));
            Orders.Add(new DeliveryOrder(randomTitles[2], 2, new List<TimePhase> { TimePhase.Evening }));
            Orders.Add(new DeliveryOrder(randomTitles[3], 3, new List<TimePhase> { TimePhase.Night }));
            Orders.Add(new DeliveryOrder(randomTitles[4], 4, new List<TimePhase> { TimePhase.Day, TimePhase.Evening }));
            Orders.Add(new DeliveryOrder(randomTitles[5], 5, new List<TimePhase> { TimePhase.Morning, TimePhase.Night }));
            Orders.Add(new DeliveryOrder(randomTitles[6], 6, new List<TimePhase> { TimePhase.Morning }));
            Orders.Add(new DeliveryOrder(randomTitles[7], 7, new List<TimePhase> { TimePhase.Day }));
            Orders.Add(new DeliveryOrder(randomTitles[8], 8, new List<TimePhase> { TimePhase.Night }));
            Orders.Add(new DeliveryOrder(randomTitles[9], 9, new List<TimePhase> { TimePhase.Morning, TimePhase.Day }));
        }

        private List<string> GetRandomOrderTitles(int count)
        {
            List<string> pool = new List<string>
            {
                "Горячая еда",
                "Документы из офиса",
                "Пакет с продуктами",
                "Ночной заказ",
                "Подарочный пакет",
                "Срочная доставка",
                "Лекарства",
                "Небольшая техника",
                "Заказ из магазина",
                "Комплект одежды",
                "Книги",
                "Косметика",
                "Товары для дома",
                "Заказ из аптеки",
                "Электроника",
                "Пакет документов",
                "Кофе и десерты",
                "Подарок на праздник",
                "Набор продуктов",
                "Заказ из супермаркета"
            };

            List<string> result = new List<string>();
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                int index = random.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        public void AdvanceTime(int deltaMs)
        {
            if (GameWon || GameLost)
            {
                return;
            }

            PhaseTimerMs += deltaMs;
            while (PhaseTimerMs >= PhaseDurationMs)
            {
                PhaseTimerMs -= PhaseDurationMs;
                NextPhase();
            }

            MatchTimerMs += deltaMs;
            if (MatchTimerMs >= MatchDurationMs)
            {
                MatchTimerMs = MatchDurationMs;
                GameLost = true;
            }

            if (GetDeliveredCount() == Orders.Count)
            {
                GameWon = true;
            }
        }

        public DeliveryAttemptResult TryDeliver(int houseIndex)
        {
            if (GameWon || GameLost)
            {
                return DeliveryAttemptResult.GameEnded;
            }

            if (houseIndex < 0)
            {
                return DeliveryAttemptResult.TooFar;
            }

            DeliveryOrder? currentOrder = null;

            for (int i = 0; i < Orders.Count; i++)
            {
                if (Orders[i].HouseIndex == houseIndex && !Orders[i].Delivered)
                {
                    currentOrder = Orders[i];
                    break;
                }
            }

            if (currentOrder == null)
            {
                return DeliveryAttemptResult.NoOrder;
            }

            if (!currentOrder.CanDeliverNow(CurrentPhase))
            {
                return DeliveryAttemptResult.WrongTime;
            }

            currentOrder.Delivered = true;

            if (GetDeliveredCount() == Orders.Count)
            {
                GameWon = true;
            }

            return DeliveryAttemptResult.Delivered;
        }

        private void NextPhase()
        {
            if (CurrentPhase == TimePhase.Morning)
            {
                CurrentPhase = TimePhase.Day;
            }
            else if (CurrentPhase == TimePhase.Day)
            {
                CurrentPhase = TimePhase.Evening;
            }
            else if (CurrentPhase == TimePhase.Evening)
            {
                CurrentPhase = TimePhase.Night;
            }
            else
            {
                CurrentPhase = TimePhase.Morning;
            }
        }

        public string GetPhaseText(TimePhase phase)
        {
            if (phase == TimePhase.Morning) return "Утро";
            if (phase == TimePhase.Day) return "День";
            if (phase == TimePhase.Evening) return "Вечер";
            return "Ночь";
        }

        public string GetAllowedPhasesText(DeliveryOrder order)
        {
            string text = "";

            for (int i = 0; i < order.AllowedPhases.Count; i++)
            {
                if (i > 0)
                {
                    text += ", ";
                }

                text += GetPhaseText(order.AllowedPhases[i]);
            }

            return text;
        }

        public int GetDeliveredCount()
        {
            int count = 0;

            for (int i = 0; i < Orders.Count; i++)
            {
                if (Orders[i].Delivered)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetAvailableNowCount()
        {
            int count = 0;

            for (int i = 0; i < Orders.Count; i++)
            {
                if (!Orders[i].Delivered && Orders[i].CanDeliverNow(CurrentPhase))
                {
                    count++;
                }
            }

            return count;
        }

        public int GetMatchSecondsLeft()
        {
            int left = (MatchDurationMs - MatchTimerMs) / 1000;
            if (left < 0) left = 0;
            return left;
        }

        public int GetPhaseSecondsLeft()
        {
            int left = (PhaseDurationMs - PhaseTimerMs) / 1000;
            if (left < 0) left = 0;
            return left;
        }

        public int GetElapsedMatchSeconds()
        {
            return MatchTimerMs / 1000;
        }

        public int GetCompletionPercent()
        {
            if (Orders.Count == 0)
            {
                return 0;
            }

            return GetDeliveredCount() * 100 / Orders.Count;
        }
    }
}