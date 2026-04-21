using System.Collections.Generic;

namespace TimePostman
{
    public class GameSession
    {
        public List<DeliveryOrder> Orders { get; private set; } = new List<DeliveryOrder>();

        public TimePhase CurrentPhase { get; private set; } = TimePhase.Morning;

        public int PhaseTimerMs { get; private set; } = 0;
        public int PhaseDurationMs { get; private set; } = 20000;

        public int MatchTimerMs { get; private set; } = 0;
        public int MatchDurationMs { get; private set; } = 300000;

        public bool GameWon { get; private set; } = false;
        public bool GameLost { get; private set; } = false;

        public bool TestMode { get; private set; }

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

            Orders.Add(new DeliveryOrder("Заказ для Дома 1", 0, new List<TimePhase> { TimePhase.Morning }));
            Orders.Add(new DeliveryOrder("Заказ для Дома 2", 1, new List<TimePhase> { TimePhase.Day }));
            Orders.Add(new DeliveryOrder("Заказ для Дома 3", 2, new List<TimePhase> { TimePhase.Evening }));
            Orders.Add(new DeliveryOrder("Заказ для Дома 4", 3, new List<TimePhase> { TimePhase.Night }));
            Orders.Add(new DeliveryOrder("Заказ для Дома 5", 4, new List<TimePhase> { TimePhase.Day, TimePhase.Evening }));
            Orders.Add(new DeliveryOrder("Заказ для Дома 6", 5, new List<TimePhase> { TimePhase.Morning, TimePhase.Night }));
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
    }
}