using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Strato_Format
{
    internal static class Builder
    {
        internal static void TupleTable(string rel, List<(string, List<string>)> tuplepaths, bool xml)
        {
            List<List<Formatter.TimestampValuePair>> cols = [];
            foreach ((string, List<string>) path in tuplepaths)
            {
                List<Formatter.TimestampValuePair> tsvps = Formatter.Process(path.Item2);
                Console.WriteLine(path.Item1);
                foreach (string pa in path.Item2)
                {
                    Console.WriteLine(pa);
                }
                if (tsvps.Count > 0)
                {

                    foreach (Formatter.TimestampValuePair v in tsvps)
                    {
                        Console.WriteLine($"{v.Ts} - {v.Val}");
                    }
                }
                cols.Add(tsvps);
            }

            //Fuse by TimeStamp
            DataSet ds = new("Daten");
            DataTable dt = new("Tabelle");
            dt.Columns.Add("Zeitstempel",typeof(DateTime));
            tuplepaths.ForEach(x => dt.Columns.Add(x.Item1,typeof(double)));

            for (int c = 0; c < cols.Count; c++)
            {
                foreach (Formatter.TimestampValuePair ft in cols[c])
                {
                    List<DataRow> dr = [.. dt.Select().Where(x => (DateTime)x["Zeitstempel"] == ft.Ts)];
                    if (dr.Count == 0)
                    {
                        DataRow d = dt.NewRow();
                        d["Zeitstempel"] = ft.Ts;
                        d[tuplepaths[c].Item1] = ft.Val;
                        int index = 0;
                        for (; index < dt.Rows.Count; index++)
                        {
                            if (ft.Ts < (DateTime)dt.Rows[index]["Zeitstempel"]) break;
                        }
                        dt.Rows.InsertAt(d,index);
                    }
                    else
                    {
                        dr.First()[tuplepaths[c].Item1] = ft.Val;

                    }
                }
            }
            ds.Tables.Add(dt);

            int max = 0;
            int i = 0;
            int x = 0;
            //CSV oder XML Bauen
            if (xml)
            {

                do
                {
                    max += 65536;
                    XmlSerializer ser = new(typeof(DataTable));
                    TextWriter writer = new StreamWriter(Path.Combine(rel,$"Zusammenfassung-{++x}.xml"));
                    ser.Serialize(writer,dt.AsEnumerable().Skip(i).Take(max));
                    writer.Close();
                    i = max;
                }
                while (i < dt.Rows.Count);
            }
            else
            {
                do
                {
                    StringBuilder sb = new();

                    IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                    sb.AppendLine(string.Join(",",columnNames));

                    max += 65536;
                    for (; i < max && i < dt.Rows.Count; i++)
                    {
                        List<string?> fields = [.. dt.Rows[i].ItemArray[1..].Select(field => field?.ToString()?.Replace(',','.'))];
                        DateTime date = (DateTime)dt.Rows[i]["Zeitstempel"];
                        fields.Insert(0,date.ToString("G").Replace(',','.'));
                        string l = string.Join(",",fields);
                        Console.WriteLine(l);
                        sb.AppendLine(l);
                    }
                    File.WriteAllText(Path.Combine(rel,$"Zusammenfassung-{++x}.csv"),sb.ToString());
                }
                while (i < dt.Rows.Count);
            }
        }
    }
}
