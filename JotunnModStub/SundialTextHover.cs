// ValheimSundial
// A Valheim mod that adds a Sundial block
// 
// File:    SundialTextHover.cs
// Project: ValheimSundial

using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace ValheimSundial
{
    internal class SundialTextHover : MonoBehaviour, Hoverable
    {
        private Piece m_piece;

        public void Awake()
        {
            //Jotunn.Logger.LogInfo("Test component has awoken");
            m_piece = GetComponent<Piece>();
        }

        public string GetHoverName()
        {
            //Jotunn.Logger.LogInfo("Getting hover text name");
            return m_piece.m_name;
        }

        public string GetHoverText()
        {
            //Jotunn.Logger.LogInfo("Returning hover text");
            //return Localization.instance.Localize($"{GetHoverName()}" +" "+  $"{EnvMan.instance.GetDayFraction()}" +" "+ $"{EnvMan.instance.GetCurrentDay()}" + " "+ $"{EnvMan.instance.GetMorningStartSec(EnvMan.instance.GetCurrentDay())}");
            return GetCurrentTimeString() + "\nnight at 19:30, morning at 2";
        }

        // From ClockMod
        private string GetCurrentTimeString()
        {
            if (!EnvMan.instance)
                return "";
            float fraction = (float)typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(EnvMan.instance);
            

            int hour = (int)(fraction * 24);
            int minute = (int)((fraction * 24 - hour) * 60);
            int second = (int)((((fraction * 24 - hour) * 60) - minute) * 60);

            DateTime now = DateTime.Now;
            DateTime theTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
            int days = Traverse.Create(EnvMan.instance).Method("GetCurrentDay").GetValue<int>();
            //return GetCurrentTimeString(theTime, fraction, days);
            return String.Format(hour.ToString() + ":"+ minute.ToString());
        }
    }
}