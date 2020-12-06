using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WLPOR5_FF
{
    /// <summary>
    /// For converting the raw data for the network
    /// </summary>
    static class DataSetUtil
    {
        /// <summary>
        /// Use this to convert data from https://finance.yahoo.com
        /// The method will convert the csv to txt-s with more manageable in and output for the network
        /// Input: Date;20 previous days' close price
        /// Output: the next 10 days data
        /// Column names will be removed by the method
        /// </summary>
        /// <param name="companyName_">This will be used in the 2 txt files. Format {companyName_}_input.txt</param>
        /// <param name="filename_">Path to the .csv file from the site</param>
        public static void ConvertDataToNetworkReady(string companyName_, string filename_, int numberOfInputs_, int numberOfOutputs_)
        {
            var lines = File.ReadAllLines(filename_)
                .Skip(1)
                .Select(x => x.Split(','))
                .Select(x => new { Date = x[0], ClosePrice = x[4] })
                .ToList();

            var dates = lines.Select(x => x.Date).ToList();
            var closePrices = lines.Select(x => x.ClosePrice).ToList();

            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();

            int i = 0;

            foreach (var line in lines)
            {
                string input = line.Date;

                var previousPrices = GetElements(closePrices, i, numberOfInputs_, true);

                previousPrices.ForEach(x => input += ";" + x);

                inputs.Add(input);

                string output = string.Empty;

                var nextPrices = GetElements(closePrices, i++, numberOfOutputs_, false);

                nextPrices.ForEach(x => output += x + ";");

                outputs.Add(output.Remove(output.Length - 1));
            }

            File.WriteAllLines($"{companyName_}_input.txt", inputs);
            File.WriteAllLines($"{companyName_}_output.txt", outputs);
        }

        private static List<string> GetElements(List<string> collection_, int indexOfElement_, int numberOfElements_, bool previous_)
        {

            int normalizedNumberOfElements = previous_
                ? indexOfElement_ - numberOfElements_ < 0 ? indexOfElement_ : numberOfElements_
                : indexOfElement_ + numberOfElements_ > collection_.Count ? collection_.Count - 1 - indexOfElement_ : numberOfElements_;

            var results = collection_.GetRange(indexOfElement_ - (previous_ ? normalizedNumberOfElements : 0), normalizedNumberOfElements);

            for (int i = 0; i < numberOfElements_ - normalizedNumberOfElements; i++)
            {
                if (previous_)
                {
                    results.Insert(0, "0");
                }
                else
                {
                    results.Add("0");
                }
            }

            return results;
        }
    }
}
