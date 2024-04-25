using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqExample
{
	public class MyCustomDbSet<T> where T : class, IEntity
	{
		public Query<T> Table { get; }
		public MyCustomDbSet()
		{
			Table = new Query<T>();
		}
	}


	public class Query<T> : List<T> where T : class, IEntity
	{
		private List<(string, QueryPriority)> Queries { get; set; }

		public Query()
		{
			Queries = new()
			{
				($"SELECT * FROM [{typeof(T).Name}]", QueryPriority.Select)
			};
		}
		public Query<T> Join<J>(Expression<Func<T, string>> func) where J : class, IEntity
		{
			MemberExpression? memberExpression = (MemberExpression)func.Body;
			string propertyName = memberExpression.Member.Name;

			Queries.Add(($"INNER JOIN {typeof(T).Name} ON {typeof(T).Name}.{propertyName} = {typeof(J).Name}.Id", QueryPriority.Inner));
			return this;
		}

		public Query<T> Where(Expression<Func<T, bool>> predicate)
		{
			List<string> key = new();
			List<string> value = new();

			string expBody = "WHERE ";
			expBody += ((LambdaExpression)predicate).Body.ToString();
			expBody = expBody.Replace(predicate.Parameters[0].Name + ".", predicate.Parameters[0].Type.Name + ".")
				.Replace("AndAlso", "AND")
				.Replace("(", "")
				.Replace(")", "")
				.Replace("==", "=")
				.Replace("True", "1")
				.Replace("False", "0")
				.Replace(".Equals", " IN ")
				.Replace(".Contains", " LIKE ");

			Queries.Add((expBody, QueryPriority.Where));
			return this;
		}
		public Query<T> Order(Expression<Func<T, string>> predicate)
		{
			MemberExpression? memberExpression = (MemberExpression)predicate.Body;
			string propertyName = memberExpression.Member.Name;

			var dataControl = Queries.FirstOrDefault(x => x.Item2 == QueryPriority.Order);
			if (dataControl != default)
				dataControl.Item1 += $", {propertyName}";
			else
				Queries.Add(($"ORDER BY {propertyName}", QueryPriority.Order));

			return this;
		}

		public string CreateQuery()
		{
			string query = string.Empty;
			Queries.OrderBy(x => x.Item2);
			foreach (var item in Queries)
			{
				query += item.Item1 + " ";
			}
			return query;
		}
	}

	public enum QueryPriority
	{
		Select,
		Inner,
		Where,
		Group,
		Order,

	}
}
