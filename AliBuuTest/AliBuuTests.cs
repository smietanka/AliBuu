using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AliBuu;
using AliBuu.Base;
using AliBuu.DataAccess;
using AliBuu.Models.Includes;
using AliBuuTest.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OvhWrapper;
using OvhWrapper.Types.Cloud.Storage;
using OvhWrapper.Types.Nichandle;

namespace AliBuuTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void RunCrawl()
        {
            var crawl = new Crawl(new CrawlOptions(18) { MaxPages = 1, OrderByOrdersDesc = false, ThreadCount = 8 });
            crawl.Start();
        }

        [TestMethod]
        public void SaveProduct_test()
        {
            SqlDataAccess.SaveProduct(new AliItem());
        }

        [TestMethod]
        public void SaveCategory_test()
        {
            var catId = SqlDataAccess.SaveCategory(new List<string>() { "strona glowna", "buty" });
            catId = SqlDataAccess.SaveCategory(new List<string>() { "strona glowna", "buty", "klapki" });
            catId = SqlDataAccess.SaveCategory(new List<string>() { "strona glowna", "buty", "glany" });
            catId = SqlDataAccess.SaveCategory(new List<string>() { "strona glowna", "czapki" });
            catId = SqlDataAccess.SaveCategory(new List<string>() { "promocje" });
            catId = SqlDataAccess.SaveCategory(new List<string>() { "promocje", "jakies" });
        }

        [TestMethod]
        public void GetAliItem()
        {
            var item = PageItemConverter.ToAliItem("https://www.aliexpress.com/item/Aquarium-Moss-Fish-Aquarium-Alive-Live-Fishes-Plants-Aquaristics-Shrimp-Aquarium-Moss-Ball-Water-Grass-Shrimp/32907012782.html?spm=2114.search0604.3.8.6d7528e2CqEu2E&ws_ab_test=searchweb0_0,searchweb201602_1_10065_10068_10843_319_10059_10884_317_10887_10696_321_322_453_10084_454_10083_10103_10618_10304_10307_10820_10301_10821_537_536,searchweb201603_35,ppcSwitch_0&algo_expid=983a3af3-dddd-4739-ba7a-96d5c0709dd2-1&algo_pvid=983a3af3-dddd-4739-ba7a-96d5c0709dd2&transAbTest=ae803_4");
            Assert.IsTrue(item.OfferNumber == 32907012782);

            var bootItem = PageItemConverter.ToAliItem("https://www.aliexpress.com/item/Rainforest-Champagne-bark-Aquarium-reptile-Feed-landscap-wooden-fish-tank-Rhododendron-tree-shaped-Moss-tree-natural/32956917064.html?spm=2114.33020108.8.17.380dSHCcSHCcrB&scm=1007.17258.127437.0&pvid=bdb64bfd-da42-4577-9aa9-d9b35a4f5494");
            Assert.IsTrue(bootItem.OfferNumber == 32956917064);

            var ii = PageItemConverter.ToAliItem("https://www.aliexpress.com/item/Natural-raw-stone-dragon-stone-ohko-fish-tank-landscaping-rockery-aquarium-decoration-bonsai-landscaping-Water-grass/32956879170.html?spm=2114.10010108.100009.3.1905735f8ZfNjY&gps-id=pcDetailLeftTopSell&scm=1007.13482.95643.0&scm_id=1007.13482.95643.0&scm-url=1007.13482.95643.0&pvid=484b6ddd-c262-4b0b-b801-bac128c7d5fe");
        }

        [TestMethod]
        public void OvhAPITest()
        {
            string ApplicationKey = "";
            string ApplicationSecret = "";
            string ConsumerKey = "";
            string ServiceName = "";

            var ovh = new OvhApiAccess("ovh-eu", ApplicationKey, ApplicationSecret, ConsumerKey, ServiceName);
            var meUser = ovh.Get<Nichandle>("/me");
            var storages = ovh.Get<List<Container>>("/cloud/project/{serviceName}/storage");
            var storage = ovh.Get<ContainerDetail>("/cloud/project/{serviceName}/storage/6348427062584276636e517552314a424d513d3d");
        }

        [TestMethod]
        public void OvhStorageTest()
        {
            string containerName = "pptest";
            string containerName2 = "pptest2";
            string fileName = "avatar.jpg";
            //string fileName = "example.txt";
            string pathToFile = Path.Combine(Environment.CurrentDirectory, fileName);

            string userName = "";
            string password = "";
            string tenantId = "";
            OvhStorage.SetupConnection(userName, password, tenantId, "WAW1");

            //create test container
            Assert.IsTrue(OvhStorage.Container.Create(containerName));
            Assert.IsTrue(OvhStorage.Container.Create(containerName2));

            //put file to testContainer
            Assert.IsTrue(OvhStorage.ContainerObject.Set(containerName, fileName, pathToFile));
            var items = OvhStorage.Container.List(containerName);
            Assert.IsTrue(items.Any());
            Assert.IsTrue(items.Any(z => z.Name.Equals(fileName)));

            //get info about file
            OvhStorage.ContainerObject.Info(containerName, fileName);

            //copy file to pptest2 container
            Assert.IsTrue(OvhStorage.ContainerObject.Copy(containerName, fileName, containerName2));

            //get file from container
            var bytes = OvhStorage.ContainerObject.Get(containerName, fileName);
            Assert.IsTrue(bytes.Any());

            //delete files from test containers
            Assert.IsTrue(OvhStorage.ContainerObject.Delete(containerName, fileName));
            Assert.IsTrue(OvhStorage.ContainerObject.Delete(containerName2, fileName));

            //delete container
            Assert.IsTrue(OvhStorage.Container.Delete(containerName));
            //delete container
            Assert.IsTrue(OvhStorage.Container.Delete(containerName2));


            //create test container
            Assert.IsTrue(OvhStorage.Container.Create(containerName));
            Assert.IsTrue(OvhStorage.Container.Create(containerName2));
            Assert.IsTrue(OvhStorage.ContainerObject.Set(containerName, fileName, pathToFile));
            Assert.IsTrue(OvhStorage.ContainerObject.Copy(containerName, fileName, containerName2));
            //delete objects with container
            Assert.IsTrue(OvhStorage.Container.DeleteForce(containerName));
            Assert.IsTrue(OvhStorage.Container.DeleteForce(containerName2));
        }
        [TestMethod]
        public void DeleteAllimagesFromOVH()
        {
            string userName = "";
            string password = "";
            string tenantId = "";
            OvhStorage.SetupConnection(userName, password, tenantId, "WAW1");
            OvhStorage.Container.DeleteAllObjects("images");
        }
    }
}
