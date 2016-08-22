﻿using System;
using System.Threading;
using SimpleFMS.Base.DriverStation;
using SimpleFMS.Base.Enums;
using SimpleFMS.Base.Exceptions;
using SimpleFMS.Base.MatchTiming;

namespace SimpleFMS.MatchTiming
{
    public class MatchTimingManager : IMatchTimingManager
    {
        private const int MatchUpdatePeriod = 250;

        private TimeSpan m_teleoperatedTime = MatchTimingConstants.DefaultTeleoperatedTime;
        private TimeSpan m_autonomousTime = MatchTimingConstants.DefaultAutonomousTime;
        private TimeSpan m_delayTime = MatchTimingConstants.DefaultDelayTime;

        public bool SetMatchTimes(IMatchTimeReport times)
        {
            lock (m_lockObject)
            {
                try
                {
                    TeleoperatedTime = times.TeleoperatedTime;
                    AutonomousTime = times.AutonomousTime;
                    DelayTime = times.DelayTime;
                }
                catch (MatchEnabledException)
                {
                    // Cannot set times with match enabled
                    return false;
                }
                return true;
            }
        }


        private TimeSpan TeleoperatedTime
        {
            get
            {
                lock (m_lockObject)
                    return m_teleoperatedTime;
            }
            set
            {
                if (m_matchState != MatchState.Stopped)
                    throw new MatchEnabledException("Teleoperated time cannot be set while the match is enabled");
                lock (m_lockObject)
                    m_teleoperatedTime = value;
            }
        }

        private TimeSpan AutonomousTime
        {
            get
            {
                lock (m_lockObject)
                    return m_autonomousTime;
            }
            set
            {
                if (m_matchState != MatchState.Stopped)
                    throw new MatchEnabledException("Autonomous time cannot be set while the match is enabled");
                lock (m_lockObject)
                    m_autonomousTime = value;
            }
        }

        private TimeSpan DelayTime
        {
            get
            {
                lock (m_lockObject)
                    return m_delayTime;
            }
            set
            {
                if (m_matchState != MatchState.Stopped)
                    throw new MatchEnabledException("Delay time cannot be set while the match is enabled");
                lock (m_lockObject)
                    m_delayTime = value;
            }
        }

        public IMatchStateReport GetMatchTimingReport()
        {
            lock (m_lockObject)
            {
                MatchStateReport report = new MatchStateReport(m_matchState, GetRemainingPeriodTime(),
                    m_teleoperatedTime, m_delayTime, m_autonomousTime);
                return report;
            }
        }

        private MatchState m_matchState = MatchState.Stopped;
        private DateTime m_periodEndTime = DateTime.MinValue;
        private bool m_fullMatch = false;

        private readonly Timer m_matchTimer;

        private readonly IDriverStationManager m_driverStationManager;

        private readonly object m_lockObject = new object();

        public MatchTimingManager(IDriverStationManager dsManager)
        {
            m_driverStationManager = dsManager;

            m_matchTimer = new Timer(OnTimerUpdate, null, MatchUpdatePeriod, MatchUpdatePeriod);
        }

        private void OnTimerUpdate(object state)
        {
            lock (m_lockObject)
            {
                if (m_matchState == MatchState.Stopped) return;

                DateTime now = DateTime.UtcNow;
                TimeSpan remaining = m_periodEndTime - now;
                if (remaining <= TimeSpan.Zero)
                {
                    // Period Expired
                    // Stop the current period.
                    m_driverStationManager.StopMatchPeriod();

                    // If we were in autonomous and running a full match
                    if (m_matchState == MatchState.Autonomous && m_fullMatch)
                    {
                        m_matchState = MatchState.Delay;
                        m_periodEndTime = now + DelayTime;
                    }
                    // Finished a pause
                    else if (m_matchState == MatchState.Delay)
                    {
                        m_matchState = MatchState.Teleoperated;
                        m_periodEndTime = now + TeleoperatedTime;
                        m_driverStationManager.StartMatchPertiod(false);
                    }
                    else
                    {
                        // Finished a match
                        OnMatchStop();
                    }
                }
                else
                {
                    OnMatchTimerUpdate?.Invoke(remaining);
                    m_driverStationManager.SetRemainingMatchTime((int)remaining.TotalSeconds);
                }
            }
        }

        private void OnMatchStop()
        {
            lock (m_lockObject)
            {
                m_periodEndTime = DateTime.MinValue;
                m_matchState = MatchState.Stopped;
            }
        }

        public void Dispose()
        {
            m_matchTimer.Dispose();
        }

        public event Action<TimeSpan> OnMatchTimerUpdate;
        public event Action<MatchState, MatchState> OnMatchPeriodUpdate;

        public TimeSpan GetRemainingPeriodTime()
        {
            lock (m_lockObject)
            {
                if (m_matchState == MatchState.Stopped)
                    return AutonomousTime;

                if (m_periodEndTime == DateTime.MinValue)
                    return TimeSpan.Zero;

                return m_periodEndTime - DateTime.UtcNow;
            }
        }

        public MatchState GetMatchState()
        {
            lock (m_lockObject)
                return m_matchState;
        }

        public bool StartMatch()
        {
            bool started = false;
            lock (m_lockObject)
            {
                if (m_matchState != MatchState.Stopped) return false;

                started = m_driverStationManager.StartMatchPertiod(true);
                if (started)
                {

                    m_matchState = MatchState.Autonomous;
                    m_periodEndTime = DateTime.UtcNow + m_autonomousTime;
                    m_fullMatch = true;
                }
            }
            return started;
        }

        public void StopCurrentPeriod()
        {
            m_driverStationManager.StopMatchPeriod();
            OnMatchStop();
        }

        public bool StartAutonomous()
        {
            lock (m_lockObject)
            {
                if (m_matchState != MatchState.Stopped) return false;

                bool started = m_driverStationManager.StartMatchPertiod(true);
                if (started)
                {

                    m_matchState = MatchState.Autonomous;
                    m_periodEndTime = DateTime.UtcNow + m_autonomousTime;
                    m_fullMatch = false;
                }
                return started;
            }
        }

        public bool StartTeleop()
        {
            lock (m_lockObject)
            {
                if (m_matchState != MatchState.Stopped) return false;
                bool started = m_driverStationManager.StartMatchPertiod(false);
                if (started)
                {

                    m_matchState = MatchState.Teleoperated;
                    m_periodEndTime = DateTime.UtcNow + m_teleoperatedTime;
                    m_fullMatch = false;
                }
                return started;
            }
        }
    }
}
