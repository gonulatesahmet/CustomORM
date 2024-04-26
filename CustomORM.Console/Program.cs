

using LinqExample;

MyCustomDbSet<Product> product = new();
var data = product.Table
	.Join<Category>(x=>x.CategoryId)
	.Where(x=>x.Name.Contains("ahmet") || x.Name.Contains("mehmet"))
	.Order(x=>x.Id)
	.Group(x=>x.CategoryId)
	.CreateQuery();


Console.WriteLine(data);
Console.Read();