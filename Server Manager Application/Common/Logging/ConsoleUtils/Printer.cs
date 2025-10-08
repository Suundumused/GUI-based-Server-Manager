using Server_Manager_Application.Resources.Languages;


namespace Server_Manager_Application.Common.Logging.ConsoleUtils
{
    public static class Printer
    {
        private static readonly string header = AppResources.AppName;

        public enum Type_Headers 
        {
            ERROR,
            WARNING,
            INFO
        }


        public static void Print(string text = "", Type_Headers type_log = Type_Headers.INFO) 
        {
            text = header + " --> " + type_log.ToString() + ": " + text;

            Console.WriteLine(text);
            System.Diagnostics.Debug.WriteLine(text);
        }
    }
}
