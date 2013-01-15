using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EncryptionBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            const int n = 10 * 1000;
            var sw = new Stopwatch();
            Random r = new Random();
            var data = new byte[1024];
            var key8B = new byte[8];
            var key16B = new byte[16];
            var key24B = new byte[24];
            var key32B = new byte[32];
            r.NextBytes(data);
            r.NextBytes(key8B);
            r.NextBytes(key16B);
            r.NextBytes(key24B);
            r.NextBytes(key32B);
            Action<string> outputToConsole = (s) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(s);
            };

            // AES
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("AES");
            var aes = new AesCryptoServiceProvider();
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key16B;
            Action doAes = () => EncryptDecryptAndDispose(aes.CreateEncryptor(), aes.CreateDecryptor(), data);
            doAes.Repeat(n)
                .OutputPerformance(sw, outputToConsole)();
            aes.Dispose();
            
            // RSA
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("DES");
            var des = new DESCryptoServiceProvider();
            des.IV = key8B;
            des.Key = key8B;
            Action doDes = () => EncryptDecryptAndDispose(des.CreateEncryptor(), des.CreateDecryptor(), data);
            doDes.Repeat(n)
                .OutputPerformance(sw, outputToConsole)();
            des.Dispose();

            // RC2
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("RC2");
            var rc2 = new RC2CryptoServiceProvider();
            rc2.IV = key8B;
            rc2.Key = key8B;
            Action doRc2 = () => EncryptDecryptAndDispose(rc2.CreateEncryptor(), rc2.CreateDecryptor(), data);
            doRc2.Repeat(n)
                .OutputPerformance(sw, outputToConsole)();
            rc2.Dispose();

            // Rijndael
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Rijndael");
            var rijndael = new RijndaelManaged();
            rijndael.IV = key16B;
            rijndael.Key = key16B;
            Action doRijndael = () => EncryptDecryptAndDispose(rijndael.CreateEncryptor(), rijndael.CreateDecryptor(), data);
            doRijndael.Repeat(n)
                .OutputPerformance(sw, outputToConsole)();
            rijndael.Dispose();

            // 3DES
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("3DES");
            var tripleDes = new TripleDESCryptoServiceProvider();
            tripleDes.IV = key8B;
            tripleDes.Key = key24B;
            Action do3des = () => EncryptDecryptAndDispose(tripleDes.CreateEncryptor(), tripleDes.CreateDecryptor(), data);
            do3des.Repeat(n)
                .OutputPerformance(sw, outputToConsole)();
            tripleDes.Dispose();


            Console.Read();
        }

        private static byte[] Encrypt(ICryptoTransform crypto, byte[] data)
        {
            return crypto.TransformFinalBlock(data, 0, data.Length);
        }

        private static byte[] Decrypt(ICryptoTransform crypto, byte[] encryptedData)
        {
            return new byte[0];

        }

        private static void EncryptDecryptAndDispose(ICryptoTransform crypto, ICryptoTransform decrypto, byte[] data)
        {
            var encryptedData = crypto.TransformFinalBlock(data, 0, data.Length);
            var decryptedData = decrypto.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            //if (!decryptedData.AllEqual(data))
               // throw new InvalidProgramException();

            crypto.Dispose();
            decrypto.Dispose();
        }
    }

    public static class ActionExtensions
    {
        public static Action Wrap(this Action action, Action pre, Action post)
        {
            return () =>
            {
                pre();
                action();
                post();
            };
        }

        public static Action OutputPerformance(this Action action, Stopwatch stopwatch, Action<string> output)
        {
            return action.Wrap(
             () => stopwatch.Start(),
             () =>
             {
                 stopwatch.Stop();
                 output(stopwatch.Elapsed.ToString());
                 stopwatch.Reset();
             }
             );
        }

        public static Action Repeat(this Action action, int times)
        {
            return () => Enumerable.Range(1, times).ToList()
             .ForEach(x => action());
        }
    }

    static class ArrayExtensions
    {

        public static bool AllEqual(this byte[] array, byte[] compareTo)
        {
            if(array == null)
            {
                return compareTo == null;
            }

            if(compareTo == null)
                return false;

            if(compareTo.Length != array.Length)
                return false;

            return ! Enumerable.Range(0,array.Length)
                .Any(i => array[i] != compareTo[i]);


        }

    }
}
