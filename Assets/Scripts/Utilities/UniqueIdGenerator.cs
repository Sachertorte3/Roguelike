namespace Utilities
{
    public static class UniqueIdGenerator
    {
        private static int _id = 0;

        public static int GenerateId()
        {
            return _id++;
        }
    }
}


