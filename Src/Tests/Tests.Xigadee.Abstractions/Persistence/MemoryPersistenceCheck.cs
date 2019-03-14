﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xigadee;

namespace Test.Xigadee
{
    public class TestClass 
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VersionId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the unique name.
        /// </summary>
        public string Name { get; set; }

        public string Type { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateUpdated { get; set; } 
    }

    [TestClass]
    public class TestMemoryPersistenceCheck
    {
        IRepositoryAsync<Guid, TestClass> _repo;

        private IEnumerable<Tuple<string, string>> References(TestClass c)
        {
            yield return new Tuple<String, string>("name", c.Name);
        }

        private IEnumerable<Tuple<string, string>> Properties(TestClass c)
        {
            yield return new Tuple<String, string>("type", c.Type);
            yield return new Tuple<String, string>("datecreated", c.DateCreated.ToString("o"));
        }
    
        //protected Tuple<string, Func<E, List<KeyValuePair<string, string>>, bool>> TBuild<E>(string key, Func<E, List<KeyValuePair<string, string>>, bool> function)
        //{
        //    return new Tuple<string, Func<E, List<KeyValuePair<string, string>>, bool>>(key, function);
        //}

        //protected string FilterValue(List<KeyValuePair<string, string>> p, string key)
        //{
        //    return p.Where((i) => i.Key == key)
        //            .Select((i) => i.Value)
        //            .FirstOrDefault();
        //}

        //protected IEnumerable<Tuple<string, Func<TestClass, List<KeyValuePair<string, string>>, bool>>> TransactionStateSearches()
        //{
        //    //This is the default state holder search
        //    yield return TBuild<TestClass>("", (e, p) =>
        //    {
        //        var typeCode = FilterValue(p, "type");

        //        return e.Type == typeCode;
        //    });
        //}


        [TestInitialize]
        public void Init()
        {
            _repo = new RepositoryMemory<Guid, TestClass>((r) => r.Id
                , referenceMaker: References
                , propertiesMaker: Properties
                , versionPolicy: new VersionPolicy<TestClass>(
                    (e) => e.VersionId.ToString("N").ToUpperInvariant()
                    , (e) => e.VersionId = Guid.NewGuid()
                )
                );
        }

        [TestMethod]
        public async Task Create()
        {
            var result2 = await _repo.Create(new TestClass() { Name = "Ferd", Type = "nerd" });
            var result3 = await _repo.Create(new TestClass() { Name = "Freda", Type = "nerd" });

            //This will fail as it has the same 'Name' as an earlier entity.
            var result4 = await _repo.Create(new TestClass() { Name = "Ferd", Type = "geek" });

            var result5 = await _repo.Create(new TestClass() { Name = "Ferdy", Type = "geek" });

            Assert.IsTrue(result2.IsSuccess);
            Assert.IsTrue(result3.IsSuccess);
            Assert.IsFalse(result4.IsSuccess);
            Assert.IsTrue(result5.IsSuccess);

            //var result = await _repo.Search();
            //Assert.IsTrue(result.Entity.Data.Count == 3);
        }


        [TestMethod]
        public async Task Update()
        {
            var e1 = new TestClass() { Name = "Gangly" };

            var result = await _repo.Create(e1);

            Assert.IsTrue(result.IsSuccess);

            var e2 = result.Entity;

            e2.DateUpdated = DateTime.UtcNow;

            var result2 = await _repo.Update(e2);

            Assert.IsTrue(result2.IsSuccess);

            //This should now fail as the version id has been changed by the earlier update.
            var result3 = await _repo.Update(e2);

            Assert.IsTrue(!result3.IsSuccess);
        }


        [TestMethod]
        public async Task Delete()
        {
            var result = await _repo.Create(new TestClass() { Name = "Ismail" });

            Assert.IsTrue(result.IsSuccess);

            var entity = result.Entity;

            var resultr1 = await _repo.Read(entity.Id);
            Assert.IsTrue(resultr1.IsSuccess);

            var result2 = await _repo.Delete(entity.Id);
            Assert.IsTrue(result2.IsSuccess);

            var resultr2 = await _repo.Read(entity.Id);
            Assert.IsFalse(resultr2.IsSuccess);

        }
    }
}
