
namespace WLPOR5_FF
{
    class Program
    {
        private const string COMPANY_NAME = "Morgan Stanley";
        private const string YAHOO_FILE_PATH = "MS.csv";

        static void Main(string[] args)
        {
            DataSetUtil.ConvertDataToNetworkReady(COMPANY_NAME, YAHOO_FILE_PATH);

            DataSet dataSet = new DataSet(COMPANY_NAME + "_input.txt", COMPANY_NAME + "_output.txt");


        }
    }
}
