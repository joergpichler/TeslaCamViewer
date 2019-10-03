using System;
using System.Globalization;

namespace TeslaCamViewer
{
    public class TeslaCamDate
    {
        private const string FileFormat = "yyyy-MM-dd_HH-mm-ss";
        private const string DisplayFormat = "M/d/yyyy h:mm tt";

        public string UTCDateString { get; }
        public string DisplayValue => LocalTimeStamp.ToString(DisplayFormat);

        public DateTime UTCTimeStamp
        {
            get
            {
                if (DateTime.TryParseExact(UTCDateString, FileFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                {
                    return dateTime;
                }

                throw new Exception("Invalid date format: " + UTCDateString);
            }
        }

        public DateTime LocalTimeStamp => UTCTimeStamp;

        public TeslaCamDate(string dateString)
        {
            UTCDateString = dateString;
        }
    }
}
