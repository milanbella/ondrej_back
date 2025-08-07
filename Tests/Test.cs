#pragma warning disable 8600, 8601, 8618, 8602, 8604, 8603, 8765


namespace RetailAppS.Tests
{
    public class Test
    {


        static void TestEncryptionHelper()
        {
            string password = "password";
            string plaintext = "Hello, World!";

            string encrypted = Encryption.EncryptionHelper.Encrypt(password, plaintext);
            Console.WriteLine($"Encrypted: {encrypted}");

            string decrypted = Encryption.EncryptionHelper.Decrypt(password, encrypted);
            Console.WriteLine($"Decrypted: {decrypted}");
        }

        static void TestEncryptionHelper_1()
        {
            string plaintext = "skakavykonik";

            string encrypted = Encryption.EncryptionHelper.Encrypt(plaintext);
            Console.WriteLine($"Encrypted: {encrypted}");

            string decrypted = Encryption.EncryptionHelper.Decrypt(encrypted);
            Console.WriteLine($"Decrypted: {decrypted}");
        }

        static void TestEncryptionHelper_2()
        {

            string encrypted = "Ip7QFArcnh63P2oFWI4+DRjD3V58:fq365suzXri51hoQ:3YHpEkZEIGRersog7scuWg==:oggmfd1SaXxYYPv4/Q1Y/Q==";

            string decrypted = Encryption.EncryptionHelper.Decrypt(encrypted);
            Console.WriteLine($"Decrypted: {decrypted}");
        }


        public static void TestAll() {
            if (true) {
                //TestEncryptionHelper();
                TestEncryptionHelper_1();
                //TestEncryptionHelper_2();
            }
        }
    }
}
