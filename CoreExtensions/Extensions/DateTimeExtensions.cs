using System;
using System.Collections.Generic;

namespace PenguinSoft.CoreExtensions.Extensions
{
                public static class DateTimeExtensions
    {
                                                public static DateTime ThatDay(this DateTime aDate)
        {
            return new DateTime(aDate.Year, aDate.Month, aDate.Day);
        }
                                                public static DateTime PreviousDay(this DateTime aDate)
        {
            return new DateTime(aDate.Year, aDate.Month, aDate.Day).AddDays(-1);
        }
                                                public static DateTime NextDay(this DateTime aDate)
        {
            return new DateTime(aDate.Year, aDate.Month, aDate.Day).AddDays(1);
        }
                                                        public static bool IsBefore(this DateTime aDate, DateTime otherDate)
        {
            return aDate.CompareTo(otherDate) < 0;
        }

                                                        public static bool IsAfter(this DateTime aDate, DateTime otherDate)
        {
            return aDate.CompareTo(otherDate) > 0;
        }

                                                public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

                                                public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1).AddDays(-1);
        }

                                                public static DateTime FirstDayOfNextMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1);
        }

                                                public static DateTime FirstDayOfWeek(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day).AddDays(-((int)date.DayOfWeek));
        }

                                                public static DateTime LastDayOfWeek(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day).AddDays(-((int)date.DayOfWeek)).AddDays(6);
        }

                                                public static DateTime FirstDayOfNextWeek(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day).AddDays(-((int)date.DayOfWeek)).AddDays(7);
        }

                                                                public static List<DateTime> NextDays(this DateTime actual, int daysAhead, List<DateTime> list = null)
        {
            if (list == null)
                list = new List<DateTime>();

            list.Add(actual);

            if (daysAhead == 0)
                return list;

            if (daysAhead > 0)
                return NextDays(actual.AddDays(1), --daysAhead, list);

            return NextDays(actual.AddDays(-1), ++daysAhead, list);
        }

                                                                                public static List<DateTime> NextWeekDays(this DateTime actual, int daysAhead, List<DateTime> list = null, bool? backwards = null)
        {
            if (list == null)
                list = new List<DateTime>();
            if (backwards == null)
                backwards = daysAhead < 0;

            if (actual.DayOfWeek == DayOfWeek.Saturday)
                actual = actual.AddDays(backwards.Value ? -1 : 2);
            else if (actual.DayOfWeek == DayOfWeek.Sunday)
                actual = actual.AddDays(backwards.Value ? -2 : 1);

            list.Add(actual);

            if (daysAhead == 0)
                return list;

            if (daysAhead > 0)
                return NextWeekDays(actual.AddDays(1), --daysAhead, list, backwards);

            return NextWeekDays(actual.AddDays(-1), ++daysAhead, list, backwards);
        }


        public static DateTime RoundTime(this DateTime theDate, int decimalRound = 10, DateTimeFragment fragmentToRound = DateTimeFragment.Seconds)
        {
            int toAdd;

            switch (fragmentToRound)
            {
                case DateTimeFragment.Days:
                    toAdd = (theDate.Day % decimalRound);
                    toAdd = theDate.Day - toAdd < 1 ? toAdd - 1 : toAdd;
                    return new DateTime(theDate.Year, theDate.Month, theDate.Day - toAdd, 0, 0, 0);
                case DateTimeFragment.Hours:
                    toAdd = (theDate.Hour % decimalRound);
                    return new DateTime(theDate.Year, theDate.Month, theDate.Day, theDate.Hour - toAdd, 0, 0);
                case DateTimeFragment.Minutes:
                    toAdd = (theDate.Minute % decimalRound);
                    return new DateTime(theDate.Year, theDate.Month, theDate.Day, theDate.Hour, theDate.Minute - toAdd, 0);
                case DateTimeFragment.Seconds:
                default:
                    toAdd = (theDate.Second % decimalRound);
                    return new DateTime(theDate.Year, theDate.Month, theDate.Day, theDate.Hour, theDate.Minute, theDate.Second - toAdd);
            }
        }

                                                                        public static (DateTime, DateTime) ConvertFromUTC(DateTime time1, DateTime? time2 = null, DateTime? now = null, DateTime? utcNow = null)
        {
            now = now ?? DateTime.Now;
            utcNow = utcNow ?? now.Value.ToUniversalTime();
            time2 = time2 ?? time1;
            if (time1.CompareTo(time2) == 0) 
            {
                var isToday = time1.ThatDay().CompareTo(now?.ThatDay()) == 0;

                var isNextToDayLocal = time1.ThatDay().CompareTo(now?.ThatDay().NextDay()) == 0
                                     || time1.ThatDay().CompareTo(now?.ThatDay().PreviousDay()) == 0;
                var isTodayUTC = time1.ThatDay().CompareTo(utcNow?.ThatDay()) == 0;

                if (isToday || (isTodayUTC && isNextToDayLocal))
                    return (now.Value.ThatDay(), now.Value.ThatDay());
            }

            return (time1, time2.Value);
        }

                                                                public static bool IsInRange(this DateTime date, DateTime toCompare, TimeSpan time)
        {
            return toCompare.IsBefore(date.Add(time)) && toCompare.IsAfter(date.Subtract(time));
        }

    }

    public enum DateTimeFragment
    {
        Seconds = 0,
        Minutes = 1,
        Hours = 2,
        Days = 3
    }
}
