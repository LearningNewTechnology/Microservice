﻿using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xigadee;

namespace Test.Xigadee
{
    [TestClass]
    public class AesEncryptionTests
    {
        private readonly byte[] mKey = Convert.FromBase64String("hNCV1t5sA/xQgDkHeuXYhrSu8kF72p9H436nQoLDC28=");
        private readonly AesEncryptionHandler mAesEncryption;

        public AesEncryptionTests()
        {
            mAesEncryption = new AesEncryptionHandler("test", mKey, false);
        }

        [TestMethod]
        public void EncryptDecrypt()
        {
            var secret = "I know a secret";
            var encryptedData = mAesEncryption.Encrypt(Encoding.UTF8.GetBytes(secret));
            Assert.AreNotEqual(secret, Encoding.UTF8.GetString(encryptedData));

            // Verify that the string can be decrypted
            var decryptedData = mAesEncryption.Decrypt(encryptedData);
            Assert.AreEqual(secret, Encoding.UTF8.GetString(decryptedData));

            var encryptedData2 = mAesEncryption.Encrypt(Encoding.UTF8.GetBytes(secret));
            Assert.IsFalse(encryptedData.SequenceEqual(encryptedData2));
        }

        [TestMethod]
        public void EncryptDecryptWithCompression()
        {
            var encryption = new AesEncryptionHandler("EncryptDecryptWithCompression", mKey);
            var secret = "I know a secret";
            var encryptedData = encryption.Encrypt(Encoding.UTF8.GetBytes(secret));
            Assert.AreNotEqual(secret, Encoding.UTF8.GetString(encryptedData));

            // Verify that the string can be decrypted
            var decryptedData = encryption.Decrypt(encryptedData);
            Assert.AreEqual(secret, Encoding.UTF8.GetString(decryptedData));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidKeySize()
        {
            var encryption = new AesEncryptionHandler("InvalidKeySize", mKey, keySize:128);
            Assert.Fail("Key size is incorrect so should have thrown an exception");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidKey()
        {
            var encryption = new AesEncryptionHandler("InvalidKey", Convert.FromBase64String("hNCV1t5sA/xQgDkHeuXYhrSu8kF72p9H436nQoLD"));
            Assert.Fail("Key is incorrect so should have thrown an exception");
        }
    }
}
