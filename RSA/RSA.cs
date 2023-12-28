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

    public BigInteger ApplyModulus(BigInteger data, BigInteger key, BigInteger modulus)
    {
        var ModdedData = BigInteger.ModPow(data, key, modulus);
        return ModdedData;
    }

    public byte[] BigIntegerToByteArray(BigInteger bigInt)
    {
        var byteArray = bigInt.ToByteArray();

        if (byteArray[byteArray.Length - 1] == 0)
            byteArray = byteArray.Take(byteArray.Length - 1).ToArray();

        Array.Reverse(byteArray);
        return byteArray;
    }

    public BigInteger[][] GenerateKeys()
    {
        var p = GeneratePrime();
        var q = GeneratePrime();

        var n = p * q;
        var phi = (p - 1) * (q - 1);

        var e = GenerateE(phi, n);
        var d = GenerateD(e, phi);

        return [[e, n], [d, n]];
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

        return ModInverse(e, phi);
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

    public BigInteger XGCD(BigInteger a, BigInteger n) 
{
    BigInteger i = n, v = 0, d = 1;
    while (a > 0) 
    {
        BigInteger t = i / a, x = a;
        a = i % x;
        i = x;
        x = d;
        d = v - t * x;
        v = x;
    }

    v %= n;

    if (v < 0) v = (v + n) % n;
        return v;
    }

    public BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        var ans = XGCD(a, m);
        return ans;
    }
}