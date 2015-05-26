using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net; // For generating HTTP requests and getting responses.
using NiceHashBotLib; // Include this for Order class, which contains stats for our order.
using Newtonsoft.Json; // For JSON parsing of remote APIs.

public class HandlerClass
{
    /// <summary>
    /// This method is called every 0.5 seconds.
    /// </summary>
    /// <param name="OrderStats">Order stats - do not change any properties or call any methods. This is provided only as read-only object.</param>
    /// <param name="MaxPrice">Current maximal price. Change this, if you would like to change the price.</param>
    /// <param name="NewLimit">Current speed limit. Change this, if you would like to change the limit.</param>
    public static void HandleOrder(ref Order OrderStats, ref double MaxPrice, ref double NewLimit)
    {
        // Following line of code makes the rest of the code to run only once per minute.
        if ((++Tick % 120) != 0) return;

        // Perform check, if order has been created at all. If not, stop executing the code.
        if (OrderStats == null) return;

        List<Order> orders = APIWrapper.GetAllOrders(OrderStats.ServiceLocation, OrderStats.Algorithm, true);
        double min = 10, max = 0;
        foreach (Order o in orders)
        {
            if (!o.Alive) continue;
            if (o.OrderType != 0) continue;
            if (o.Workers == 0) continue;
            if (max < o.Price) max = o.Price;
            if (min > o.Price) min = o.Price;
        }

        if (max == 0 || min == 10) return;

        // Set new maximal price.
        MaxPrice = Math.Floor((max - min) / 2 * 10000) / 10000;
        //MaxPrice = Math.Floor(C * 10000) / 10000;

        // Example how to print some data on console...
        Console.WriteLine("Adjusting order #" + OrderStats.ID.ToString() + " maximal price to: " + MaxPrice.ToString("F4"));
    }

    /// <summary>
    /// Property used for measuring time.
    /// </summary>
    private static int Tick = -10;


    // Following methods do not need to be altered.
    #region PRIVATE_METHODS

    /// <summary>
    /// Get HTTP JSON response for provided URL.
    /// </summary>
    /// <param name="URL">URL.</param>
    /// <returns>JSON data returned by webserver or null if error occured.</returns>
    private static string GetHTTPResponseInJSON(string URL)
    {
        try
        {
            HttpWebRequest WReq = (HttpWebRequest)WebRequest.Create(URL);
            WReq.Timeout = 60000;
            WebResponse WResp = WReq.GetResponse();
            Stream DataStream = WResp.GetResponseStream();
            DataStream.ReadTimeout = 60000;
            StreamReader SReader = new StreamReader(DataStream);
            string ResponseData = SReader.ReadToEnd();
            if (ResponseData[0] != '{')
                throw new Exception("Not JSON data.");

            return ResponseData;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    #endregion
}
