

using LinqExample;

MyCustomDbSet<Product> product = new();
var data = product.Table
	.Join<Category>(x=>x.CategoryId)
	.Where(x=>x.Name.Contains("ahmet"))
	.Order(x=>x.Id)
	.CreateQuery();

Console.WriteLine(data);











Console.Read();