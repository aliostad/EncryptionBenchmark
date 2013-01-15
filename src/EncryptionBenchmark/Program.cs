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
            

            var aes = new AesCryptoServiceProvider();
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key16B;

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000000; i++)
            {
                
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Action doAes = () => EncryptDecryptAndDispose(aes.CreateEncryptor(), aes.CreateDecryptor(), data);
            doAes.Repeat(10 * 1000 * 1000)
                .OutputPerformance(sw, (s) => Console.WriteLine(s));

                

            aes.Dispose();
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
            if (!decryptedData.AllEqual(data))
                throw new InvalidProgramException();

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
