using HtmlAgilityPack;
using System.Data;

HtmlDocument html = new HtmlDocument();
html.Load("HtmlTxt.txt");
int columns = 0;
DataTable dt = new DataTable();

if (isContainsTableTag(html))
    dt = extractTable(html);
else
{
    var nodes = html.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']");
    if (columns > 0)
        dt = CreateDataTable(nodes, columns);    
    else
    {
        var nod = html.DocumentNode.SelectSingleNode("(//text()[normalize-space(.) != ''])[1]");
        var count = getParentNode(nod).SelectNodes("./child::*").Count;
        if (count > 0)
            dt = CreateDataTable(nodes, count);
        else throw new Exception("Unable to find columns length please enter coulmns length");
    }
}
print_results(dt);
static HtmlNode getParentNode(HtmlNode node)
{
    HtmlNode? currentNode = node.ParentNode;
    for(; ; )
    {
        if (currentNode.ParentNode != null)
            if (currentNode.ParentNode.ChildNodes.Count > 1)
            {
                currentNode = currentNode.ParentNode;
                break;
            }
            else currentNode = currentNode.ParentNode;
    }
    return currentNode;
}
static DataTable CreateDataTable(HtmlNodeCollection nodes, int columns)
{
    DataTable table = new DataTable();
    for (int i = 0; i < columns; i++)
        table.Columns.Add(nodes[i].InnerText.Trim(), typeof(string));
    HtmlNode[] buffer;
    for (int i = columns; i < nodes.Count; i+= columns)
    {
        buffer = new HtmlNode[columns];
        Array.Copy(nodes.ToArray(), i, buffer, 0, columns);
        string[] str = new string[buffer.Length];
        for(int j=0; j<buffer.Length; j++)
            str[j] = buffer[j].InnerText;
        table.Rows.Add(str);
    }
    return table;
}
static DataTable extractTable(HtmlDocument doc)
{
   DataTable dt = new();
   var columns =  doc.DocumentNode.SelectNodes("/table/thead/tr/th");
   var rows = doc.DocumentNode.SelectNodes("/table/tbody/tr");
    foreach (var col in columns)
        dt.Columns.Add(col.InnerHtml.Trim(),typeof(string));

    foreach (var row in rows) {
        var tRow = dt.NewRow();
        int counter = 0;
        foreach (var ro in row.ChildNodes)
            tRow[counter++] = ro.InnerText.Trim();
        dt.Rows.Add(tRow);
    }
    return dt;
}


static bool isContainsTableTag(HtmlDocument doc)
{
    if(doc.DocumentNode.FirstChild.Name.Equals("table"))
        return true;
    foreach (var child in doc.DocumentNode.ChildNodes)
        if(child.Name.Equals("table"))
            return true;
    return false;
}
static void print_results(DataTable data)
{
    Console.WriteLine();
    Dictionary<string, int> colWidths = new Dictionary<string, int>();

    foreach (DataColumn col in data.Columns)
    {
        Console.Write(col.ColumnName);
        var maxLabelSize = data.Rows.OfType<DataRow>()
                .Select(m => (m.Field<object>(col.ColumnName)?.ToString() ?? "").Length)
                .OrderByDescending(m => m).FirstOrDefault();

        colWidths.Add(col.ColumnName, maxLabelSize);
        for (int i = 0; i < maxLabelSize - col.ColumnName.Length + 10; i++) Console.Write(" ");
    }

    Console.WriteLine();

    foreach (DataRow dataRow in data.Rows)
    {
        for (int j = 0; j < dataRow.ItemArray.Length; j++)
        {
            Console.Write(dataRow.ItemArray[j]);
            for (int i = 0; i < colWidths[data.Columns[j].ColumnName] - dataRow.ItemArray[j].ToString().Length + 10; i++) Console.Write(" ");
        }
        Console.WriteLine();
    }
}