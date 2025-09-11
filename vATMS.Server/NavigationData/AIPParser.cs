using Newtonsoft.Json;

static class AIPParser
{
    static void ParseAIP()
    {
        try
        {
            StreamReader streamReader = new StreamReader("./EG_AIP.geojson");
            string aipJson = streamReader.ReadToEnd();

            dynamic aipData = JsonConvert.DeserializeObject(aipJson)!;

            if (aipData == null)
                throw new ArgumentException("AIP data JSON is empty");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}