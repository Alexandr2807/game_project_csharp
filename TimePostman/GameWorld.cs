using System.Drawing;

namespace TimePostman
{
    public class GameWorld
    {
        private int statusMessageTimerMs;

        public GameMap Map { get; }
        public GameSession Session { get; }
        public PlayerState Player { get; }

        public bool GameStarted { get; private set; }
        public bool GamePaused { get; private set; }
        public int NearbyHouseIndex { get; private set; } = -1;
        public string StatusMessage { get; private set; } = "Курьер готов. Нажми Enter, чтобы начать смену.";

        public GameWorld(bool testMode)
        {
            Map = new GameMap();
            Session = new GameSession(testMode);
            Player = new PlayerState(Map.StartPlayerX, Map.StartPlayerY);
        }

        public void StartGame()
        {
            GameStarted = true;
            GamePaused = false;
            Player.Reset(Map.StartPlayerX, Map.StartPlayerY);
            NearbyHouseIndex = -1;
            StatusMessage = "Смена началась. Подходи к дому и нажимай E.";
            statusMessageTimerMs = 2200;
        }

        public void ResetGame()
        {
            Session.Reset();
            GameStarted = true;
            GamePaused = false;
            Player.Reset(Map.StartPlayerX, Map.StartPlayerY);
            NearbyHouseIndex = -1;
            StatusMessage = "Новая смена началась. Доставляй заказы.";
            statusMessageTimerMs = 2200;
        }

        public void TogglePause()
        {
            if (!GameStarted || Session.GameWon || Session.GameLost)
                return;

            GamePaused = !GamePaused;
            StatusMessage = GamePaused ? "Пауза" : "Игра продолжена";
            statusMessageTimerMs = GamePaused ? 0 : 1200;
        }

        public void Update(int deltaMs, int dx, int dy)
        {
            if (!GameStarted || GamePaused)
                return;

            TimePhase oldPhase = Session.CurrentPhase;
            bool oldWon = Session.GameWon;
            bool oldLost = Session.GameLost;

            if (!Session.GameWon && !Session.GameLost)
            {
                MovePlayer(dx, dy);
                NearbyHouseIndex = GetNearbyHouseIndex();
            }

            Session.AdvanceTime(deltaMs);

            if (!oldWon && Session.GameWon)
            {
                StatusMessage = "Все заказы доставлены. Смена закрыта успешно.";
                statusMessageTimerMs = 0;
            }

            if (!oldLost && Session.GameLost)
            {
                StatusMessage = "Смена закончилась. Не все заказы были закрыты.";
                statusMessageTimerMs = 0;
            }

            if (oldPhase != Session.CurrentPhase && !Session.GameWon && !Session.GameLost)
            {
                StatusMessage = "Время сменилось: " + Session.GetPhaseText(Session.CurrentPhase);
                statusMessageTimerMs = 2200;
            }

            if (statusMessageTimerMs > 0)
            {
                statusMessageTimerMs -= deltaMs;
                if (statusMessageTimerMs <= 0)
                {
                    statusMessageTimerMs = 0;
                    if (!Session.GameWon && !Session.GameLost && !GamePaused)
                        StatusMessage = "Подойди к дому и нажми E, чтобы передать заказ.";
                }
            }
        }

        public DeliveryAttemptResult TryInteract()
        {
            if (!GameStarted || GamePaused)
                return DeliveryAttemptResult.GameEnded;

            DeliveryAttemptResult result = Session.TryDeliver(NearbyHouseIndex);

            if (result == DeliveryAttemptResult.TooFar)
            {
                StatusMessage = "Слишком далеко. Подойди ближе к дому.";
                statusMessageTimerMs = 1800;
            }
            else if (result == DeliveryAttemptResult.NoOrder)
            {
                StatusMessage = "Для этого адреса активных заказов уже нет.";
                statusMessageTimerMs = 2200;
            }
            else if (result == DeliveryAttemptResult.WrongTime)
            {
                DeliveryOrder? order = Session.GetActiveOrderForHouse(NearbyHouseIndex);
                if (order != null)
                    StatusMessage = "Клиента нет дома. Доступно: " + Session.GetAllowedPhasesText(order);
                else
                    StatusMessage = "Сейчас заказ не принимается.";

                statusMessageTimerMs = 3000;
            }
            else if (result == DeliveryAttemptResult.Delivered)
            {
                StatusMessage = "Заказ доставлен: " + Map.HouseNames[NearbyHouseIndex];
                statusMessageTimerMs = 2200;

                if (Session.GameWon)
                {
                    StatusMessage = "Все заказы доставлены. Смена закрыта успешно.";
                    statusMessageTimerMs = 0;
                }
            }

            return result;
        }

        public HouseInteractionState GetHouseInteractionState(int houseIndex)
        {
            DeliveryOrder? activeOrder = Session.GetActiveOrderForHouse(houseIndex);
            if (activeOrder == null)
            {
                bool houseHasOrder = false;
                for (int i = 0; i < Session.Orders.Count; i++)
                {
                    if (Session.Orders[i].HouseIndex == houseIndex)
                    {
                        houseHasOrder = true;
                        break;
                    }
                }

                return houseHasOrder ? HouseInteractionState.Completed : HouseInteractionState.NoOrder;
            }

            return activeOrder.CanDeliverNow(Session.CurrentPhase)
                ? HouseInteractionState.CanDeliver
                : HouseInteractionState.NotAvailableNow;
        }

        public string GetNearbyHouseHintText()
        {
            if (!GameStarted || Session.GameWon || Session.GameLost || GamePaused)
                return string.Empty;

            if (NearbyHouseIndex < 0)
                return string.Empty;

            HouseInteractionState state = GetHouseInteractionState(NearbyHouseIndex);

            if (state == HouseInteractionState.Completed)
                return "Готово";
            if (state == HouseInteractionState.CanDeliver)
                return "E — передать заказ";
            if (state == HouseInteractionState.NotAvailableNow)
                return "Не сейчас";

            return string.Empty;
        }

        public Rectangle GetPlayerRectangle()
        {
            return new Rectangle(Player.X, Player.Y, Player.Width, Player.Height);
        }

        public bool IsInsideGame(Rectangle rect)
        {
            return rect.Left >= Map.GameArea.Left &&
                   rect.Right <= Map.GameArea.Right &&
                   rect.Top >= Map.GameArea.Top &&
                   rect.Bottom <= Map.GameArea.Bottom;
        }

        private void MovePlayer(int dx, int dy)
        {
            if (dx != 0)
            {
                Rectangle newPlayer = new Rectangle(Player.X + dx, Player.Y, Player.Width, Player.Height);
                if (IsInsideGame(newPlayer) && !TouchesHouse(newPlayer))
                    Player.X += dx;
            }

            if (dy != 0)
            {
                Rectangle newPlayer = new Rectangle(Player.X, Player.Y + dy, Player.Width, Player.Height);
                if (IsInsideGame(newPlayer) && !TouchesHouse(newPlayer))
                    Player.Y += dy;
            }
        }

        private bool TouchesHouse(Rectangle rect)
        {
            for (int i = 0; i < Map.Houses.Count; i++)
            {
                if (rect.IntersectsWith(Map.Houses[i]))
                    return true;
            }
            return false;
        }

        private int GetNearbyHouseIndex()
        {
            Rectangle playerRect = GetPlayerRectangle();

            for (int i = 0; i < Map.Houses.Count; i++)
            {
                Rectangle zone = Map.Houses[i];
                zone.Inflate(35, 35);
                if (zone.IntersectsWith(playerRect))
                    return i;
            }

            return -1;
        }
    }
}
