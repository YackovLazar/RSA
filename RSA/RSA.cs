using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;

namespace RSA;
public class RSAEncryption
{
    private readonly int _size;
    private RandomNumberGenerator _rng;
    public RSAEncryption(int intensity = 2)
    {
        _size = intensity switch
        {
            1 => 1024,
            2 => 2048,
            _ => throw new ArgumentException("Invalid intensity")
        };
        _rng = RandomNumberGenerator.Create();
    }

    public byte[] ApplyModulus(byte[] data, byte[] key, byte[] modulus)
    {
        var IntKey = new BigInteger(key);
        var IntMod = new BigInteger(modulus);

        var IntData = new BigInteger(data);

        var DecryptedData = BigInteger.ModPow(IntData, IntKey, IntMod);
        return BigIntegerToByteArray(DecryptedData);
    }

    public byte[] BigIntegerToByteArray(BigInteger bigInt)
    {
        var byteArray = bigInt.ToByteArray();

        if (byteArray[byteArray.Length - 1] == 0)
            byteArray = byteArray.Take(byteArray.Length - 1).ToArray();

        Array.Reverse(byteArray);
        return byteArray;
    }

    public byte[][][] GenerateKeys()
    {
        var p = new BigInteger(5);//GeneratePrime();
        var q = new BigInteger(7);//GeneratePrime();

        var n = p * q;
        var phi = (p - 1) * (q - 1);

        var e = GenerateE(phi, n);
        var d = GenerateD(e, phi);

        return [[e.ToByteArray(), n.ToByteArray()], [d.ToByteArray(), n.ToByteArray()]];
    }

    private BigInteger GeneratePrime()
    {
        var min = BigInteger.Pow(2, _size - 1) + 1;
        var max = BigInteger.Pow(2, _size) - 1;

        BigInteger candidate;
        do
        {
            candidate = min + GenerateRandomBigInteger(0, max - min + 1);
        } while (!IsPrime(candidate));
        return candidate;
    }

    private BigInteger GenerateE(BigInteger phi, BigInteger n)
    {
        do
        {
            var e = GenerateRandomBigInteger(1, phi);
            if (isCoprime(e, phi) && isCoprime(e, n))
            {
                return e;
            }
        } while (true);

    }

    private bool IsPrime(BigInteger n)
    {
        foreach (var prime in GeneratePrimes(100))
        {
            if (n % prime == 0)
            {
                return false;
            }
        }

        return isMillerRabinPassed(n);
    }

    private BigInteger GenerateD(BigInteger e, BigInteger phi)
    {
        var k = e / phi;
        var difference = k * e - phi;

        // TODO: Implemment Extended Euclidean Algorithm
    }

    private int[] GeneratePrimes(int max)
    {
        // Sieve of Eratosthenes

        var primes = new List<int>();
        var isPrime = new bool[max + 1];

        for (var i = 2; i <= max; i++)
        {
            isPrime[i] = true;
        }

        for (var i = 2; i <= max; i++)
        {
            if (isPrime[i])
            {
                for (var j = i * i; j <= max; j += i)
                {
                    isPrime[j] = false;
                }
            }
        }
        
        for (var i = 2; i <= max; i++)
        {
            if (isPrime[i])
            {
                primes.Add(i);
            }
        }
        return primes.ToArray();
    }

    private bool trialComposite(BigInteger round_tester, BigInteger evenComponent, BigInteger candidate, BigInteger maxDivisionsByTwo) 
    {
        if (BigInteger.ModPow(round_tester, evenComponent, candidate) == 1 )
            return false;

        for (int i = 0; i < maxDivisionsByTwo; i++)
        {
            if (BigInteger.ModPow(round_tester, (1 << i) * evenComponent, candidate) == candidate - 1) 
                return false;
        }

        return true;
    }

    private bool isMillerRabinPassed(BigInteger candidate) 
    {
        // Run 20 iterations of Rabin Miller Primality test

        int maxDivisionsByTwo = 0;
        BigInteger evenComponent = (candidate - 1);

        while (evenComponent % 2 == 0)
        {
            evenComponent >>= 1;
            maxDivisionsByTwo += 1;
        }

        // Set number of trials here 
        int numberOfRabinTrials = 20;
        for (int i = 0; i < numberOfRabinTrials ; i++)
        {
            Random rand = new Random();
            BigInteger round_tester = GenerateRandomBigInteger(2, candidate);

            if (trialComposite(round_tester, evenComponent, candidate, maxDivisionsByTwo))
                return false;
        }
        return true;
    }

    private bool isCoprime(BigInteger a, BigInteger b)
    {
        return BigInteger.GreatestCommonDivisor(a, b) == 1;
    }

    // This may make you vomit but at least its cryptographically secure...?
    public BigInteger GenerateRandomBigInteger(BigInteger lowerBound, BigInteger upperBound)
    {
        byte[] bytes = new byte[_size / 8];
        BigInteger result;

        do
        {
            _rng.GetBytes(bytes);
            result = new BigInteger(bytes);

            if (result < 0)
                result = -result;

            if (result >= upperBound)
                result = result % (upperBound - lowerBound) + lowerBound;
        }
        while (result < lowerBound || result >= upperBound);

        return result;
    }
}