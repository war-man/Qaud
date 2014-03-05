﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qaud.Test;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Qaud.RavenDB.Test
{
    [TestClass]
    public class RavenDataStoreSimpleTest : DataStoreSimpleTest
    {
        public class TestRavenDataStore : RavenClientDataStore<FooModel>, IDisposable
        {
            public TestRavenDataStore() : base(CreateDocumentStore())
            {
            }

            private static DocumentStore CreateDocumentStore()
            {
                var ret = new EmbeddableDocumentStore()
                {
                    Configuration =
                    {
                        RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true,
                        RunInMemory = true,
                    }
                };
                ret.Initialize();
                return ret;
            }

            void IDisposable.Dispose()
            {
                ((DocumentStore)DataSetImplementation).Dispose();
                base.Dispose();
            }
        }
        public RavenDataStoreSimpleTest() : base(new TestRavenDataStore())
        {
        }

        private DocumentStore DocumentStore
        {
            get { return (DocumentStore)base.DataStore.DataSetImplementation; }
        }

        protected override void AddItemToStore(FooModel item)
        {
            var session = DocumentStore.OpenSession();
            session.Store(item);
            session.SaveChanges();
            session.Dispose();
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            var session = DocumentStore.OpenSession();
            var refitem = session.Load<FooModel>(item.ID);
            session.Delete(refitem);
            session.SaveChanges();
            session.Dispose();
        }

        protected override FooModel GetItemById(long id)
        {
            var session = DocumentStore.OpenSession();
            var ret = session.Load<FooModel>(id.ToString());
            session.Dispose();
            return ret;
        }

        [TestMethod]
        public override void DataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public override void DataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public override void DataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public override void DataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public override void DataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public override void DataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public override void DataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public override void DataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public override void DataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public override void DataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
