namespace Solution.Security;

#region Symmetric cryptography class...

/// <summary>Contiene i metodi e le proprietà per utilizzare algoritmi di crittografia simmetrica</summary>
/// <example>
///     <code>
///cSymmetricCryptAlgorithm oCrypt = new cSymmetricCryptAlgorithm(cSymmetricCryptAlgorithm.ServiceProviderEnum.TripleDES);
///string s2 = oCrypt.Encrypt(s1, "12345");
///string s3 = oCrypt.Decrypt(s2, "12345");
///     </code>
/// </example>
public class SymmetricCryptAlgorithm
{
    public enum ServiceProviderEnum : int
    {
        // Supported service providers
        Aes,
        RC2,
        DES,
        TripleDES
    }

    private ServiceProviderEnum mAlgorithm;
    private SymmetricAlgorithm mCryptoService;
    /// <summary>
    /// Inizializza l'istanza.
    /// </summary>
    public SymmetricCryptAlgorithm()
    {
        // Default symmetric algorithm
        mCryptoService = Aes.Create();
        mAlgorithm = ServiceProviderEnum.Aes;
    }
    /// <summary>
    /// Inizializza l'istanza impostando il provider di crittografia.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public SymmetricCryptAlgorithm(ServiceProviderEnum serviceProvider)
    {
        // Select symmetric algorithm
        switch (serviceProvider)
        {
            case ServiceProviderEnum.Aes:
                mCryptoService = Aes.Create(); //  new RijndaelManaged();
                mAlgorithm = ServiceProviderEnum.Aes;
                break;
            case ServiceProviderEnum.RC2:
                mCryptoService = RC2.Create(); // new RC2CryptoServiceProvider();
                mAlgorithm = ServiceProviderEnum.RC2;
                break;
            case ServiceProviderEnum.DES:
                mCryptoService = DES.Create(); // new DESCryptoServiceProvider();
                mAlgorithm = ServiceProviderEnum.DES;
                break;
            case ServiceProviderEnum.TripleDES:
                mCryptoService = TripleDES.Create(); //new TripleDESCryptoServiceProvider();
                mAlgorithm = ServiceProviderEnum.TripleDES;
                break;
        }
    }
    ////
    //public cSymmetricCryptAlgorithm(string serviceProviderName)
    //{
    //    try
    //    {
    //        // Select symmetric algorithm
    //        switch(serviceProviderName.ToLower())
    //        {
    //            case "rijndael":
    //                serviceProviderName = "Rijndael"; 
    //                mAlgorithm = ServiceProviderEnum.Rijndael;
    //                break;
    //            case "rc2":
    //                serviceProviderName = "RC2";
    //                mAlgorithm = ServiceProviderEnum.RC2;
    //                break;
    //            case "des":
    //                serviceProviderName = "DES";
    //                mAlgorithm = ServiceProviderEnum.DES;
    //                break;
    //            case "tripledes":
    //                serviceProviderName = "TripleDES";
    //                mAlgorithm = ServiceProviderEnum.TripleDES;
    //                break;
    //        }

