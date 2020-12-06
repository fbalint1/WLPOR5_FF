using System;
using System.Linq;


namespace WLPOR5_FF
{
    class Program
    {
        private const string COMPANY_NAME = "Morgan Stanley";
        private const string YAHOO_FILE_PATH = "MS.csv";

        static void Main(string[] args)
        {
            DataSetUtil.ConvertDataToNetworkReady(COMPANY_NAME, YAHOO_FILE_PATH, 20, 10);

            DataSet dataSet = new DataSet(COMPANY_NAME + "_input.txt", COMPANY_NAME + "_output.txt");

            StockPricePrediction stockPricePrediction = new StockPricePrediction(10, dataSet);

            stockPricePrediction.Train();

            var prediction = stockPricePrediction.Prediction(dataSet.Input.Reverse<float>().Take(20).ToList());

            Console.WriteLine("Prediction for next 14 days:");

            prediction.ForEach(x => Console.WriteLine(x));

            Console.ReadLine();
        }
    }
}
