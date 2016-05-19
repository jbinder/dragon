﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Dragon.Security.Hmac.Core.Service;
using Dragon.Security.Hmac.Module.Configuration;
using Dragon.Security.Hmac.Module.Models;
using Dragon.Security.Hmac.Module.Repositories;
using Dragon.Security.Hmac.Module.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dragon.Security.Hmac.Module.Tests.Services
{
    [TestClass]
    public class HmacServiceTest
    {
        private static readonly Guid AppId = Guid.NewGuid();
        private static readonly Guid ServiceId = Guid.NewGuid();
        private static readonly Guid UserId = Guid.NewGuid();
        private const string Secret = "%DF47hf*hdf";
        private const string DefaultSignatureParameterKey = "signature";

        [TestMethod]
        public void IsRequestAuthorized_rawUrlIsExcluded_shouldAllowAccess()
        {
            // Arrange
            var service = new HmacHttpService(ServiceId.ToString(), CreatePathCollection(), "signature", false)
            {
                UserRepository = new Mock<IUserRepository>().Object,
                AppRepository = new Mock<IAppRepository>().Object,
                HmacService = new HmacSha256Service()
            };

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(false), CreateInvalidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.Authorized, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_rawUrlIsExcludedButIncludedBeforeThat_shouldDisallowRequest()
        {
            // Arrange
            var pathCollection = new PathCollection
            {
                new PathConfig {Name = "included", Path = ".*", Type = PathConfig.PathType.Include},
                new PathConfig {Name = "excluded", Path = "/public/.*", Type = PathConfig.PathType.Exclude}
            };
            var service = new HmacHttpService(ServiceId.ToString(), pathCollection, "signature", false)
            {
                UserRepository = new Mock<IUserRepository>().Object,
                AppRepository = new Mock<IAppRepository>().Object,
                HmacService = new HmacSha256Service()
            };

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(false), CreateInvalidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.ParameterMissing, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_rawUrlIsIncludedInvalidQueryString_shouldDisallowRequest()
        {
            // Arrange
            var service = new HmacHttpService(ServiceId.ToString(), CreatePathCollection(), "signature", false)
            {
                UserRepository = new Mock<IUserRepository>().Object,
                AppRepository = new Mock<IAppRepository>().Object,
                HmacService = new HmacSha256Service()
            };

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateInvalidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.ParameterMissing, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_mismatchingServiceId_shouldDisallowRequest()
        {
            // Arrange
            var service = new HmacHttpService(Guid.NewGuid().ToString(), CreatePathCollection(), "signature", false);

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.InvalidOrDisabledServiceId, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_missingParameter_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateInvalidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.ParameterMissing, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_invalidAppId_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var queryString = CreateValidQueryString();
            queryString["appid"] = "23";

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidParameterFormat, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_invalidUserId_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();  
            var queryString = CreateValidQueryString();
            queryString["userid"] = "23";

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidParameterFormat, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_invalidServiceId_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var queryString = CreateValidQueryString();
            queryString["serviceid"] = "23";

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidParameterFormat, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_invalidExpiry_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var queryString = CreateValidQueryString();
            queryString["expiry"] = DateTime.UtcNow.AddDays(-1).Ticks.ToString();

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidExpiryOrExpired, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_malformedExpiry_shouldReturnDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var queryString = CreateValidQueryString();
            queryString["expiry"] = DateTime.UtcNow.AddDays(-1).ToString(CultureInfo.InvariantCulture);

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidExpiryOrExpired, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_invalidSignature_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var queryString = CreateValidQueryString();
            queryString["signature"] = Guid.NewGuid().ToString();

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidSignature, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_validParameter_shouldAllowRequest()
        {
            // Arrange
            var service = CreateHmacService();

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.Authorized, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_customSignatureKeyValidParameter_shouldAllowRequest()
        {
            // Arrange
            var service = CreateHmacService("s");

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString("s"));

            // Assert
            Assert.AreEqual(StatusCode.Authorized, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_usingHexEncodingValidParameter_shouldAllowRequest()
        {
            // Arrange
            var service = CreateHmacService("signature", true);

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString("signature", true));

            // Assert
            Assert.AreEqual(StatusCode.Authorized, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_userDoesntExistInDB_shouldStoreUserInDB()
        {
            // Arrange
            var service = CreateHmacService();
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns((UserModel)null);
            service.UserRepository = mockUserRepository.Object;

            // Act
            service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            mockUserRepository.Verify(x => x.Insert(It.Is<UserModel>(
                y => y.AppId == AppId && y.ServiceId == ServiceId && y.UserId == UserId && y.Enabled && y.CreatedAt.Date == DateTime.UtcNow.Date
                )), Times.Once);
        }

        [TestMethod]
        public void IsRequestAuthorized_userDoesntExistInDBInvalidRequest_shouldNotStoreUserInDB()
        {
            // Arrange
            var service = CreateHmacService();
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns((UserModel)null);
            service.UserRepository = mockUserRepository.Object;

            // Act
            service.IsRequestAuthorized(GetValidRawUrl(), CreateInvalidQueryString());

            // Assert
            mockUserRepository.Verify(x => x.Insert(It.IsAny<UserModel>()), Times.Never);
        }

        [TestMethod]
        public void IsRequestAuthorized_validParameterButUserDisabled_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new UserModel { Enabled = false });
            service.UserRepository = mockUserRepository.Object;

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.InvalidOrDisabledUserId, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_userExistsInDB_shouldNotStoreUserInDB()
        {
            // Arrange
            var service = CreateHmacService();
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new UserModel());
            service.UserRepository = mockUserRepository.Object;

            // Act
            service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            mockUserRepository.Verify(x => x.Insert(It.IsAny<UserModel>()), Times.Never);
        }

        [TestMethod]
        public void IsRequestAuthorized_validParameterButAppDisabled_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var mockAppRepository = new Mock<IAppRepository>();
            mockAppRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new AppModel { Enabled = false, Secret = Secret });
            service.AppRepository = mockAppRepository.Object;

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.InvalidOrDisabledAppId, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_wrongSecret_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var mockAppRepository = new Mock<IAppRepository>();
            mockAppRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(
                new AppModel { Enabled = true, Secret = Secret + "!" });
            service.AppRepository = mockAppRepository.Object;

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.InvalidSignature, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_wrongAppId_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();
            var mockAppRepository = new Mock<IAppRepository>();
            mockAppRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns((AppModel)null);
            service.AppRepository = mockAppRepository.Object;

            // Act
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), CreateValidQueryString());

            // Assert
            Assert.AreEqual(StatusCode.InvalidOrDisabledAppId, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_includeParameterThatHasNotBeenUsedForSignatureGeneration_shouldDisallowRequest()
        {
            // Arrange
            var service = CreateHmacService();

            // Act
            var queryString = CreateValidQueryString();
            queryString.Add("p1", "v1");
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.InvalidSignature, actual);
        }

        [TestMethod]
        public void IsRequestAuthorized_includeParameterThatHasNotBeenUsedForSignatureGeneration_validateUsingHmacParametersOnly_shouldAllowRequest()
        {
            // Arrange
            var service = CreateHmacService(DefaultSignatureParameterKey, false, true);

            // Act
            var queryString = CreateValidQueryString();
            queryString.Add("p1", "v1");
            var actual = service.IsRequestAuthorized(GetValidRawUrl(), queryString);

            // Assert
            Assert.AreEqual(StatusCode.Authorized, actual);
        }

        #region helper

        private static HmacHttpService CreateHmacService(string signatureParameterKey = DefaultSignatureParameterKey, bool useHexEncoding = false, bool validateSignatureUsingHmacParametersOnly = false)
        {
            var mockAppRepository = new Mock<IAppRepository>();
            mockAppRepository.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new AppModel { Enabled = true, Secret = Secret });
            return new HmacHttpService(ServiceId.ToString(), CreatePathCollection(), signatureParameterKey, validateSignatureUsingHmacParametersOnly)
            {
                UserRepository = new Mock<IUserRepository>().Object,
                AppRepository = mockAppRepository.Object,
                HmacService = signatureParameterKey == DefaultSignatureParameterKey ? 
                    new HmacSha256Service { UseHexEncoding = useHexEncoding } : 
                    new HmacSha256Service { SignatureParameterKey = signatureParameterKey, UseHexEncoding = useHexEncoding }
            };
        }

        private static PathCollection CreatePathCollection()
        {
            return new PathCollection
            {
                new PathConfig {Name = "excluded", Path = "/public/.*", Type = PathConfig.PathType.Exclude},
                new PathConfig {Name = "included", Path = ".*", Type = PathConfig.PathType.Include}
            };
        }

        private static NameValueCollection CreateValidQueryString(string signatureParameterKey = DefaultSignatureParameterKey, bool useHexEncoding = false)
        {
            return CreateValidQueryString(new Dictionary<string, string>(), signatureParameterKey, useHexEncoding);
        }

        private static NameValueCollection CreateValidQueryString(Dictionary<string, string> parameters, string signatureParameterKey = DefaultSignatureParameterKey, bool useHexEncoding = false)
        {
            var queryString = new NameValueCollection
            {
                { "appid", AppId.ToString() },
                { "serviceid", ServiceId.ToString() },
                { "userid", UserId.ToString() },
                { "expiry", DateTime.UtcNow.AddDays(+1).Ticks.ToString() },
            };
            parameters.ToList().ForEach(x => queryString.Add(x.Key, x.Value));
            var hmacService = signatureParameterKey == DefaultSignatureParameterKey ?
                new HmacSha256Service { UseHexEncoding = useHexEncoding } :
                new HmacSha256Service { SignatureParameterKey = signatureParameterKey, UseHexEncoding = useHexEncoding };
            queryString.Add(signatureParameterKey, hmacService.CalculateHash(hmacService.CreateSortedQueryString(queryString), Secret));
            return queryString;
        }

        private static NameValueCollection CreateInvalidQueryString()
        {
            return new NameValueCollection { { "id", "23" } };
        }

        private static string GetValidRawUrl(bool secret = true)
        {
            var dir = !secret ? "public" : "private";
            return $"http://localhost/{dir}/index.html";
        }

        #endregion
    }
}