    //        // Set symmetric algorithm
    //        mCryptoService = (SymmetricAlgorithm)CryptoConfig.CreateFromName(serviceProviderName);
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}

    private byte[] GetLegalKey(string key)
    {
        // Adjust key if necessary, and return a valid key
        if (mCryptoService.LegalKeySizes.Length > 0)
        {
            // Key sizes in bits
            int keySize = key.Length * 8;
            int minSize = mCryptoService.LegalKeySizes[0].MinSize;
            int maxSize = mCryptoService.LegalKeySizes[0].MaxSize;
            int skipSize = mCryptoService.LegalKeySizes[0].SkipSize;

            if (keySize > maxSize)
            {
                // Extract maximum size allowed
                key = key.Substring(0, maxSize / 8);
            }
            else if (keySize < maxSize)
            {
                // Set valid size
                int validSize = (keySize <= minSize) ? minSize : (keySize - keySize % skipSize) + skipSize;
                if (keySize < validSize)
                {
                    // Pad the key with asterisk to make up the size
                    key = key.PadRight(validSize / 8, '*');
                }
            }
        }
        return ASCIIEncoding.ASCII.GetBytes(key);
    }

    private void SetLegalIV()
    {
        // Set symmetric algorithm
        switch (mAlgorithm)
        {
            case ServiceProviderEnum.Aes:
                mCryptoService.IV = new byte[] { 0xb, 0x6e, 0x13, 0x2e, 0x31, 0xd2, 0xcd, 0xf7, 0x5, 0x36, 0x9c, 0xea, 0xa8, 0x4c, 0x63, 0xcc };
                break;
            default:
                mCryptoService.IV = new byte[] { 0xb, 0x6e, 0x13, 0x2e, 0x31, 0xd2, 0xcd, 0xf7 };
                break;
        }
    }
    /// <summary>
    /// Crittografa la stringa spacificata, con la chiave "key"
    /// </summary>
    /// <param name="plainText">Stringa in chiaro.</param>
    /// <param name="key">Chiave di crittografia.</param>
    /// <returns>La stringa crittografata.</returns>
    public virtual string Encrypt(string plainText, string key)
    {
        byte[] plainByte = ASCIIEncoding.ASCII.GetBytes(plainText);
        return Convert.ToBase64String(Encrypt(plainByte, key));
        /*
        byte[] keyByte = GetLegalKey(key);

        // Set private key
        mCryptoService.Key = keyByte;
        SetLegalIV();

        // Encryptor object
        ICryptoTransform cryptoTransform = mCryptoService.CreateEncryptor();

        // Memory stream object
        MemoryStream ms = new MemoryStream();

        // Crpto stream object
        CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);

        // Write encrypted byte to memory stream
        cs.Write(plainByte, 0, plainByte.Length);
        cs.FlushFinalBlock();

        // Get the encrypted byte length
        //byte[] cryptoByte = ms.ToArray();

        // Convert into base 64 to enable result to be used in Xml
        //return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0));
        return Convert.ToBase64String(ms.ToArray());
        */
    }
    /// <summary>
    /// Crittografa l'array di byte specificato, con la chiave "key"
    /// </summary>
    /// <param name="plainByte">Array di byte in chiaro.</param>
    /// <param name="key">Chiave di crittografia.</param>
    /// <returns>Array di byte crittografato.</returns>
    public virtual byte[] Encrypt(byte[] plainByte, string key)
    {
        byte[] keyByte = GetLegalKey(key);

        // Set private key
        mCryptoService.Key = keyByte;
        SetLegalIV();

        // Encryptor object
        ICryptoTransform cryptoTransform = mCryptoService.CreateEncryptor();

        // Memory stream object
        MemoryStream ms = new MemoryStream();

        // Crpto stream object
        CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);

        // Write encrypted byte to memory stream
        cs.Write(plainByte, 0, plainByte.Length);
        cs.FlushFinalBlock();

        return ms.ToArray();
    }

    /// <summary>
    /// Decrittografa la stringa spacificata, con la chiave "key"
    /// </summary>
    /// <param name="cryptoText">Stringa crittografata.</param>
    /// <param name="key">Chiave di crittografia.</param>
    /// <returns>La stringa in chiaro.</returns>
    public virtual string Decrypt(string cryptoText, string key)
    {
        // Convert from base 64 string to bytes
        byte[] cryptoByte = Convert.FromBase64String(cryptoText);
        byte[] keyByte = GetLegalKey(key);

        // Set private key
        mCryptoService.Key = keyByte;
        SetLegalIV();

        // Decryptor object
        ICryptoTransform cryptoTransform = mCryptoService.CreateDecryptor();
        try
        {
            // Memory stream object
            MemoryStream ms = new MemoryStream(cryptoByte, 0, cryptoByte.Length);

            // Crpto stream object
            CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read);

            // Get the result from the Crypto stream
            StreamReader sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Decrittografa l'array di byte spacificato, con la chiave "key"
    /// </summary>
    /// <param name="cryptoByte">Attary di byte da crittografare.</param>
    /// <param name="key">Chiave di crittografia.</param>
    /// <returns>Array di byte in chiaro.</returns>
    public virtual byte[] Decrypt(byte[] cryptoByte, string key)
    {
        byte[] keyByte = GetLegalKey(key);

        // Set private key
        mCryptoService.Key = keyByte;
        SetLegalIV();

        // Decryptor object
        ICryptoTransform cryptoTransform = mCryptoService.CreateDecryptor();
        try
        {
            // Memory stream object
            MemoryStream ms = new MemoryStream(cryptoByte, 0, cryptoByte.Length);

            // Crpto stream object
            CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read);

            byte[] fromEncrypt = new byte[cryptoByte.Length];
            cs.Read(fromEncrypt, 0, fromEncrypt.Length);
            return fromEncrypt;
        }
        catch
        {
            return null;
        }
    }

}
#endregion

