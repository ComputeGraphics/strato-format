using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Strato_Format
{
    internal static class Formatter
    {
        public static DateTime GetTime(string t)
        {
            if (DateTime.TryParseExact(t.Trim().Replace(";",""),"yyyy-MM-dd_HH-mm-ss",CultureInfo.InvariantCulture,DateTimeStyles.AssumeLocal,out DateTime ft)) return ft;
            else return default;
        }

        public static double GetValue(string v)
        {
            if (Double.TryParse(v.Trim().Replace(";",""),CultureInfo.InvariantCulture,out double fv)) return fv;
            else return double.NaN;
        }
        public struct TimestampValuePair
        {
            public DateTime Ts { get; set; }
            public double Val { get; set; }
        }
        public static List<TimestampValuePair> Process(List<string> paths)
        {
            try
            {
                List<TimestampValuePair> tsvps = [];
                foreach (string path in paths)
                {
                    StreamReader sr = new(path,Encoding.UTF8);
                    string? line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        TimestampValuePair tsvp = new();
                        if (line == null) break;
                        Console.WriteLine(line);
                        string[] split = line.Trim().Split(';');
                        if (split.Length > 1)
                        {
                            tsvp.Ts = GetTime(split[0]);
                            tsvp.Val = GetValue(split[1]);
                        }
                        tsvps.Add(tsvp);             
                    }
                    sr.Close();
                }
                return [..tsvps.OrderBy(x => x.Ts)];
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine("Eine Datei konnte nicht gefunden werden: " +fnfe.FileName);
                return [];
            }
            catch (Exception e)
            {
                Console.WriteLine("Fehler: " + e.Message);
                return [];
            }

        }
    }
}
