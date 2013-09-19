﻿using System;
using System.IO;
using Dragon.Interfaces.Files;
using File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileTest
{
    /// <summary>
    /// Needs valid configuration provided in the application configuration file.
    /// See concrete test classes for details.
    /// Note: The tests will upload/download/remove test data to the storage provider!
    /// </summary>
    [TestClass]
    public abstract class FileStorageIntegrationTestBase
    {
        public abstract IFileStorage CreateFileStorage();

        private const string TEST_FILE_PATH = "resources/test.txt";

        [TestMethod]
        [TestCategory("IntegrationTest")]
        [ExpectedException(typeof(FileStoreResourceNotFoundException))]
        public void Delete_inexistentFile_shouldThrowException()
        {
            var fileStorage = CreateFileStorage();
            fileStorage.Delete("blah");
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_validFile_shouldDeleteFile()
        {
            var fileStorage = CreateFileStorage();
            var id = fileStorage.Store(TEST_FILE_PATH);
            Assert.AreNotEqual("", id);
            fileStorage.Delete(id);
            Assert.AreEqual(false, fileStorage.Exists(id));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Exists_inexistentFile_shouldReturnFalse()
        {
            var fileStorage = CreateFileStorage();
            var actual = fileStorage.Exists(Guid.NewGuid().ToString());
            Assert.AreEqual(false, actual);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Exists_validFile_shouldReturnTrue()
        {
            var fileStorage = CreateFileStorage();
            var id = fileStorage.Store(TEST_FILE_PATH);
            var actual = fileStorage.Exists(id);
            Assert.AreEqual(true, actual);
            // cleanup
            fileStorage.Delete(id);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Store_validFile_shouldUploadFile()
        {
            var fileStorage = CreateFileStorage();
            var id = fileStorage.Store(TEST_FILE_PATH);
            Assert.AreNotEqual("", id);
            Assert.AreEqual(true, fileStorage.Exists(id));
            // cleanup
            fileStorage.Delete(id);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        [ExpectedException(typeof(FileStoreResourceNotFoundException))]
        public void Retrieve_invalidFile_shoulThrowException()
        {
            var fileStorage = CreateFileStorage();
            fileStorage.Retrieve(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Retrieve_validFile_shouldDownloadFile()
        {
            var fileStorage = CreateFileStorage();
            var id = fileStorage.Store(TEST_FILE_PATH);
            var actual = new StreamReader(fileStorage.Retrieve(id)).ReadToEnd();
            Assert.AreEqual("hello s3!\r\n...\r\n..\r\n.\r\n", actual);
            // cleanup
            fileStorage.Delete(id);
        }

    }
}