#region Asymmetric cryptography class...

/// <summary>Contiene le chiavi per il funzionamento degli algoritmi asimmetrici.</summary>
public class CryptKeys
{
    object _PublicKey;
    object _PrivateKey;

    public CryptKeys()
    {
    }
    /// <summary>
    /// Chiave pubblica generata.
    /// </summary>
    public object PublicKey
    {
        get { return _PublicKey; }
        set { _PublicKey = value; }
    }
    /// <summary>
    /// Chiave privata generata.
    /// </summary>
    public object PrivateKey
    {
        get { return _PrivateKey; }
        set { _PrivateKey = value; }
    }


}
/// <summary>Contiene i metodi e le proprietà per utilizzare algoritmi di crittografia asimmetrica.</summary>
public class AsymmetricCryptAlgorithm
{
    /// <summary>
    /// Providers supportati.
    /// </summary>
    public enum ServiceProviderEnum : int
    {
        // Supported service providers
        RSA,
        DSA
    }

    private ServiceProviderEnum mAlgorithm;
    /// <summary>
    /// Inizializza una nuova istanza della classe, per default viene utilizzato l'algoritmo DSA.
    /// </summary>
    public AsymmetricCryptAlgorithm()
    {
        mAlgorithm = ServiceProviderEnum.DSA;
    }
    /// <summary>
    /// Inizializza una nuova istanza della classe.
    /// </summary>
    /// <param name="serviceProvider">Provider da utilizzare per la crittografia.</param>
    public AsymmetricCryptAlgorithm(ServiceProviderEnum serviceProvider)
    {
        mAlgorithm = serviceProvider;
    }
    ///// <summary>
    ///// Inizializza una nuova istanza della classe.
    ///// </summary>
    ///// <param name="serviceProviderName">Algoritmo da utilizzare per la crittografia.</param>
    //public cAsymmetricCryptAlgorithm(string serviceProviderName)
    //{
    //    try
    //    {
    //        // Select asymmetric algorithm
    //        switch (serviceProviderName.ToLower())
    //        {
    //            case "rsa":
    //                mAlgorithm = ServiceProviderEnum.RSA;
    //                break;
    //            case "dsa":
    //                mAlgorithm = ServiceProviderEnum.DSA;
    //                break;
    //        }
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}
    //
    private AsymmetricAlgorithm CreateProvider()
    {
        AsymmetricAlgorithm oAA = null;
        // Select asymmetric algorithm
        switch (mAlgorithm)
        {
            case ServiceProviderEnum.RSA:
                oAA = new RSACryptoServiceProvider();
                break;
            case ServiceProviderEnum.DSA:
                oAA = new DSACryptoServiceProvider();
                break;
        }
        return oAA;
    }
    /// <summary>
    /// Genartore di chiavi.
    /// </summary>
    /// <returns></returns>
    public CryptKeys CreateKeys()
    {
        CryptKeys oKeys = new CryptKeys();
        AsymmetricAlgorithm oAA = CreateProvider();
        oKeys.PrivateKey = oAA.ToXmlString(true);
        oKeys.PublicKey = oAA.ToXmlString(false);
        return oKeys;
    }
    /// <summary>
    /// Consente la crittografia della stringa specificata mediante chiave pubblica.
    /// </summary>
    /// <param name="cryptoText">Stringa da crittografate.</param>
    /// <param name="cPublicKey">Chiave pubblica.</param>
    /// <returns>Stringa crittografata.</returns>
    public virtual string Encrypt(string cryptoText, CryptKeys cPublicKey)
    {
        byte[] cryptoByte = System.Text.Encoding.ASCII.GetBytes(cryptoText);
        //byte[] cryptoByte = Convert.FromBase64String(cryptoText);
        byte[] cryptoByte2 = Encrypt(cryptoByte, cPublicKey);
        return System.Convert.ToBase64String(cryptoByte2);
    }
    /// <summary>
    /// Consente la decrittografia della stringa specificata mediante chiave privata.
    /// </summary>
    /// <param name="cryptoText">Stringa da decrittografate.</param>
    /// <param name="cPrivateKey">Chiave privata.</param>
    /// <returns>Stringa in chiaro.</returns>
    public virtual string Decrypt(string cryptoText, CryptKeys cPrivateKey)
    {
        byte[] cryptoByte = System.Convert.FromBase64String(cryptoText);
        byte[] cryptoByte2 = Decrypt(cryptoByte, cPrivateKey);
        return System.Text.Encoding.ASCII.GetString(cryptoByte2);
    }

    //public static byte[] StringToByteArray(string str)
    //{
    //    //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
    //    //return encoding.GetBytes(str);
    //    return System.Convert.FromBase64String(str);
    //}

    //public static string ByteArrayToString(byte[] bytes)
    //{
    //    //System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
    //    //return enc.GetString(bytes);
    //    return System.Convert.ToBase64String(bytes);
    //}

    /// <summary>
    /// Consente la crittografia della buffer specificato mediante chiave pubblica.
    /// </summary>
    /// <param name="cryptoBuffer">Buffer da crittografate.</param>
    /// <param name="cPublicKey">Chiave pubblica.</param>
    /// <returns>Buffer crittografato.</returns>
    public virtual byte[] Encrypt(byte[] cryptoBuffer, CryptKeys cPublicKey)
    {
        AsymmetricAlgorithm oAA = null;
        switch (mAlgorithm)
        {
            case ServiceProviderEnum.RSA:
                oAA = new RSACryptoServiceProvider();
                ((RSACryptoServiceProvider)oAA).FromXmlString((string)cPublicKey.PublicKey);
                cryptoBuffer = ((RSACryptoServiceProvider)oAA).Encrypt(cryptoBuffer, false);
                break;
        }
        //
        return cryptoBuffer;
    }
    /// <summary>
    /// Consente la decrittografia del buffer specificato mediante chiave privata.
    /// </summary>
    /// <param name="cryptoBuffer">Buffer da decrittografate.</param>
    /// <param name="cPrivateKey">Chiave privata.</param>
    /// <returns>Buffer in chiaro.</returns>
    public virtual byte[] Decrypt(byte[] cryptoBuffer, CryptKeys cPrivateKey)
    {
        AsymmetricAlgorithm oAA = null;
        switch (mAlgorithm)
        {
            case ServiceProviderEnum.RSA:
                oAA = new RSACryptoServiceProvider();
                ((RSACryptoServiceProvider)oAA).FromXmlString((string)cPrivateKey.PrivateKey);
                cryptoBuffer = ((RSACryptoServiceProvider)oAA).Decrypt(cryptoBuffer, false);
                break;
        }
        return cryptoBuffer;
    }
}
#endregion

