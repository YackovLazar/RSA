
using System.Numerics;
using System.Text;
using RSA;

internal class Program
{
    private static void Main(string[] args)
    {
        var rsa = new RSAEncryption();
        var data = new BigInteger(Encoding.UTF8.GetBytes("Hello"));
        var keys = rsa.GenerateKeys();
        var PublicKey = keys[0][0];
        var modulus = keys[0][1];
        var PrivateKey = keys[1][0];
        var encryptedData = rsa.ApplyModulus(data, PublicKey, modulus);
        var decryptedData = rsa.ApplyModulus(encryptedData, PrivateKey, modulus);


        Console.WriteLine($"Original data: {data}");

        Console.WriteLine($"Decrypted data: {Encoding.UTF8.GetString(decryptedData.ToByteArray())}");
    }
}