using AliBuu.DataAccess.Helpers;
using AliBuu.Logger;
using AliBuu.Models.Includes;
using OvhWrapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;

namespace AliBuu.DataAccess
{
    public class SqlDataAccess
    {
        public static long SaveProduct(AliItem item)
        {
            // zapisuje kategorie
            var insertedId = SaveCategory(item.CategoryTree);
            if (insertedId == -1) throw new ArgumentException("item.CategoryTree cannot save category");
            item.CategoryId = insertedId;

            // connect to SQL
            using (SqlConnection connection = new SqlConnection(Keys.Default.DataAccess))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    //cmd.CommandText = "insert into Product values ('test', 'testurl', 1, 1,'test','test',2);";
                    cmd.CommandText = $@"
                    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                    merge Product as target
                    using (select {item.OfferNumber} as offNum) as src
                    on target.OfferNumber = src.offNum
                    when not matched then
                        insert values (@productName, @url, {item.OfferNumber}, {item.OwnerMemberId}, @priceUnit, @priceSymbol, {item.CategoryId})
                    when matched then
                        update set target.OfferNumber = src.offNum
                    output $action, inserted.id, src.*;";
                    cmd.Parameters.AddWithValue("@productName", item.Name);
                    cmd.Parameters.AddWithValue("@url", item.Url);
                    cmd.Parameters.AddWithValue("@priceUnit", item.PriceUnit);
                    cmd.Parameters.AddWithValue("@priceSymbol", item.PriceSymbol);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            var id = Convert.ToInt64(Convert.ToInt32(reader[1]));
                            item.Id = id;
                            // zapisz cene do bazy
                            SaveOffer(item);

                            // zapisz specyfikacje
                            SaveSpecification(item.Id, item.Specification);

                            // zapisuje zdjecia produktu na ovh
                            // N - normal
                            // S - small
                            SaveImages(item.Id, item.OfferNumber, item.Images, 'N');
                            SaveImages(item.Id, item.OfferNumber, item.LittleImages, 'S');
                            // zapisz zdjecia w bazie danych

                            // dla kazdego feedbacka
                            // zapisuje do bazy
                            // zapisuje do ovh
                        }
                    }


