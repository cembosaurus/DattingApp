namespace DatingApp.API.Helpers
{
    public sealed class SingletonTest : ISingletonTest
    {

        private static ISingletonTest _instance;
        private static readonly object _lockObject = new object();
        private string _text1 = "0123456789";
        private string _text2 = "abcdefghijklmnopqrstuvwxyz";

        static SingletonTest()
        {
            _instance = new SingletonTest();
        }

        private SingletonTest()
        {

        }

        public static ISingletonTest instance
        {
            get
            {
                lock (_lockObject)
                {
                    return _instance;
                }
            }
        }

        public string text1 { get { return _text1; } }

        public string text2 { get { return _text2; } }
    }
}