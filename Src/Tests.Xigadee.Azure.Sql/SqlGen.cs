using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xigadee;

namespace Tests.Xigadee.Azure.Sql
{
    [PrepopulateEntities(typeof(Test1), typeof(User), typeof(Account))]
    public class TestSqlGenerationCollection : SqlGeneratorCollection
    {

        public SqlJsonGenerator Test1 => Get<Test1>();

        public SqlJsonGenerator User => Get<User>();

        public SqlJsonGenerator Account => Get<Account>();

    }

    [TestClass]
    public class SqlGen
    {
        [TestMethod]
        public void Verify_SqlStoredProcedureResolverUser()
        {
            var coll = new TestSqlGenerationCollection();

            var sgTest1 = new SqlJsonGenerator<Test1>(
                options: new RepositorySqlJsonOptions(true));

            var result = sgTest1.ExtractToFolder(mode: SqlExtractMode.MultipleFiles);
            var result2 = sgTest1.ExtractToFolder(mode: SqlExtractMode.SingleFileAndExtensions);

            var sqlTest1 = sgTest1.ScriptEntityWithoutExtension;


            var sqlTest1Extension = sgTest1.ScriptExtensionLogic;
            var sqlTest1ExtensionTable = sgTest1.ScriptExtensionTable;

            var sgUser = new SqlJsonGenerator<User>();
            var sqlUser = sgUser.SqlEntity;

            var sgAccount = new SqlJsonGenerator<Account>(
                options: new RepositorySqlJsonOptions(true));
            var sqlAccount = sgAccount.SqlEntity;

            var ancillary = sgTest1.Generator.ScriptAncillary;
        }

        [TestMethod]
        public void Verify_SqlStoredProcedureResolver()
        {
            var spNames = new SqlStoredProcedureResolver<Account>(schemaName: "External", interfix: "_");
            var generator = new RepositorySqlJsonGenerator<Account>(spNames);

            var sql1 = generator.TableEntity;
            var sql2 = generator.TableHistory;
            var sql3 = generator.TableProperty;
            var sql4 = generator.TablePropertyKey;
            var sql5 = generator.TableReference;
            var sql6 = generator.TableReferenceKey;

            var tab = generator.ScriptTables;

            var sql7 = generator.AncillarySchema;
            var sql8 = generator.AncillaryKvpTableType;
            var sql9 = generator.AncillaryIdTableType;

            var sp1 = generator.SpCreate;
            var sp2 = generator.SpDelete;
            var sp3 = generator.SpDeleteByRef;
            var sp4 = generator.SpUpdate;
            var sp5 = generator.SpRead;
            var sp6 = generator.SpReadByRef;
            var sp7 = generator.SpUpsertRP;
            var sp8 = generator.SpVersion;
            var sp9 = generator.SpVersionByRef;

            var spsearch = generator.SpSearch;
            var spsearchb = generator.SpSearchEntity;
            var spsearche = generator.SpSearchInternalBuild;

            var sps = generator.ScriptStoredProceduresManual(true);
            var spc = generator.ScriptStoredProceduresManual(false);

            var anc = generator.ScriptAncillary;

            var scr = generator.ScriptEntity;
        }

        [TestMethod]
        public async Task AccountTest()
        {
            var server = "tpjrtest";
            var uname = "ExternalAccess";
            var pwd = "123Tomorrow";
            var catalog = "xgtest3";//This db is deleted at the end of each test set.
            var conn = $"Server=tcp:{server}.database.windows.net,1433;Initial Catalog={catalog};Persist Security Info=False;User ID={uname};Password={pwd};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            var spNames2 = new SqlStoredProcedureResolver<Account>(schemaName: "External", interfix: "_");

            var repo = new RepositorySqlJson<Guid, Account>(conn, spNamer: spNames2);

            var account = new Account();
            account.Type = AccountType.Type0;
            account.GroupId = Guid.NewGuid();
            account.UserId = new Guid("{5C30F253-B01D-4794-A53D-CB3E08C660A8}");
            account.PropertiesSet("email", "pstancer@outlook.com");

            try
            {
                var attempt1 = await repo.Create(account);
                var read1 = await repo.Read(account.Id);
                account.PropertiesSet("wahey", "123");
                account.Type = AccountType.Type1;
                var attempt2 = await repo.Update(account);
                var attempt3 = await repo.Update(account);
                var v1 = await repo.Version(account.Id);

                var s1 = await repo.Search(new SearchRequest());
                var s1e = await repo.SearchEntity(new SearchRequest());

                var s1m = await repo.Search(new SearchRequest());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
