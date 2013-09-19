﻿using System.Collections.Specialized;
using System.Configuration;
using Dragon.Interfaces.Files;
using File;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileTest
{
    /// <summary>
    /// Needs valid Amazon S3 credentials configured in the application configuration file.
    /// See <see cref="S3FileStorage"/> for details.
    /// </summary>
    [TestClass]
    public class S3FileStorageIntegrationTest : FileStorageIntegrationTestBase
    {
        public override IFileStorage CreateFileStorage()
        {
            var fileStorage = new S3FileStorage(TestHelper.CreateConfigurationMock(new NameValueCollection
            {
                {"Dragon.Files.S3.AccessKeyID", ConfigurationManager.AppSettings["Dragon.Files.S3.AccessKeyID"]},
                {"Dragon.Files.S3.AccessKeySecret", ConfigurationManager.AppSettings["Dragon.Files.S3.AccessKeySecret"]},
                {"Dragon.Files.S3.Bucket", ConfigurationManager.AppSettings["Dragon.Files.S3.Bucket"]}
            }).Object);
            return fileStorage;
        }
    }
}