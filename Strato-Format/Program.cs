using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Serialization;

namespace Strato_Format
{
    internal static class Program
    {
        static string rel = "";
        static void Main(string[] args)
        {
            Console.WriteLine("Bitte alle gefragten Dateien in Chronologischer Reihenfolge anhängen.\r\nPfade einzeln eingeben und mit Enter eine neue Zeile erstellen, falls mehrere Datensätze vorhanden sind.\r\nDrucktaste betätigen, wenn die Eingabe abgeschlossen ist.\r\n`exit` eingeben, um die Eingabe zu beenden");
            Console.WriteLine();
            Console.WriteLine("Tupelwerte");

            string? p = GetConsole("Relativer Pfad: ");
            rel = p == "" || p == null ? "" : p;
            


            List<(string, List<string>)> tuplepaths = [];

            while(Question("Zeitstempel-Wert-Tupel-Datensatz hinzufügen? (y/n)"))
            {
                Console.WriteLine();
                Console.Write("Name für das Tupel: ");
                //string? n = Console.ReadLine()?.Replace("Name für das Tupel: ","");
                string? n = Console.ReadLine();
                if (n != null)
                {
                    List<string> path = [];
                    bool px = Question("Eingabe über Präfix? (y/n)");
                    if (rel != "" && px)
                    {
                        path = RequestPrefix();
                    }
                    else
                    {
                        Console.WriteLine($"Pfade zu den Angaben eingeben: ");
                        path = RequestPaths();       
                    }
                    while (path.Count == 0)
                    {
                        path = Fail(px);
                        if (path.FirstOrDefault() == "exit") break;
                    }
                    if (path.Count > 0) tuplepaths.Add((n.Trim(), path));
                }
            }

            bool xml = Question("Soll eine (y) XML oder eine (n) CSV Datei hergestellt werden? ");
            Builder.TupleTable(rel, tuplepaths, xml);



            /*foreach (DataRow row in dt.Rows)
            {
                List<string?> fields = [.. row.ItemArray[1..].Select(field => field?.ToString()?.Replace(',','.'))];
                DateTime date = (DateTime)row["Zeitstempel"];
                fields.Insert(0,date.ToString("F"));
                sb.AppendLine(string.Join(",",fields));
            }*/

            Console.WriteLine("Verarbeitung abgeschlossen.");
            while (true) { }


        }

        static List<string> RequestPrefix()
        {
            //Console.Write($"Geben Sie ein Präfix ein: ");
            //string? pr = Console.ReadLine();
            string? pr = GetConsole("Geben Sie ein Präfix ein: ");
            if (pr != null)
            {
                string[] found = Directory.GetFiles(rel,$"{pr}*");
                if (found.Length != 0)
                {
                    foreach (string f in found)
                    {
                        Console.WriteLine(f);
                    }
                    return [.. found];
                }
            }
            return [];
        }

        static List<string> Fail(bool prefix)
        {
            Console.WriteLine("Pfaderkennung fehlgeschlagen!");
            if (Question("(y) Erneut versuchen oder (n) beenden?"))
            {
                return prefix ? RequestPrefix() : RequestPaths();
            }
            else
            {
                return ["exit"];
            }
        }

        static bool Question(string question)
        {
            Console.Write(question);
            ConsoleKey pk = Console.ReadKey().Key;
            Console.WriteLine();
            return ConsoleKey.Y == pk;
        }

        //Empty Finished, Null Invalid, Content Continue
        static string? GetConsole(string prefix)
        {
            Console.Write(prefix);
            return Console.ReadLine();
        }

        static List<string> RequestPaths()
        {
            List<string> paths = [];
            for (int i = 0; i < 64; i++)
            {
                string prefix = $"{i + 1}: ";
                string? p = GetConsole(prefix);
                switch (p)
                {
                    case null:
                        return [];
                    case "exit":
                        return paths;
                    default:
                        paths.Add(Path.Combine(rel,p.Replace(prefix,"").Trim()));
                        break;
                }
            }
            return [];
        }
    }
}