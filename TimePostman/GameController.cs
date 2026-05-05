using System.Drawing;
using System.Windows.Forms;

namespace TimePostman
{
    public class GameController
    {
        private readonly GameSession session;
        private readonly GameMap map;

        private bool up;
        private bool down;
        private bool left;
        private bool right;

        private int statusMessageTimerMs;

        public bool GameStarted { get; private set; }
        public int PlayerX { get; private set; }
        public int PlayerY { get; private set; }
        public int PlayerWidth { get; } = 28;
        public int PlayerHeight { get; } = 28;
        public int PlayerSpeed { get; } = 4;

        public int CurrentNearbyHouseIndex { get; private set; } = -1;
        public string StatusMessage { get; private set; } = "Курьер готов. Нажми Enter, чтобы начать смену.";

        public GameController(GameSession session, GameMap map)
        {
            this.session = session;
            this.map = map;

            PlayerX = map.StartPlayerX;
            PlayerY = map.StartPlayerY;
        }

        public void HandleKeyDown(Keys key)
        {
            if (!GameStarted && key == Keys.Enter)
            {
                StartGame();
                return;
            }

            if (key == Keys.W || key == Keys.Up) up = true;
            if (key == Keys.S || key == Keys.Down) down = true;
            if (key == Keys.A || key == Keys.Left) left = true;
            if (key == Keys.D || key == Keys.Right) right = true;

            if (key == Keys.E)
            {
                TryInteract();
            }

            if (key == Keys.R)
            {
                ResetGame();
            }
        }

        public void HandleKeyUp(Keys key)
        {
            if (key == Keys.W || key == Keys.Up) up = false;
            if (key == Keys.S || key == Keys.Down) down = false;
            if (key == Keys.A || key == Keys.Left) left = false;
            if (key == Keys.D || key == Keys.Right) right = false;
        }

        public void Update(int deltaMs)
        {
            if (!GameStarted)
            {
                return;
            }

            TimePhase oldPhase = session.CurrentPhase;
            bool oldWon = session.GameWon;
            bool oldLost = session.GameLost;

            if (!session.GameWon && !session.GameLost)
            {
                int dx = 0;
                int dy = 0;

                if (up) dy -= PlayerSpeed;
                if (down) dy += PlayerSpeed;
                if (left) dx -= PlayerSpeed;
                if (right) dx += PlayerSpeed;

                MovePlayer(dx, dy);
                CurrentNearbyHouseIndex = GetNearbyHouseIndex();
            }

            session.AdvanceTime(deltaMs);

            if (!oldWon && session.GameWon)
            {
                StatusMessage = "Все заказы доставлены. Смена закрыта успешно.";
                statusMessageTimerMs = 0;
            }

            if (!oldLost && session.GameLost)
            {
                StatusMessage = "Смена закончилась. Не все заказы были закрыты.";
                statusMessageTimerMs = 0;
            }

            if (oldPhase != session.CurrentPhase && !session.GameWon && !session.GameLost)
            {
                StatusMessage = "Время сменилось: " + session.GetPhaseText(session.CurrentPhase);
                statusMessageTimerMs = 2200;
            }

            if (statusMessageTimerMs > 0)
            {
                statusMessageTimerMs -= deltaMs;

                if (statusMessageTimerMs <= 0)
                {
                    statusMessageTimerMs = 0;

                    if (!session.GameWon && !session.GameLost)
                    {
                        StatusMessage = "Подойди к дому и нажми E, чтобы передать заказ.";
                    }
                }
            }
        }

        public string GetInteractionHintText()
        {
            if (!GameStarted || session.GameWon || session.GameLost)
            {
                return "";
            }

            if (CurrentNearbyHouseIndex < 0)
            {
                return "";
            }

            DeliveryOrder? order = GetNearbyOrder();

            if (order == null)
            {
                return "Здесь уже нет активных заказов.";
            }

            if (order.CanDeliverNow(session.CurrentPhase))
            {
                return "Нажми E, чтобы передать заказ.";
            }

            return "Клиент принимает: " + session.GetAllowedPhasesText(order);
        }

        public void StartGame()
        {
            GameStarted = true;
            PlayerX = map.StartPlayerX;
            PlayerY = map.StartPlayerY;
            CurrentNearbyHouseIndex = -1;
            StatusMessage = "Смена началась. Подходи к дому и нажимай E.";
            statusMessageTimerMs = 2200;
        }

        public void ResetGame()
        {
            up = false;
            down = false;
            left = false;
            right = false;

            GameStarted = true;
            PlayerX = map.StartPlayerX;
            PlayerY = map.StartPlayerY;
            CurrentNearbyHouseIndex = -1;

            session.Reset();

            StatusMessage = "Новая смена началась. Доставляй заказы.";
            statusMessageTimerMs = 2200;
        }

        private void TryInteract()
        {
            if (!GameStarted)
            {
                return;
            }

            DeliveryAttemptResult result = session.TryDeliver(CurrentNearbyHouseIndex);

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
                DeliveryOrder? order = GetNearbyOrder();

                if (order != null)
                {
                    StatusMessage = "Клиента нет дома. Доступно: " + session.GetAllowedPhasesText(order);
                }
                else
                {
                    StatusMessage = "Сейчас заказ не принимается.";
                }

                statusMessageTimerMs = 3000;
            }
            else if (result == DeliveryAttemptResult.Delivered)
            {
                StatusMessage = "Заказ доставлен: " + map.HouseNames[CurrentNearbyHouseIndex];
                statusMessageTimerMs = 2200;

                if (session.GameWon)
                {
                    StatusMessage = "Все заказы доставлены. Смена закрыта успешно.";
                    statusMessageTimerMs = 0;
                }
            }
        }

        private DeliveryOrder? GetNearbyOrder()
        {
            if (CurrentNearbyHouseIndex < 0)
            {
                return null;
            }

            for (int i = 0; i < session.Orders.Count; i++)
            {
                if (session.Orders[i].HouseIndex == CurrentNearbyHouseIndex && !session.Orders[i].Delivered)
                {
                    return session.Orders[i];
                }
            }

            return null;
        }

        private void MovePlayer(int dx, int dy)
        {
            if (dx != 0)
            {
                Rectangle newPlayer = new Rectangle(PlayerX + dx, PlayerY, PlayerWidth, PlayerHeight);

                if (InsideGame(newPlayer) && !TouchHouse(newPlayer))
                {
                    PlayerX += dx;
                }
            }

            if (dy != 0)
            {
                Rectangle newPlayer = new Rectangle(PlayerX, PlayerY + dy, PlayerWidth, PlayerHeight);

                if (InsideGame(newPlayer) && !TouchHouse(newPlayer))
                {
                    PlayerY += dy;
                }
            }
        }

        private bool InsideGame(Rectangle rect)
        {
            if (rect.Left < map.GameArea.Left) return false;
            if (rect.Right > map.GameArea.Right) return false;
            if (rect.Top < map.GameArea.Top) return false;
            if (rect.Bottom > map.GameArea.Bottom) return false;

            return true;
        }

        private bool TouchHouse(Rectangle rect)
        {
            for (int i = 0; i < map.Houses.Count; i++)
            {
                if (rect.IntersectsWith(map.Houses[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetNearbyHouseIndex()
        {
            Rectangle player = new Rectangle(PlayerX, PlayerY, PlayerWidth, PlayerHeight);

            for (int i = 0; i < map.Houses.Count; i++)
            {
                Rectangle zone = map.Houses[i];
                zone.Inflate(35, 35);

                if (zone.IntersectsWith(player))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