#region Hash Class...

/// <summary>Contiene i metodi e le proprietà per utilizzare algoritmi di hashing</summary>
public class HashAlgorithm
{
    private System.Security.Cryptography.HashAlgorithm mCryptoService;
    /// <summary>
    /// Providers supportati.
    /// </summary>
    public enum ServiceProviderEnum : int
    {
        // Supported algorithms
        SHA1,
        SHA256,
        SHA384,
        SHA512,
        MD5
    }
    /// <summary>
    /// Inizializza una nuova istanza della classe, per default viene utilizzato l'algoritmo SHA1.
    /// </summary>
    public HashAlgorithm()
    {
        mCryptoService = SHA512.Create(); //SHA1Managed();
    }
    /// <summary>
    /// Inizializza una nuova istanza della classe.
    /// </summary>
    /// <param name="serviceProvider">Provider da utlizzare.</param>
    public HashAlgorithm(ServiceProviderEnum serviceProvider)
    {
        // Select hash algorithm
        switch (serviceProvider)
        {
            case ServiceProviderEnum.MD5:
                mCryptoService = MD5.Create(); //new MD5CryptoServiceProvider();
                break;
            case ServiceProviderEnum.SHA1:
                mCryptoService = SHA1.Create(); //new SHA1Managed();
                break;
            case ServiceProviderEnum.SHA256:
                mCryptoService = SHA256.Create(); //new SHA256Managed();
                break;
            case ServiceProviderEnum.SHA384:
                mCryptoService = SHA384.Create(); //new SHA384Managed();
                break;
            case ServiceProviderEnum.SHA512:
                mCryptoService = SHA512.Create(); //new SHA512Managed();
                break;
        }
    }

