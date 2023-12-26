
using System.Numerics;
using System.Text;
using RSA;

internal class Program
{
    private static void Main(string[] args)
    {
        var rsa = new RSAEncryption();
        var data = BitConverter.GetBytes(3);
        var keys = rsa.GenerateKeys();
        var PublicKey = keys[0][0];
        var modulus = keys[0][1];
        var PrivateKey = keys[1][0];
        var encryptedData = rsa.ApplyModulus(data, PublicKey, modulus);
        var decryptedData = rsa.ApplyModulus(encryptedData, PrivateKey, modulus);


        Console.WriteLine($"Original data: {Encoding.UTF8.GetString(data)}");

        Console.WriteLine($"Encrypted data: {decryptedData[0]}");
    }
}