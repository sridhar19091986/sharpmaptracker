using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace SharpTibiaProxy.Network
{
    public class Rsa
    {
        private static readonly RsaEngine openTibiaDecryptEngine;
        private static readonly RsaEngine realTibiaEncryptEngine;

        static Rsa()
        {
            var openTibiaDecryptKey = new RsaPrivateCrtKeyParameters(new BigInteger(Constants.RSAKey.OpenTibiaM), new BigInteger(Constants.RSAKey.OpenTibiaE), 
                new BigInteger(Constants.RSAKey.OpenTibiaE), new BigInteger(Constants.RSAKey.OpenTibiaP), new BigInteger(Constants.RSAKey.OpenTibiaQ), 
                new BigInteger(Constants.RSAKey.OpenTibiaDP), new BigInteger(Constants.RSAKey.OpenTibiaDQ), new BigInteger(Constants.RSAKey.OpenTibiaInverseQ));

            openTibiaDecryptEngine = new RsaEngine();
            openTibiaDecryptEngine.Init(false, openTibiaDecryptKey);

            var realTibiaEncryptKey = new RsaKeyParameters(false, new BigInteger(Constants.RSAKey.RealTibiaM), new BigInteger(Constants.RSAKey.RealTibiaE));
            realTibiaEncryptEngine = new RsaEngine();
            realTibiaEncryptEngine.Init(true, realTibiaEncryptKey);
        }

        public static void OpenTibiaDecrypt(InMessage msg)
        {
            if (msg.Size - msg.ReadPosition != 128)
                throw new Exception("Invalid message size.");

            var decrypted = openTibiaDecryptEngine.ProcessBlock(msg.Buffer, msg.ReadPosition, 128);
            var padSize = 128 - decrypted.Length;

            if (padSize > 0)
                Array.Clear(msg.Buffer, msg.ReadPosition, padSize);
            else
                padSize = 0;

            Array.Copy(decrypted, 0, msg.Buffer, msg.ReadPosition + padSize, decrypted.Length);

            if (msg.ReadByte() != 0)
                throw new Exception("Invalid decrypted message.");

            msg.Encrypted = false;
        }

        public static void RealTibiaEncrypt(OutMessage msg)
        {
            var encrpted = realTibiaEncryptEngine.ProcessBlock(msg.Buffer, msg.WritePosition, 128);
            
            if (msg.WritePosition + 128 > msg.Size)
                msg.Size = msg.WritePosition + 128;

            Array.Copy(encrpted, 0, msg.Buffer, msg.WritePosition, 128);
            msg.Encrypted = true;
        }
    }
}
