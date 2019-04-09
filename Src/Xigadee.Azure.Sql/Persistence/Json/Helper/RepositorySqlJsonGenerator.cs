﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xigadee
{
    /// <summary>
    /// This class can be used to generate the SQL Schema for the JSON based tables and stored procedures.
    /// </summary>
    /// <typeparam name="E">The entity type.</typeparam>
    public class RepositorySqlJsonGenerator<E>
    {
        #region Declarations
        private SqlStoredProcedureResolver<E> _resolver;
        #endregion
        #region Constructor
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        public RepositorySqlJsonGenerator(SqlStoredProcedureResolver<E> resolver)
        {
            _resolver = resolver;
        } 
        #endregion

        #region ProcessTemplate(string resourceName)
        /// <summary>
        /// This method reads the internal template 
        /// </summary>
        /// <param name="resourceName">The resource partial path.</param>
        /// <returns>Returns the script.</returns>
        public string ProcessTemplate(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream($"Xigadee.Persistence.Json.SqlTemplates.{resourceName}"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();

                return ReplaceTokens(result);
            }
        }
        #endregion
        #region ReplaceTokens(string script)
        /// <summary>
        /// This method replaces the magic tokens in the Sql Script templates.
        /// </summary>
        /// <param name="script">The script to process.</param>
        /// <returns>Returns the script with the tokens replaced.</returns>
        public string ReplaceTokens(string script)
        {
            script = script.Replace("{EntityName}", _resolver.EntityName);

            script = script.Replace("{NamespaceTable}", _resolver.TableSchemaName);

            script = script.Replace("{NamespaceExternal}", _resolver.ExternalSchemaName);

            //Set the stored procedure names.
            Enum.GetValues(typeof(RepositoryMethod)).Cast<RepositoryMethod>().ForEach((m) => script = script.Replace($"{{sp{m.ToString()}}}", _resolver[m]));

            script = script.Replace("{spUpsertRP}", _resolver.StoredProcedureNameUpsertRP);

            return script;
        }
        #endregion


        public string AncillarySchema => ProcessTemplate("Ancillary.Schema.sql");

        public string AncillaryKvpTableType => ProcessTemplate("Ancillary.KvpTableType.sql");

        public string AncillaryIdTableType => ProcessTemplate("Ancillary.IdTableType.sql");

        #region ScriptAncillary()
        /// <summary>
        /// This method returns the Ancillary SQL for the application. This is the set of helper definitions that are needed to make the stored procedures work.
        /// </summary>
        /// <returns></returns>
        public string ScriptAncillary()
        {
            var sb = new StringBuilder();

            sb.AppendLine(AncillarySchema);
            sb.AppendLine("GO");
            sb.AppendLine(AncillaryKvpTableType);
            sb.AppendLine("GO");
            sb.AppendLine(AncillaryIdTableType);
            sb.AppendLine("GO");

            return sb.ToString();
        } 
        #endregion

        #region Tables
        public string TableEntity => ProcessTemplate("Tables.Table.sql");

        public string TableHistory => ProcessTemplate("Tables.TableHistory.sql");

        public string TableProperty => ProcessTemplate("Tables.TableProperty.sql");

        public string TablePropertyKey => ProcessTemplate("Tables.TablePropertyKey.sql");

        public string TableReference => ProcessTemplate("Tables.TableReference.sql");

        public string TableReferenceKey => ProcessTemplate("Tables.TableReferenceKey.sql");
        #endregion

        #region ScriptTable()
        /// <summary>
        /// This method returns the SQL scripts to create the necessary tables.
        /// </summary>
        /// <returns>Returns a SQL script.</returns>
        public string ScriptTable()
        {
            var sb = new StringBuilder();

            sb.AppendLine(TableReferenceKey);
            sb.AppendLine("GO");
            sb.AppendLine(TablePropertyKey);
            sb.AppendLine("GO");
            sb.AppendLine(TableEntity);
            sb.AppendLine("GO");
            sb.AppendLine(TableHistory);
            sb.AppendLine("GO");
            sb.AppendLine(TableProperty);
            sb.AppendLine("GO");
            sb.AppendLine(TableReference);
            sb.AppendLine("GO");

            return sb.ToString();
        } 
        #endregion

        /// <summary>
        /// This is the entity create stored procedure.
        /// </summary>
        public string SpCreate => ProcessTemplate("StoredProcedures.spCreate.sql");
        /// <summary>
        /// This is the entity delete stored procedure.
        /// </summary>
        public string SpDelete => ProcessTemplate("StoredProcedures.spDelete.sql");
        /// <summary>
        /// This is the entity delete by reference stored procedure.
        /// </summary>
        public string SpDeleteByRef => ProcessTemplate("StoredProcedures.spDeleteByRef.sql");
        /// <summary>
        /// This is the entity read stored procedure.
        /// </summary>
        public string SpRead => ProcessTemplate("StoredProcedures.spRead.sql");
        /// <summary>
        /// This is the entity read by reference stored procedure.
        /// </summary>
        public string SpReadByRef => ProcessTemplate("StoredProcedures.spReadByRef.sql");
        /// <summary>
        /// This is the entity update stored procedure.
        /// </summary>
        public string SpUpdate => ProcessTemplate("StoredProcedures.spUpdate.sql");
        /// <summary>
        /// This is the entity version check stored procedure.
        /// </summary>
        public string SpVersion => ProcessTemplate("StoredProcedures.spVersion.sql");
        /// <summary>
        /// This is the entity version by reference check stored procedure.
        /// </summary>
        public string SpVersionByRef => ProcessTemplate("StoredProcedures.spVersionByRef.sql");
        /// <summary>
        /// This is the internal reference/properties stored procedure
        /// </summary>
        public string SpUpsertRP => ProcessTemplate("StoredProcedures.spUpsertRP.sql");
        /// <summary>
        /// This is the entity search stored procedure.
        /// </summary>
        public string SpSearch => ProcessTemplate("StoredProcedures.spSearch.sql");
        /// <summary>
        /// This is the entity search entity stored procedure.
        /// </summary>
        public string SpSearchEntity => ProcessTemplate("StoredProcedures.spSearchEntity.sql");


        /// <summary>
        /// This method returns all the stored procedures.
        /// </summary>
        /// <param name="createoralter">Specifies whether the stored procedure should be created or altered.</param>
        /// <returns>Returns the SQL script.</returns>
        public string ScriptStoredProcedures(bool createoralter = false)
        {
            var sb = new StringBuilder();

            sb.AppendLine(SpUpsertRP);
            sb.AppendLine("GO");
            sb.AppendLine(SpCreate);
            sb.AppendLine("GO");
            sb.AppendLine(SpDelete);
            sb.AppendLine("GO");
            sb.AppendLine(SpDeleteByRef);
            sb.AppendLine("GO");
            sb.AppendLine(SpRead);
            sb.AppendLine("GO");
            sb.AppendLine(SpReadByRef);
            sb.AppendLine("GO");
            sb.AppendLine(SpUpdate);
            sb.AppendLine("GO");
            sb.AppendLine(SpVersion);
            sb.AppendLine("GO");
            sb.AppendLine(SpVersionByRef);
            sb.AppendLine("GO");

            if (createoralter)
                return sb.ToString().Replace("CREATE PROCEDURE", "CREATE OR ALTER PROCEDURE");
            else
                return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the S</returns>
        public string ScriptEntity()
        {
            var sb = new StringBuilder();

            sb.AppendLine(ScriptTable());
            sb.AppendLine(ScriptStoredProcedures());

            return sb.ToString();
        }
    }
}