    /// <summary>
    /// Genera l'hesh dalla stringa specificata.
    /// </summary>
    /// <param name="oBuffer">Array di byte su cui generare l'hesh</param>
    /// <returns>Stringa contenente il valore hash.</returns>
    public virtual string Generate(byte[] oBuffer)
    {
        byte[] cryptoByte = mCryptoService.ComputeHash(oBuffer);
        return Convert.ToBase64String(cryptoByte, 0, cryptoByte.Length);
    }

    /// <summary>
    /// Genera l'hesh dalla stringa specificata.
    /// </summary>
    /// <param name="plainText">Stringa su cui generare l'hesh</param>
    /// <returns>Stringa contenente il valore hash.</returns>
    public virtual string Generate(string plainText)
    {
        byte[] cryptoByte = mCryptoService.ComputeHash(ASCIIEncoding.ASCII.GetBytes(plainText));
        return Convert.ToBase64String(cryptoByte, 0, cryptoByte.Length);
    }
}
#endregion

#region CRC Class...

public class CRC16
{
    const ushort polynomial = 0xA001;
    ushort[] table = new ushort[256];

    public ushort ComputeChecksum(byte[] bytes)
    {
        ushort crc = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            byte index = (byte)(crc ^ bytes[i]);
            crc = (ushort)((crc >> 8) ^ table[index]);
        }
        return crc;
    }

    public CRC16()
    {
        ushort value;
        ushort temp;
        for (ushort i = 0; i < table.Length; i++)
        {
            value = 0;
            temp = i;
            for (byte j = 0; j < 8; j++)
            {
                if (((value ^ temp) & 0x0001) != 0)
                {
                    value = (ushort)((value >> 1) ^ polynomial);
                }
                else
                {
                    value >>= 1;
                }
                temp >>= 1;
            }
            table[i] = value;
        }
    }
}

public enum InitialCrcValue { Zeros, NonZero1 = 0xffff, NonZero2 = 0x1D0F }

public class CRC16CCITT
{
    const ushort poly = 4129;
    ushort[] table = new ushort[256];
    ushort initialValue = 0;

    public ushort ComputeChecksum(byte[] bytes)
    {
        ushort crc = this.initialValue;
        for (int i = 0; i < bytes.Length; i++)
        {
            crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
        }
        return crc;
    }

    public CRC16CCITT(InitialCrcValue initialValue)
    {
        this.initialValue = (ushort)initialValue;
        ushort temp, a;
        for (int i = 0; i < table.Length; i++)
        {
            temp = 0;
            a = (ushort)(i << 8);
            for (int j = 0; j < 8; j++)
            {
                if (((temp ^ a) & 0x8000) != 0)
                {
                    temp = (ushort)((temp << 1) ^ poly);
                }
                else
                {
                    temp <<= 1;
                }
                a <<= 1;
            }
            table[i] = temp;
        }
    }
}

public class CRC32
{
    uint[] table;

    public uint ComputeChecksum(byte[] bytes)
    {
        uint crc = 0xffffffff;
        for (int i = 0; i < bytes.Length; i++)
        {
            byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
            crc = (uint)((crc >> 8) ^ table[index]);
        }
        return ~crc;
    }

    public CRC32()
    {
        uint poly = 0xedb88320;
        table = new uint[256];
        uint temp = 0;
        for (uint i = 0; i < table.Length; i++)
        {
            temp = i;
            for (int j = 8; j > 0; j--)
            {
                if ((temp & 1) == 1)
                {
                    temp = (uint)((temp >> 1) ^ poly);
                }
                else
                {
                    temp >>= 1;
                }
            }
            table[i] = temp;
        }
    }
}
#endregion