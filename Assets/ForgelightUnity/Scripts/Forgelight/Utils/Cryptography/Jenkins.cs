namespace ForgelightUnity.Forgelight.Utils.Cryptography
{
    public class Jenkins
    {
        /// <summary>
        /// Forgelight performs the Jenkins hash on a signed int, but is stored/referenced as unsigned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static uint OneAtATime(string key)
        {
            int hash = 0;

            for (int i = 0; i < key.Length; ++i)
            {
                hash += key[i];
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);

            return (uint) hash;
        }
    }
}