                    return item.Id;
                }
            }
        }

        private static void SaveImages(long productId, long offerNumber, List<string> images, char flag)
        {
            var uploadedImages = new Dictionary<int, string>();
            var counter = 0;
            foreach (var img in images)
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        var hash = img.GetHashCode();
                        var imageBytes = webClient.DownloadData(img);
                        var ext = img.Split('.').Last();
                        var imageName = $"{productId}_{counter}_{offerNumber}_{hash}.{ext}";
                        var uri = OvhStorage.ContainerObject.Set("images", imageName, imageBytes);
                        uploadedImages.Add(counter, uri.ToString());
                        counter++;
                    }
                    catch (WebException e)
                    {
                        Log.Exception("AliBuu.DataAccess", new string[] { "SqlDataAccess", "SaveImages" }, new Dictionary<string, object>()
                        {
                            {"PRODUCT_ID", productId },
                            {"IMAGE", img }
                        }, "Nie udalo zapisac sie zdjecia", e);
                    }
                }

            }
            if (!uploadedImages.Any()) return;
            using (SqlConnection connection = new SqlConnection(Keys.Default.DataAccess))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    var table = SqlPartHelper.GenerateIntStringTable("images", uploadedImages);
                    cmd.CommandText = $@"
                    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                    {table}
                    merge ProductImages as target
                    using @images as src
                    on target.ProductId = @productId and target.ImageNumber = src.tabKey and target.Flag = '{flag}'
                    when not matched then
                        insert (ProductId, Image, ImageNumber, Flag) values (@productId, src.tabValue, src.tabKey, '{flag}')
                    when matched then
                        update set target.Image = src.tabValue;";
                    cmd.Parameters.AddWithValue("@productId", productId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void SaveSpecification(long productId, Dictionary<string, string> specification)
        {
            if (!specification.Any()) return;
            // connect to SQL
            using (SqlConnection connection = new SqlConnection(Keys.Default.DataAccess))
            {
                connection.Open();
                var mappedSpecsToId = new Dictionary<string, int>();
                using (var cmd = connection.CreateCommand())
                {
                    var specTable = SqlPartHelper.GenerateStringTable("specs", specification.Select(q => q.Key).ToList());
                    cmd.CommandText = $@"
                    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                    {specTable}
                    merge SpecificationKeys as target
                    using @specs as src
                    on target.Name = src.id
                    when not matched then
                        insert (Name) values (src.id)
                    when matched then
                        update set target.Name = src.id
                    output $action, inserted.id, src.id;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            mappedSpecsToId.Add(reader[2].ToString(), Convert.ToInt32(reader[1]));
                        }
                    }
                }
                // specId, wartosc
                var mappedSpecs = new List<KeyValuePair<int, string>>();
                foreach(var spec in specification)
                {
                    if(mappedSpecsToId.TryGetValue(spec.Key, out int specId))
                    {
                        mappedSpecs.Add(new KeyValuePair<int, string>(specId, spec.Value));
                    }
                }


                using (var cmd = connection.CreateCommand())
                {
                    var table = SqlPartHelper.GenerateIntStringTable("table", mappedSpecs);

                    cmd.CommandText = $@"
                    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                    {table}
                    merge ProductSpecification as target
                    using @table as src
                    on target.ProductId = @productId and target.SpecificationId = src.tabKey
                    when not matched then
                        insert (ProductId, SpecificationId, SpecificationValue)
                        values (@productId, src.tabKey, src.tabValue)
                    when matched and target.SpecificationValue <> src.tabValue then
                        update set target.SpecificationValue = src.tabValue;";
                    cmd.Parameters.AddWithValue("@productId", productId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void SaveOffer(AliItem item)
        {
            // connect to SQL
            using (SqlConnection connection = new SqlConnection(Keys.Default.DataAccess))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = $@"
                    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;   
                    insert into ProductPrice values (
                    {item.Id}, 
                    @price, 
                    @discountPrice, 
                    {item.Orders});";
                    cmd.Parameters.AddWithValue("@price", item.Price);
                    cmd.Parameters.AddWithValue("@discountPrice", item.DiscountPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static int SaveCategory(List<string> category)
        {
            //prepare data
            category = category.Select(z => z.Replace("'", "''")).ToList();
            int AddCategory(string parent, string current, string fullCat)
            {
                int insertedId = -1;
                using (SqlConnection connection = new SqlConnection(Keys.Default.DataAccess))
                {
                    connection.Open();
                    // parent jest pusty wiec null
                    if (string.IsNullOrEmpty(parent))
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = 
                                string.Format(@"
                                SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                                merge Categories as target
                                using (select '{0}' as currCat) as src
                                on target.Name = src.currCat
                                when not matched then
                                    insert(ParentId, Name, FullName)
                                    values(null, src.currCat, '{1}');

                                select Id from Categories where Name = '{1}';", current, fullCat);
                            insertedId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = 
                                string.Format(@"
                                SET TRANSACTION ISOLATION LEVEL SNAPSHOT;
                                declare @parentId int;
                                select @parentId = id from Categories where Name = '{0}';

                                merge Categories as target
                                using (select '{1}' as currCat) as src
                                on target.Name = src.currCat
                                when not matched then
                                    insert(ParentId, Name,FullName)
                                    values (@parentId, src.currCat, '{2}');

                                select Id from Categories where Name = '{1}';", parent, current, fullCat);
                            insertedId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
                return insertedId;
            };

            var full = new List<string>();
            int addedCategoryId = -1;
            for (var i = 0; i < category.Count(); i++)
            {
                var parentCategory = "";
                if (i > 0)
                {
                    parentCategory = category[i - 1];
                }
                full.Add(category[i]);
                addedCategoryId = AddCategory(parentCategory, category[i], string.Join(" > ", full));
            }
            return addedCategoryId;
        }
    }
}
