using System.Security.Cryptography;

public static class EncryptionHelper
{
    private static string EncryptionKey;

    static EncryptionHelper()
    {
        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfiguration config = builder.Build();

        // Retrieve encryption key from configuration
        EncryptionKey = config["Encryption:EncryptionKey"];
    }

    //public static string EncryptString(string plainText)
    //{
    //    byte[] key = Convert.FromBase64String(EncryptionKey);
    //    using (Aes aes = Aes.Create())
    //    {
    //        aes.Key = key;
    //        aes.IV = new byte[16]; // Initialization vector with zeros
    //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
    //            {
    //                using (StreamWriter sw = new StreamWriter(cs))
    //                {
    //                    sw.Write(plainText);
    //                }
    //                return Convert.ToBase64String(ms.ToArray());
    //            }
    //        }
    //    }
    //}

    //public static string DecryptString(string cipherText)
    //{
    //    byte[] key = Convert.FromBase64String(EncryptionKey);
    //    byte[] buffer = Convert.FromBase64String(cipherText);
    //    using (Aes aes = Aes.Create())
    //    {
    //        aes.Key = key;
    //        aes.IV = new byte[16]; // Initialization vector with zeros
    //        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
    //        using (MemoryStream ms = new MemoryStream(buffer))
    //        {
    //            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
    //            {
    //                using (StreamReader sr = new StreamReader(cs))
    //                {
    //                    return sr.ReadToEnd();
    //                }
    //            }
    //        }
    //    }
    //}

    public static string EncryptString(string plainText)
    {
        byte[] key = Convert.FromBase64String(EncryptionKey);
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV(); // Generate a random IV

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length); // Prepend IV to the ciphertext
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string DecryptString(string cipherText)
    {
        byte[] key = Convert.FromBase64String(EncryptionKey);
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                byte[] iv = new byte[16];
                ms.Read(iv, 0, iv.Length); // Extract the IV from the beginning

                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

}
