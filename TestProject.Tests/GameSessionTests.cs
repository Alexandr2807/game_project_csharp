using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimePostman;

namespace TimePostman.Tests
{
    [TestClass]
    public class GameSessionTests
    {
        [TestMethod]
        public void Game_Starts_With_Morning_Phase()
        {
            GameSession session = new GameSession(true);

            Assert.AreEqual(TimePhase.Morning, session.CurrentPhase);
        }

        [TestMethod]
        public void Phase_Changes_After_Enough_Time()
        {
            GameSession session = new GameSession(true);

            session.AdvanceTime(session.PhaseDurationMs);

            Assert.AreEqual(TimePhase.Day, session.CurrentPhase);
        }

        [TestMethod]
        public void Phase_Cycles_Back_To_Morning()
        {
            GameSession session = new GameSession(true);

            session.AdvanceTime(session.PhaseDurationMs);
            session.AdvanceTime(session.PhaseDurationMs);
            session.AdvanceTime(session.PhaseDurationMs);
            session.AdvanceTime(session.PhaseDurationMs);

            Assert.AreEqual(TimePhase.Morning, session.CurrentPhase);
        }

        [TestMethod]
        public void Can_Deliver_Order_In_Correct_Time()
        {
            GameSession session = new GameSession(true);

            DeliveryAttemptResult result = session.TryDeliver(0);

            Assert.AreEqual(DeliveryAttemptResult.Delivered, result);
            Assert.AreEqual(1, session.GetDeliveredCount());
        }

        [TestMethod]
        public void Cannot_Deliver_Order_In_Wrong_Time()
        {
            GameSession session = new GameSession(true);

            DeliveryAttemptResult result = session.TryDeliver(1);

            Assert.AreEqual(DeliveryAttemptResult.WrongTime, result);
            Assert.AreEqual(0, session.GetDeliveredCount());
        }

        [TestMethod]
        public void Cannot_Deliver_Same_Order_Twice()
        {
            GameSession session = new GameSession(true);

            DeliveryAttemptResult first = session.TryDeliver(0);
            DeliveryAttemptResult second = session.TryDeliver(0);

            Assert.AreEqual(DeliveryAttemptResult.Delivered, first);
            Assert.AreEqual(DeliveryAttemptResult.NoOrder, second);
        }

        [TestMethod]
        public void Game_Wins_When_All_Orders_Are_Delivered()
        {
            GameSession session = new GameSession(true);

            Assert.AreEqual(DeliveryAttemptResult.Delivered, session.TryDeliver(0));

            session.AdvanceTime(session.PhaseDurationMs);
            Assert.AreEqual(DeliveryAttemptResult.Delivered, session.TryDeliver(1));
            Assert.AreEqual(DeliveryAttemptResult.Delivered, session.TryDeliver(4));

            session.AdvanceTime(session.PhaseDurationMs);
            Assert.AreEqual(DeliveryAttemptResult.Delivered, session.TryDeliver(2));

            session.AdvanceTime(session.PhaseDurationMs);
            Assert.AreEqual(DeliveryAttemptResult.Delivered, session.TryDeliver(3));
            Assert.AreEqual(DeliveryAttemptResult.Delivered, session.TryDeliver(5));

            Assert.IsTrue(session.GameWon);
        }

        [TestMethod]
        public void Game_Loses_When_Match_Time_Runs_Out()
        {
            GameSession session = new GameSession(true);

            session.AdvanceTime(session.MatchDurationMs);

            Assert.IsTrue(session.GameLost);
        }

        [TestMethod]
        public void Reset_Clears_Progress()
        {
            GameSession session = new GameSession(true);

            session.TryDeliver(0);
            session.AdvanceTime(session.MatchDurationMs / 2);

            session.Reset();

            Assert.AreEqual(0, session.GetDeliveredCount());
            Assert.AreEqual(TimePhase.Morning, session.CurrentPhase);
            Assert.AreEqual(0, session.MatchTimerMs);
            Assert.IsFalse(session.GameWon);
            Assert.IsFalse(session.GameLost);
        }

        [TestMethod]
        public void Cannot_Deliver_When_Game_Already_Ended()
        {
            GameSession session = new GameSession(true);

            session.AdvanceTime(session.MatchDurationMs);

            DeliveryAttemptResult result = session.TryDeliver(0);

            Assert.AreEqual(DeliveryAttemptResult.GameEnded, result);
        }
    }
}