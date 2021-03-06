﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using Dragon.Data;

namespace Dapper
{
    public static partial class SqlMapperExtensions
    {
        public static bool ExistsTable<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);

            var values = new Dictionary<string, object>();
            var metadata = MetadataFor(type);

            var sql = TSQLGenerator.BuildExistsTable(metadata);

            return connection.Query(sql, transaction: transaction, commandTimeout: commandTimeout).Any();
        }

        #region TableDropper

        public static void DropTableIfExists<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            DropTable<T>(connection, true, transaction, commandTimeout);
        }

        public static void DropTableIfExists(this IDbConnection connection, string name, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            DropTable(connection, name, true, transaction, commandTimeout);
        }

        public static void DropTable<T>(this IDbConnection connection, bool onlyIfExists, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var sql = DropTableSQL<T>(onlyIfExists);

            connection.Execute(sql, transaction: transaction, commandTimeout: commandTimeout);
        }

        public static void DropTable(this IDbConnection connection, string name, bool onlyIfExists, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var sql = DropTableSQL(name, onlyIfExists);

            connection.Execute(sql, transaction: transaction, commandTimeout: commandTimeout);
        }

        private static string DropTableSQL<T>(bool onlyIfExists) where T : class
        {
            var type = typeof(T);
            var metadata = MetadataFor(typeof (T));

            return DropTableSQL(metadata.TableName, onlyIfExists);
        }

        private static string DropTableSQL(string name, bool onlyIfExists)
        {
            StringBuilder sqlDrop = new StringBuilder();

            if (onlyIfExists)
            {
                sqlDrop.AppendFormat("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}')", name);
            }

            sqlDrop.AppendFormat("DROP TABLE [dbo].[{0}]", name);

            return sqlDrop.ToString();
        }
        #endregion

        #region TableCreator

        public static void CreateTableIfNotExists<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            CreateTable<T>(connection, true, transaction, commandTimeout);
        }

        public static void CreateTable<T>(this IDbConnection connection, bool onlyIfNotExists, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);

            var values = new Dictionary<string, object>();
            var metadata = MetadataFor(type);

            var sql = TSQLGenerator.BuildCreate(metadata, onlyIfNotExists: onlyIfNotExists);

            connection.Execute(sql, transaction: transaction, commandTimeout: commandTimeout);
        }

        #endregion

    }
}
